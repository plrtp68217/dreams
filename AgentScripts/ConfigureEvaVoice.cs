using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public static class ConfigureEvaVoice
{
    public static string PrepareProfile()
    {
        if (Application.isPlaying) throw new InvalidOperationException("Use Edit Mode.");
        const string profilePath = "Assets/Audio/Voices/EvaWarmLow.asset";
        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
        if (AssetDatabase.LoadAssetAtPath<DialogueVoiceProfile>(profilePath) != null)
            throw new InvalidOperationException("Eva profile already exists; adjust it in Inspector.");
        var paths = AssetDatabase.FindAssets("t:AudioClip", new[] { "Assets/Audio/Voices/EvaWarmLow" })
            .Select(AssetDatabase.GUIDToAssetPath).OrderBy(p => p).ToArray();
        if (paths.Length != 31) throw new InvalidOperationException("Expected 31 prepared Eva clips.");
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
        var profile = ScriptableObject.CreateInstance<DialogueVoiceProfile>();
        AssetDatabase.CreateAsset(profile, profilePath);
        var fields = new SerializedObject(profile);
        var clips = fields.FindProperty("_clips");
        clips.arraySize = paths.Length;
        for (int i = 0; i < paths.Length; i++)
            clips.GetArrayElementAtIndex(i).objectReferenceValue = AssetDatabase.LoadAssetAtPath<AudioClip>(paths[i]);
        fields.FindProperty("_volume").floatValue = 0.18f;
        fields.FindProperty("_fragmentsPerSecond").floatValue = 3f;
        fields.FindProperty("_pauseRange").vector2Value = new Vector2(0.08f, 0.18f);
        fields.FindProperty("_pitchRange").vector2Value = new Vector2(0.97f, 1.03f);
        fields.ApplyModifiedPropertiesWithoutUndo();
        AssetDatabase.SaveAssetIfDirty(profile);
        return "Created EvaWarmLow: 31 mono PCM fragments; volume 0.18, rate 3, pause 0.08-0.18, pitch 0.97-1.03. Scenes untouched.";
    }
}
