using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class ConfigureDialogueVoices
{
    const string ProfilePath = "Assets/Audio/Voices/FacelessDarkRadio.asset";
    const string ClipsPath = "Assets/Audio/Voices/FacelessDarkRadio";

    public static string Apply()
    {
        var scene = SceneManager.GetActiveScene();
        if (Application.isPlaying || scene.path != "Assets/Scenes/FastFood.unity")
            throw new InvalidOperationException("Open FastFood in Edit Mode first.");
        // Save a copy, including any unsaved user edits, before touching the open scene.
        Directory.CreateDirectory("Logs/DialogueVoice");
        EditorSceneManager.SaveScene(scene, "Logs/DialogueVoice/FastFood.before-configure.unity", true);
        var profile = PrepareProfile(ProfilePath, ClipsPath, 13, 0.18f,
            new Vector2(0.08f, 0.18f), new Vector2(0.94f, 1.04f));

        const string prefabPath = "Assets/Prefabs/CAMERA.prefab";
        var prefab = PrefabUtility.LoadPrefabContents(prefabPath);
        int sources = 0;
        try
        {
            sources += ConfigureSources(prefab, false);
            PrefabUtility.SaveAsPrefabAsset(prefab, prefabPath);
        }
        finally { PrefabUtility.UnloadPrefabContents(prefab); }
        foreach (var root in scene.GetRootGameObjects()) sources += ConfigureSources(root, true);
        int speakers = 0;
        foreach (var root in scene.GetRootGameObjects())
        foreach (var visit in root.GetComponentsInChildren<NpcVisit>(true))
        {
            var npc = new SerializedObject(visit);
            var ids = npc.FindProperty("_phraseIds");
            bool faceless = Enumerable.Range(0, ids.arraySize).Any(i =>
                ids.GetArrayElementAtIndex(i).stringValue.StartsWith("fastfood_faceless_man_", StringComparison.Ordinal));
            if (!faceless) continue;
            AssignProfile(visit, profile);
            speakers++;
        }
        if (speakers != 1) throw new InvalidOperationException("Expected one faceless NPC visit.");
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        return $"Configured {speakers} faceless speaker, 13 clips, {sources} voice sources; typing loop disabled everywhere.";
    }

    static DialogueVoiceProfile PrepareProfile(string profilePath, string clipsPath, int count,
        float volume, Vector2 pause, Vector2 pitch)
    {
        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
        var paths = AssetDatabase.FindAssets("t:AudioClip", new[] { clipsPath })
            .Select(AssetDatabase.GUIDToAssetPath).OrderBy(p => p).ToArray();
        if (paths.Length != count) throw new InvalidOperationException($"Expected {count} prepared clips in {clipsPath}.");
        foreach (var path in paths)
        {
            var importer = (AudioImporter)AssetImporter.GetAtPath(path);
            importer.forceToMono = true;
            importer.loadInBackground = false;
            var settings = importer.defaultSampleSettings;
            settings.loadType = AudioClipLoadType.DecompressOnLoad;
            settings.compressionFormat = AudioCompressionFormat.PCM;
            settings.sampleRateSetting = AudioSampleRateSetting.PreserveSampleRate;
            settings.preloadAudioData = true;
            importer.defaultSampleSettings = settings;
            importer.SaveAndReimport();
        }
        var profile = AssetDatabase.LoadAssetAtPath<DialogueVoiceProfile>(profilePath);
        if (profile == null)
        {
            profile = ScriptableObject.CreateInstance<DialogueVoiceProfile>();
            AssetDatabase.CreateAsset(profile, profilePath);
        }
        var fields = new SerializedObject(profile);
        var clips = fields.FindProperty("_clips");
        clips.arraySize = paths.Length;
        for (int i = 0; i < paths.Length; i++)
            clips.GetArrayElementAtIndex(i).objectReferenceValue = AssetDatabase.LoadAssetAtPath<AudioClip>(paths[i]);
        fields.FindProperty("_volume").floatValue = volume;
        fields.FindProperty("_fragmentsPerSecond").floatValue = 3f;
        fields.FindProperty("_pauseRange").vector2Value = pause;
        fields.FindProperty("_pitchRange").vector2Value = pitch;
        fields.ApplyModifiedPropertiesWithoutUndo();

        return profile;
    }

    static void AssignProfile(NpcVisit visit, DialogueVoiceProfile profile)
    {
        var npc = new SerializedObject(visit);
        npc.FindProperty("_voiceProfile").objectReferenceValue = profile;
        npc.ApplyModifiedProperties();
        var canvas = npc.FindProperty("_subtitles").objectReferenceValue as DialogCanvas;
        var canvasFields = new SerializedObject(canvas);
        canvasFields.FindProperty("_voiceProfile").objectReferenceValue = profile;
        canvasFields.ApplyModifiedProperties();
    }

    static int ConfigureSources(GameObject root, bool skipInherited)
    {
        int count = 0;
        foreach (var writer in root.GetComponentsInChildren<TypewriterText>(true))
        {
            if (skipInherited && PrefabUtility.GetCorrespondingObjectFromSource(writer) != null) continue;
            var fields = new SerializedObject(writer);
            var source = fields.FindProperty("_voiceAudio").objectReferenceValue as AudioSource;
            if (source == null)
            {
                var child = new GameObject("VoiceAudio");
                child.transform.SetParent(writer.transform, false);
                source = child.AddComponent<AudioSource>();
            }
            source.gameObject.name = "VoiceAudio";
            source.Stop();
            source.clip = null;
            source.loop = false;
            source.playOnAwake = false;
            source.spatialBlend = 0f;
            source.volume = 1f;
            source.pitch = 1f;
            source.ignoreListenerPause = false;
            source.priority = 64;
            fields.FindProperty("_voiceAudio").objectReferenceValue = source;
            fields.ApplyModifiedPropertiesWithoutUndo();
            count++;
        }
        return count;
    }
}
