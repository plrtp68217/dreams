using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

public static class ConfigureCharacterShadows
{
    public static string Apply()
    {
        var scene = SceneManager.GetActiveScene();
        if (Application.isPlaying || scene.path != "Assets/Scenes/FastFood.unity")
            throw new InvalidOperationException("Open FastFood in Edit Mode first.");
        if (scene.GetRootGameObjects().Any(g => g.name == "CharacterShadowFloors"))
            throw new InvalidOperationException("Shadow floors already exist; adjust their settings directly.");
        Directory.CreateDirectory("Logs/CharacterShadows");
        EditorSceneManager.SaveScene(scene, "Logs/CharacterShadows/FastFood.before-configure.unity", true);
        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
        var shader = Shader.Find("DREAMS/Character Floor Shadow");
        if (shader == null || ShaderUtil.ShaderHasError(shader)) throw new InvalidOperationException("Shadow shader is not ready.");
        const string materialPath = "Assets/Shaders/Shadows/CharacterFloorShadow.mat";
        var material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
        if (material == null)
        {
            material = new Material(shader);
            AssetDatabase.CreateAsset(material, materialPath);
        }
        var lights = scene.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<Light2D>(true))
            .Where(l => l.lightType != Light2D.LightType.Global && l.transform.position.y > .5f).ToArray();
        var root = new GameObject("CharacterShadowFloors");
        Undo.RegisterCreatedObjectUndo(root, "Configure character floor shadows");
        var hall = Surface(root.transform, "HallShadowFloor", new Rect(-8.2f, -.9f, 13.2f, .41f),
            lights.Where(l => l.transform.position.x > -10).ToArray(), 3.57f);
        var kitchen = Surface(root.transform, "KitchenShadowFloor", new Rect(-22.5f, -.9f, 5, .41f),
            lights.Where(l => l.transform.position.x < -10).ToArray(), .65f);
        var sources = scene.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<SpriteRenderer>(true))
            .Where(r => r.name == "Player" || r.name == "Admin_FastFood" || r.name == "Guest_FacelessMan").ToArray();
        if (sources.Length != 3) throw new InvalidOperationException("Expected player and two NPC sprites.");
        foreach (var source in sources)
        {
            var shadow = source.GetComponent<CharacterGroundShadow>();
            if (shadow == null) shadow = Undo.AddComponent<CharacterGroundShadow>(source.gameObject);
            var fields = new SerializedObject(shadow);
            fields.FindProperty("_source").objectReferenceValue = source;
            fields.FindProperty("_material").objectReferenceValue = material;
            var surfaces = fields.FindProperty("_surfaces");
            surfaces.arraySize = 2;
            surfaces.GetArrayElementAtIndex(0).objectReferenceValue = hall;
            surfaces.GetArrayElementAtIndex(1).objectReferenceValue = kitchen;
            fields.FindProperty("_contactSize").vector2Value = new Vector2(source.name == "Player" ? .25f : .29f, .06f);
            fields.ApplyModifiedPropertiesWithoutUndo();
            shadow.Refresh();
        }
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        return "Configured contact and animated silhouette shadows for three FastFood characters; hall and kitchen floor clipping with their ceiling lights.";
    }

    private static CharacterShadowSurface Surface(Transform parent, string name, Rect bounds, Light2D[] lights, float intensity)
    {
        var obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        var surface = obj.AddComponent<CharacterShadowSurface>();
        var fields = new SerializedObject(surface);
        fields.FindProperty("_floorBounds").rectValue = bounds;
        fields.FindProperty("_groundY").floatValue = -.73f;
        fields.FindProperty("_referenceIntensity").floatValue = intensity;
        var array = fields.FindProperty("_lights");
        array.arraySize = lights.Length;
        for (int i = 0; i < lights.Length; i++) array.GetArrayElementAtIndex(i).objectReferenceValue = lights[i];
        fields.ApplyModifiedPropertiesWithoutUndo();
        return surface;
    }

    public static string Preview(string name, float x)
    {
        var original = Camera.main;
        var obj = new GameObject("Temporary character shadow preview camera");
        var target = new RenderTexture(1600, 900, 24);
        var image = new Texture2D(1600, 900, TextureFormat.RGB24, false);
        var previous = RenderTexture.active;
        try
        {
            var camera = obj.AddComponent<Camera>();
            camera.CopyFrom(original);
            camera.enabled = false;
            camera.transform.SetPositionAndRotation(new Vector3(x, original.transform.position.y,
                original.transform.position.z), original.transform.rotation);
            camera.ResetWorldToCameraMatrix();
            camera.ResetCullingMatrix();
            camera.ResetProjectionMatrix();
            EditorUtility.CopySerialized(original.GetComponent<UniversalAdditionalCameraData>(),
                obj.AddComponent<UniversalAdditionalCameraData>());
            camera.targetTexture = target;
            camera.aspect = 1600f/900f;
            camera.Render();
            RenderTexture.active = target;
            image.ReadPixels(new Rect(0,0,1600,900),0,0);
            image.Apply();
            Directory.CreateDirectory("Logs/CharacterShadows");
            string path = "Logs/CharacterShadows/" + name + ".png";
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
