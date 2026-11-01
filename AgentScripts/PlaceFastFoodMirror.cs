using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

public static class PlaceFastFoodMirror
{
    const string SpritePath = "Assets/Sprites/Backgrounds/FastFood/Props/StoryMirror.png";

    public static string Apply()
    {
        var scene = SceneManager.GetActiveScene();
        if (Application.isPlaying || scene.path != "Assets/Scenes/FastFood.unity")
            throw new InvalidOperationException("Open FastFood in Edit Mode first.");
        var till = scene.GetRootGameObjects().Single(g => g.name == "TILL");
        if (till.transform.Find("StoryMirror") != null)
            throw new InvalidOperationException("StoryMirror already exists; adjust the existing object manually.");
        Directory.CreateDirectory("Logs/FastFoodMirror");
        EditorSceneManager.SaveScene(scene, "Logs/FastFoodMirror/FastFood.before-place.unity", true);
        AssetDatabase.ImportAsset(SpritePath, ImportAssetOptions.ForceSynchronousImport);
        var importer = (TextureImporter)AssetImporter.GetAtPath(SpritePath);
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.spritePixelsPerUnit = 100;
        var settings = new TextureImporterSettings();
        importer.ReadTextureSettings(settings);
        settings.spriteAlignment = (int)SpriteAlignment.BottomCenter;
        settings.spritePivot = new Vector2(.5f, 0);
        settings.spriteMeshType = SpriteMeshType.FullRect;
        importer.SetTextureSettings(settings);
        importer.filterMode = FilterMode.Point;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.mipmapEnabled = false;
        importer.alphaIsTransparency = true;
        importer.wrapMode = TextureWrapMode.Clamp;
        importer.SaveAndReimport();
        var mirror = new GameObject("StoryMirror");
        Undo.RegisterCreatedObjectUndo(mirror, "Place FastFood story mirror");
        mirror.transform.SetParent(till.transform, false);
        mirror.transform.position = new Vector3(-4.98f, -.73f, 0);
        var renderer = mirror.AddComponent<SpriteRenderer>();
        renderer.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(SpritePath);
        renderer.sharedMaterial = scene.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<SpriteRenderer>(true))
            .First(r => r.name == "FastFoodHall - 6").sharedMaterial;
        renderer.sortingLayerName = "Objects";
        renderer.sortingOrder = 7;
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        return "Placed TILL/StoryMirror at (-4.98, -0.73, 0), PPU 100, scale 1, Point filter; no interactions or colliders.";
    }

    public static string Preview()
    {
        var original = Camera.main;
        var obj = new GameObject("Temporary mirror preview camera");
        var target = new RenderTexture(1600, 900, 24);
        var image = new Texture2D(1600, 900, TextureFormat.RGB24, false);
        var previous = RenderTexture.active;
        try
        {
            var camera = obj.AddComponent<Camera>();
            camera.CopyFrom(original);
            camera.enabled = false;
            camera.transform.SetPositionAndRotation(new Vector3(-4.9f, original.transform.position.y,
                original.transform.position.z), original.transform.rotation);
            camera.ResetWorldToCameraMatrix();
            camera.ResetCullingMatrix();
            camera.ResetProjectionMatrix();
            EditorUtility.CopySerialized(original.GetComponent<UniversalAdditionalCameraData>(),
                obj.AddComponent<UniversalAdditionalCameraData>());
            camera.targetTexture = target;
            camera.aspect = 1600f / 900f;
            camera.Render();
            RenderTexture.active = target;
            image.ReadPixels(new Rect(0, 0, 1600, 900), 0, 0);
            image.Apply();
            string path = "Logs/FastFoodMirror/preview.png";
            File.WriteAllBytes(path, image.EncodeToPNG());
            return path;
        }
        finally
        {
            RenderTexture.active = previous;
            UnityEngine.Object.DestroyImmediate(image);
            target.Release();
            UnityEngine.Object.DestroyImmediate(target);
            UnityEngine.Object.DestroyImmediate(obj);
        }
    }
}
