using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Cinemachine;

public static class ConfigureFastFoodAdminIntro
{
    public static string Apply()
    {
        var scene = SceneManager.GetActiveScene();
        if (Application.isPlaying || scene.path != "Assets/Scenes/FastFood.unity")
            throw new InvalidOperationException("Open FastFood in Edit Mode first.");
        var intro = GameObject.Find("SCENE/SceneIntro").GetComponent<SceneIntro>();
        var briefing = GameObject.Find("TILL/AdminBriefing").GetComponent<NpcVisit>();
        var briefingFields = new SerializedObject(briefing);
        var npc = (Npc)briefingFields.FindProperty("_npc").objectReferenceValue;
        var player = GameObject.Find("Player").transform;
        var originalStand = (Transform)briefingFields.FindProperty("_standPoint").objectReferenceValue;
        var spawn = Point("AdminWakeSpawn", new Vector3(player.position.x + 2.2f, originalStand.position.y, originalStand.position.z));
        var stand = Point("AdminWakeStand", new Vector3(player.position.x + 0.85f, originalStand.position.y, originalStand.position.z));

        var visitObject = GameObject.Find("SCENE").transform.Find("AdminWakeUpVisit");
        if (visitObject == null)
        {
            var go = new GameObject("AdminWakeUpVisit");
            Undo.RegisterCreatedObjectUndo(go, "Add administrator wake-up visit");
            go.transform.SetParent(GameObject.Find("SCENE").transform, false);
            visitObject = go.transform;
        }
        var visit = visitObject.GetComponent<NpcVisit>();
        if (visit == null) visit = Undo.AddComponent<NpcVisit>(visitObject.gameObject);
        var introFields = new SerializedObject(intro);
        var visitFields = new SerializedObject(visit);
        visitFields.FindProperty("_npc").objectReferenceValue = npc;
        visitFields.FindProperty("_inputService").objectReferenceValue = introFields.FindProperty("_inputService").objectReferenceValue;
        visitFields.FindProperty("_subtitles").objectReferenceValue = introFields.FindProperty("_subtitles").objectReferenceValue;
        visitFields.FindProperty("_spawnPoint").objectReferenceValue = spawn;
        visitFields.FindProperty("_standPoint").objectReferenceValue = stand;
        visitFields.FindProperty("_exitPoint").objectReferenceValue = null;
        var phrases = introFields.FindProperty("_phraseIds");
        var destinationPhrases = visitFields.FindProperty("_phraseIds");
        destinationPhrases.arraySize = phrases.arraySize;
        for (int i = 0; i < phrases.arraySize; i++)
            destinationPhrases.GetArrayElementAtIndex(i).stringValue = phrases.GetArrayElementAtIndex(i).stringValue;
        visitFields.ApplyModifiedProperties();
        introFields.FindProperty("_wakeUpVisit").objectReferenceValue = visit;
        introFields.ApplyModifiedProperties();
        briefingFields.FindProperty("_continueFromCurrentPosition").boolValue = false;
        briefingFields.ApplyModifiedProperties();
        var npcFields = new SerializedObject(npc);
        npcFields.FindProperty("_speed").floatValue = 0.6f;
        npcFields.ApplyModifiedProperties();
        Undo.RecordObject(npc.gameObject, "Start administrator offstage");
        npc.gameObject.SetActive(false);
        var camera = GameObject.Find("CAMERA/CinemachineCamera").GetComponent<CinemachineCamera>();
        var confiner = camera.GetComponent<CinemachineConfiner2D>();
        Undo.RecordObject(confiner, "Start FastFood in counter bounds");
        confiner.BoundingShape2D = GameObject.Find("CAMERA/Confiner").GetComponent<BoxCollider2D>();
        confiner.InvalidateBoundingShapeCache();
        PrefabUtility.RecordPrefabInstancePropertyModifications(confiner);
        camera.ForceCameraPosition(new Vector3(player.position.x, 0, Camera.main.transform.position.z), camera.transform.rotation);
        camera.PreviousStateIsValid = false;
        PrefabUtility.RecordPrefabInstancePropertyModifications(camera.transform);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        return "Administrator approaches Eva and waits at speed 0.6; cashier briefing starts from the original spawn point.";
    }

    private static Transform Point(string name, Vector3 position)
    {
        var parent = GameObject.Find("TILL").transform;
        var point = parent.Find(name);
        if (point == null)
        {
            var go = new GameObject(name);
            Undo.RegisterCreatedObjectUndo(go, "Add " + name);
            go.transform.SetParent(parent, false);
            point = go.transform;
        }
        Undo.RecordObject(point, "Set " + name);
        point.position = position;
        return point;
    }
}
