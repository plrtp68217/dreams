using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class ConfigureCounterServing
{
    static T Read<T>(UnityEngine.Object o,string field) where T:UnityEngine.Object => (T)new SerializedObject(o).FindProperty(field).objectReferenceValue;
    static void Set(UnityEngine.Object o,string field,UnityEngine.Object value){var s=new SerializedObject(o);s.FindProperty(field).objectReferenceValue=value;s.ApplyModifiedPropertiesWithoutUndo();}
    public static string RefineDeparture()
    {
        var scene=SceneManager.GetActiveScene();if(Application.isPlaying||scene.path!="Assets/Scenes/FastFood.unity")throw new InvalidOperationException("FastFood Edit Mode required.");
        Directory.CreateDirectory("Logs/ServingRefinement");File.Copy(scene.path,"Logs/ServingRefinement/FastFood.disk-before.unity",true);EditorSceneManager.SaveScene(scene,"Logs/ServingRefinement/FastFood.editor-before.unity",true);
        var clown=UnityEngine.Object.FindFirstObjectByType<FastFoodTension>();var fields=new SerializedObject(clown);
        fields.FindProperty("_lingerDuration").floatValue=.25f;fields.FindProperty("_exitSpeedMultiplier").floatValue=2.5f;fields.ApplyModifiedPropertiesWithoutUndo();
        EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);return "Departure delay 0.25s; exit movement and walk animation x2.5. Other scene settings preserved.";
    }
    public static string Apply()
    {
        var scene=SceneManager.GetActiveScene();if(Application.isPlaying||scene.path!="Assets/Scenes/FastFood.unity")throw new InvalidOperationException("Open FastFood in Edit Mode.");
        Directory.CreateDirectory("Logs/ServingFlow");EditorSceneManager.SaveScene(scene,"Logs/ServingFlow/FastFood.before-configure.unity",true);
        var service=UnityEngine.Object.FindFirstObjectByType<FacelessOrderService>();var till=UnityEngine.Object.FindFirstObjectByType<TillTrigger>();var view=UnityEngine.Object.FindFirstObjectByType<FastFoodServiceView>();
        var fryer=view.GetComponent<FastFoodFryerCloseUp>();var original=Read<UnityEngine.UI.Image>(fryer,"_burger");
        if(Read<UnityEngine.UI.Image>(fryer,"_pickup")!=null)throw new Exception("Already configured; edit existing settings.");
        var pickupObject=new GameObject("BurgerPickup",typeof(RectTransform),typeof(UnityEngine.UI.Image));pickupObject.transform.SetParent(original.transform.parent,false);
        var pickup=pickupObject.GetComponent<UnityEngine.UI.Image>();pickup.sprite=AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/FastFoodService/FryerCloseUp/BurgerItem.png");pickup.raycastTarget=false;pickup.enabled=false;
        // Original alpha bounds in the 320x180 source: [130,122,167,148].
        pickup.rectTransform.sizeDelta=new Vector2(148,104);pickup.rectTransform.anchoredPosition=new Vector2(-46,-180);
        Set(fryer,"_pickup",pickup);var fields=new SerializedObject(fryer);fields.FindProperty("_pickupDuration").floatValue=.7f;fields.ApplyModifiedPropertiesWithoutUndo();
        Set(till,"_service",service);Set(till,"_serviceView",view);Set(service,"_counter",till);
        var key=GameObject.Find("FacelessService/ServiceCanvas/PlayerInteractionPrompt").GetComponent<FastFoodKeyPrompt>();Set(till,"_keyPrompt",key);
        var sequence=Read<GuestOrderSequence>(till,"_orderSequence");var completion=Read<NpcVisit>(sequence,"_completionVisit");
        if(completion==null||Read<Transform>(completion,"_exitPoint")==null)throw new Exception("Guest completion must have an exit point.");
        var receipt=Read<ReceiptHover>(sequence,"_receipt");if(receipt.GetComponent<CanvasGroup>()==null)receipt.gameObject.AddComponent<CanvasGroup>();
        EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.ImportAsset("Assets/Resources/dialogues.json",ImportAssetOptions.ForceUpdate);
        return "Configured cropped burger pickup, counter-owned delivery, receipt dismissal and guest-exit prerequisite. Saved current manual scene edits.";
    }
}
