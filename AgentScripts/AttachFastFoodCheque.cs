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

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        return "Receipt sprite assigned to the existing order panel; starts hidden and fades in through GuestOrderSequence after the guest's order.";
    }
}
