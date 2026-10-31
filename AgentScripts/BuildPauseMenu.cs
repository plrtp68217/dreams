using System;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Events;

public static class BuildPauseMenu
{
    private static readonly Color Panel = Hex("30293F");
    private static readonly Color Text = Hex("E8E0EE");
    private static readonly Color Muted = Hex("AFA3BC");
    private static readonly Color Button = Hex("43374F");
    private static readonly Color Accent = Hex("D4B78E");
    private static TMP_FontAsset _font;

    public static string Build()
    {
        if (Application.isPlaying) throw new InvalidOperationException("Exit Play Mode first.");
        _font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/UI/NytrockFont/TMP/GNF.asset");
        const string path = "Assets/Prefabs/CAMERA.prefab";
        var prefab = PrefabUtility.LoadPrefabContents(path);
        try
        {
            Configure(prefab.transform.Find("Main Camera/CanvasPause").gameObject);
            PrefabUtility.SaveAsPrefabAsset(prefab, path);
        }
        finally { PrefabUtility.UnloadPrefabContents(prefab); }
        // Scene overrides may contain old layout values; change only this pause subtree.
        Configure(GameObject.Find("CAMERA/Main Camera/CanvasPause"));
        var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        return "Updated shared CAMERA pause menu and FastFood instance: DREAMS, resume/settings/menu, volume, brightness, language, fullscreen and defaults.";
    }

    private static void Configure(GameObject canvasObject)
    {
        var canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 200;
        canvasObject.transform.localScale = Vector3.one;
        var scaler = canvasObject.GetComponent<UnityEngine.UI.CanvasScaler>();
        scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;
        Ensure<UnityEngine.UI.GraphicRaycaster>(canvasObject);

        var window = canvasObject.transform.Find("PauseWindow").gameObject;
        Stretch(window.GetComponent<RectTransform>());
        var controller = window.GetComponent<PauseWindowController>();
        var group = window.GetComponent<CanvasGroup>();
        group.alpha = 0;
        group.blocksRaycasts = group.interactable = false;

        var dimmer = window.transform.Find("ImageBG").GetComponent<UnityEngine.UI.Image>();
        Stretch(dimmer.rectTransform);
        dimmer.sprite = null;
        dimmer.type = UnityEngine.UI.Image.Type.Simple;
        dimmer.color = new Color(0.025f, 0.015f, 0.045f, 0.78f);
        dimmer.raycastTarget = true;

        var panel = window.transform.Find("ImageWindow").GetComponent<UnityEngine.UI.Image>();
        Rect(panel.rectTransform, new Vector2(760, 740), Vector2.zero);
        panel.sprite = null;
        panel.type = UnityEngine.UI.Image.Type.Simple;
        panel.color = Panel;
        panel.raycastTarget = true;
        var shadow = Ensure<UnityEngine.UI.Shadow>(panel.gameObject);
        shadow.effectColor = new Color(0.01f, 0, 0.025f, 0.35f);
        shadow.effectDistance = new Vector2(10, -10);

        var title = Label(panel.transform, "DreamsTitle", "DREAMS", "DREAMS", 88, new Vector2(640, 110), new Vector2(0, 247));
        title.characterSpacing = 12;
        title.color = Text;
        var rule = Image(panel.transform, "TitleRule", new Color(0.68f, 0.60f, 0.75f, 0.35f));
        Rect(rule.rectTransform, new Vector2(72, 3), new Vector2(0, 178));

        var main = Child(panel.transform, "MainPage");
        Stretch(main.GetComponent<RectTransform>());
        Label(main.transform, "Heading", "Пауза", "Paused", 24, new Vector2(500, 44), new Vector2(0, 138)).color = Muted;

        var layoutObject = window.transform.Find("VerticalLayout");
        if (layoutObject == null) layoutObject = main.transform.Find("VerticalLayout");
        layoutObject.SetParent(main.transform, false);
        Rect((RectTransform)layoutObject, new Vector2(520, 260), new Vector2(0, -25));
        var layout = layoutObject.GetComponent<UnityEngine.UI.VerticalLayoutGroup>();
        layout.spacing = 18;
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childControlHeight = layout.childControlWidth = true;
        layout.childForceExpandHeight = layout.childForceExpandWidth = false;

        var resume = window.transform.Find("ButtonClose");
        if (resume == null) resume = layoutObject.Find("ButtonClose");
        resume.SetParent(layoutObject, false);
        resume.SetAsFirstSibling();
        var resumeButton = StyleButton(resume.gameObject, "Продолжить", "Continue", new Vector2(520, 68));
        Click(resumeButton, controller.CloseWindow);
        var settingsButton = StyleButton(layoutObject.Find("ButtonTBC").gameObject, "Настройки", "Settings", new Vector2(520, 68));
        settingsButton.transform.SetSiblingIndex(1);
        Click(settingsButton, controller.ShowSettings);
        var menuButton = StyleButton(layoutObject.Find("ButtonMenu").gameObject, "В главное меню", "Main menu", new Vector2(520, 68));
        menuButton.transform.SetSiblingIndex(2);
        Click(menuButton, controller.ReturnToMainMenu);
        // Retain old objects for scene references, but remove the obsolete placeholder from the menu.
        foreach (Transform child in layoutObject)
        {
            bool used = child == resumeButton.transform || child == settingsButton.transform || child == menuButton.transform;
            child.gameObject.SetActive(used);
            if (!used) Ensure<UnityEngine.UI.LayoutElement>(child.gameObject).ignoreLayout = true;
        }
        Label(main.transform, "EscapeHint", "Esc — продолжить", "Esc — continue", 20, new Vector2(600, 40), new Vector2(0, -291)).color = Muted;

        var settings = Child(panel.transform, "SettingsPage");
        Stretch(settings.GetComponent<RectTransform>());
        var view = Ensure<PauseSettingsPanel>(settings);
        Label(settings.transform, "Heading", "Настройки", "Settings", 24, new Vector2(500, 44), new Vector2(0, 138)).color = Muted;
        var volume = Slider(settings.transform, "Volume", "Общая громкость", "Master volume", 1f, 0f, 1f, 62, out var volumeValue);
        var brightness = Slider(settings.transform, "Brightness", "Яркость", "Brightness", 1f, 0.5f, 1.5f, -43, out var brightnessValue);
        Label(settings.transform, "LanguageLabel", "Язык", "Language", 25, new Vector2(300, 48), new Vector2(-130, -143), TextAlignmentOptions.MidlineLeft);
        var languageButton = NewButton(settings.transform, "LanguageButton", "Русский", "English", new Vector2(220, 48), new Vector2(170, -143));
        var languageValue = languageButton.GetComponentInChildren<TextMeshProUGUI>(true);
        var localizedLanguage = languageValue.GetComponent<PauseMenuText>();
        if (localizedLanguage != null) UnityEngine.Object.DestroyImmediate(localizedLanguage);
        Click(languageButton, view.ChangeLanguage);

        Label(settings.transform, "FullscreenLabel", "Полный экран", "Fullscreen", 25, new Vector2(430, 48), new Vector2(-65, -213), TextAlignmentOptions.MidlineLeft);
        var toggleObject = Child(settings.transform, "Fullscreen");
        Rect(toggleObject.GetComponent<RectTransform>(), new Vector2(56, 38), new Vector2(250, -213));
        var toggle = Ensure<UnityEngine.UI.Toggle>(toggleObject);
        var toggleBackground = Image(toggleObject.transform, "Background", Button);
        Stretch(toggleBackground.rectTransform);
        toggleBackground.raycastTarget = true;
        toggleBackground.color = Color.white;
        var check = Image(toggleBackground.transform, "Checkmark", Accent);
        Rect(check.rectTransform, new Vector2(28, 16), Vector2.zero);
        toggle.targetGraphic = toggleBackground;
        toggle.graphic = check;
        toggle.toggleTransition = UnityEngine.UI.Toggle.ToggleTransition.None;
        toggle.colors = Colors();
        toggle.onValueChanged = new UnityEngine.UI.Toggle.ToggleEvent();
        UnityEventTools.AddPersistentListener(toggle.onValueChanged, view.ChangeFullscreen);

        var defaults = NewButton(settings.transform, "DefaultsButton", "По умолчанию", "Defaults", new Vector2(260, 54), new Vector2(-150, -291));
        Click(defaults, view.ResetDefaults);
        var back = NewButton(settings.transform, "BackButton", "Назад", "Back", new Vector2(260, 54), new Vector2(150, -291));
        Click(back, controller.ShowMainPage);

        Set(view, "_volume", volume);
        Set(view, "_brightness", brightness);
        Set(view, "_fullscreen", toggle);
        Set(view, "_volumeValue", volumeValue);
        Set(view, "_brightnessValue", brightnessValue);
        Set(view, "_languageValue", languageValue);
        volume.onValueChanged = new UnityEngine.UI.Slider.SliderEvent();
        brightness.onValueChanged = new UnityEngine.UI.Slider.SliderEvent();
        UnityEventTools.AddPersistentListener(volume.onValueChanged, view.ChangeVolume);
        UnityEventTools.AddPersistentListener(brightness.onValueChanged, view.ChangeBrightness);
        Set(controller, "_pauseCanvasGroup", group);
        Set(controller, "_mainPage", main);
        Set(controller, "_settingsPage", settings);
        Set(controller, "_resumeButton", resumeButton);
        Set(controller, "_firstSetting", volume);
        Set(controller, "_dimmer", dimmer);
        main.SetActive(true);
        settings.SetActive(false);
    }

    private static UnityEngine.UI.Slider Slider(Transform parent, string name, string ru, string en,
        float value, float min, float max, float y, out TextMeshProUGUI valueLabel)
    {
        var row = Child(parent, name + "Row");
        Rect(row.GetComponent<RectTransform>(), new Vector2(560, 94), new Vector2(0, y));
        Label(row.transform, "Label", ru, en, 25, new Vector2(440, 38), new Vector2(-60, 25), TextAlignmentOptions.MidlineLeft);
        valueLabel = Label(row.transform, "Value", "100%", "100%", 23, new Vector2(100, 38), new Vector2(230, 25), TextAlignmentOptions.MidlineRight);
        var localizedValue = valueLabel.GetComponent<PauseMenuText>();
        if (localizedValue != null) UnityEngine.Object.DestroyImmediate(localizedValue);
        valueLabel.color = Accent;
        var root = Child(row.transform, name + "Slider");
        Rect(root.GetComponent<RectTransform>(), new Vector2(560, 36), new Vector2(0, -20));
        var slider = Ensure<UnityEngine.UI.Slider>(root);
        var background = Image(root.transform, "Background", Hex("52465F"));
        Rect(background.rectTransform, new Vector2(560, 8), Vector2.zero);
        background.raycastTarget = true;
        var fillArea = Child(root.transform, "FillArea");
        Rect(fillArea.GetComponent<RectTransform>(), new Vector2(560, 8), Vector2.zero);
        var fill = Image(fillArea.transform, "Fill", Accent);
        Stretch(fill.rectTransform);
        var handleArea = Child(root.transform, "HandleArea");
        Stretch(handleArea.GetComponent<RectTransform>());
        var handle = Image(handleArea.transform, "Handle", Text);
        Rect(handle.rectTransform, new Vector2(14, -10), Vector2.zero);
        handle.raycastTarget = true;
        slider.fillRect = fill.rectTransform;
        slider.handleRect = handle.rectTransform;
        slider.targetGraphic = handle;
        slider.direction = UnityEngine.UI.Slider.Direction.LeftToRight;
        slider.minValue = min;
        slider.maxValue = max;
        slider.SetValueWithoutNotify(value);
        var colors = UnityEngine.UI.ColorBlock.defaultColorBlock;
        colors.highlightedColor = Accent;
        colors.selectedColor = Accent;
        colors.pressedColor = Color.white;
        colors.fadeDuration = 0.1f;
        slider.colors = colors;
        return slider;
    }

    private static UnityEngine.UI.Button NewButton(Transform parent, string name, string ru, string en, Vector2 size, Vector2 position)
    {
        var go = Child(parent, name);
        var button = StyleButton(go, ru, en, size);
        Rect(go.GetComponent<RectTransform>(), size, position);
        return button;
    }

    private static UnityEngine.UI.Button StyleButton(GameObject go, string ru, string en, Vector2 size)
    {
        go.transform.localScale = Vector3.one;
        var image = Ensure<UnityEngine.UI.Image>(go);
        image.sprite = null;
        image.type = UnityEngine.UI.Image.Type.Simple;
        image.color = Color.white;
        image.raycastTarget = true;
        var button = Ensure<UnityEngine.UI.Button>(go);
        button.targetGraphic = image;
        button.colors = Colors();
        button.transition = UnityEngine.UI.Selectable.Transition.ColorTint;
        var nav = button.navigation;
        nav.mode = UnityEngine.UI.Navigation.Mode.Automatic;
        button.navigation = nav;
        var element = Ensure<UnityEngine.UI.LayoutElement>(go);
        element.preferredWidth = size.x;
        element.preferredHeight = size.y;
        element.flexibleHeight = element.flexibleWidth = 0;
        var label = go.GetComponentInChildren<TextMeshProUGUI>(true);
        if (label == null) label = Label(go.transform, "Text", ru, en, 27, size, Vector2.zero);
        else ConfigureLabel(label, ru, en, 27, TextAlignmentOptions.Center);
        Stretch(label.rectTransform);
        label.rectTransform.offsetMin = new Vector2(16, 0);
        label.rectTransform.offsetMax = new Vector2(-16, 0);
        return button;
    }

    private static TextMeshProUGUI Label(Transform parent, string name, string ru, string en, float size, Vector2 dimensions, Vector2 position, TextAlignmentOptions alignment = TextAlignmentOptions.Center)
    {
        var go = Child(parent, name);
        Rect(go.GetComponent<RectTransform>(), dimensions, position);
        var label = Ensure<TextMeshProUGUI>(go);
        ConfigureLabel(label, ru, en, size, alignment);
        return label;
    }

    private static void ConfigureLabel(TextMeshProUGUI label, string ru, string en, float size, TextAlignmentOptions alignment)
    {
        label.font = _font;
        label.fontSize = size;
        label.enableAutoSizing = false;
        label.fontStyle = FontStyles.Normal;
        label.color = Text;
        label.alignment = alignment;
        label.margin = Vector4.zero;
        label.raycastTarget = false;
        label.text = ru;
        var localized = Ensure<PauseMenuText>(label.gameObject);
        var so = new SerializedObject(localized);
        so.FindProperty("_russian").stringValue = ru;
        so.FindProperty("_english").stringValue = en;
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    private static UnityEngine.UI.Image Image(Transform parent, string name, Color color)
    {
        var image = Ensure<UnityEngine.UI.Image>(Child(parent, name));
        image.sprite = null;
        image.color = color;
        image.raycastTarget = false;
        return image;
    }

    private static UnityEngine.UI.ColorBlock Colors()
    {
        var colors = UnityEngine.UI.ColorBlock.defaultColorBlock;
        colors.normalColor = Button;
        colors.highlightedColor = Hex("655475");
        colors.selectedColor = Hex("655475");
        colors.pressedColor = Hex("89718E");
        colors.disabledColor = Hex("3B3344");
        colors.fadeDuration = 0.1f;
        return colors;
    }

    private static void Click(UnityEngine.UI.Button button, UnityAction action)
    {
        button.onClick = new UnityEngine.UI.Button.ButtonClickedEvent();
        UnityEventTools.AddPersistentListener(button.onClick, action);
    }

    private static GameObject Child(Transform parent, string name)
    {
        var existing = parent.Find(name);
        if (existing != null) return existing.gameObject;
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return go;
    }

    private static T Ensure<T>(GameObject go) where T : Component => go.TryGetComponent<T>(out var value) ? value : go.AddComponent<T>();
    private static void Rect(RectTransform rect, Vector2 size, Vector2 position)
    {
        rect.localScale = Vector3.one;
        rect.localRotation = Quaternion.identity;
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = size;
        rect.anchoredPosition3D = new Vector3(position.x, position.y, 0);
    }

    private static void Stretch(RectTransform rect)
    {
        rect.localScale = Vector3.one;
        rect.localRotation = Quaternion.identity;
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.offsetMin = rect.offsetMax = Vector2.zero;
        var p = rect.localPosition;
        p.z = 0;
        rect.localPosition = p;
    }

    private static void Set(UnityEngine.Object target, string field, UnityEngine.Object value)
    {
        var so = new SerializedObject(target);
        so.FindProperty(field).objectReferenceValue = value;
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    private static Color Hex(string hex)
    {
        ColorUtility.TryParseHtmlString("#" + hex, out var color);
        return color;
    }
}
