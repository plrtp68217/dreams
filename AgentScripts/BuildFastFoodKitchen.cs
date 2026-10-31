using System;
using TMPro;
using Unity.Cinemachine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class BuildFastFoodKitchen
{
    public static string Build()
    {
        var scene = SceneManager.GetActiveScene();
        if (Application.isPlaying || scene.path != "Assets/Scenes/FastFood.unity")
            throw new InvalidOperationException("Build requires FastFood in Edit Mode.");

        const string texturePath = "Assets/Sprites/Backgrounds/FastFood/Kitchen/bg_kitchen.png";
        AssetDatabase.ImportAsset(texturePath, ImportAssetOptions.ForceSynchronousImport);
        var importer = (TextureImporter)AssetImporter.GetAtPath(texturePath);
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.spritePixelsPerUnit = 100;
        importer.filterMode = FilterMode.Point;
        importer.mipmapEnabled = false;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.wrapMode = TextureWrapMode.Clamp;
        importer.npotScale = TextureImporterNPOTScale.None;
        importer.spritePivot = new Vector2(0.5f, 0.5f);
        importer.SaveAndReimport();

        Undo.IncrementCurrentGroup();
        Undo.SetCurrentGroupName("FastFood kitchen route");
        int undoGroup = Undo.GetCurrentGroup();

        var routeRoot = Child(null, "FastFoodKitchenRoute");
        var route = Component<FastFoodKitchenRoute>(routeRoot);
        var kitchen = Child(null, "Kitchen");
        var background = Child(kitchen.transform, "Background");
        background.transform.position = new Vector3(-20f, 0f, 0f);
        var renderer = Component<SpriteRenderer>(background);
        renderer.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(texturePath);
        renderer.sortingLayerName = "Background";
        renderer.sortingOrder = 0;
        renderer.sharedMaterial = GameObject.Find("Background/FastFoodTill - 1").GetComponent<SpriteRenderer>().sharedMaterial;

        Box(kitchen.transform, "Floor", new Vector2(-20f, -1.01f), new Vector2(5.4f, 0.54f), false);
        Box(kitchen.transform, "LeftBoundary", new Vector2(-22.65f, 1f), new Vector2(0.3f, 10f), false);
        Box(kitchen.transform, "RightBoundary", new Vector2(-17.35f, 1f), new Vector2(0.3f, 10f), false);
        var kitchenBounds = ConfigureCamera();
        var kitchenSpawn = Child(kitchen.transform, "SpawnFromCounter");
        kitchenSpawn.transform.position = new Vector3(-18.15f, -0.337f, -3f);

        var counter = GameObject.Find("TILL").transform;
        Box(counter, "LeftBoundary", new Vector2(-8.35f, 1f), new Vector2(0.3f, 10f), false);
        var counterSpawn = Child(counter, "SpawnFromKitchen");
        counterSpawn.transform.position = new Vector3(-7.65f, -0.337f, -3f);

        var enter = Box(counter, "KitchenEntrance", new Vector2(-8.1f, 1f), new Vector2(0.2f, 10f), true);
        var exit = Box(kitchen.transform, "ReturnToCounter", new Vector2(-17.7f, 1f), new Vector2(0.2f, 10f), true);
        var enterDoor = Component<FastFoodKitchenDoor>(enter.gameObject);
        SetObject(enterDoor, "_route", route);
        SetBool(enterDoor, "_toKitchen", true);
        var exitDoor = Component<FastFoodKitchenDoor>(exit.gameObject);
        SetObject(exitDoor, "_route", route);
        SetBool(exitDoor, "_toKitchen", false);

        var overlay = Child(routeRoot.transform, "RouteCanvas", true);
        var canvas = Component<Canvas>(overlay);
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        var scaler = Component<UnityEngine.UI.CanvasScaler>(overlay);
        scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        var hintRoot = Child(overlay.transform, "KitchenHint", true);
        var hintRect = (RectTransform)hintRoot.transform;
        hintRect.anchorMin = hintRect.anchorMax = new Vector2(0.5f, 0f);
        hintRect.pivot = new Vector2(0.5f, 0f);
        hintRect.anchoredPosition = new Vector2(0, 54);
        hintRect.sizeDelta = new Vector2(660, 74);
        var hint = Component<CanvasGroup>(hintRoot);
        hint.alpha = 0;
        hint.blocksRaycasts = false;
        hint.interactable = false;
        var panel = Component<UnityEngine.UI.Image>(hintRoot);
        panel.color = new Color(0.035f, 0.02f, 0.035f, 0.88f);
        panel.raycastTarget = false;
        var hintLabel = Child(hintRoot.transform, "Text", true);
        Stretch((RectTransform)hintLabel.transform);
        var text = Component<TextMeshProUGUI>(hintLabel);
        text.font = GameObject.Find("Text DreamTitle").GetComponent<TextMeshProUGUI>().font;
        text.fontSize = 32;
        text.alignment = TextAlignmentOptions.Center;
        text.color = new Color(1f, 0.95f, 0.8f, 1f);
        text.text = "На кухню: налево (A)";
        text.raycastTarget = false;

        var black = Child(overlay.transform, "TransitionBlackMask", true);
        Stretch((RectTransform)black.transform);
        black.transform.SetAsLastSibling();
        var mask = Component<UnityEngine.UI.Image>(black);
        mask.color = new Color(0, 0, 0, 0);
        mask.raycastTarget = false;

        SetObject(route, "_till", GameObject.Find("TILL/TillTrigger").GetComponent<TillTrigger>());
        SetObject(route, "_player", GameObject.Find("Player").GetComponent<Player>());
        SetObject(route, "_playerController", UnityEngine.Object.FindFirstObjectByType<PlayerController>());
        SetObject(route, "_inputService", UnityEngine.Object.FindFirstObjectByType<InputService>());
        SetObject(route, "_camera", GameObject.Find("CAMERA/CinemachineCamera").GetComponent<CinemachineCamera>());
        SetObject(route, "_confiner", GameObject.Find("CAMERA/CinemachineCamera").GetComponent<CinemachineConfiner2D>());
        SetObject(route, "_counterCameraBounds", GameObject.Find("CAMERA/Confiner").GetComponent<BoxCollider2D>());
        SetObject(route, "_kitchenCameraBounds", kitchenBounds);
        SetObject(route, "_kitchenSpawn", kitchenSpawn.transform);
        SetObject(route, "_counterSpawn", counterSpawn.transform);
        SetObject(route, "_hint", hint);
        SetObject(route, "_hintText", text);
        SetObject(route, "_blackMask", mask);
        var routeSettings = new SerializedObject(route);
        routeSettings.FindProperty("_fadeDuration").floatValue = 0.4f;
        routeSettings.FindProperty("_blackHoldDuration").floatValue = 0.1f;
        routeSettings.ApplyModifiedProperties();

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Undo.CollapseUndoOperations(undoGroup);
        return "Saved FastFood: Kitchen, floor and boundaries, two doors, first-order hint and black transition overlay.";
    }

    public static string FixCamera()
    {
        var scene = SceneManager.GetActiveScene();
        if (Application.isPlaying || scene.path != "Assets/Scenes/FastFood.unity")
            throw new InvalidOperationException("Camera fix requires FastFood in Edit Mode.");
        Undo.IncrementCurrentGroup();
        int group = Undo.GetCurrentGroup();
        Undo.SetCurrentGroupName("Fix FastFood kitchen camera");
        var bounds = ConfigureCamera();
        var route = GameObject.Find("FastFoodKitchenRoute").GetComponent<FastFoodKitchenRoute>();
        SetObject(route, "_kitchenCameraBounds", bounds);
        var settings = new SerializedObject(route);
        settings.FindProperty("_fadeDuration").floatValue = 0.4f;
        settings.FindProperty("_blackHoldDuration").floatValue = 0.1f;
        settings.ApplyModifiedProperties();
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Undo.CollapseUndoOperations(group);
        return "Updated kitchen Confiner, fixed camera horizon, and eased 0.4s fades. Existing CAMERA/Confiner retained.";
    }

    private static BoxCollider2D ConfigureCamera()
    {
        // Leave the counter's authored CAMERA/Confiner intact. A small vertical margin
        // prevents the kitchen's confined region collapsing when the view is one screen tall.
        var cameraRoot = GameObject.Find("CAMERA").transform;
        var oldBounds = GameObject.Find("Kitchen").transform.Find("CameraBounds");
        if (oldBounds != null)
        {
            Undo.SetTransformParent(oldBounds, cameraRoot, "Move kitchen camera bounds");
            Undo.RecordObject(oldBounds.gameObject, "Name kitchen camera bounds");
            oldBounds.name = "KitchenConfiner";
        }
        var bounds = Box(cameraRoot, "KitchenConfiner", new Vector2(-20f, 0f), new Vector2(5f, 2f), true);
        var camera = GameObject.Find("CAMERA/CinemachineCamera");
        Component<FastFoodCameraHorizon>(camera);
        var confiner = camera.GetComponent<CinemachineConfiner2D>();
        Undo.RecordObject(confiner, "Tune camera boundary slowdown");
        confiner.SlowingDistance = 0.2f;
        return bounds;
    }

    private static GameObject Child(Transform parent, string name, bool ui = false)
    {
        Transform found = parent != null ? parent.Find(name) : GameObject.Find(name)?.transform;
        if (found != null) return found.gameObject;
        var go = ui ? new GameObject(name, typeof(RectTransform)) : new GameObject(name);
        Undo.RegisterCreatedObjectUndo(go, "Create " + name);
        if (parent != null) go.transform.SetParent(parent, false);
        return go;
    }

    private static T Component<T>(GameObject go) where T : Component
    {
        return go.TryGetComponent<T>(out var component) ? component : Undo.AddComponent<T>(go);
    }

    private static BoxCollider2D Box(Transform parent, string name, Vector2 position, Vector2 size, bool trigger)
    {
        var go = Child(parent, name);
        go.transform.position = new Vector3(position.x, position.y, 0);
        go.layer = trigger ? 0 : LayerMask.NameToLayer("Ground");
        var box = Component<BoxCollider2D>(go);
        box.size = size;
        box.offset = Vector2.zero;
        box.isTrigger = trigger;
        return box;
    }

    private static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.offsetMin = rect.offsetMax = Vector2.zero;
    }

    private static void SetObject(UnityEngine.Object target, string name, UnityEngine.Object value)
    {
        var so = new SerializedObject(target);
        so.FindProperty(name).objectReferenceValue = value;
        so.ApplyModifiedProperties();
    }

    private static void SetBool(UnityEngine.Object target, string name, bool value)
    {
        var so = new SerializedObject(target);
        so.FindProperty(name).boolValue = value;
        so.ApplyModifiedProperties();
    }
}
