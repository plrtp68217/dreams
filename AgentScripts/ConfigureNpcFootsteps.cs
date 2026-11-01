using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class ConfigureNpcFootsteps
{
    public static string Apply()
    {
        var scene = SceneManager.GetActiveScene();
        if (Application.isPlaying || scene.path != "Assets/Scenes/FastFood.unity")
            throw new InvalidOperationException("Open FastFood in Edit Mode first.");
        Directory.CreateDirectory("Logs/NpcFootsteps");
        EditorSceneManager.SaveScene(scene, "Logs/NpcFootsteps/FastFood.before-configure.unity", true);
        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
        Configure(scene, "Admin_FastFood", "Admin",
            "Assets/Animations/Units/Admin/AdminFastFoodWalking.anim", new[] { .2f, .6f });
        Configure(scene, "Guest_FacelessMan", "FacelessMan",
            "Assets/Animations/Units/Guests/FacelessMan/FacelessManFastFoodWalking.anim", new[] { 3f/12f, 7f/12f });
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        return "Configured two dedicated NPC footstep sources and two foot contacts per walk animation; original player audio retained.";
    }

    static void Configure(Scene scene, string objectName, string character, string walkPath, float[] times)
    {
        var npc = scene.GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<Npc>(true))
            .Single(n => n.name == objectName);
        var source = npc.GetComponent<AudioSource>();
        if (source == null) source = Undo.AddComponent<AudioSource>(npc.gameObject);
        source.playOnAwake = false;
        source.loop = false;
        source.clip = null;
        source.spatialBlend = 0;
        source.volume = 1f;
        source.priority = 128;
        source.ignoreListenerPause = false;
        var audio = npc.GetComponent<NpcFootstepAudio>();
        if (audio == null) audio = Undo.AddComponent<NpcFootstepAudio>(npc.gameObject);
        var fields = new SerializedObject(audio);
        fields.FindProperty("_audioSource").objectReferenceValue = source;
        fields.FindProperty("_animator").objectReferenceValue = npc.GetComponent<Animator>();
        fields.FindProperty("_volume").floatValue = .08f;
        fields.FindProperty("_pitchRange").vector2Value = new Vector2(.98f, 1.02f);
        var clips = fields.FindProperty("_clips");
        clips.arraySize = 3;
        for (int i = 0; i < 3; i++)
        {
            string path = $"Assets/Audio/FastFood/NpcFootsteps/{character}/fastfood_sfx_{character.ToLowerInvariant()}_walk_{i+1:00}.wav";
            var importer = AssetImporter.GetAtPath(path) as AudioImporter;
            if (importer == null) throw new InvalidOperationException("Missing prepared footstep: " + path);
            importer.forceToMono = true;
            importer.loadInBackground = false;
            var settings = importer.defaultSampleSettings;
            settings.compressionFormat = AudioCompressionFormat.PCM;
            settings.loadType = AudioClipLoadType.DecompressOnLoad;
            settings.sampleRateSetting = AudioSampleRateSetting.PreserveSampleRate;
            settings.preloadAudioData = true;
            importer.defaultSampleSettings = settings;
            importer.SaveAndReimport();
            clips.GetArrayElementAtIndex(i).objectReferenceValue = AssetDatabase.LoadAssetAtPath<AudioClip>(path);
        }
        fields.ApplyModifiedProperties();
        var walk = AssetDatabase.LoadAssetAtPath<AnimationClip>(walkPath);
        string backup = "Logs/NpcFootsteps/" + Path.GetFileName(walkPath);
        if (!File.Exists(backup)) File.Copy(walkPath, backup);
        var events = AnimationUtility.GetAnimationEvents(walk)
            .Where(e => e.functionName != nameof(NpcFootstepAudio.PlayFootStepSound)).ToList();
        events.AddRange(times.Select(t => new AnimationEvent { time = t,
            functionName = nameof(NpcFootstepAudio.PlayFootStepSound) }));
        AnimationUtility.SetAnimationEvents(walk, events.OrderBy(e => e.time).ToArray());
        EditorUtility.SetDirty(walk);
    }
}
