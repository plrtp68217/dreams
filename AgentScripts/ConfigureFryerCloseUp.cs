using System;
using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// One-time migration of the open FastFood scene, preserving its manual settings.
public static class ConfigureFryerCloseUp
{
    const string Folder = "Assets/Sprites/FastFoodService/FryerCloseUp/";
    static Sprite Sprite(string name) => AssetDatabase.LoadAssetAtPath<Sprite>(Folder + name + ".png");
    static UnityEngine.Object Read(UnityEngine.Object obj, string name) => new SerializedObject(obj).FindProperty(name).objectReferenceValue;
    static void Set(UnityEngine.Object obj, string name, UnityEngine.Object value)
    {
        var so = new SerializedObject(obj); so.FindProperty(name).objectReferenceValue = value; so.ApplyModifiedPropertiesWithoutUndo();
    }
    static RectTransform Rect(Transform parent, string name, Vector2 size, Vector2 position)
    {
        var go = new GameObject(name, typeof(RectTransform)); go.transform.SetParent(parent, false);
        var rect = (RectTransform)go.transform; rect.sizeDelta = size; rect.anchoredPosition = position;
        return rect;
    }
    static UnityEngine.UI.Image Image(Transform parent, string name, string sprite, Vector2 size, Vector2 position)
    {
        var image = Rect(parent, name, size, position).gameObject.AddComponent<UnityEngine.UI.Image>();
        image.sprite = Sprite(sprite); image.raycastTarget = false;
        return image;
    }
    public static string Apply()
    {
        var scene = SceneManager.GetActiveScene();
        if (Application.isPlaying || scene.path != "Assets/Scenes/FastFood.unity") throw new InvalidOperationException("Open FastFood in Edit Mode");
        var view = UnityEngine.Object.FindFirstObjectByType<FastFoodServiceView>();
        if (view.GetComponent<FastFoodFryerCloseUp>() != null) throw new InvalidOperationException("Already configured; edit existing settings");
        Directory.CreateDirectory("Logs/FryerUpdate");
        EditorSceneManager.SaveScene(scene, "Logs/FryerUpdate/FastFood.editor-before.unity", true);
        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
        foreach (string file in Directory.GetFiles(Folder, "*.png"))
        {
            var importer = (TextureImporter)AssetImporter.GetAtPath(file.Replace('\\', '/'));
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = 100;
            importer.spritePivot = new Vector2(.5f, .5f);
            importer.filterMode = FilterMode.Point; importer.mipmapEnabled = false;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.npotScale = TextureImporterNPOTScale.None;
            importer.isReadable = true; importer.alphaIsTransparency = true;
            var settings = new TextureImporterSettings(); importer.ReadTextureSettings(settings);
            settings.spriteMeshType = SpriteMeshType.FullRect; settings.spriteGenerateFallbackPhysicsShape = false;
            importer.SetTextureSettings(settings); importer.SaveAndReimport();
        }
        var modal = (GameObject)Read(view, "_modal");
        var root = Rect(modal.transform, "FryerCloseUp", new Vector2(1280, 720), Vector2.zero);
        root.anchorMin = Vector2.zero; root.anchorMax = Vector2.one; root.offsetMin = root.offsetMax = Vector2.zero;
        var backdrop = root.gameObject.AddComponent<UnityEngine.UI.Image>(); backdrop.color = Color.black; backdrop.raycastTarget = true;
        var canvas = Rect(root, "ArtCanvas", new Vector2(1280, 720), Vector2.zero);
        var fit = canvas.gameObject.AddComponent<UnityEngine.UI.AspectRatioFitter>();
        fit.aspectMode = UnityEngine.UI.AspectRatioFitter.AspectMode.FitInParent; fit.aspectRatio = 16f / 9;
        var content = Rect(canvas, "SourceCoordinates", new Vector2(1280,720), Vector2.zero);
        // Scale one fixed coordinate system uniformly as the fitted parent resizes.
        content.gameObject.AddComponent<FryerArtScale>();
        var size = new Vector2(1280, 720);
        Image(content, "Background", "Background", size, Vector2.zero);
        var burger = Image(content, "Burger", "Burger", size, Vector2.zero);
        var foreground = Image(content, "OilAndGauge", "Drain_00", size, Vector2.zero);
        var switchOff = Image(content, "SwitchOff", "SwitchOff", size, Vector2.zero);
        var switchOutline = Image(content, "SwitchOutline", "SwitchOutline", size, Vector2.zero);
        var burgerOutline = Image(content, "BurgerOutline", "BurgerOutline", size, Vector2.zero);
        var tumbler = Image(content, "Tumbler", "Tumbler", new Vector2(64, 144), new Vector2(500, -72));
        var tumblerOutline = Image(content, "TumblerOutline", "TumblerOutline", new Vector2(64,144), new Vector2(500,-72));
        var sourceLabel = (TextMeshProUGUI)Read(view, "_message");
        var hint = UnityEngine.Object.Instantiate(sourceLabel, root); hint.name = "Hint";
        hint.rectTransform.anchorMin = new Vector2(0,0); hint.rectTransform.anchorMax = new Vector2(1,0);
        hint.rectTransform.pivot = new Vector2(.5f,0); hint.rectTransform.offsetMin = new Vector2(24,15); hint.rectTransform.offsetMax = new Vector2(-24,85);
        hint.fontSize = 24; hint.enableAutoSizing = false; hint.alignment = TextAlignmentOptions.Center;
        hint.color = new Color(1,.94f,.8f); hint.raycastTarget = false;
        var hintBackground = Rect(root, "HintBackground", Vector2.zero, Vector2.zero);
        hintBackground.anchorMin = Vector2.zero; hintBackground.anchorMax = new Vector2(1,0);
        hintBackground.offsetMin = Vector2.zero; hintBackground.offsetMax = new Vector2(0,92);
        hintBackground.gameObject.AddComponent<UnityEngine.UI.Image>().color = new Color(0,0,0,.85f);
        hintBackground.SetSiblingIndex(hint.transform.GetSiblingIndex());
        var close = UnityEngine.Object.Instantiate((UnityEngine.UI.Button)Read(view, "_close"), root);
        close.name = "CloseFryer"; var cr = (RectTransform)close.transform;
        cr.anchorMin = cr.anchorMax = new Vector2(1,1); cr.pivot = new Vector2(1,1); cr.anchoredPosition = new Vector2(-16,-16);
        cr.sizeDelta = new Vector2(210,42); close.onClick = new UnityEngine.UI.Button.ButtonClickedEvent();
        UnityEditor.Events.UnityEventTools.AddPersistentListener(close.onClick, view.Close);
        close.gameObject.AddComponent<LocalizedPosterText>();
        var localized = close.GetComponent<LocalizedPosterText>();
        Set(localized,"_title",close.GetComponentInChildren<TextMeshProUGUI>());
        var fields = new SerializedObject(localized); fields.FindProperty("_titleId").stringValue="service_close"; fields.ApplyModifiedPropertiesWithoutUndo();
        var controller = view.gameObject.AddComponent<FastFoodFryerCloseUp>();
        Set(controller,"_service",Read(view,"_service")); Set(controller,"_player",Read(view,"_player"));
        Set(controller,"_root",root.gameObject); Set(controller,"_canvas",content);
        Set(controller,"_foreground",foreground); Set(controller,"_switchOff",switchOff);
        Set(controller,"_switchOutline",switchOutline); Set(controller,"_burger",burger); Set(controller,"_burgerOutline",burgerOutline);
        Set(controller,"_tumbler",tumbler); Set(controller,"_tumblerOutline",tumblerOutline); Set(controller,"_switchHit",Sprite("SwitchHit"));
        Set(controller,"_hint",hint); Set(controller,"_audio",Read(view,"_audio"));
        Set(controller,"_drainSound",Read(view,"_drainSound")); Set(controller,"_pickupSound",Read(view,"_pickupSound"));
        var so = new SerializedObject(controller); var frames = so.FindProperty("_drainFrames"); frames.arraySize = 21;
        for(int i=0;i<21;i++) frames.GetArrayElementAtIndex(i).objectReferenceValue=Sprite("Drain_"+i.ToString("00"));
        var switchFrames = so.FindProperty("_switchFrames"); switchFrames.arraySize = 3;
        for(int i=0;i<3;i++) switchFrames.GetArrayElementAtIndex(i).objectReferenceValue=Sprite("Switch_"+i.ToString("00"));
        so.FindProperty("_idleForeground").objectReferenceValue = Sprite("Idle");
        so.ApplyModifiedPropertiesWithoutUndo(); Set(view,"_fryerCloseUp",controller);
        var inventory = new SerializedObject(Read(view,"_inventory"));
        inventory.FindProperty("_orderIcons").GetArrayElementAtIndex(0).objectReferenceValue = Sprite("BurgerItem");
        inventory.ApplyModifiedPropertiesWithoutUndo();
        var fuse = scene.GetRootGameObjects().SelectMany(r=>r.GetComponentsInChildren<FastFoodServiceStation>(true)).Single(s=>s.Kind==FastFoodStationKind.FuseBox);
        fuse.gameObject.SetActive(false);
        root.gameObject.SetActive(false);
        EditorUtility.SetDirty(view); EditorSceneManager.MarkSceneDirty(scene);
        if (!EditorSceneManager.SaveScene(scene)) throw new IOException("Scene save failed");
        return "Fryer configured with original layered art, 21 drain frames, mouse extraction and unified kitchen station";
    }
}
