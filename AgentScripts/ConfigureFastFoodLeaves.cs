using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class ConfigureFastFoodLeaves
{
    const string Folder = "Assets/VFX/FallingLeaves/";

    public static string Apply()
    {
        var scene = SceneManager.GetActiveScene();
        if (Application.isPlaying || scene.path != "Assets/Scenes/FastFood.unity")
            throw new InvalidOperationException("Open FastFood in Edit Mode first.");
        var background = scene.GetRootGameObjects().Single(g => g.name == "Background");
        if (background.transform.Find("OutsideWindowLeaves") != null)
            throw new InvalidOperationException("Leaves already exist; tune the existing Particle System in Inspector.");

        Directory.CreateDirectory("Logs/FallingLeaves");
        EditorSceneManager.SaveScene(scene, "Logs/FallingLeaves/FastFood.before-apply.unity", true);
        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
        var importer = (TextureImporter)AssetImporter.GetAtPath(Folder + "LeafPoses.png");
        importer.textureType = TextureImporterType.Default;
        importer.filterMode = FilterMode.Point;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.mipmapEnabled = false;
        importer.alphaIsTransparency = true;
        importer.wrapMode = TextureWrapMode.Clamp;
        importer.npotScale = TextureImporterNPOTScale.None;
        importer.SaveAndReimport();

        string materialPath = Folder + "WindowLeaves.mat";
        if (AssetDatabase.LoadAssetAtPath<Material>(materialPath) != null)
            throw new InvalidOperationException("Leaf material already exists; avoid overwriting manual settings.");
        var shader = Shader.Find("DREAMS/VFX/Window Leaves");
        if (shader == null) throw new InvalidOperationException("Leaf shader did not import.");
        var material = new Material(shader);
        material.mainTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(Folder + "LeafPoses.png");
        AssetDatabase.CreateAsset(material, materialPath);

        var go = new GameObject("OutsideWindowLeaves");
        Undo.RegisterCreatedObjectUndo(go, "Add leaves outside FastFood windows");
        go.transform.SetParent(background.transform, false);
        // At the facade's depth, in front of the outdoor scenery, behind the window/floor sprite.
        go.transform.position = new Vector3(0, 1.12f, 1);
        var particles = go.AddComponent<ParticleSystem>();
        particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        var main = particles.main;
        main.duration = 20;
        main.loop = true;
        main.prewarm = true;
        main.startLifetime = new ParticleSystem.MinMaxCurve(16, 19);
        main.startSpeed = 0;
        main.startRotation = new ParticleSystem.MinMaxCurve(-.35f, .35f);
        main.gravityModifier = 0;
        main.simulationSpace = ParticleSystemSimulationSpace.Local;
        main.simulationSpeed = 1;
        main.useUnscaledTime = false;
        main.cullingMode = ParticleSystemCullingMode.AlwaysSimulate;
        main.playOnAwake = true;

        var emission = particles.emission;
        emission.enabled = true;
        var shape = particles.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.scale = new Vector3(11.2f, .12f, 0);

        var velocity = particles.velocityOverLifetime;
        velocity.enabled = true;
        velocity.space = ParticleSystemSimulationSpace.Local;
        // Smooth opposing curves interpolate independently per particle; no shared field motion.
        velocity.x = new ParticleSystem.MinMaxCurve(1, Sway(-1), Sway(1));
        velocity.y = new ParticleSystem.MinMaxCurve(1, AnimationCurve.Constant(0,1,-.23f), AnimationCurve.Constant(0,1,-.15f));
        velocity.z = new ParticleSystem.MinMaxCurve(1, AnimationCurve.Constant(0,1,0), AnimationCurve.Constant(0,1,0));

        var noise = particles.noise;
        noise.enabled = true;
        noise.separateAxes = true;
        noise.strengthX = .055f;
        noise.strengthY = 0;
        noise.strengthZ = 0;
        noise.frequency = .85f;
        noise.scrollSpeed = .18f;
        noise.damping = true;
        noise.octaveCount = 2;
        noise.quality = ParticleSystemNoiseQuality.Medium;
        noise.rotationAmount = 12;

        var rotation = particles.rotationOverLifetime;
        rotation.enabled = true;
        rotation.z = new ParticleSystem.MinMaxCurve(-.22f, .22f);

        var sheet = particles.textureSheetAnimation;
        sheet.enabled = true;
        sheet.mode = ParticleSystemAnimationMode.Grid;
        sheet.numTilesX = 10;
        sheet.numTilesY = 3;
        sheet.animation = ParticleSystemAnimationType.SingleRow;
        sheet.rowMode = ParticleSystemAnimationRowMode.Random;
        sheet.timeMode = ParticleSystemAnimationTimeMode.FPS;
        sheet.fps = 8;
        sheet.startFrame = new ParticleSystem.MinMaxCurve(0, 1);
        sheet.frameOverTime = new ParticleSystem.MinMaxCurve(1, AnimationCurve.Linear(0, 0, 1, 1));
        sheet.cycleCount = 1;

        var color = particles.colorOverLifetime;
        color.enabled = true;
        var gradient = new Gradient();
        gradient.SetKeys(new[] {new GradientColorKey(Color.white, 0), new GradientColorKey(Color.white, 1)},
            new[] {new GradientAlphaKey(0,0), new GradientAlphaKey(1,.035f), new GradientAlphaKey(1,.85f), new GradientAlphaKey(0,1)});
        color.color = gradient;

        var renderer = particles.GetComponent<ParticleSystemRenderer>();
        renderer.sharedMaterial = material;
        renderer.renderMode = ParticleSystemRenderMode.Billboard;
        renderer.alignment = ParticleSystemRenderSpace.View;
        renderer.sortMode = ParticleSystemSortMode.None;
        renderer.sortingLayerName = "Background";
        renderer.sortingOrder = 5;
        renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        renderer.receiveShadows = false;
        renderer.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;
        renderer.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;
        renderer.maxParticleSize = .25f;
        renderer.minParticleSize = 0;
        ApplyAppearance(particles, material);

        AssetDatabase.SaveAssets();
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        return "Added Background/OutsideWindowLeaves: native Particle System, green leaf poses at half original size, 14/s, downward velocity 0.15–0.23, smooth sideways sway/noise, behind facade; saved current FastFood including existing manual edits.";
    }

    public static string RefineAppearance()
    {
        var scene = SceneManager.GetActiveScene();
        if (Application.isPlaying || scene.path != "Assets/Scenes/FastFood.unity")
            throw new InvalidOperationException("Open FastFood in Edit Mode first.");
        AssetDatabase.ImportAsset(Folder + "WindowLeaves.shader", ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate);
        var particles = GameObject.Find("Background/OutsideWindowLeaves").GetComponent<ParticleSystem>();
        var material = particles.GetComponent<ParticleSystemRenderer>().sharedMaterial;
        Directory.CreateDirectory("Logs/FallingLeaves/Refinement");
        EditorSceneManager.SaveScene(scene, "Logs/FallingLeaves/Refinement/FastFood.before-refine.unity", true);
        Undo.RecordObject(particles, "Resize and increase window leaves");
        Undo.RecordObject(material, "Match leaves to hall foliage palette");
        ApplyAppearance(particles, material);
        EditorUtility.SetDirty(material);
        AssetDatabase.SaveAssets();
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        return "Leaf size halved to 0.064–0.088; emission 4.2 -> 14/s; max 384; exact hall foliage colors #245942 / #327457. Motion preserved.";
    }

    static void ApplyAppearance(ParticleSystem particles, Material material)
    {
        var main = particles.main;
        main.startSize = new ParticleSystem.MinMaxCurve(.064f, .088f);
        main.startColor = new ParticleSystem.MinMaxGradient(new Color(1,1,1,.90f), Color.white);
        main.maxParticles = 384;
        var emission = particles.emission;
        emission.rateOverTime = 14;
        // These two exact shades come from FastFoodHall - 3.png.
        material.SetColor("_LeafShadow", new Color(36/255f,89/255f,66/255f,1));
        material.SetColor("_LeafLight", new Color(50/255f,116/255f,87/255f,1));
    }

    static AnimationCurve Sway(float sign)
    {
        var keys = new Keyframe[49];
        for (int i = 0; i < keys.Length; ++i)
        {
            float t = i / 48f;
            float angle = t * Mathf.PI * 12;
            float windAngle = t * Mathf.PI * 4 + .7f;
            float value = sign * .11f * Mathf.Sin(angle) + .035f * Mathf.Sin(windAngle);
            float tangent = sign * .11f * Mathf.PI * 12 * Mathf.Cos(angle) + .035f * Mathf.PI * 4 * Mathf.Cos(windAngle);
            keys[i] = new Keyframe(t, value, tangent, tangent);
        }
        return new AnimationCurve(keys);
    }

}
