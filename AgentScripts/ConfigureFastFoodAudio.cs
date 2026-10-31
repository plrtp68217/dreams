using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class ConfigureFastFoodAudio
{
    private const string Root = "Assets/Audio/FastFood/";

    public static string Apply()
    {
        var scene = EditorSceneManager.GetActiveScene();
        if (Application.isPlaying || scene.path != "Assets/Scenes/FastFood.unity")
            throw new InvalidOperationException("Open FastFood in Edit Mode first.");
        if (scene.isDirty)
            throw new InvalidOperationException("Scene contains unsaved edits; preserve them before configuring audio.");

        var movement = UnityEngine.Object.FindFirstObjectByType<AudioMovement>();
        var controller = UnityEngine.Object.FindFirstObjectByType<AudioController>();
        var starter = UnityEngine.Object.FindFirstObjectByType<SceneStarter>();
        var input = UnityEngine.Object.FindFirstObjectByType<InputService>();
        if (movement == null || controller == null || starter == null || input == null)
            throw new InvalidOperationException("Required scene audio components were not found.");

        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
        var clips = AssetDatabase.FindAssets("t:AudioClip", new[] { Root.TrimEnd('/') })
            .Select(AssetDatabase.GUIDToAssetPath).ToArray();
        if (clips.Length != 21) throw new InvalidOperationException("Expected 21 prepared clips, found " + clips.Length);
        foreach (string path in clips)
        {
            var importer = (AudioImporter)AssetImporter.GetAtPath(path);
            var settings = importer.defaultSampleSettings;
            bool ambience = path.Contains("/Ambience/");
            bool shortEffect = path.Contains("/Movement/") || path.Contains("/Interactions/");
            importer.forceToMono = path.Contains("/Movement/");
            importer.loadInBackground = false;
            settings.preloadAudioData = true;
            settings.loadType = ambience ? AudioClipLoadType.Streaming :
                shortEffect ? AudioClipLoadType.DecompressOnLoad : AudioClipLoadType.CompressedInMemory;
            settings.compressionFormat = shortEffect ? AudioCompressionFormat.ADPCM : AudioCompressionFormat.Vorbis;
            settings.quality = 0.8f;
            settings.sampleRateSetting = AudioSampleRateSetting.PreserveSampleRate;
            importer.defaultSampleSettings = settings;
            importer.SaveAndReimport();
        }

        var starterFields = new SerializedObject(starter);
        var ambienceSource = starterFields.FindProperty("_audioSource").objectReferenceValue as AudioSource;
        if (ambienceSource == null) throw new InvalidOperationException("SceneStarter ambience source is missing.");
        Undo.RecordObject(ambienceSource, "Configure FastFood ambience");
        SetSource(ambienceSource, 192);
        ambienceSource.loop = true;
        ambienceSource.clip = Clip("Ambience/fastfood_amb_room_tone_01.ogg");
        ambienceSource.volume = 0.22f;
        PrefabUtility.RecordPrefabInstancePropertyModifications(ambienceSource);

        var playerSource = movement.GetComponent<AudioSource>();
        if (playerSource == null) playerSource = Undo.AddComponent<AudioSource>(movement.gameObject);
        Undo.RecordObject(playerSource, "Configure FastFood player audio");
        SetSource(playerSource, 96);
        playerSource.volume = 1f;
        PrefabUtility.RecordPrefabInstancePropertyModifications(playerSource);

        Undo.RecordObject(controller, "Assign FastFood atmosphere");
        var controllerFields = new SerializedObject(controller);
        controllerFields.FindProperty("_audioSourceMain").objectReferenceValue = ambienceSource;
        controllerFields.FindProperty("_ambientClip").objectReferenceValue = ambienceSource.clip;
        controllerFields.FindProperty("_volume").floatValue = 0.22f;
        controllerFields.ApplyModifiedProperties();

        Undo.RecordObject(movement, "Assign FastFood player sounds");
        var movementFields = new SerializedObject(movement);
        movementFields.FindProperty("_audioSource").objectReferenceValue = playerSource;
        movementFields.FindProperty("_inputService").objectReferenceValue = input;
        movementFields.FindProperty("_volume").floatValue = 0.5f;
        SetClips(movementFields, "_audioFootstepClips", "walk");
        SetClips(movementFields, "_audioRunningClips", "run");
        SetClips(movementFields, "_audioJumpClips", "jump");
        movementFields.FindProperty("_audioInteractionClip").objectReferenceValue = Clip("Interactions/fastfood_sfx_item_pickup_01.mp3");
        movementFields.ApplyModifiedProperties();
        PrefabUtility.RecordPrefabInstancePropertyModifications(movement);

        var pause = controller.GetComponent<FastFoodAudioPause>();
        if (pause == null) pause = Undo.AddComponent<FastFoodAudioPause>(controller.gameObject);
        var pauseFields = new SerializedObject(pause);
        var sources = pauseFields.FindProperty("_sources");
        sources.arraySize = 2;
        sources.GetArrayElementAtIndex(0).objectReferenceValue = ambienceSource;
        sources.GetArrayElementAtIndex(1).objectReferenceValue = playerSource;
        pauseFields.ApplyModifiedProperties();
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        return "Configured FastFood: streaming ambience, 3 walk/run/jump variations, interaction clip, pause/resume; 21 clips imported.";
    }

    private static void SetSource(AudioSource source, int priority)
    {
        source.playOnAwake = false;
        source.loop = false;
        source.spatialBlend = 0f;
        source.dopplerLevel = 0f;
        source.priority = priority;
        source.ignoreListenerPause = false;
    }

    private static AudioClip Clip(string path)
    {
        var clip = AssetDatabase.LoadAssetAtPath<AudioClip>(Root + path);
        if (clip == null) throw new InvalidOperationException("Missing clip: " + path);
        return clip;
    }

    private static void SetClips(SerializedObject target, string field, string action)
    {
        var array = target.FindProperty(field);
        array.arraySize = 3;
        for (int i = 0; i < 3; i++)
            array.GetArrayElementAtIndex(i).objectReferenceValue = Clip($"Movement/fastfood_sfx_player_{action}_{i + 1:00}.wav");
    }
}
