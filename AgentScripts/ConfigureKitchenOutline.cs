using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using Newtonsoft.Json;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.U2D.Aseprite;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

// One-time migration. Uses the open scene so unsaved user edits are preserved.
public static class ConfigureKitchenOutline
{
    const string ArtPath = "Assets/Sprites/Backgrounds/FastFood/Kitchen/bg_kitchen.aseprite";
    const string MaskFolder = "Assets/Sprites/Backgrounds/FastFood/Kitchen/InteractionMasks";
    const string MaterialPath = "Assets/Shaders/Interaction/LayerOutline.mat";
    const int Padding = 3;
    static readonly (FastFoodStationKind kind, string layer, string mask)[] Objects = {
        (FastFoodStationKind.Fryer, "Layer 3 Fries", "Fryer"),
        (FastFoodStationKind.Fridge, "Layer 3 Fridge", "Fridge"),
        (FastFoodStationKind.Dispenser, "Layer 3 Soda", "Dispenser")
    };

    public static string Apply(bool fuseAtFryer = true)
    {
        var scene = SceneManager.GetActiveScene();
        if (Application.isPlaying || scene.path != "Assets/Scenes/FastFood.unity")
            throw new InvalidOperationException("Open FastFood in Edit Mode first.");
        var background = GameObject.Find("Kitchen/Background").GetComponent<SpriteRenderer>();
        if (background.transform.Find("InteractionOutlines") != null)
            throw new InvalidOperationException("Kitchen outlines already exist; adjust them in Inspector.");
        Directory.CreateDirectory("Logs/KitchenOutline");
        if (!EditorSceneManager.SaveScene(scene, "Logs/KitchenOutline/FastFood.before-apply.unity", true))
            throw new IOException("Could not back up the open scene.");
        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
        using var source = AsepriteReader.ReadFile(ArtPath);
        if (source.width != 700 || source.height != 180 || source.frameData.Count != 10)
            throw new InvalidOperationException("Expected the supplied 700x180 kitchen with ten frames.");
        var layers = source.frameData.SelectMany(f => f.chunks).OfType<LayerChunk>().ToArray();
        foreach (var obj in Objects)
            if (!layers.Any(l => l.name == obj.layer)) throw new InvalidOperationException("Missing " + obj.layer);
        var shader = Shader.Find("DREAMS/Interaction Layer Outline");
        if (shader == null || ShaderUtil.ShaderHasError(shader)) throw new InvalidOperationException("Outline shader is not ready.");
        var importer = (AsepriteImporter)AssetImporter.GetAtPath(ArtPath);
        importer.importMode = FileImportModes.AnimatedSprite;
        importer.layerImportMode = LayerImportModes.MergeFrame;
        importer.pivotSpace = PivotSpaces.Canvas;
        importer.pivotAlignment = SpriteAlignment.Center;
        importer.spritePixelsPerUnit = 100;
        importer.filterMode = FilterMode.Point;
        importer.mipmapEnabled = false;
        importer.generatePhysicsShape = false;
        importer.SaveAndReimport();
        var frames = AssetDatabase.LoadAllAssetsAtPath(ArtPath).OfType<Sprite>().ToDictionary(s => s.name);
        if (frames.Count != 10) throw new InvalidOperationException("Expected ten imported background frames.");
        var material = AssetDatabase.LoadAssetAtPath<Material>(MaterialPath);
        if (material == null) { material = new Material(shader); AssetDatabase.CreateAsset(material, MaterialPath); }
        material.SetColor("_OutlineColor", new Color(1, .88f, .58f, .95f));
        material.SetFloat("_Width", 1);
        AssetDatabase.SaveAssetIfDirty(material);
        Undo.RecordObject(background.transform, "Restore original layered kitchen");
        Undo.RecordObject(background, "Restore original layered kitchen");
        background.transform.localScale = Vector3.one;
        // Grow to the left, preserving the existing right-hand exit and spawn.
        background.transform.position = new Vector3(-21, 0, 0);
        background.sprite = frames["Frame_0"];
        var root = new GameObject("InteractionOutlines");
        Undo.RegisterCreatedObjectUndo(root, "Add kitchen interaction outlines");
        root.transform.SetParent(background.transform, false);
        Directory.CreateDirectory(MaskFolder);
        var stations = scene.GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<FastFoodServiceStation>(true))
            .ToDictionary(s => s.Kind);
        var manifests = new List<MaskEntry>();
        foreach (var obj in Objects)
        {
            int layer = Array.FindIndex(layers, l => l.name == obj.layer);
            var cell = Cell(source, 0, layer);
            // These object layers are static; the background's lamp animation stays merged.
            for (int f = 1; f < source.frameData.Count; f++)
            {
                var other = Cell(source, f, layer);
                if (other.width != cell.width || other.height != cell.height || other.posX != cell.posX || other.posY != cell.posY ||
                    !other.image.ToArray().SequenceEqual(cell.image.ToArray()))
                    throw new InvalidOperationException(obj.layer + " is animated; export a mask per frame first.");
            }
            string maskPath = MaskFolder + "/" + obj.mask + ".png";
            WriteMask(maskPath, cell);
            var mask = AssetDatabase.LoadAssetAtPath<Sprite>(maskPath);
            var position = new Vector3((cell.posX + cell.width * .5f - source.width * .5f) / 100f,
                (source.height * .5f - cell.posY - cell.height * .5f) / 100f, -.01f);
            var station = stations[obj.kind];
            Undo.RecordObject(station.transform, "Align kitchen interaction with Aseprite layer");
            station.transform.position = new Vector3(background.transform.position.x + position.x, station.transform.position.y, station.transform.position.z);
            SetTaskOnly(station);
            Outline(root.transform, obj.mask, station, mask, position, material, background);
            manifests.Add(new MaskEntry { file = obj.mask + ".png", layer = obj.layer,
                x = cell.posX, y = cell.posY, width = cell.width, height = cell.height, padding = Padding });
        }
        var fuse = stations[FastFoodStationKind.FuseBox];
        Undo.RecordObject(fuse.transform, "Place kitchen fuse interaction");
        if (fuseAtFryer)
        {
            fuse.transform.position = stations[FastFoodStationKind.Fryer].transform.position;
            SetTaskOnly(fuse);
            var fryer = root.transform.Find("Fryer").GetComponent<SpriteRenderer>();
            Outline(root.transform, "FryerFuse", fuse, fryer.sprite, fryer.transform.localPosition, material, background);
        }
        else
        {
            // The new artwork adds 200 pixels on the left. Keep the legacy wall interaction there.
            fuse.transform.position += Vector3.left * 2;
        }
        foreach (string name in new[] { "ServiceFryer", "ServiceFridge", "ServiceDispenser", "ServiceFuseBox" })
        {
            var old = GameObject.Find("Kitchen/" + name);
            if (old != null) Undo.DestroyObjectImmediate(old);
        }
        SetX(GameObject.Find("Kitchen/Floor").transform, -21);
        var floor = GameObject.Find("Kitchen/Floor").GetComponent<BoxCollider2D>();
        Undo.RecordObject(floor, "Extend kitchen floor"); floor.size = new Vector2(7.4f, floor.size.y);
        SetX(GameObject.Find("Kitchen/LeftBoundary").transform, -24.65f);
        var bounds = GameObject.Find("CAMERA/KitchenConfiner").GetComponent<BoxCollider2D>();
        SetX(bounds.transform, -21); Undo.RecordObject(bounds, "Extend kitchen camera bounds");
        bounds.size = new Vector2(7, bounds.size.y);
        var surface = GameObject.Find("CharacterShadowFloors/KitchenShadowFloor").GetComponent<CharacterShadowSurface>();
        var surfaceFields = new SerializedObject(surface);
        var rect = surfaceFields.FindProperty("_floorBounds").rectValue; rect.x = -24.5f; rect.width = 7;
        surfaceFields.FindProperty("_floorBounds").rectValue = rect; surfaceFields.ApplyModifiedProperties();
        ConfigureFlicker(background, source, layers, frames);
        var manifest = new MaskManifest { source = ArtPath,
            sourceSha256 = Hash(File.ReadAllBytes(ArtPath)), masks = manifests.ToArray() };
        File.WriteAllText(MaskFolder + "/source-layers.json", JsonConvert.SerializeObject(manifest, Formatting.Indented));
        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
        Physics2D.SyncTransforms();
        EditorSceneManager.MarkSceneDirty(scene);
        if (!EditorSceneManager.SaveScene(scene)) throw new IOException("Could not save FastFood.");
        AssetDatabase.SaveAssets();
        return "Restored 700x180 Aseprite kitchen, removed four generated world objects, aligned layer outlines and stations, extended floor/camera/shadows left, retained ten-frame lamp animation and existing interaction UI. Fuse at fryer=" + fuseAtFryer;
    }

    static void SetTaskOnly(FastFoodServiceStation station)
    {
        var fields = new SerializedObject(station);
        fields.FindProperty("_onlyWhileTaskPending").boolValue = true;
        fields.ApplyModifiedProperties();
    }
    static void SetX(Transform transform, float x)
    {
        Undo.RecordObject(transform, "Extend original kitchen");
        var p = transform.position; p.x = x; transform.position = p;
    }
    static void Outline(Transform parent, string name, FastFoodServiceStation station, Sprite sprite, Vector3 position, Material material, SpriteRenderer background)
    {
        var obj = new GameObject(name); Undo.RegisterCreatedObjectUndo(obj, "Add layer outline");
        obj.transform.SetParent(parent, false); obj.transform.localPosition = position;
        var renderer = obj.AddComponent<SpriteRenderer>(); renderer.sprite = sprite; renderer.sharedMaterial = material;
        renderer.sortingLayerID = background.sortingLayerID; renderer.sortingOrder = background.sortingOrder + 1;
        renderer.enabled = false;
        var outline = obj.AddComponent<FastFoodStationOutline>();
        var fields = new SerializedObject(outline);
        fields.FindProperty("_station").objectReferenceValue = station;
        fields.FindProperty("_outline").objectReferenceValue = renderer;
        fields.ApplyModifiedProperties();
    }
    static void WriteMask(string path, CellChunk cell)
    {
        int width = cell.width + 2 * Padding, height = cell.height + 2 * Padding;
        var tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
        try
        {
            var pixels = new Color32[width * height];
            for (int y = 0; y < cell.height; y++) for (int x = 0; x < cell.width; x++)
                pixels[(Padding + cell.height - 1 - y) * width + Padding + x] = new Color32(255, 255, 255, cell.image[y * cell.width + x].a);
            tex.SetPixels32(pixels); tex.Apply(); File.WriteAllBytes(path, tex.EncodeToPNG());
        }
        finally { UnityEngine.Object.DestroyImmediate(tex); }
        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
        var importer = (TextureImporter)AssetImporter.GetAtPath(path);
        importer.textureType = TextureImporterType.Sprite; importer.spriteImportMode = SpriteImportMode.Single;
        importer.spritePixelsPerUnit = 100; importer.spritePivot = new Vector2(.5f, .5f);
        var settings = new TextureImporterSettings(); importer.ReadTextureSettings(settings);
        settings.spriteMeshType = SpriteMeshType.FullRect; settings.spriteAlignment = (int)SpriteAlignment.Center;
        settings.spriteGenerateFallbackPhysicsShape = false; importer.SetTextureSettings(settings);
        importer.filterMode = FilterMode.Point; importer.mipmapEnabled = false;
        importer.wrapMode = TextureWrapMode.Clamp; importer.alphaIsTransparency = true;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.SaveAndReimport();
    }
    static CellChunk Cell(AsepriteFile file, int frame, int layer)
    {
        var cell = file.frameData[frame].chunks.OfType<CellChunk>().Single(c => c.layerIndex == layer);
        return cell.cellType == CellTypes.LinkedCell ? Cell(file, cell.linkedToFrame, layer) : cell;
    }
    static void ConfigureFlicker(SpriteRenderer background, AsepriteFile file, LayerChunk[] layers, Dictionary<string, Sprite> sprites)
    {
        int layer = Array.FindIndex(layers, l => l.name == "Layer 4.1");
        var first = Cell(file, 0, layer);
        var strips = new List<Vector2Int>(); int start = -1;
        for (int x = 0; x <= first.width; x++)
        {
            bool bright = x < first.width && Enumerable.Range(0, Math.Min(31, (int)first.height)).Any(y => {
                var p = first.image[y * first.width + x]; return p.a > 200 && p.r > 160 && p.g > 180 && p.b > 100;
            });
            if (bright && start < 0) start = x;
            if (!bright && start >= 0)
            {
                var strip = new Vector2Int(start + first.posX, x - 1 + first.posX);
                if (strips.Count > 0 && strip.x - strips.Last().y <= 3) { var old = strips.Last(); strips[strips.Count - 1] = new Vector2Int(old.x, strip.y); }
                else strips.Add(strip);
                start = -1;
            }
        }
        var flicker = background.GetComponent<KitchenLampFlicker>();
        var fields = new SerializedObject(flicker);
        var lamps = fields.FindProperty("_lamps");
        if (strips.Count != 6 || lamps.arraySize != 6) throw new InvalidOperationException("Expected six kitchen lamps.");
        fields.FindProperty("_backgroundOverride").objectReferenceValue = null;
        var frames = fields.FindProperty("_frames"); frames.arraySize = file.frameData.Count;
        for (int i = 0; i < strips.Count; i++)
        {
            float center = (strips[i].x + strips[i].y) * .5f;
            var lamp = (Light2D)lamps.GetArrayElementAtIndex(i).objectReferenceValue;
            Undo.RecordObject(lamp.transform, "Align kitchen lamp with Aseprite");
            lamp.transform.position = background.transform.TransformPoint(new Vector3((center - file.width * .5f) / 100f, (file.height * .5f - 31) / 100f, -.1f));
        }
        for (int f = 0; f < file.frameData.Count; f++)
        {
            var frame = frames.GetArrayElementAtIndex(f);
            frame.FindPropertyRelative("sprite").objectReferenceValue = sprites["Frame_" + f];
            frame.FindPropertyRelative("duration").floatValue = file.frameData[f].frameDuration / 1000f;
            var levels = frame.FindPropertyRelative("lampLevels"); levels.arraySize = strips.Count;
            for (int i = 0; i < strips.Count; i++)
            {
                int center = (strips[i].x + strips[i].y) / 2;
                double baseEnergy = Energy(first, center), energy = Energy(Cell(file, f, layer), center);
                levels.GetArrayElementAtIndex(i).floatValue = baseEnergy > 0 ? Mathf.Clamp01((float)(energy / baseEnergy)) : 0;
            }
        }
        fields.ApplyModifiedProperties();
    }
    static double Energy(CellChunk cell, int center)
    {
        double sum = 0;
        for (int y = Math.Max(0, 32 - cell.posY); y < cell.height; y++)
        for (int x = Math.Max(0, center - 25 - cell.posX); x <= Math.Min(cell.width - 1, center + 25 - cell.posX); x++)
        { var p = cell.image[y * cell.width + x]; sum += p.a * (.2126 * p.r + .7152 * p.g + .0722 * p.b); }
        return sum;
    }
    static string Hash(byte[] bytes) { using var hash = SHA256.Create(); return BitConverter.ToString(hash.ComputeHash(bytes)).Replace("-", "").ToLowerInvariant(); }
    [Serializable] class MaskManifest { public string source; public string sourceSha256; public MaskEntry[] masks; }
    [Serializable] class MaskEntry { public string file; public string layer; public int x, y, width, height, padding; }
}
