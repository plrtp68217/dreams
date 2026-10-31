using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.U2D.Aseprite;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

public static class BuildKitchenLampFlicker
{
    private const string SourcePath = "Assets/Sprites/Backgrounds/FastFood/Kitchen/bg_kitchen.aseprite";

    public static string Build()
    {
        var scene = SceneManager.GetActiveScene();
        if (Application.isPlaying || scene.path != "Assets/Scenes/FastFood.unity")
            throw new InvalidOperationException("Open FastFood in Edit Mode first.");
        AssetDatabase.ImportAsset(SourcePath, ImportAssetOptions.ForceSynchronousImport);
        var importer = (AsepriteImporter)AssetImporter.GetAtPath(SourcePath);
        importer.importMode = FileImportModes.AnimatedSprite;
        importer.layerImportMode = LayerImportModes.MergeFrame;
        importer.pivotSpace = PivotSpaces.Canvas;
        importer.pivotAlignment = SpriteAlignment.Center;
        importer.spritePixelsPerUnit = 100;
        importer.filterMode = FilterMode.Point;
        importer.mipmapEnabled = false;
        importer.generatePhysicsShape = false;
        importer.SaveAndReimport();

        using var source = AsepriteReader.ReadFile(SourcePath);
        var layers = source.frameData.SelectMany(f => f.chunks).OfType<LayerChunk>().ToArray();
        int lampLayer = Array.FindIndex(layers, l => l.name == "Layer 5");
        int glowLayer = Array.FindIndex(layers, l => l.name == "Layer 4.1");
        if (lampLayer < 0 || glowLayer < 0) throw new InvalidOperationException("Kitchen lamp/glow layers not found in source.");
        var firstLamp = Cell(source, 0, lampLayer);
        var strips = FindLampStrips(firstLamp);
        if (strips.Count != 6) throw new InvalidOperationException("Expected six ceiling lamps in kitchen source.");
        var sprites = AssetDatabase.LoadAllAssetsAtPath(SourcePath).OfType<Sprite>().ToDictionary(s => s.name);
        var background = GameObject.Find("Kitchen/Background").GetComponent<SpriteRenderer>();
        Undo.RecordObject(background, "Animate kitchen lamps from Aseprite");
        background.sprite = sprites["Frame_0"];
        var controller = background.GetComponent<KitchenLampFlicker>();
        if (controller == null) controller = Undo.AddComponent<KitchenLampFlicker>(background.gameObject);
        var root = Child(background.transform, "CeilingLights");
        var lamps = new Light2D[strips.Count];
        for (int i = 0; i < strips.Count; i++)
        {
            float centerX = (strips[i].x + strips[i].y) * 0.5f;
            var go = Child(root, "Lamp" + (i + 1).ToString("00") + "_SpotLight2D").gameObject;
            Undo.RecordObject(go.transform, "Place kitchen lamp");
            go.transform.localPosition = new Vector3((centerX - source.width * 0.5f) / 100f, (source.height * 0.5f - 31f) / 100f, -0.1f);
            go.transform.localRotation = Quaternion.Euler(0, 0, 180);
            go.transform.localScale = Vector3.one;
            var light = go.GetComponent<Light2D>();
            if (light == null) light = Undo.AddComponent<Light2D>(go);
            Undo.RecordObject(light, "Set kitchen spotlight");
            light.lightType = Light2D.LightType.Point;
            light.pointLightInnerAngle = 24;
            light.pointLightOuterAngle = 78;
            light.pointLightInnerRadius = 0.15f;
            light.pointLightOuterRadius = 1.8f;
            light.falloffIntensity = 0.55f;
            light.color = new Color(0.9f, 1f, 0.78f);
            light.intensity = 0.65f;
            light.blendStyleIndex = 0;
            light.shadowsEnabled = false;
            light.targetSortingLayers = new[] { SortingLayer.NameToID("Background"), SortingLayer.NameToID("ForeGround"), SortingLayer.NameToID("Objects"), SortingLayer.NameToID("Characters") };
            light.enabled = true;
            lamps[i] = light;
        }

        var settings = new SerializedObject(controller);
        settings.FindProperty("_background").objectReferenceValue = background;
        settings.FindProperty("_player").objectReferenceValue = GameObject.Find("Player").transform;
        settings.FindProperty("_kitchenBounds").objectReferenceValue = GameObject.Find("CAMERA/KitchenConfiner").GetComponent<Collider2D>();
        settings.FindProperty("_minInterval").floatValue = 6;
        settings.FindProperty("_maxInterval").floatValue = 12;
        var lights = settings.FindProperty("_lamps");
        lights.arraySize = lamps.Length;
        for (int i = 0; i < lamps.Length; i++) lights.GetArrayElementAtIndex(i).objectReferenceValue = lamps[i];
        var frames = settings.FindProperty("_frames");
        frames.arraySize = source.frameData.Count;
        var baseGlow = Cell(source, 0, glowLayer);
        for (int frame = 0; frame < source.frameData.Count; frame++)
        {
            var entry = frames.GetArrayElementAtIndex(frame);
            entry.FindPropertyRelative("sprite").objectReferenceValue = sprites["Frame_" + frame];
            entry.FindPropertyRelative("duration").floatValue = source.frameData[frame].frameDuration / 1000f;
            var levels = entry.FindPropertyRelative("lampLevels");
            levels.arraySize = lamps.Length;
            var lampCell = Cell(source, frame, lampLayer);
            var glowCell = Cell(source, frame, glowLayer);
            for (int lamp = 0; lamp < lamps.Length; lamp++)
            {
                float center = (strips[lamp].x + strips[lamp].y) * 0.5f;
                bool on = HasBrightBulb(lampCell, strips[lamp]);
                float brightness = on ? Mathf.Clamp01((float)(GlowEnergy(glowCell, center) / GlowEnergy(baseGlow, center))) : 0;
                levels.GetArrayElementAtIndex(lamp).floatValue = brightness;
            }
        }
        settings.ApplyModifiedProperties();
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        return "Kitchen: six synchronized Light2D spots, ten original 100ms frames, random 6-12s intervals while player is in kitchen.";
    }

    private static CellChunk Cell(AsepriteFile file, int frame, int layer) => file.frameData[frame].chunks.OfType<CellChunk>().Single(c => c.layerIndex == layer);
    private static bool Bright(Color32 p) => p.a > 200 && p.r > 160 && p.g > 180 && p.b > 100;

    private static List<Vector2Int> FindLampStrips(CellChunk cell)
    {
        var strips = new List<Vector2Int>();
        int start = -1;
        for (int x = 0; x <= cell.width; x++)
        {
            bool bright = x < cell.width && Enumerable.Range(0, cell.height).Any(y => Bright(cell.image[y * cell.width + x]));
            if (bright && start < 0) start = x;
            if (!bright && start >= 0)
            {
                strips.Add(new Vector2Int(start + cell.posX, x - 1 + cell.posX));
                start = -1;
            }
        }
        return strips;
    }

    private static bool HasBrightBulb(CellChunk cell, Vector2Int strip)
    {
        for (int x = strip.x; x <= strip.y; x++)
            for (int y = 0; y < cell.height; y++)
                if (Bright(cell.image[y * cell.width + x - cell.posX])) return true;
        return false;
    }

    private static double GlowEnergy(CellChunk cell, float center)
    {
        double energy = 0;
        for (int x = Mathf.RoundToInt(center) - 12; x <= Mathf.RoundToInt(center) + 12; x++)
        {
            if (x < cell.posX || x >= cell.posX + cell.width) continue;
            for (int y = 0; y < cell.height; y++)
            {
                var p = cell.image[y * cell.width + x - cell.posX];
                energy += p.a * (0.2126 * p.r + 0.7152 * p.g + 0.0722 * p.b);
            }
        }
        return energy;
    }

    private static Transform Child(Transform parent, string name)
    {
        var child = parent.Find(name);
        if (child != null) return child;
        var go = new GameObject(name);
        Undo.RegisterCreatedObjectUndo(go, "Create " + name);
        go.transform.SetParent(parent, false);
        return go.transform;
    }
}
