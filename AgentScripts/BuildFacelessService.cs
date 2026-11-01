using System;
using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.U2D.Sprites;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class BuildFacelessService
{
    const string Art = "Assets/Sprites/FastFoodService/";
    static TMP_FontAsset font;
    static Material lit;
    static Player player;
    static InputService input;
    static FacelessOrderService service;
    static FastFoodServiceView view;
    static Transform stations;

    public static string Apply()
    {
        var scene = SceneManager.GetActiveScene();
        if (Application.isPlaying || scene.path != "Assets/Scenes/FastFood.unity") throw new Exception("Open FastFood in Edit Mode.");
        if (GameObject.Find("FacelessService") != null) throw new Exception("Service already built. Adjust it without rebuilding the scene.");
        foreach (string name in new[] { "Fryer", "Fridge", "Dispenser", "Mirror", "Burger", "Fries", "Drink", "Toy", "Clown", "MedicalRecord", "FuseBox", "Restroom", "Tray", "Pocket", "Kitchen" })
            if (!File.Exists(Art + name + ".png")) throw new Exception("Missing generated art: " + name);
        EditorSceneManager.SaveScene(scene, "Logs/FacelessExperiment/FastFood.before-build.unity", true);
        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
        foreach (string path in Directory.GetFiles(Art, "*.png", SearchOption.AllDirectories)) Import(path.Replace('\\', '/'));
        font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/UI/NytrockFont/TMP/GNF.asset");
        player = UnityEngine.Object.FindFirstObjectByType<Player>();
        input = UnityEngine.Object.FindFirstObjectByType<InputService>();
        lit = GameObject.Find("Kitchen/Background").GetComponent<SpriteRenderer>().sharedMaterial;
        var root = new GameObject("FacelessService");
        Undo.RegisterCreatedObjectUndo(root, "Build first guest service");
        var inventory = root.AddComponent<FastFoodInventory>();
        service = root.AddComponent<FacelessOrderService>();
        view = root.AddComponent<FastFoodServiceView>();
        var sequence = UnityEngine.Object.FindFirstObjectByType<GuestOrderSequence>();
        var guest = Ref<NpcVisit>(sequence, "_guestVisit");
        Set(service, "_inventory", inventory); Set(service, "_input", input); Set(service, "_orderSequence", sequence); Set(service, "_view", view); Set(service, "_player", player);
        Set(sequence, "_service", service);
        SetArray(inventory, "_orderIcons", new[] { Sprite("Burger"), Sprite("Fries"), Sprite("Drink"), Sprite("Toy") });
        Set(inventory, "_medicalRecordIcon", Sprite("MedicalRecord"));
        Set(view, "_service", service); Set(view, "_inventory", inventory); Set(view, "_input", input); Set(view, "_player", player);
        SetArray(view, "_stationArt", new[] { Sprite("Fryer"), Sprite("FuseBox"), Sprite("Fridge"), Sprite("Dispenser"), Sprite("Mirror"), Sprite("Burger") });
        SetArray(view, "_oilFrames", Frames("OilDrain")); SetArray(view, "_pourFrames", Frames("DrinkPour")); SetArray(view, "_mirrorFrames", Frames("MirrorReveal"));
        SetArray(view, "_fridgeFrames", Frames("FridgeOpening"));
        var audio = root.AddComponent<AudioSource>(); audio.playOnAwake = false; audio.loop = false; audio.spatialBlend = 0; audio.volume = .18f;
        Set(view, "_audio", audio);
        Set(view, "_drainSound", Clip("Kitchen/fastfood_sfx_water_drain_01.wav"));
        Set(view, "_pourSound", Clip("Kitchen/fastfood_sfx_soda_dispenser_pour_01.wav"));
        Set(view, "_fridgeSound", Clip("Kitchen/fastfood_sfx_fridge_door_open_close_01.mp3"));
        Set(view, "_pickupSound", Clip("Interactions/fastfood_sfx_item_pickup_01.mp3"));

        var completion = new GameObject("FacelessOrderCompletion").AddComponent<NpcVisit>(); completion.transform.SetParent(root.transform, false);
        foreach (string field in new[] { "_inputService", "_npc", "_spawnPoint", "_standPoint", "_subtitles", "_voiceProfile" }) Set(completion, field, Ref<UnityEngine.Object>(guest, field));
        Bool(completion, "_continueFromCurrentPosition", true);
        Set(completion, "_exitPoint", Ref<Transform>(guest, "_spawnPoint"));
        Strings(completion, "_phraseIds", new[] { "fastfood_faceless_complete_1", "fastfood_faceless_complete_2" });
        Set(sequence, "_completionVisit", completion);
        Set(guest, "_exitPoint", null);

        stations = new GameObject("Stations").transform; stations.SetParent(root.transform, false);
        Station("FryerStation", FastFoodStationKind.Fryer, new Vector3(-20.25f, -.33f, 0), .45f);
        Station("FuseBoxStation", FastFoodStationKind.FuseBox, new Vector3(-22.12f, -.33f, 0), .35f);
        Station("FridgeStation", FastFoodStationKind.Fridge, new Vector3(-21.35f, -.33f, 0), .43f);
        Station("DispenserStation", FastFoodStationKind.Dispenser, new Vector3(-19.05f, -.33f, 0), .45f);
        var stand = Ref<Transform>(guest, "_standPoint");
        Station("ServeGuestStation", FastFoodStationKind.Guest, new Vector3(stand.position.x, -.33f, 0), .65f);
        WorldSprite("ServiceFryer", "Fryer", new Vector3(-20.25f, -.74f, 0), .8f, GameObject.Find("Kitchen").transform);
        WorldSprite("ServiceFridge", "Fridge", new Vector3(-21.35f, -.74f, 0), 1.18f, GameObject.Find("Kitchen").transform);
        WorldSprite("ServiceDispenser", "Dispenser", new Vector3(-19.05f, -.74f, 0), .95f, GameObject.Find("Kitchen").transform);
        WorldSprite("ServiceFuseBox", "FuseBox", new Vector3(-22.12f, -.24f, 0), .32f, GameObject.Find("Kitchen").transform);
        BuildRestroom(root.transform);
        BuildUI(root.transform, inventory);
        var tension = root.AddComponent<FastFoodTension>();
        Set(tension, "_service", service); Set(tension, "_view", view); Set(tension, "_input", input); Set(tension, "_player", player);
        var clown = WorldSprite("ClownVisitor", "Clown", new Vector3(2, -.74f, -2), .95f, root.transform);
        clown.sortingLayerName = "Characters"; clown.sortingOrder = 10; clown.enabled = false;
        Set(tension, "_clown", clown);
        var canvas = root.transform.Find("ServiceCanvas");
        Set(tension, "_caption", Label(canvas, "ClownCaption", new Vector2(1000, 45), new Vector2(0, 298), 24));
        var level = Label(canvas, "Tension", new Vector2(240, 28), new Vector2(-500, 338), 18); level.alignment = TextAlignmentOptions.Left;
        Set(tension, "_levelLabel", level);
        RefineArtAndControls();
        return "First guest service built, 6 stations, restroom, tray/pocket, receipt and generated artwork.";
    }

    public static string RefineArtAndControls()
    {
        if (Application.isPlaying) throw new Exception("Edit Mode required");
        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
        foreach (string path in Directory.GetFiles(Art, "*.png", SearchOption.AllDirectories)) Import(path.Replace('\\', '/'));
        font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/UI/NytrockFont/TMP/GNF.asset");
        view = UnityEngine.Object.FindFirstObjectByType<FastFoodServiceView>();
        service = UnityEngine.Object.FindFirstObjectByType<FacelessOrderService>();
        SetArray(view, "_oilFrames", Frames("OilDrain")); SetArray(view, "_pourFrames", Frames("DrinkPour"));
        SetArray(view, "_mirrorFrames", Frames("MirrorReveal")); SetArray(view, "_fridgeFrames", Frames("FridgeOpening"));
        var mixing = Ref<GameObject>(view, "_mixing").transform;
        var sliders = new SerializedObject(view).FindProperty("_syrups");
        var less = new UnityEngine.UI.Button[3]; var more = new UnityEngine.UI.Button[3];
        for (int i = 0; i < 3; i++)
        {
            var slider = (UnityEngine.UI.Slider)sliders.GetArrayElementAtIndex(i).objectReferenceValue;
            slider.minValue = 0; slider.maxValue = 100; slider.value = (i + 1) * 10;
            ((RectTransform)slider.transform).sizeDelta = new Vector2(260, 14);
            less[i] = mixing.Find("Less" + i)?.GetComponent<UnityEngine.UI.Button>() ?? Button(mixing, "Less" + i, new Vector2(32, 32), new Vector2(-160, 56 - i * 60), "-", 24);
            more[i] = mixing.Find("More" + i)?.GetComponent<UnityEngine.UI.Button>() ?? Button(mixing, "More" + i, new Vector2(32, 32), new Vector2(160, 56 - i * 60), "+", 24);
        }
        SetArray(view, "_syrupLess", less); SetArray(view, "_syrupMore", more);
        var sign = GameObject.Find("FacelessService/RestroomSign").GetComponent<TextMeshPro>(); sign.fontSize = .8f;
        var kitchen = GameObject.Find("Kitchen/Background").GetComponent<SpriteRenderer>(); kitchen.sprite = Sprite("Kitchen");
        var lights = kitchen.GetComponentsInChildren<UnityEngine.Rendering.Universal.Light2D>(true);
        var lightPositions = lights.Select(light => light.transform.position).ToArray();
        float horizon = new SerializedObject(UnityEngine.Object.FindFirstObjectByType<FastFoodCameraHorizon>()).FindProperty("_height").floatValue;
        var cameraBounds = Ref<Collider2D>(UnityEngine.Object.FindFirstObjectByType<FastFoodKitchenRoute>(), "_kitchenCameraBounds").bounds;
        kitchen.transform.localScale = new Vector3((cameraBounds.size.x + .04f) / kitchen.sprite.bounds.size.x, 2 * Camera.main.orthographicSize / kitchen.sprite.bounds.size.y, 1);
        kitchen.transform.position = new Vector3(cameraBounds.center.x, horizon - Camera.main.orthographicSize, 0);
        for (int i = 0; i < lights.Length; i++) lights[i].transform.position = lightPositions[i];
        Set(UnityEngine.Object.FindFirstObjectByType<KitchenLampFlicker>(), "_backgroundOverride", kitchen.sprite);
        var clown = GameObject.Find("FacelessService/ClownVisitor").GetComponent<SpriteRenderer>();
        if (clown.GetComponent<CharacterGroundShadow>() == null)
        {
            var source = UnityEngine.Object.FindFirstObjectByType<Player>().GetComponent<CharacterGroundShadow>();
            var shadow = clown.gameObject.AddComponent<CharacterGroundShadow>();
            EditorUtility.CopySerialized(source, shadow); Set(shadow, "_source", clown);
        }
        foreach (var pair in new[] { ("Kitchen/ServiceFryer", FastFoodStationKind.Fryer, "OilDrain"), ("Kitchen/ServiceFridge", FastFoodStationKind.Fridge, "FridgeOpening"), ("Kitchen/ServiceDispenser", FastFoodStationKind.Dispenser, "DrinkPour"), ("FacelessService/Restroom/ServiceMirror", FastFoodStationKind.Mirror, "MirrorReveal") })
        {
            var renderer = GameObject.Find(pair.Item1).GetComponent<SpriteRenderer>();
            var state = renderer.GetComponent<FastFoodStationArt>() ?? renderer.gameObject.AddComponent<FastFoodStationArt>();
            Set(state, "_service", service); Set(state, "_renderer", renderer); Set(state, "_initial", renderer.sprite); Set(state, "_finished", Frames(pair.Item3).Last());
            var fields = new SerializedObject(state); fields.FindProperty("_kind").enumValueIndex = (int)pair.Item2; fields.ApplyModifiedPropertiesWithoutUndo();
        }
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene()); EditorSceneManager.SaveScene(SceneManager.GetActiveScene()); AssetDatabase.SaveAssets();
        return "Updated generated final frames, kitchen backdrop and percent controls; original receipt preserved.";
    }

    static void BuildRestroom(Transform root)
    {
        var room = new GameObject("Restroom").transform; room.SetParent(root, false);
        var background = WorldSprite("Background", "Restroom", new Vector3(-40, -1.0f, 0), 2.16f, room);
        background.sortingLayerName = "Background"; background.sortingOrder = 0;
        var floor = Box(room, "Floor", new Vector2(-40, -1.01f), new Vector2(3.84f, .54f), false);
        floor.gameObject.layer = LayerMask.NameToLayer("Ground");
        var material = AssetDatabase.LoadAssetAtPath<PhysicsMaterial2D>("Assets/Materials/LevelBoundary.physicsMaterial2D");
        foreach (float x in new[] { -41.95f, -38.05f })
        {
            var wall = Box(room, x < -40 ? "LeftBoundary" : "RightBoundary", new Vector2(x, 0), new Vector2(.2f, 6), false);
            wall.sharedMaterial = material; wall.gameObject.layer = LayerMask.NameToLayer("Ground");
        }
        var mirrorObject = GameObject.Find("StoryMirror");
        if (mirrorObject != null) mirrorObject.SetActive(false);
        WorldSprite("ServiceMirror", "Mirror", new Vector3(-40.35f, -.74f, 0), 1.15f, room);
        var shadowFloor = new GameObject("RestroomShadowFloor").AddComponent<CharacterShadowSurface>();
        shadowFloor.transform.SetParent(room, false);
        var surfaceFields = new SerializedObject(shadowFloor);
        surfaceFields.FindProperty("_floorBounds").rectValue = new Rect(-41.85f, -.98f, 3.7f, .48f);
        surfaceFields.FindProperty("_groundY").floatValue = -.74f;
        surfaceFields.ApplyModifiedPropertiesWithoutUndo();
        foreach (var shadow in UnityEngine.Object.FindObjectsByType<CharacterGroundShadow>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            var fields = new SerializedObject(shadow); var array = fields.FindProperty("_surfaces");
            int count = array.arraySize; array.arraySize = count + 1; array.GetArrayElementAtIndex(count).objectReferenceValue = shadowFloor;
            fields.ApplyModifiedPropertiesWithoutUndo();
            PrefabUtility.RecordPrefabInstancePropertyModifications(shadow);
        }
        Station("MirrorStation", FastFoodStationKind.Mirror, new Vector3(-40.35f, -.33f, 0), .5f);
        var confiner = Box(room, "RestroomConfiner", new Vector2(-40, -.05f), new Vector2(3.84f, 1.8f), true);
        confiner.gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
        Transform spawn = new GameObject("SpawnFromHall").transform; spawn.SetParent(room, false); spawn.position = new Vector3(-38.95f, -.337f, -3);
        Transform hallSpawn = new GameObject("SpawnFromRestroom").transform; hallSpawn.SetParent(root, false); hallSpawn.position = new Vector3(3.7f, -.337f, -3);
        var route = UnityEngine.Object.FindFirstObjectByType<FastFoodKitchenRoute>();
        Set(route, "_restroomCameraBounds", confiner); Set(route, "_restroomSpawn", spawn); Set(route, "_hallRestroomSpawn", hallSpawn);
        Door(root, "RestroomEntrance", new Vector2(4.18f, 0), true, route);
        Door(room, "ReturnToHall", new Vector2(-38.35f, 0), false, route);
        WorldLabel(root, "RestroomSign", "УБОРНАЯ / WC →", new Vector3(3.7f, .35f, -1));
    }

    static void Door(Transform parent, string name, Vector2 position, bool enter, FastFoodKitchenRoute route)
    {
        var box = Box(parent, name, position, new Vector2(.12f, 4), true);
        var door = box.gameObject.AddComponent<FastFoodKitchenDoor>();
        Set(door, "_route", route); Bool(door, "_restroomRoute", true); Bool(door, "_toKitchen", enter);
    }

    static void BuildUI(Transform root, FastFoodInventory inventory)
    {
        var canvasObject = new GameObject("ServiceCanvas", typeof(RectTransform), typeof(Canvas), typeof(UnityEngine.UI.CanvasScaler), typeof(UnityEngine.UI.GraphicRaycaster));
        canvasObject.transform.SetParent(root, false);
        var canvas = canvasObject.GetComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay; canvas.sortingLayerName = "UI"; canvas.sortingOrder = 40;
        var scaler = canvasObject.GetComponent<UnityEngine.UI.CanvasScaler>(); scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280, 720); scaler.matchWidthOrHeight = .5f;
        var hud = Rect(canvasObject.transform, "Inventory", new Vector2(1280, 720), Vector2.zero);
        hud.anchorMin = Vector2.zero; hud.anchorMax = Vector2.one; hud.offsetMin = hud.offsetMax = Vector2.zero;
        Set(view, "_hud", hud.gameObject);
        var tray = Image(hud, "Tray", Sprite("Tray"), new Vector2(360, 145), new Vector2(25, 24));
        tray.rectTransform.anchorMin = tray.rectTransform.anchorMax = Vector2.zero; tray.rectTransform.pivot = Vector2.zero;
        var trayTitle = Label(tray.transform, "Title", new Vector2(360, 28), new Vector2(0, 161), 21); trayTitle.alignment = TextAlignmentOptions.Left;
        trayTitle.rectTransform.anchorMin = trayTitle.rectTransform.anchorMax = Vector2.zero; trayTitle.rectTransform.pivot = new Vector2(0, .5f);
        Set(view, "_trayTitle", trayTitle);
        var icons = new UnityEngine.UI.Image[4]; var buttons = new UnityEngine.UI.Button[4]; var slotLabels = new TextMeshProUGUI[4];
        for (int i = 0; i < 4; i++)
        {
            var button = Button(tray.transform, "Slot" + (i + 1), new Vector2(78, 74), new Vector2(-130 + i * 86, -1), "", 16);
            button.GetComponent<UnityEngine.UI.Image>().color = new Color(0, 0, 0, .08f);
            icons[i] = Image(button.transform, "Icon", inventory.Icon((FastFoodOrderItem)i), new Vector2(62, 62), new Vector2(0, -1));
            slotLabels[i] = Label(button.transform, "SlotLabel", new Vector2(70, 22), new Vector2(0, 31), 15);
            slotLabels[i].alignment = TextAlignmentOptions.Left; buttons[i] = button;
        }
        SetArray(view, "_trayIcons", icons); SetArray(view, "_trayButtons", buttons); SetArray(view, "_slotLabels", slotLabels);
        var pocket = Image(hud, "Pocket", Sprite("Pocket"), new Vector2(255, 94), new Vector2(-25, 24));
        pocket.rectTransform.anchorMin = pocket.rectTransform.anchorMax = new Vector2(1, 0); pocket.rectTransform.pivot = new Vector2(1, 0);
        var pocketTitle = Label(pocket.transform, "Title", new Vector2(255, 28), new Vector2(0, 60), 21);
        pocketTitle.alignment = TextAlignmentOptions.Right; Set(view, "_pocketTitle", pocketTitle);
        var pocketButton = Button(pocket.transform, "MedicalRecordSlot", new Vector2(76, 72), new Vector2(-82, 0), "", 16);
        pocketButton.GetComponent<UnityEngine.UI.Image>().color = new Color(0, 0, 0, .03f);
        Set(view, "_pocketButton", pocketButton);
        Set(view, "_pocketIcon", Image(pocketButton.transform, "Icon", inventory.MedicalRecordIcon, new Vector2(60, 60), Vector2.zero));
        Set(view, "_ready", Label(hud, "Ready", new Vector2(900, 45), new Vector2(0, -197), 22));
        Set(view, "_prompt", Label(canvasObject.transform, "InteractionPrompt", new Vector2(900, 40), new Vector2(0, -155), 24));

        var modal = Image(canvasObject.transform, "Interaction", null, new Vector2(1280, 720), Vector2.zero);
        modal.color = new Color(.015f, .012f, .014f, .94f); modal.raycastTarget = true;
        modal.rectTransform.anchorMin = Vector2.zero; modal.rectTransform.anchorMax = Vector2.one; modal.rectTransform.offsetMin = modal.rectTransform.offsetMax = Vector2.zero;
        Set(view, "_modal", modal.gameObject);
        var frame = Image(modal.transform, "Panel", null, new Vector2(1170, 625), Vector2.zero); frame.color = new Color(.075f, .055f, .055f, 1);
        Set(view, "_title", Label(frame.transform, "Title", new Vector2(1060, 55), new Vector2(0, 273), 30));
        var art = Image(frame.transform, "Artwork", Sprite("Fryer"), new Vector2(570, 420), new Vector2(-245, 15));
        Set(view, "_art", art);
        var description = Label(frame.transform, "Description", new Vector2(350, 112), new Vector2(315, 210), 18);
        description.alignment = TextAlignmentOptions.TopLeft; description.rectTransform.pivot = new Vector2(.5f, 1);
        Set(view, "_description", description);
        Set(view, "_message", Label(frame.transform, "Message", new Vector2(1070, 40), new Vector2(0, -225), 18));
        var close = Button(frame.transform, "Close", new Vector2(255, 42), new Vector2(0, -276), "ЗАКРЫТЬ", 20);
        Set(view, "_close", close); Set(view, "_closeLabel", close.GetComponentInChildren<TextMeshProUGUI>());
        var actions = new UnityEngine.UI.Button[2]; var labels = new TextMeshProUGUI[2];
        for (int i = 0; i < 2; i++)
        {
            actions[i] = Button(frame.transform, "Action" + i, new Vector2(355, 47), new Vector2(315, -172 + i * 55), "", 18);
            labels[i] = actions[i].GetComponentInChildren<TextMeshProUGUI>();
        }
        SetArray(view, "_actions", actions); SetArray(view, "_actionLabels", labels);

        var keypad = Rect(frame.transform, "Keypad", new Vector2(330, 245), new Vector2(315, -15));
        Set(view, "_keypad", keypad.gameObject);
        var inputImage = Image(keypad, "Code", null, new Vector2(270, 39), new Vector2(0, 103)); inputImage.color = new Color(.025f, .025f, .03f);
        var codeLabel = Label(inputImage.transform, "Text", new Vector2(245, 38), Vector2.zero, 26);
        var code = inputImage.gameObject.AddComponent<TMP_InputField>(); code.textComponent = codeLabel; code.textViewport = inputImage.rectTransform;
        code.characterLimit = 4; code.contentType = TMP_InputField.ContentType.IntegerNumber; code.characterValidation = TMP_InputField.CharacterValidation.Digit;
        code.lineType = TMP_InputField.LineType.SingleLine; code.targetGraphic = inputImage;
        inputImage.raycastTarget = true; Set(view, "_code", code);
        var digits = new UnityEngine.UI.Button[12];
        for (int i = 0; i < 12; i++)
        {
            int display = i < 9 ? i + 1 : i == 9 ? 10 : i == 10 ? 0 : 11;
            string caption = display == 10 ? "C" : display == 11 ? "OK" : display.ToString();
            digits[display] = Button(keypad, "Key" + display, new Vector2(80, 40), new Vector2(-90 + (i % 3) * 90, 54 - (i / 3) * 47), caption, 22);
        }
        SetArray(view, "_digitButtons", digits);

        var mixing = Rect(frame.transform, "SyrupMixing", new Vector2(350, 190), new Vector2(315, 2));
        var syrups = new UnityEngine.UI.Slider[3]; var syrupLabels = new TextMeshProUGUI[3];
        for (int i = 0; i < 3; i++)
        {
            syrupLabels[i] = Label(mixing, "SyrupLabel" + i, new Vector2(350, 25), new Vector2(0, 80 - i * 60), 19);
            syrupLabels[i].alignment = TextAlignmentOptions.Left;
            var track = Image(mixing, "Syrup" + i, null, new Vector2(330, 14), new Vector2(0, 56 - i * 60));
            track.color = new Color(.02f, .02f, .025f);
            var handle = Image(track.transform, "Handle", null, new Vector2(18, 27), Vector2.zero);
            handle.color = i == 0 ? new Color(.65f, .15f, .15f) : i == 1 ? new Color(.72f, .46f, .12f) : new Color(.3f, .52f, .2f);
            handle.raycastTarget = true; track.raycastTarget = true;
            var slider = track.gameObject.AddComponent<UnityEngine.UI.Slider>(); slider.handleRect = handle.rectTransform; slider.targetGraphic = handle;
            slider.minValue = 0; slider.maxValue = 10; slider.wholeNumbers = true; slider.value = i + 1;
            syrups[i] = slider;
        }
        Set(view, "_mixing", mixing.gameObject); SetArray(view, "_syrups", syrups); SetArray(view, "_syrupLabels", syrupLabels);
        modal.gameObject.SetActive(false); hud.gameObject.SetActive(false);
    }

    static FastFoodServiceStation Station(string name, FastFoodStationKind kind, Vector3 position, float reach)
    {
        var obj = new GameObject(name); obj.transform.SetParent(stations, false); obj.transform.position = position;
        var station = obj.AddComponent<FastFoodServiceStation>();
        Set(station, "_service", service); Set(station, "_view", view); Set(station, "_player", player); Set(station, "_input", input);
        var fields = new SerializedObject(station); fields.FindProperty("_kind").enumValueIndex = (int)kind; fields.FindProperty("_reach").floatValue = reach; fields.ApplyModifiedPropertiesWithoutUndo();
        return station;
    }
    static SpriteRenderer WorldSprite(string name, string art, Vector3 position, float height, Transform parent)
    {
        var obj = new GameObject(name); obj.transform.SetParent(parent, false); obj.transform.position = position;
        var renderer = obj.AddComponent<SpriteRenderer>(); renderer.sprite = Sprite(art); renderer.sharedMaterial = lit;
        renderer.sortingLayerName = "Objects"; renderer.sortingOrder = 3;
        obj.transform.localScale = Vector3.one * height / renderer.sprite.bounds.size.y;
        return renderer;
    }
    static void WorldLabel(Transform parent, string name, string value, Vector3 position)
    {
        var go = new GameObject(name); go.transform.SetParent(parent, false); go.transform.position = position;
        var text = go.AddComponent<TextMeshPro>(); text.font = font; text.fontSharedMaterial = font.material; text.text = value;
        text.fontSize = .8f; text.color = new Color(.8f, .6f, .3f); text.alignment = TextAlignmentOptions.Center;
        text.rectTransform.sizeDelta = new Vector2(1, .16f); text.GetComponent<MeshRenderer>().sortingLayerName = "Objects"; text.GetComponent<MeshRenderer>().sortingOrder = 5;
        var localized = go.AddComponent<LocalizedPosterText>();
        Set(localized, "_title", text);
        var fields = new SerializedObject(localized); fields.FindProperty("_titleId").stringValue = "service_bathroom"; fields.ApplyModifiedPropertiesWithoutUndo();
    }
    static BoxCollider2D Box(Transform parent, string name, Vector2 position, Vector2 size, bool trigger)
    {
        var go = new GameObject(name); go.transform.SetParent(parent, false); go.transform.position = position;
        var box = go.AddComponent<BoxCollider2D>(); box.size = size; box.isTrigger = trigger; return box;
    }
    static RectTransform Rect(Transform parent, string name, Vector2 size, Vector2 position)
    {
        var go = new GameObject(name, typeof(RectTransform)); var rect = (RectTransform)go.transform; rect.SetParent(parent, false);
        rect.anchorMin = rect.anchorMax = new Vector2(.5f, .5f); rect.pivot = new Vector2(.5f, .5f); rect.sizeDelta = size; rect.anchoredPosition = position; return rect;
    }
    static UnityEngine.UI.Image Image(Transform parent, string name, Sprite sprite, Vector2 size, Vector2 position)
    {
        var rect = Rect(parent, name, size, position); var image = rect.gameObject.AddComponent<UnityEngine.UI.Image>();
        image.sprite = sprite; image.preserveAspect = sprite != null; image.raycastTarget = false; return image;
    }
    static TextMeshProUGUI Label(Transform parent, string name, Vector2 size, Vector2 position, float fontSize)
    {
        var rect = Rect(parent, name, size, position); var text = rect.gameObject.AddComponent<TextMeshProUGUI>();
        text.font = font; text.fontSharedMaterial = font.material; text.fontSize = fontSize; text.enableAutoSizing = false;
        text.color = new Color(.89f, .79f, .59f); text.raycastTarget = false; text.alignment = TextAlignmentOptions.Center;
        text.textWrappingMode = TextWrappingModes.Normal; text.overflowMode = TextOverflowModes.Overflow; return text;
    }
    static UnityEngine.UI.Button Button(Transform parent, string name, Vector2 size, Vector2 position, string value, float fontSize)
    {
        var image = Image(parent, name, null, size, position); image.color = new Color(.31f, .075f, .075f, 1); image.raycastTarget = true;
        var button = image.gameObject.AddComponent<UnityEngine.UI.Button>(); button.targetGraphic = image;
        var label = Label(image.transform, "Label", size - new Vector2(12, 4), Vector2.zero, fontSize); label.text = value;
        return button;
    }
    static Sprite Sprite(string name) => AssetDatabase.LoadAllAssetsAtPath(Art + name + ".png").OfType<Sprite>().First();
    static Sprite[] Frames(string name) => Directory.GetFiles(Art + name, "*.png").OrderBy(p => p).Select(p => AssetDatabase.LoadAssetAtPath<Sprite>(p.Replace('\\', '/'))).ToArray();
    static AudioClip Clip(string name) => AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/FastFood/" + name);
    static void Import(string path)
    {
        var importer = (TextureImporter)AssetImporter.GetAtPath(path);
        importer.textureType = TextureImporterType.Sprite; importer.spriteImportMode = SpriteImportMode.Single;
        importer.filterMode = FilterMode.Point; importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.mipmapEnabled = false; importer.alphaIsTransparency = true; importer.spritePixelsPerUnit = Path.GetFileNameWithoutExtension(path) == "Kitchen" ? 128 : 100;
        var settings = new TextureImporterSettings(); importer.ReadTextureSettings(settings);
        settings.spriteAlignment = (int)SpriteAlignment.BottomCenter; settings.spritePivot = new Vector2(.5f, 0); settings.spriteMeshType = SpriteMeshType.FullRect;
        importer.SetTextureSettings(settings); importer.SaveAndReimport();
        if (new[] { "Tray", "Pocket" }.Contains(Path.GetFileNameWithoutExtension(path))) CropEmptySpriteBorder(path, importer);
    }
    static void CropEmptySpriteBorder(string path, TextureImporter importer)
    {
        importer.spriteImportMode = SpriteImportMode.Multiple; importer.SaveAndReimport();
        var factory = new SpriteDataProviderFactories(); factory.Init();
        var provider = factory.GetSpriteEditorDataProviderFromObject(importer); provider.InitSpriteEditorDataProvider();
        var capability = provider.GetDataProvider<ISpriteFrameEditCapability>();
        if (capability == null || !capability.GetEditCapability().HasCapability(EEditCapability.CreateAndDeleteSprite) ||
            !capability.GetEditCapability().HasCapability(EEditCapability.EditSpriteRect)) throw new Exception("Sprite border cropping is not supported.");
        var ids = provider.GetDataProvider<ISpriteNameFileIdDataProvider>();
        if (ids == null) throw new Exception("Sprite name mapping is unavailable.");
        var pixels = new Texture2D(2, 2, TextureFormat.RGBA32, false); pixels.LoadImage(File.ReadAllBytes(path));
        int minX = pixels.width, minY = pixels.height, maxX = 0, maxY = 0;
        var colors = pixels.GetPixels32();
        for (int y = 0; y < pixels.height; y++) for (int x = 0; x < pixels.width; x++)
            if (colors[y * pixels.width + x].a > 0) { minX = Mathf.Min(minX, x); minY = Mathf.Min(minY, y); maxX = Mathf.Max(maxX, x); maxY = Mathf.Max(maxY, y); }
        UnityEngine.Object.DestroyImmediate(pixels);
        var prior = provider.GetSpriteRects().FirstOrDefault();
        var rect = new SpriteRect { name = Path.GetFileNameWithoutExtension(path), rect = new Rect(minX, minY, maxX - minX + 1, maxY - minY + 1),
            alignment = SpriteAlignment.Center, pivot = new Vector2(.5f, .5f), spriteID = prior != null ? prior.spriteID : GUID.Generate() };
        provider.SetSpriteRects(new[] { rect }); ids.SetNameFileIdPairs(new[] { new SpriteNameFileIdPair(rect.name, rect.spriteID) });
        provider.Apply(); importer.SaveAndReimport();
    }
    static T Ref<T>(UnityEngine.Object target, string field) where T : UnityEngine.Object => (T)new SerializedObject(target).FindProperty(field).objectReferenceValue;
    static void Set(UnityEngine.Object target, string field, UnityEngine.Object value)
    {
        var obj = new SerializedObject(target); obj.FindProperty(field).objectReferenceValue = value; obj.ApplyModifiedPropertiesWithoutUndo();
    }
    static void Bool(UnityEngine.Object target, string field, bool value)
    {
        var obj = new SerializedObject(target); obj.FindProperty(field).boolValue = value; obj.ApplyModifiedPropertiesWithoutUndo();
    }
    static void Strings(UnityEngine.Object target, string field, string[] values)
    {
        var obj = new SerializedObject(target); var array = obj.FindProperty(field); array.arraySize = values.Length;
        for (int i = 0; i < values.Length; i++) array.GetArrayElementAtIndex(i).stringValue = values[i]; obj.ApplyModifiedPropertiesWithoutUndo();
    }
    static void SetArray<T>(UnityEngine.Object target, string field, T[] values) where T : UnityEngine.Object
    {
        var obj = new SerializedObject(target); var array = obj.FindProperty(field); array.arraySize = values.Length;
        for (int i = 0; i < values.Length; i++) array.GetArrayElementAtIndex(i).objectReferenceValue = values[i]; obj.ApplyModifiedPropertiesWithoutUndo();
    }
}
