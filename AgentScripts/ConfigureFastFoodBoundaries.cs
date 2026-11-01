using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class ConfigureFastFoodBoundaries
{
    public const string MaterialPath = "Assets/Materials/LevelBoundary.physicsMaterial2D";

    public static string Apply()
    {
        var scene = SceneManager.GetActiveScene();
        if (Application.isPlaying || scene.path != "Assets/Scenes/FastFood.unity")
            throw new InvalidOperationException("Open FastFood in Edit Mode first.");

        var boundaries = scene.GetRootGameObjects()
            .SelectMany(root => root.GetComponentsInChildren<Collider2D>(true))
            .Where(collider => !collider.isTrigger &&
                (string.Equals(collider.name, "LeftBoundary", StringComparison.OrdinalIgnoreCase) ||
                 string.Equals(collider.name, "RightBoundary", StringComparison.OrdinalIgnoreCase)))
            .ToArray();
        if (boundaries.Length == 0) throw new InvalidOperationException("No level boundaries found.");

        var material = AssetDatabase.LoadAssetAtPath<PhysicsMaterial2D>(MaterialPath);
        if (material == null)
        {
            material = new PhysicsMaterial2D("LevelBoundary");
            AssetDatabase.CreateAsset(material, MaterialPath);
        }
        Undo.RecordObject(material, "Remove boundary friction");
        material.friction = 0f;
        material.bounciness = 0f;
        material.frictionCombine = PhysicsMaterialCombine2D.Mean;
        EditorUtility.SetDirty(material);
        foreach (var boundary in boundaries)
        {
            Undo.RecordObject(boundary, "Remove boundary friction");
            boundary.sharedMaterial = material;
            EditorUtility.SetDirty(boundary);
        }
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        return "Zero-friction material assigned to " + boundaries.Length + " boundaries.";
    }
}
