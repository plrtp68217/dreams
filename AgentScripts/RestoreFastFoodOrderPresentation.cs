using System;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// Original saved receipt values from FastFood before the PixelLab experiment.
// This one-off restoration does not rebuild the service or change its puzzles.
public static class RestoreFastFoodOrderPresentation
{
    public static string Apply()
    {
        var scene = SceneManager.GetActiveScene();
        if (Application.isPlaying || scene.path != "Assets/Scenes/FastFood.unity") throw new Exception("Open FastFood in Edit Mode.");
        var sequence = UnityEngine.Object.FindFirstObjectByType<GuestOrderSequence>();
        var fields = new SerializedObject(sequence);
        var panel = (DialogCanvas)fields.FindProperty("_orderPanel").objectReferenceValue;
        var receipt = (ReceiptHover)fields.FindProperty("_receipt").objectReferenceValue;
        var canvas = UnityEngine.Object.FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None).Single(c => c.name == "CanvasDialog");
        var panelRect = (RectTransform)panel.transform;
        Undo.SetTransformParent(panelRect, canvas.transform, "Restore original order receipt");
        Rect(panelRect, new Vector2(.5f, .5f), new Vector2(.5f, .5f), new Vector2(.5f, .5f), new Vector2(100, 100), Vector2.zero, Vector3.one);
        var paper = (RectTransform)receipt.transform;
        Rect(paper, new Vector2(.5f, .5f), new Vector2(.5f, .5f), Vector2.one, new Vector2(179, 303.6607f), new Vector2(154.64f, 83.58571f), Vector3.one * .32f);
        var paperFields = new SerializedObject(paper); paperFields.FindProperty("m_ConstrainProportionsScale").boolValue = true; paperFields.ApplyModifiedPropertiesWithoutUndo();
        var image = receipt.GetComponent<UnityEngine.UI.Image>();
        image.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(AssetDatabase.GUIDToAssetPath("556ec62d74081c44383ca28a6a53a221"));
        if (image.sprite == null) throw new Exception("Original cheque sprite is missing.");
        image.type = UnityEngine.UI.Image.Type.Simple; image.preserveAspect = true; image.useSpriteMesh = false;
        image.color = new Color(1, 1, 1, 0); image.raycastTarget = false;
        var text = receipt.GetComponentInChildren<TextMeshProUGUI>(true);
        Rect(text.rectTransform, new Vector2(.1f, .23f), new Vector2(.9f, .67f), new Vector2(.5f, 1), Vector2.zero, Vector2.zero, Vector3.one);
        text.font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(AssetDatabase.GUIDToAssetPath("241c6acceca8b844baad6cbcf60cefc7"));
        text.fontSharedMaterial = text.font.material; text.fontSize = 12; text.fontSizeMin = 10; text.fontSizeMax = 12; text.enableAutoSizing = true;
        text.alignment = TextAlignmentOptions.TopLeft; text.textWrappingMode = TextWrappingModes.Normal; text.overflowMode = TextOverflowModes.Overflow;
        text.margin = Vector4.zero; text.color = new Color(.4056604f, .18543704f, .04018334f, 0);
        var intro = UnityEngine.Object.FindFirstObjectByType<SceneIntro>();
        var wake = (NpcVisit)new SerializedObject(intro).FindProperty("_wakeUpVisit").objectReferenceValue;
        var wakeFields = new SerializedObject(wake); var phrases = wakeFields.FindProperty("_phraseIds"); phrases.arraySize = 2;
        phrases.GetArrayElementAtIndex(0).stringValue = "fastfood_admin_wake_1";
        phrases.GetArrayElementAtIndex(1).stringValue = "fastfood_admin_wake_2";
        wakeFields.ApplyModifiedPropertiesWithoutUndo();
        EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene);
        return "Restored original receipt asset, dimensions, font and parent; wake-up dialogue restored.";
    }

    static void Rect(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 size, Vector2 position, Vector3 scale)
    {
        Undo.RecordObject(rect, "Restore original receipt geometry");
        rect.anchorMin = anchorMin; rect.anchorMax = anchorMax; rect.pivot = pivot; rect.sizeDelta = size;
        rect.anchoredPosition3D = new Vector3(position.x, position.y, 0); rect.localScale = scale; rect.localRotation = Quaternion.identity;
    }
}
