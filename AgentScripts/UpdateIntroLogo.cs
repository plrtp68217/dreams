using System;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>Updates only the studio card, preserving the scene's current in-memory settings.</summary>
public static class UpdateIntroLogo
{
    public static string Apply()
    {
        var scene = SceneManager.GetActiveScene();
        if (Application.isPlaying || scene.path != "Assets/Scenes/Intro.unity")
            throw new InvalidOperationException("Open Intro in Edit Mode first.");

        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
        foreach (var path in new[] { "Assets/Sprites/Intro/Studio.png", "Assets/Sprites/Intro/StudioShadow.png" })
        {
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Default;
            importer.filterMode = FilterMode.Point;
            importer.mipmapEnabled = false;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.npotScale = TextureImporterNPOTScale.None;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.SaveAndReimport();
        }

        var sequence = UnityEngine.Object.FindFirstObjectByType<IntroSequence>();
        var studio = sequence.transform.Find("Presentation/Composition/Studio");
        if (studio == null) throw new InvalidOperationException("Studio card is missing.");
        Undo.IncrementCurrentGroup();
        int undoGroup = Undo.GetCurrentGroup();
        Undo.SetCurrentGroupName("Update SCL GAMES studio logo");
        var emblem = studio.Find("Emblem").GetComponent<CanvasGroup>();
        var artwork = emblem.transform.Find("StudioArtwork").GetComponent<UnityEngine.UI.RawImage>();
        Undo.RecordObjects(new UnityEngine.Object[] { artwork, artwork.rectTransform }, "Update window artwork");
        artwork.texture = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Sprites/Intro/Studio.png");
        artwork.rectTransform.anchoredPosition = new Vector2(0, 154.5f);
        artwork.rectTransform.sizeDelta = new Vector2(522, 390);

        var brand = studio.Find("Brand").GetComponent<CanvasGroup>();
        var title = brand.transform.Find("StudioName").GetComponent<TMP_Text>();
        Undo.RecordObjects(new UnityEngine.Object[] { title, title.rectTransform }, "Update studio name");
        title.text = "SCL GAMES";
        title.fontSize = 80;
        title.characterSpacing = 28;
        title.rectTransform.anchoredPosition = new Vector2(0, -91.5f);
        title.rectTransform.sizeDelta = new Vector2(800, 100);
        foreach (string name in new[] { "Company", "LeftRule", "RightRule" })
        {
            var obsolete = brand.transform.Find(name);
            if (obsolete == null) continue;
            Undo.RecordObject(obsolete.gameObject, "Hide previous studio caption");
            obsolete.gameObject.SetActive(false);
        }

        var shadowTransform = studio.Find("Shadow");
        if (shadowTransform == null)
        {
            var obj = new GameObject("Shadow", typeof(RectTransform), typeof(CanvasGroup));
            Undo.RegisterCreatedObjectUndo(obj, "Add studio shadow reveal");
            obj.transform.SetParent(studio, false);
            shadowTransform = obj.transform;
            var rect = (RectTransform)shadowTransform;
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(1280, 720);
        }
        var shadow = shadowTransform.GetComponent<CanvasGroup>();
        shadow.blocksRaycasts = false;
        var shadowArtwork = shadowTransform.Find("ShadowArtwork");
        if (shadowArtwork == null)
        {
            var obj = new GameObject("ShadowArtwork", typeof(RectTransform), typeof(UnityEngine.UI.RawImage));
            Undo.RegisterCreatedObjectUndo(obj, "Add shadow artwork");
            obj.transform.SetParent(shadowTransform, false);
            shadowArtwork = obj.transform;
        }
        var shadowImage = shadowArtwork.GetComponent<UnityEngine.UI.RawImage>();
        Undo.RecordObjects(new UnityEngine.Object[] { shadowImage, shadowImage.rectTransform }, "Configure shadow artwork");
        shadowImage.texture = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Sprites/Intro/StudioShadow.png");
        shadowImage.material = artwork.material;
        shadowImage.raycastTarget = false;
        shadowImage.rectTransform.anchorMin = shadowImage.rectTransform.anchorMax = shadowImage.rectTransform.pivot = new Vector2(0.5f, 0.5f);
        shadowImage.rectTransform.anchoredPosition = new Vector2(-6, -187.5f);
        shadowImage.rectTransform.sizeDelta = new Vector2(480, 102);

        var fields = new SerializedObject(sequence);
        var elements = fields.FindProperty("_cards").GetArrayElementAtIndex(0).FindPropertyRelative("Elements");
        elements.arraySize = 3;
        Reveal(elements.GetArrayElementAtIndex(0), emblem, 0, 1.3f, new Vector2(0, -6), 0.985f);
        Reveal(elements.GetArrayElementAtIndex(1), shadow, 0.35f, 1.4f, new Vector2(0, -3), 0.97f);
        Reveal(elements.GetArrayElementAtIndex(2), brand, 0.8f, 1f, new Vector2(0, -5), 1f);
        fields.ApplyModifiedProperties();
        Undo.CollapseUndoOperations(undoGroup);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        return "Updated SCL GAMES window, title and shadow. Music loop preserved: " + sequence.GetComponent<AudioSource>().loop + ". Title width: " + title.GetPreferredValues().x;
    }

    private static void Reveal(SerializedProperty element, CanvasGroup group, float delay, float duration, Vector2 offset, float scale)
    {
        element.FindPropertyRelative("Group").objectReferenceValue = group;
        element.FindPropertyRelative("Delay").floatValue = delay;
        element.FindPropertyRelative("RevealDuration").floatValue = duration;
        element.FindPropertyRelative("EnterOffset").vector2Value = offset;
        element.FindPropertyRelative("EnterScale").floatValue = scale;
        element.FindPropertyRelative("FloatAmplitude").floatValue = 0;
    }
}
