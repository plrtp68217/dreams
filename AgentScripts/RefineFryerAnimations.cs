using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// Narrow update of the existing close-up; preserves scene layout and manual settings.
public static class RefineFryerAnimations
{
    const string Folder = "Assets/Sprites/FastFoodService/FryerCloseUp/";
    public static string Apply()
    {
        var scene = SceneManager.GetActiveScene();
        if (Application.isPlaying || scene.path != "Assets/Scenes/FastFood.unity")
            throw new InvalidOperationException("Open FastFood in Edit Mode");
        var fryer = UnityEngine.Object.FindFirstObjectByType<FastFoodFryerCloseUp>();
        if (fryer == null) throw new InvalidOperationException("Configure the existing fryer first");
        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
        foreach (string name in new[] { "Idle", "Switch_00", "Switch_01", "Switch_02" })
        {
            string path = Folder + name + ".png";
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.filterMode = FilterMode.Point;
            importer.mipmapEnabled = false;
            importer.npotScale = TextureImporterNPOTScale.None;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.isReadable = true;
            importer.alphaIsTransparency = true;
            var settings = new TextureImporterSettings(); importer.ReadTextureSettings(settings);
            settings.spriteMeshType = SpriteMeshType.FullRect;
            settings.spriteGenerateFallbackPhysicsShape = false;
            importer.SetTextureSettings(settings); importer.SaveAndReimport();
        }
        var so = new SerializedObject(fryer);
        var frames = so.FindProperty("_switchFrames"); frames.arraySize = 3;
        for (int i = 0; i < 3; i++)
            frames.GetArrayElementAtIndex(i).objectReferenceValue = AssetDatabase.LoadAssetAtPath<Sprite>(Folder + "Switch_" + i.ToString("00") + ".png");
        var idle = AssetDatabase.LoadAssetAtPath<Sprite>(Folder + "Idle.png");
        so.FindProperty("_idleForeground").objectReferenceValue = idle;
        so.ApplyModifiedPropertiesWithoutUndo();
        ((UnityEngine.UI.Image)so.FindProperty("_foreground").objectReferenceValue).sprite = idle;
        EditorUtility.SetDirty(fryer); EditorSceneManager.MarkSceneDirty(scene);
        if (!EditorSceneManager.SaveScene(scene)) throw new Exception("Scene save failed");
        return "Added three authored switch states, smoke-free idle and smooth burger pickup";
    }
}
