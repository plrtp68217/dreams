using System;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>Switches the existing Intro typography to the game's static bitmap font.</summary>
public static class ConfigureIntroPixelText
{
    public static string Apply()
    {
        var scene = SceneManager.GetActiveScene();
        if (Application.isPlaying || scene.path != "Assets/Scenes/Intro.unity")
            throw new InvalidOperationException("Open Intro in Edit Mode first.");
        var font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/UI/NytrockFont/TMP/GNF.asset");
        if (font == null || font.atlasTexture.filterMode != FilterMode.Point || font.material.shader.name != "TextMeshPro/Bitmap")
            throw new InvalidOperationException("The project's point-filtered bitmap font is missing.");
        var intro = UnityEngine.Object.FindFirstObjectByType<IntroSequence>();
        var texts = intro.GetComponentsInChildren<TMP_Text>(true);
        var report = new List<string>();
        Undo.IncrementCurrentGroup();
        int undoGroup = Undo.GetCurrentGroup();
        Undo.SetCurrentGroupName("Use pixel typography in Intro");
        foreach (var text in texts)
        {
            Undo.RecordObject(text, "Use GNF bitmap font");
            text.font = font;
            text.fontSharedMaterial = font.material;
            text.enableAutoSizing = false;
            text.characterSpacing = 0;
            text.lineSpacing = 0;
            switch (text.name)
            {
                case "StudioName": text.fontSize = 80; text.characterSpacing = 28; break;
                case "RecommendationText": text.fontSize = 60; text.characterSpacing = 10; break;
                case "WarningTitle": text.fontSize = 60; text.characterSpacing = 8; break;
                case "AtmosphereText":
                case "PhotosensitivityText": text.fontSize = 30; break;
                default: text.fontSize = 20; break;
            }
            text.ForceMeshUpdate(true);
            report.Add(text.name + ": " + text.GetPreferredValues() + " / " + text.rectTransform.sizeDelta);
        }
        Undo.CollapseUndoOperations(undoGroup);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        return string.Join("\n", report);
    }
}
