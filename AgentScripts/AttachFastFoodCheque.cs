using System;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class AttachFastFoodCheque
{
    public static string Apply()
    {
        var scene = SceneManager.GetActiveScene();
        if (Application.isPlaying || scene.path != "Assets/Scenes/FastFood.unity")
            throw new InvalidOperationException("Open FastFood in Edit Mode first.");

        const string path = "Assets/Sprites/Dialog/FastFoodCheque.png";
        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
        var importer = (TextureImporter)AssetImporter.GetAtPath(path);
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.spritePixelsPerUnit = 100;
        importer.filterMode = FilterMode.Point;
        importer.mipmapEnabled = false;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.wrapMode = TextureWrapMode.Clamp;
        importer.npotScale = TextureImporterNPOTScale.None;
        importer.SaveAndReimport();

        var image = GameObject.Find("CAMERA/Main Camera/CanvasDialog/Dialog_Order/Image_Order")
            .GetComponent<UnityEngine.UI.Image>();
        var text = image.GetComponentInChildren<TextMeshProUGUI>();
        Undo.RecordObjects(new UnityEngine.Object[] { image, image.rectTransform, text, text.rectTransform }, "Use FastFood receipt art");

        image.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
        image.type = UnityEngine.UI.Image.Type.Simple;
        image.preserveAspect = true;
        image.raycastTarget = false;
        image.color = new Color(1, 1, 1, 0);
        // Keep the existing HUD width and use the receipt's natural proportions.
        image.rectTransform.pivot = new Vector2(0.5f, 0.5f);
        image.rectTransform.sizeDelta = new Vector2(179, 179f * 190f / 112f);
        image.rectTransform.anchoredPosition = new Vector2(126, 35);

        // The art's middle is writable; keep the burger header and barcode clear.
        var rect = text.rectTransform;
        rect.anchorMin = new Vector2(0.1f, 0.23f);
        rect.anchorMax = new Vector2(0.9f, 0.67f);
        rect.pivot = new Vector2(0.5f, 1);
        rect.offsetMin = rect.offsetMax = Vector2.zero;
        text.margin = Vector4.zero;
        text.alignment = TextAlignmentOptions.TopLeft;
        text.enableAutoSizing = true;
        text.fontSize = 12;
        text.fontSizeMax = 12;
        text.fontSizeMin = 10;
        text.raycastTarget = false;
        Color color = text.color;
        color.a = 0;
        text.color = color;

        // Hold the top-right paper corner in place as it folds down and opens on hover.
        var receiptRect = image.rectTransform;
        receiptRect.anchoredPosition += Vector2.Scale(new Vector2(0.5f, 0.5f), Vector2.Scale(receiptRect.rect.size, receiptRect.localScale));
        receiptRect.pivot = Vector2.one;
        var hover = image.GetComponent<ReceiptHover>();
        if (hover == null) hover = Undo.AddComponent<ReceiptHover>(image.gameObject);
        var shadow = image.GetComponent<UnityEngine.UI.Shadow>();
        if (shadow == null) shadow = Undo.AddComponent<UnityEngine.UI.Shadow>(image.gameObject);
        shadow.effectColor = new Color(0.04f, 0.015f, 0.025f, 0.38f);
        shadow.effectDistance = new Vector2(4, -6);
        var hoverSettings = new SerializedObject(hover);
        hoverSettings.FindProperty("_shadow").objectReferenceValue = shadow;
        hoverSettings.ApplyModifiedProperties();
        var orderSettings = new SerializedObject(GameObject.Find("TILL/TillTrigger").GetComponent<GuestOrderSequence>());
        orderSettings.FindProperty("_receipt").objectReferenceValue = hover;
        orderSettings.ApplyModifiedProperties();

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        return "Receipt starts hidden, opens for five seconds after the order, then compacts and expands on hover.";
    }
}
