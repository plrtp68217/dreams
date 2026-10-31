using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class ConfigureDialogueTypingAudio
{
    private const string PrefabPath = "Assets/Prefabs/CAMERA.prefab";
    private const string ClipPath = "Assets/Audio/UI/DialogueTyping.mp3";

    public static string Apply()
    {
        if (Application.isPlaying) throw new InvalidOperationException("Stop Play Mode first.");
        var original = SceneManager.GetActiveScene();
        var fastFood = SceneManager.GetSceneByPath("Assets/Scenes/FastFood.unity");
        if (fastFood.isLoaded && fastFood.isDirty) throw new InvalidOperationException("Preserve unsaved FastFood edits before configuring audio.");
        Directory.CreateDirectory("Logs/DialogueTypingAudio");
        File.Copy(PrefabPath, "Logs/DialogueTypingAudio/CAMERA.before.prefab", true);
        File.Copy("Assets/Scenes/FastFood.unity", "Logs/DialogueTypingAudio/FastFood.before.unity", true);

        var importer = (AudioImporter)AssetImporter.GetAtPath(ClipPath);
        importer.forceToMono = true;
        var sample = importer.defaultSampleSettings;
        sample.loadType = AudioClipLoadType.CompressedInMemory;
        sample.compressionFormat = AudioCompressionFormat.Vorbis;
        sample.preloadAudioData = true;
        sample.quality = 0.85f;
        importer.defaultSampleSettings = sample;
        importer.SaveAndReimport();
        var clip = AssetDatabase.LoadAssetAtPath<AudioClip>(ClipPath);
        var prefab = PrefabUtility.LoadPrefabContents(PrefabPath);
        int count;
        try
        {
            count = ConfigureRoot(prefab, clip, false);
            PrefabUtility.SaveAsPrefabAsset(prefab, PrefabPath);
        }
        finally { PrefabUtility.UnloadPrefabContents(prefab); }

        bool opened = !fastFood.isLoaded;
        if (opened) fastFood = EditorSceneManager.OpenScene("Assets/Scenes/FastFood.unity", OpenSceneMode.Additive);
        try
        {
            foreach (var root in fastFood.GetRootGameObjects()) count += ConfigureRoot(root, clip, true);
            EditorSceneManager.MarkSceneDirty(fastFood);
            EditorSceneManager.SaveScene(fastFood);
        }
        finally
        {
            SceneManager.SetActiveScene(original);
            if (opened) EditorSceneManager.CloseScene(fastFood, true);
        }
        AssetDatabase.SaveAssets();
        return "Configured dedicated typing sources: " + count + " (shared dialogue prefab + FastFood local dialogues).";
    }

    private static int ConfigureRoot(GameObject root, AudioClip clip, bool skipInherited)
    {
        int count = 0;
        var seen = new HashSet<TypewriterText>();
        foreach (var dialog in root.GetComponentsInChildren<DialogCanvas>(true))
        {
            var writer = new SerializedObject(dialog).FindProperty("_typewriter").objectReferenceValue as TypewriterText;
            if (writer == null || !seen.Add(writer) || (skipInherited && PrefabUtility.GetCorrespondingObjectFromSource(writer) != null)) continue;
            var child = writer.transform.Find("TypingAudio");
            if (child == null)
            {
                child = new GameObject("TypingAudio").transform;
                child.SetParent(writer.transform, false);
            }
            var source = child.GetComponent<AudioSource>();
            if (source == null) source = child.gameObject.AddComponent<AudioSource>();
            source.clip = clip;
            source.playOnAwake = false;
            source.loop = true;
            source.volume = 0.03f;
            source.spatialBlend = 0;
            source.priority = 64;
            var fields = new SerializedObject(writer);
            fields.FindProperty("_typingAudio").objectReferenceValue = source;
            fields.ApplyModifiedPropertiesWithoutUndo();
            count++;
        }
        return count;
    }

    public static string Inspect()
    {
        var lines = new List<string>();
        var prefab = PrefabUtility.LoadPrefabContents(PrefabPath);
        try { InspectRoot(prefab, "CAMERA prefab", lines); }
        finally { PrefabUtility.UnloadPrefabContents(prefab); }
        var original = SceneManager.GetActiveScene();
        foreach (string path in new[] { "Assets/Scenes/NightField.unity", "Assets/Scenes/FastFood.unity" })
        {
            var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);
            try { foreach (var root in scene.GetRootGameObjects()) InspectRoot(root, scene.name, lines); }
            finally { SceneManager.SetActiveScene(original); EditorSceneManager.CloseScene(scene, true); }
        }
        return string.Join("\n", lines);
    }

    private static void InspectRoot(GameObject root, string scope, List<string> lines)
    {
        foreach (var dialog in root.GetComponentsInChildren<DialogCanvas>(true))
        {
            var fields = new SerializedObject(dialog);
            var writer = fields.FindProperty("_typewriter").objectReferenceValue;
            string hierarchy = dialog.name;
            for (var t = dialog.transform.parent; t != null; t = t.parent) hierarchy = t.name + "/" + hierarchy;
            lines.Add(scope + " " + hierarchy + " writer=" + (writer == null ? "none" : writer.name) + " prefab=" + PrefabUtility.IsPartOfPrefabInstance(dialog));
        }
    }
}
