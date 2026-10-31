using System;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEditor.U2D.Sprites;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class ConfigureNightFieldTurns
{
    public static string Apply()
    {
        if (Application.isPlaying) throw new InvalidOperationException("Edit Mode required.");
        var night = SceneManager.GetSceneByPath("Assets/Scenes/NightField.unity");
        bool opened = !night.isLoaded;
        if (opened) night = EditorSceneManager.OpenScene("Assets/Scenes/NightField.unity", OpenSceneMode.Additive);
        if (night.isDirty) throw new InvalidOperationException("NightField contains unsaved edits; preserve them before applying.");
        try
        {
            var hospital = BuildClip("Hospital", "EvaHospitalTurn");
            var dress = BuildClip("Dress", "EvaDressTurn");
            var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>("Assets/Animations/Player/PlayerAnimatorController.controller");
            var machine = controller.layers[0].stateMachine;
            string speed = AnimatorFloat.TurnSpeed.ToString();
            if (!controller.parameters.Any(p => p.name == speed))
                controller.AddParameter(new AnimatorControllerParameter
                {
                    name = speed, type = AnimatorControllerParameterType.Float, defaultFloat = 1f
                });
            var state = machine.states.FirstOrDefault(s => s.state.name == "Turn").state;
            if (state == null) state = machine.AddState("Turn", new Vector3(600, 350));
            state.motion = hospital;
            state.writeDefaultValues = true;
            state.speedParameter = speed;
            state.speedParameterActive = true;
            foreach (var transition in state.transitions) state.RemoveTransition(transition);
            Exit(state, machine, "Crouching", AnimatorBool.IsCrouching);
            Exit(state, machine, "Sitting", AnimatorBool.IsSitting);
            Exit(state, machine, "Running", AnimatorBool.IsRunning);
            var walk = Exit(state, machine, "Walk", AnimatorBool.IsWalking);
            walk.AddCondition(AnimatorConditionMode.IfNot, 0, AnimatorBool.IsRunning.ToString());
            Exit(state, machine, "Idle", null);
            EditorUtility.SetDirty(controller);
            AssetDatabase.SaveAssetIfDirty(controller);

            var dressController = AssetDatabase.LoadAssetAtPath<AnimatorOverrideController>(
                "Assets/Animations/Player/Dress/EvaDressAnimatorOverride.overrideController");
            dressController[hospital.name] = dress;
            EditorUtility.SetDirty(dressController);
            AssetDatabase.SaveAssetIfDirty(dressController);

            var roots = night.GetRootGameObjects();
            var player = roots.SelectMany(g => g.GetComponentsInChildren<Player>(true)).Single();
            var input = roots.SelectMany(g => g.GetComponentsInChildren<InputService>(true)).Single();
            var movement = roots.SelectMany(g => g.GetComponentsInChildren<PlayerController>(true)).Single();
            var turn = player.GetComponent<PlayerTurnAnimation>();
            if (turn == null) turn = Undo.AddComponent<PlayerTurnAnimation>(player.gameObject);
            var fields = new SerializedObject(turn);
            fields.FindProperty("_inputService").objectReferenceValue = input;
            fields.FindProperty("_controller").objectReferenceValue = movement;
            fields.FindProperty("_turnSpeed").floatValue = 2f;
            fields.FindProperty("_rapidTurnSpeed").floatValue = 3f;
            fields.FindProperty("_rapidTurnWindow").floatValue = 0.35f;
            fields.ApplyModifiedProperties();
            PrefabUtility.RecordPrefabInstancePropertyModifications(turn);
            EditorSceneManager.MarkSceneDirty(night);
            EditorSceneManager.SaveScene(night);
            return "Hospital and Dress turns imported; NightField player configured at normal speed 2 / rapid multiplier 3.";
        }
        finally
        {
            if (opened && night.isLoaded && !night.isDirty) EditorSceneManager.CloseScene(night, true);
        }
    }

    private static AnimationClip BuildClip(string skin, string name)
    {
        string path = "Assets/Sprites/Player/" + skin + "/" + name + ".png";
        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
        var importer = (TextureImporter)AssetImporter.GetAtPath(path);
        importer.GetSourceTextureWidthAndHeight(out int width, out int height);
        if (width != 240 || height != 80) throw new InvalidOperationException("Expected three 80x80 frames: " + path);
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
        if (provider == null) throw new InvalidOperationException("Sprite provider unavailable.");
        provider.InitSpriteEditorDataProvider();
        var capability = provider.GetDataProvider<ISpriteFrameEditCapability>();
        if (capability == null) throw new InvalidOperationException("Sprite edit capabilities unavailable.");
        foreach (var required in new[] { EEditCapability.CreateAndDeleteSprite, EEditCapability.EditSpriteName,
                                         EEditCapability.EditSpriteRect, EEditCapability.EditPivot })
            if (!capability.GetEditCapability().HasCapability(required)) throw new InvalidOperationException("Missing capability: " + required);
        var ids = provider.GetDataProvider<ISpriteNameFileIdDataProvider>();
        if (ids == null) throw new InvalidOperationException("Sprite name/ID provider unavailable.");
        var old = provider.GetSpriteRects().ToDictionary(r => r.name);
        var rects = Enumerable.Range(0, 3).Select(i => new SpriteRect
        {
            name = name + "_" + i, rect = new Rect(i * 80, 0, 80, 80),
            alignment = SpriteAlignment.Center, pivot = new Vector2(0.5f, 0.5f),
            spriteID = old.TryGetValue(name + "_" + i, out var previous) ? previous.spriteID : GUID.Generate()
        }).ToArray();
        provider.SetSpriteRects(rects);
        ids.SetNameFileIdPairs(rects.Select(r => new SpriteNameFileIdPair(r.name, r.spriteID)));
        provider.Apply();
        importer.SaveAndReimport();
        var sprites = AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().OrderBy(s => s.name).ToArray();
        if (sprites.Length != 3) throw new InvalidOperationException("Slicing failed: " + path);
        string clipPath = "Assets/Animations/Player/" + skin + "/" + name + ".anim";
        var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(clipPath);
        if (clip == null)
        {
            clip = new AnimationClip { name = name };
            AssetDatabase.CreateAsset(clip, clipPath);
        }
        clip.frameRate = 10f;
        AnimationUtility.SetObjectReferenceCurve(clip, EditorCurveBinding.PPtrCurve("", typeof(SpriteRenderer), "m_Sprite"),
            sprites.Select((sprite, i) => new ObjectReferenceKeyframe { time = i * 0.1f, value = sprite }).ToArray());
        var settings = AnimationUtility.GetAnimationClipSettings(clip);
        settings.loopTime = false;
        AnimationUtility.SetAnimationClipSettings(clip, settings);
        EditorUtility.SetDirty(clip);
        AssetDatabase.SaveAssetIfDirty(clip);
        return clip;
    }

    private static AnimatorStateTransition Exit(AnimatorState from, AnimatorStateMachine machine, string name, AnimatorBool? condition)
    {
        var transition = from.AddTransition(machine.states.Single(s => s.state.name == name).state);
        transition.hasExitTime = true;
        transition.exitTime = 1f;
        transition.duration = 0f;
        transition.hasFixedDuration = true;
        transition.canTransitionToSelf = false;
        if (condition.HasValue) transition.AddCondition(AnimatorConditionMode.If, 0f, condition.Value.ToString());
        return transition;
    }
}
