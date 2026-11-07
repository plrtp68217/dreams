using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// Narrow update of the existing FastFood UI. Does not rebuild authored layouts.
public static class ConfigureCloseupCompletion
{
    public static string Apply()
    {
        var scene = SceneManager.GetActiveScene();
        if (Application.isPlaying || scene.path != "Assets/Scenes/FastFood.unity")
            throw new InvalidOperationException("Open FastFood in Edit Mode.");
        var view = UnityEngine.Object.FindFirstObjectByType<FastFoodServiceView>();
        var hub = view.GetComponent<FastFoodNotificationHub>();
        var card = view.transform.Find("ServiceCanvas/NotificationCard");
        if (hub == null || card == null) throw new InvalidOperationException("Existing notification UI required.");
        hub.enabled = false;
        card.gameObject.SetActive(false);
        EditorUtility.SetDirty(hub);
        int restored = 0;
        foreach (var notice in view.GetComponentsInChildren<SmoothServiceNotice>(true))
        {
            var fields = new SerializedObject(notice);
            var target = fields.FindProperty("_notification");
            if (target.objectReferenceValue == null) continue;
            target.objectReferenceValue = null;
            fields.ApplyModifiedPropertiesWithoutUndo();
            restored++;
        }
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        return $"Top notification hub/card disabled; {restored} inline notices restored. Scene saved with existing manual edits.";
    }
}
