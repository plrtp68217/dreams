using System;
using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class ConfigureMirrorToy
{
    const string Folder = "Assets/Sprites/FastFoodService/MirrorToy/";
    static TMP_FontAsset font;
#pragma warning disable CS0649
    [Serializable] class Manifest { public int[] standingToyBounds; public int[] durationsMs; }
#pragma warning restore CS0649
    static UnityEngine.Object Read(UnityEngine.Object o, string name) => new SerializedObject(o).FindProperty(name).objectReferenceValue;
    static void Set(UnityEngine.Object o, string name, UnityEngine.Object value)
    {
        var so = new SerializedObject(o); so.FindProperty(name).objectReferenceValue = value; so.ApplyModifiedPropertiesWithoutUndo();
    }
    static void Sprites(UnityEngine.Object o, string name, Sprite[] values)
    {
        var so = new SerializedObject(o); var p = so.FindProperty(name); p.arraySize = values.Length;
        for (int i = 0; i < values.Length; ++i) p.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
        so.ApplyModifiedPropertiesWithoutUndo();
    }
    static Sprite Sprite(string name) => AssetDatabase.LoadAssetAtPath<Sprite>(Folder + name + ".png");
    static RectTransform Rect(Transform parent, string name, Vector2 size, Vector2 pos)
    {
        var obj = new GameObject(name, typeof(RectTransform)); obj.transform.SetParent(parent, false);
        var r = (RectTransform)obj.transform; r.sizeDelta = size; r.anchoredPosition = pos; return r;
    }
    static UnityEngine.UI.Image Image(Transform parent, string name, Sprite sprite, int[] bounds)
    {
        var size = new Vector2(bounds[2] - bounds[0], bounds[3] - bounds[1]) * 4;
        var position = new Vector2((bounds[0] + bounds[2]) * 2 - 256, 256 - (bounds[1] + bounds[3]) * 2);
        var image = Rect(parent, name, size, position).gameObject.AddComponent<UnityEngine.UI.Image>();
        image.sprite = sprite; image.raycastTarget = false; return image;
    }
    static TextMeshProUGUI Text(Transform parent, string name, Vector2 size, float fontSize)
    {
        var label = Rect(parent, name, size, Vector2.zero).gameObject.AddComponent<TextMeshProUGUI>();
        label.font = font; label.fontSharedMaterial = font.material;
        label.fontSize = fontSize; label.enableAutoSizing = false; label.alignment = TextAlignmentOptions.Center;
        label.textWrappingMode = TextWrappingModes.NoWrap; label.raycastTarget = false;
        label.color = new Color(1, .95f, .86f); return label;
    }
    public static string Apply()
    {
        var scene = SceneManager.GetActiveScene();
        if (Application.isPlaying || scene.path != "Assets/Scenes/FastFood.unity")
            throw new InvalidOperationException("Open FastFood in Edit Mode first.");
        var view = UnityEngine.Object.FindFirstObjectByType<FastFoodServiceView>();
        if (view.GetComponent<FastFoodMirrorCloseUp>() != null || GameObject.Find("TILL/MirrorToy") != null)
            throw new InvalidOperationException("Mirror toy already configured; adjust existing settings.");
        Directory.CreateDirectory("Logs/MirrorToy");
        EditorSceneManager.SaveScene(scene, "Logs/MirrorToy/FastFood.before-configure.unity", true);
        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
        foreach (string file in Directory.GetFiles(Folder, "*.png"))
        {
            var importer = (TextureImporter)AssetImporter.GetAtPath(file.Replace('\\', '/'));
            importer.textureType = TextureImporterType.Sprite; importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = 100; importer.filterMode = FilterMode.Point; importer.mipmapEnabled = false;
            importer.npotScale = TextureImporterNPOTScale.None; importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.isReadable = true; importer.alphaIsTransparency = true; importer.wrapMode = TextureWrapMode.Clamp;
            var settings = new TextureImporterSettings(); importer.ReadTextureSettings(settings);
            settings.spriteAlignment = (int)SpriteAlignment.Center; settings.spritePivot = Vector2.one * .5f;
            settings.spriteMeshType = SpriteMeshType.FullRect; settings.spriteGenerateFallbackPhysicsShape = false;
            importer.SetTextureSettings(settings); importer.SaveAndReimport();
        }
        var manifest = Newtonsoft.Json.JsonConvert.DeserializeObject<Manifest>(File.ReadAllText(Folder + "source-layers.json"));
        font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/UI/NytrockFont/TMP/GNF.asset");
        var modal = (GameObject)Read(view, "_modal");
        var root = Rect(modal.transform, "MirrorCloseUp", new Vector2(1280, 720), Vector2.zero);
        Undo.RegisterCreatedObjectUndo(root.gameObject, "Add authored mirror close-up");
        root.anchorMin = Vector2.zero; root.anchorMax = Vector2.one; root.offsetMin = root.offsetMax = Vector2.zero;
        var backdrop = root.gameObject.AddComponent<UnityEngine.UI.Image>();
        backdrop.color = new Color(.035f, .030f, .035f); backdrop.raycastTarget = true;
        var fit = Rect(root, "ArtCanvas", new Vector2(512, 512), Vector2.zero);
        var fitter = fit.gameObject.AddComponent<UnityEngine.UI.AspectRatioFitter>();
        fitter.aspectMode = UnityEngine.UI.AspectRatioFitter.AspectMode.FitInParent; fitter.aspectRatio = 1;
        var content = Rect(fit, "SourceCoordinates", new Vector2(512, 512), Vector2.zero);
        var scale = content.gameObject.AddComponent<CloseUpArtScale>();
        var scaleFields = new SerializedObject(scale); scaleFields.FindProperty("_referenceSize").vector2Value = new Vector2(512, 512); scaleFields.ApplyModifiedPropertiesWithoutUndo();
        var full = new[] { 0, 0, 128, 128 };
        Image(content, "Mirror", Sprite("Mirror"), full);
        var toy = Image(content, "Toy", Sprite("Toy_00"), full);
        var outline = Image(content, "ToyOutline", Sprite("ToyOutline"), full); outline.enabled = false;
        var pickup = Image(content, "ToyPickup", Sprite("ToyStanding"), manifest.standingToyBounds); pickup.enabled = false;
        var hintR = Rect(root, "Hint", new Vector2(650, 46), new Vector2(0, 32));
        hintR.anchorMin = hintR.anchorMax = new Vector2(.5f, 0);
        var hintBG = hintR.gameObject.AddComponent<UnityEngine.UI.Image>(); hintBG.color = new Color(.045f, .025f, .035f, .88f); hintBG.raycastTarget = false;
        var label = Text(hintR, "Text", new Vector2(620, 42), 26);
        var group = hintR.gameObject.AddComponent<CanvasGroup>(); group.alpha = 0; group.interactable = group.blocksRaycasts = false;
        var hint = hintR.gameObject.AddComponent<SmoothServiceNotice>(); Set(hint, "_label", label); Set(hint, "_group", group);
        var closeR = Rect(root, "CloseMirror", new Vector2(36, 36), new Vector2(-8, -8));
        closeR.anchorMin = closeR.anchorMax = Vector2.one; closeR.pivot = Vector2.one;
        var closeImage = closeR.gameObject.AddComponent<UnityEngine.UI.Image>(); closeImage.color = new Color(.1f, .07f, .08f, .85f);
        var close = closeR.gameObject.AddComponent<UnityEngine.UI.Button>(); close.targetGraphic = closeImage;
        close.navigation = new UnityEngine.UI.Navigation { mode = UnityEngine.UI.Navigation.Mode.None };
        Text(closeR, "Label", new Vector2(36, 36), 28).text = "X";
        UnityEditor.Events.UnityEventTools.AddPersistentListener(close.onClick, view.Close);
        var controller = view.gameObject.AddComponent<FastFoodMirrorCloseUp>();
        Set(controller, "_service", Read(view, "_service")); Set(controller, "_player", Read(view, "_player"));
        Set(controller, "_root", root.gameObject); Set(controller, "_toy", toy); Set(controller, "_pickup", pickup);
        Set(controller, "_outline", outline); Set(controller, "_hint", hint);
        Set(controller, "_audio", Read(view, "_audio")); Set(controller, "_pickupSound", Read(view, "_pickupSound"));
        Sprites(controller, "_toyFrames", Enumerable.Range(0, 17).Select(i => Sprite("Toy_" + i.ToString("00"))).ToArray());
        var fields = new SerializedObject(controller); var times = fields.FindProperty("_frameDurations"); times.arraySize = manifest.durationsMs.Length;
        for (int i = 0; i < times.arraySize; ++i) times.GetArrayElementAtIndex(i).floatValue = manifest.durationsMs[i] / 1000f;
        fields.ApplyModifiedPropertiesWithoutUndo(); Set(view, "_mirrorCloseUp", controller);

        var service = (FacelessOrderService)Read(view, "_service");
        var station = UnityEngine.Object.FindObjectsByType<FastFoodServiceStation>(FindObjectsSortMode.None).Single(s => s.Kind == FastFoodStationKind.Mirror);
        Undo.RecordObject(station.transform, "Move toy interaction to hall");
        station.transform.position = new Vector3(-5.27f, -.33f, 0);
        var stationFields = new SerializedObject(station); stationFields.FindProperty("_onlyWhileTaskPending").boolValue = true;
        stationFields.FindProperty("_reach").floatValue = .48f; stationFields.ApplyModifiedPropertiesWithoutUndo();
        var world = new GameObject("MirrorToy"); Undo.RegisterCreatedObjectUndo(world, "Place toy mirror in hall");
        world.transform.SetParent(GameObject.Find("TILL").transform, false);
        world.transform.position = new Vector3(-5.27f, .08f, 0); world.transform.localScale = Vector3.one * .8f;
        var renderer = world.AddComponent<SpriteRenderer>(); renderer.sprite = Sprite("WorldSitting");
        renderer.sharedMaterial = GameObject.Find("Background/FastFoodHall - 6").GetComponent<SpriteRenderer>().sharedMaterial;
        renderer.sortingLayerName = "Objects"; renderer.sortingOrder = 7;
        var art = world.AddComponent<FastFoodStationArt>(); Set(art, "_service", service); Set(art, "_renderer", renderer);
        Set(art, "_initial", Sprite("WorldSitting")); Set(art, "_finished", Sprite("WorldStanding")); Set(art, "_collected", Sprite("WorldEmpty"));
        var artFields = new SerializedObject(art); artFields.FindProperty("_kind").enumValueIndex = (int)FastFoodStationKind.Mirror; artFields.ApplyModifiedPropertiesWithoutUndo();
        var edgeObj = new GameObject("InteractionOutline"); edgeObj.transform.SetParent(world.transform, false);
        var edge = edgeObj.AddComponent<SpriteRenderer>(); edge.sprite = renderer.sprite;
        edge.sharedMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/Shaders/Interaction/LayerOutline.mat");
        edge.sortingLayerName = "Objects"; edge.sortingOrder = 8; edge.enabled = false;
        var edgeControl = edgeObj.AddComponent<FastFoodStationOutline>(); Set(edgeControl, "_station", station); Set(edgeControl, "_outline", edge); Set(edgeControl, "_source", renderer);
        var old = GameObject.Find("FacelessService/Restroom/ServiceMirror");
        if (old != null) { Undo.RecordObject(old, "Use hall mirror for toy order"); old.SetActive(false); }
        var inventoryFields = new SerializedObject(Read(view, "_inventory"));
        inventoryFields.FindProperty("_orderIcons").GetArrayElementAtIndex((int)FastFoodOrderItem.Toy).objectReferenceValue = Sprite("ToyStanding");
        inventoryFields.ApplyModifiedPropertiesWithoutUndo();
        root.gameObject.SetActive(false);
        EditorSceneManager.MarkSceneDirty(scene); AssetDatabase.SaveAssets();
        if (!EditorSceneManager.SaveScene(scene)) throw new IOException("Could not save FastFood");
        return "MirrorToy at (-5.27,0.08) between cashier and tables, world outline, 17-frame authored standing animation (1.7s), hover/E pickup (.75s), empty mirror after collection; old restroom mirror disabled, existing order/inventory retained.";
    }
    public static string ResizeToyPickup()
    {
        var scene = SceneManager.GetActiveScene();
        if (Application.isPlaying || scene.path != "Assets/Scenes/FastFood.unity")
            throw new InvalidOperationException("Open FastFood in Edit Mode first.");
        Directory.CreateDirectory("Logs/MirrorToy/Resize");
        EditorSceneManager.SaveScene(scene, "Logs/MirrorToy/Resize/FastFood.editor-before.unity", true);
        File.Copy(scene.path, "Logs/MirrorToy/Resize/FastFood.disk-before.unity", true);
        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
        var manifest = Newtonsoft.Json.JsonConvert.DeserializeObject<Manifest>(File.ReadAllText(Folder + "source-layers.json"));
        var controller = UnityEngine.Object.FindFirstObjectByType<FastFoodMirrorCloseUp>();
        var pickup = (UnityEngine.UI.Image)Read(controller, "_pickup");
        var b = manifest.standingToyBounds;
        Undo.RecordObject(pickup.rectTransform, "Fit pickup to smaller toy");
        pickup.rectTransform.sizeDelta = new Vector2(b[2] - b[0], b[3] - b[1]) * 4;
        pickup.rectTransform.anchoredPosition = new Vector2((b[0] + b[2]) * 2 - 256, 256 - (b[1] + b[3]) * 2);
        EditorSceneManager.MarkSceneDirty(scene);
        if (!EditorSceneManager.SaveScene(scene)) throw new IOException("Could not save FastFood");
        return "Toy layer reduced to half size around shelf contact in all authored poses and world states; pickup rectangle aligned. Mirror size and position preserved.";
    }
}
