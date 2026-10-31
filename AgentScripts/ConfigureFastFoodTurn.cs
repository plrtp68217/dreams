using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEditor.U2D.Sprites;
using UnityEngine;

public static class ConfigureFastFoodTurn
{
    private const string TexturePath = "Assets/Sprites/Player/FastFood/EvaFastFoodTurn.png";
    private const string ClipPath = "Assets/Animations/Player/FastFood/EvaFastFoodTurn.anim";
    private const string ControllerPath = "Assets/Animations/Player/FastFood/EvaFastFoodAnimatorController.controller";

    public static string Apply()
    {
        var scene = EditorSceneManager.GetActiveScene();
        if (Application.isPlaying || scene.path != "Assets/Scenes/FastFood.unity")
            throw new InvalidOperationException("Open FastFood in Edit Mode first.");
        // Preserve the full Editor state, including manual edits, before adding the component.
        EditorSceneManager.SaveScene(scene, "Logs/FastFoodTurnSession/FastFood.pre-configuration.unity", true);
        var player = UnityEngine.Object.FindFirstObjectByType<Player>();
        var controller = UnityEngine.Object.FindFirstObjectByType<PlayerController>();
        var input = UnityEngine.Object.FindFirstObjectByType<InputService>();
        var animator = player.GetComponent<Animator>();
        var overrides = animator.runtimeAnimatorController as AnimatorOverrideController;
        if (overrides == null || overrides.name != "EvaFastFoodAnimatorOverride")
            throw new InvalidOperationException("Unexpected player Animator; no changes applied.");

        var importer = (TextureImporter)AssetImporter.GetAtPath(TexturePath);
        importer.GetSourceTextureWidthAndHeight(out int width, out int height);
        if (width != 240 || height != 80) throw new InvalidOperationException("Expected three 80x80 frames.");
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Multiple;
        importer.spritePixelsPerUnit = 100f;
        importer.filterMode = FilterMode.Point;
        importer.mipmapEnabled = false;
        importer.alphaIsTransparency = true;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.npotScale = TextureImporterNPOTScale.None;
        importer.SaveAndReimport();

        var factories = new SpriteDataProviderFactories();
        factories.Init();
        var provider = factories.GetSpriteEditorDataProviderFromObject(importer);
        if (provider == null) throw new InvalidOperationException("Sprite data provider is unavailable.");
        provider.InitSpriteEditorDataProvider();
        var capabilityProvider = provider.GetDataProvider<ISpriteFrameEditCapability>();
        if (capabilityProvider == null) throw new InvalidOperationException("Sprite edit capability unavailable; slicing aborted.");
        var capabilities = capabilityProvider.GetEditCapability();
        foreach (var required in new[] { EEditCapability.CreateAndDeleteSprite, EEditCapability.EditSpriteName,
                                         EEditCapability.EditSpriteRect, EEditCapability.EditPivot })
            if (!capabilities.HasCapability(required)) throw new InvalidOperationException("Missing sprite capability: " + required);
        var ids = provider.GetDataProvider<ISpriteNameFileIdDataProvider>();
        if (ids == null) throw new InvalidOperationException("Sprite name/ID provider unavailable; slicing aborted.");
        var existing = provider.GetSpriteRects().ToDictionary(r => r.name);
        var rects = Enumerable.Range(0, 3).Select(i =>
        {
            string name = "EvaFastFoodTurn_" + i;
            return new SpriteRect
            {
                name = name, rect = new Rect(i * 80, 0, 80, 80),
                alignment = SpriteAlignment.Center, pivot = new Vector2(0.5f, 0.5f),
                spriteID = existing.TryGetValue(name, out var old) ? old.spriteID : GUID.Generate()
            };
        }).ToArray();
        provider.SetSpriteRects(rects);
        ids.SetNameFileIdPairs(rects.Select(r => new SpriteNameFileIdPair(r.name, r.spriteID)));
        provider.Apply();
        importer.SaveAndReimport();
        var sprites = AssetDatabase.LoadAllAssetsAtPath(TexturePath).OfType<Sprite>().OrderBy(s => s.name).ToArray();
        if (sprites.Length != 3) throw new InvalidOperationException("Sprite slicing failed.");

        var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(ClipPath);
        if (clip == null)
        {
            clip = new AnimationClip { name = "EvaFastFoodTurn" };
            AssetDatabase.CreateAsset(clip, ClipPath);
        }
        clip.frameRate = 10;
        var keys = Enumerable.Range(0, 3).Select(i => new ObjectReferenceKeyframe
        {
            time = i * 0.1f, value = sprites[i]
        }).ToArray();
        AnimationUtility.SetObjectReferenceCurve(clip, EditorCurveBinding.PPtrCurve("", typeof(SpriteRenderer), "m_Sprite"), keys);
        var clipSettings = AnimationUtility.GetAnimationClipSettings(clip);
        clipSettings.loopTime = false;
        AnimationUtility.SetAnimationClipSettings(clip, clipSettings);
        EditorUtility.SetDirty(clip);

        var fastFoodController = AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath);
        if (fastFoodController == null)
        {
            string originalPath = AssetDatabase.GetAssetPath(overrides.runtimeAnimatorController);
            if (!AssetDatabase.CopyAsset(originalPath, ControllerPath)) throw new InvalidOperationException("Controller copy failed.");
            fastFoodController = AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath);
        }
        var machine = fastFoodController.layers[0].stateMachine;
        var turn = machine.states.FirstOrDefault(s => s.state.name == "Turn").state;
        if (turn == null) turn = machine.AddState("Turn", new Vector3(600, 350));
        turn.motion = clip;
        turn.writeDefaultValues = true;
        string speedParameter = AnimatorFloat.TurnSpeed.ToString();
        if (!fastFoodController.parameters.Any(p => p.name == speedParameter))
            fastFoodController.AddParameter(new AnimatorControllerParameter
            {
                name = speedParameter, type = AnimatorControllerParameterType.Float, defaultFloat = 1f
            });
        turn.speedParameter = speedParameter;
        turn.speedParameterActive = true;
        foreach (var transition in turn.transitions) turn.RemoveTransition(transition);
        AddExit(turn, machine, "Crouching", AnimatorBool.IsCrouching);
        AddExit(turn, machine, "Sitting", AnimatorBool.IsSitting);
        AddExit(turn, machine, "Running", AnimatorBool.IsRunning);
        var walk = AddExit(turn, machine, "Walk", AnimatorBool.IsWalking);
        walk.AddCondition(AnimatorConditionMode.IfNot, 0, AnimatorBool.IsRunning.ToString());
        AddExit(turn, machine, "Idle", null);
        EditorUtility.SetDirty(fastFoodController);

        var mappings = new List<KeyValuePair<AnimationClip, AnimationClip>>();
        overrides.GetOverrides(mappings);
        overrides.runtimeAnimatorController = fastFoodController;
        overrides.ApplyOverrides(mappings);
        EditorUtility.SetDirty(overrides);

        var turnComponent = player.GetComponent<PlayerTurnAnimation>();
        if (turnComponent == null) turnComponent = Undo.AddComponent<PlayerTurnAnimation>(player.gameObject);
        var fields = new SerializedObject(turnComponent);
        fields.FindProperty("_inputService").objectReferenceValue = input;
        fields.FindProperty("_controller").objectReferenceValue = controller;
        fields.ApplyModifiedProperties();
        PrefabUtility.RecordPrefabInstancePropertyModifications(turnComponent);
        AssetDatabase.SaveAssetIfDirty(clip);
        AssetDatabase.SaveAssetIfDirty(fastFoodController);
        AssetDatabase.SaveAssetIfDirty(overrides);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        return "FastFood turn configured: three 80x80 frames, 0.3s, mirrored for left-to-right; shared player controller asset unchanged.";
    }

    private static AnimatorStateTransition AddExit(AnimatorState turn, AnimatorStateMachine machine, string name, AnimatorBool? condition)
    {
        var destination = machine.states.Single(s => s.state.name == name).state;
        var transition = turn.AddTransition(destination);
        transition.hasExitTime = true;
        transition.exitTime = 1f;
        transition.duration = 0f;
        transition.hasFixedDuration = true;
        transition.canTransitionToSelf = false;
        if (condition.HasValue) transition.AddCondition(AnimatorConditionMode.If, 0, condition.Value.ToString());
        return transition;
    }
}
