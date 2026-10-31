using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.TextCore.LowLevel;
using UnityEngine.SceneManagement;

/// <summary>Creates Intro once. Does not rebuild an existing scene or save other open scenes.</summary>
public static class BuildIntro
{
    private const string ScenePath = "Assets/Scenes/Intro.unity";
    private const string UiPath = "Assets/UI/Intro/";
    private static TMP_FontAsset _font;
    private static Material _art;
    private static Texture2D _line;
    private static readonly List<Caption> Captions = new List<Caption>();

    private sealed class Caption
    {
        public TMP_Text Text;
        public string Ru;
        public string En;
    }

    private sealed class Reveal
    {
        public CanvasGroup Group;
        public float Delay;
        public float Float;
        public Reveal(CanvasGroup group, float delay, float drift = 0) { Group = group; Delay = delay; Float = drift; }
    }

    public static string Build()
    {
        if (Application.isPlaying) throw new InvalidOperationException("Stop Play Mode first.");
        if (File.Exists(ScenePath)) throw new InvalidOperationException("Intro already exists. Edit it in place.");
        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
        PrepareAssets();
        var previous = SceneManager.GetActiveScene();
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
        SceneManager.SetActiveScene(scene);
        try
        {
            Captions.Clear();
            var cameraObject = new GameObject("IntroCamera", typeof(Camera), typeof(AudioListener));
            cameraObject.tag = "MainCamera";
            var camera = cameraObject.GetComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = Color.black;
            camera.cullingMask = 0;
            camera.orthographic = true;
            cameraObject.transform.position = new Vector3(0, 0, -10);

            var root = new GameObject("IntroSequence", typeof(RectTransform), typeof(Canvas),
                typeof(UnityEngine.UI.CanvasScaler), typeof(UnityEngine.UI.GraphicRaycaster), typeof(AudioSource), typeof(IntroSequence));
            var canvas = root.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 1000;
            canvas.pixelPerfect = true;
            var scaler = root.GetComponent<UnityEngine.UI.CanvasScaler>();
            scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280, 720);
            scaler.matchWidthOrHeight = 0.5f;
            var sequence = root.GetComponent<IntroSequence>();
            var audio = root.GetComponent<AudioSource>();
            audio.clip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/Intro/IntroMusic.mp3");
            audio.playOnAwake = false;
            audio.loop = true;
            audio.spatialBlend = 0;
            audio.volume = 0.42f;

            var presentation = Rect("Presentation", root.transform, Vector2.zero, new Vector2(1280, 720));
            Stretch(presentation);
            var backdrop = Rect("Backdrop", presentation, Vector2.zero, Vector2.zero);
            Stretch(backdrop);
            var backdropImage = backdrop.gameObject.AddComponent<UnityEngine.UI.RawImage>();
            backdropImage.material = AssetDatabase.LoadAssetAtPath<Material>(UiPath + "Backdrop.mat");
            backdropImage.raycastTarget = false;
            var composition = Rect("Composition", presentation, Vector2.zero, new Vector2(1280, 720));

            var logo = Group("Studio", composition);
            var emblem = Group("Emblem", logo.transform);
            Artwork("StudioArtwork", emblem.transform, "Studio", new Vector2(0, 62), new Vector2(248.4f, 201.6f));
            var brand = Group("Brand", logo.transform);
            Text("StudioName", brand.transform, "SAITOVSKI", null, new Vector2(0, -83), new Vector2(580, 74), 58, 10);
            Text("Company", brand.transform, "COMPANY LIMITED", null, new Vector2(0, -132), new Vector2(500, 32), 19, 10);
            Line("LeftRule", brand.transform, new Vector2(-170, -132), new Vector2(70, 2));
            Line("RightRule", brand.transform, new Vector2(170, -132), new Vector2(70, 2));

            var headphones = Group("Headphones", composition);
            var moon = Group("Moon", headphones.transform);
            Artwork("MoonArtwork", moon.transform, "Moon", new Vector2(0, 151), new Vector2(1287, 267));
            var recommendation = Group("Recommendation", headphones.transform);
            Text("RecommendationText", recommendation.transform,
                "РЕКОМЕНДУЕТСЯ ИГРАТЬ\nВ НАУШНИКАХ", "HEADPHONES RECOMMENDED\nFOR THE BEST EXPERIENCE",
                new Vector2(0, -72), new Vector2(1180, 145), 54, 5);
            var phones = Group("HeadphonesArtwork", headphones.transform);
            Artwork("HeadphonesAndRules", phones.transform, "Headphones", new Vector2(0, -225), new Vector2(1287, 114));

            var warning = Group("Warning", composition);
            var heading = Group("Heading", warning.transform);
            Artwork("WarningIcon", heading.transform, "Warning", new Vector2(0, 257), new Vector2(102, 87));
            Text("WarningTitle", heading.transform, "ПРЕДУПРЕЖДЕНИЕ", "CONTENT WARNING", new Vector2(0, 165), new Vector2(800, 80), 54, 7);
            Line("LeftTitleRule", heading.transform, new Vector2(-393, 163), new Vector2(130, 2));
            Line("RightTitleRule", heading.transform, new Vector2(393, 163), new Vector2(130, 2));
            var atmosphere = Group("Atmosphere", warning.transform);
            Artwork("EyeIcon", atmosphere.transform, "Eye", new Vector2(-350, 40), new Vector2(72, 42));
            Divider("AtmosphereDivider", atmosphere.transform, new Vector2(-278, 37), new Vector2(2, 90));
            Text("AtmosphereText", atmosphere.transform,
                "Игра содержит тревожные образы,\nнапряжённую атмосферу и сцены\nпсихологического характера.",
                "This game contains disturbing imagery,\nan unsettling atmosphere and scenes\nof a psychological nature.",
                new Vector2(142, 38), new Vector2(760, 128), 32, 1, TextAlignmentOptions.MidlineLeft);
            var sensitivity = Group("Photosensitivity", warning.transform);
            Artwork("LightningIcon", sensitivity.transform, "Lightning", new Vector2(-350, -120), new Vector2(45, 72));
            Divider("SensitivityDivider", sensitivity.transform, new Vector2(-278, -120), new Vector2(2, 105));
            Text("PhotosensitivityText", sensitivity.transform,
                "В некоторых эпизодах присутствуют\nмерцающие эффекты и резкие изменения\nяркости, которые могут вызвать дискомфорт\nу людей с фоточувствительностью.",
                "Some sequences contain flashing effects\nand sudden changes in brightness,\nwhich may cause discomfort for people\nwith photosensitivity.",
                new Vector2(142, -119), new Vector2(760, 152), 31, 0.5f, TextAlignmentOptions.MidlineLeft);
            var footer = Group("Footer", warning.transform);
            Artwork("SmallMoon", footer.transform, "SmallMoon", new Vector2(0, -236), new Vector2(30, 36));
            Line("LeftFooterRule", footer.transform, new Vector2(-203, -236), new Vector2(330, 2));
            Line("RightFooterRule", footer.transform, new Vector2(203, -236), new Vector2(330, 2));

            var skip = Group("Skip", presentation);
            skip.blocksRaycasts = true;
            var skipRect = (RectTransform)skip.transform;
            skipRect.anchorMin = skipRect.anchorMax = new Vector2(1, 0);
            skipRect.pivot = new Vector2(1, 0);
            skipRect.anchoredPosition = new Vector2(-32, 24);
            skipRect.sizeDelta = new Vector2(360, 54);
            var skipText = Text("SkipText", skip.transform, "Esc / Пробел — пропустить", "Esc / Space — skip",
                Vector2.zero, new Vector2(360, 54), 23, 0, TextAlignmentOptions.MidlineRight);
            skipText.color = new Color(0.6f, 0.65f, 0.68f);
            skipText.raycastTarget = true;
            var button = skipText.gameObject.AddComponent<UnityEngine.UI.Button>();
            button.targetGraphic = skipText;
            var colors = button.colors;
            colors.highlightedColor = new Color(1.25f, 1.25f, 1.25f);
            colors.pressedColor = Color.white;
            button.colors = colors;
            button.navigation = new UnityEngine.UI.Navigation { mode = UnityEngine.UI.Navigation.Mode.None };
            UnityEventTools.AddPersistentListener(button.onClick, sequence.Skip);

            var blackout = Group("Blackout", root.transform);
            Stretch((RectTransform)blackout.transform);
            var black = blackout.gameObject.AddComponent<UnityEngine.UI.Image>();
            black.color = Color.black;
            black.raycastTarget = true;
            blackout.alpha = 1;

            new GameObject("IntroEventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
            var fields = new SerializedObject(sequence);
            SetReference(fields, "_music", audio);
            SetReference(fields, "_skipHint", skip);
            SetReference(fields, "_blackout", blackout);
            SetReference(fields, "_presentation", presentation.gameObject);
            SetReference(fields, "_composition", composition);
            var cards = fields.FindProperty("_cards");
            cards.arraySize = 3;
            Card(cards.GetArrayElementAtIndex(0), logo, 4.5f, new Reveal(emblem, 0, 1.5f), new Reveal(brand, 0.65f));
            Card(cards.GetArrayElementAtIndex(1), headphones, 5.5f, new Reveal(moon, 0, 2.5f), new Reveal(recommendation, 0.55f), new Reveal(phones, 1.05f));
            Card(cards.GetArrayElementAtIndex(2), warning, 12f, new Reveal(heading, 0), new Reveal(atmosphere, 0.4f), new Reveal(sensitivity, 0.8f), new Reveal(footer, 1.1f));
            var captions = fields.FindProperty("_captions");
            captions.arraySize = Captions.Count;
            for (int i = 0; i < Captions.Count; i++)
            {
                var item = captions.GetArrayElementAtIndex(i);
                item.FindPropertyRelative("Text").objectReferenceValue = Captions[i].Text;
                item.FindPropertyRelative("Russian").stringValue = Captions[i].Ru;
                item.FindPropertyRelative("English").stringValue = Captions[i].En;
            }
            fields.ApplyModifiedPropertiesWithoutUndo();
            // Readable logo preview in Edit Mode; Awake initializes every card for playback.
            logo.alpha = 1;
            headphones.gameObject.SetActive(false);
            warning.gameObject.SetActive(false);
            skip.alpha = 0;
            blackout.alpha = 0;
            EditorSceneManager.SaveScene(scene, ScenePath);
            var existing = EditorBuildSettings.scenes.Where(s => s.path != ScenePath).ToList();
            existing.Insert(0, new EditorBuildSettingsScene(ScenePath, true));
            EditorBuildSettings.scenes = existing.ToArray();
            AssetDatabase.SaveAssets();
            return "Created Intro: Studio 4.5 s / Headphones 5.5 s / Warning 12 s, plus fades. Music: " + audio.clip.length + " s. Build order: " + string.Join(", ", existing.Select(s => s.path));
        }
        finally
        {
            SceneManager.SetActiveScene(previous);
            EditorSceneManager.CloseScene(scene, true);
        }
    }

    private static void PrepareAssets()
    {
        foreach (var path in AssetDatabase.FindAssets("t:Texture2D", new[] { "Assets/Sprites/Intro" }).Select(AssetDatabase.GUIDToAssetPath))
        {
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Default;
            importer.mipmapEnabled = false;
            importer.filterMode = FilterMode.Point;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.npotScale = TextureImporterNPOTScale.None;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.SaveAndReimport();
        }
        var audio = (AudioImporter)AssetImporter.GetAtPath("Assets/Audio/Intro/IntroMusic.mp3");
        var sample = audio.defaultSampleSettings;
        sample.loadType = AudioClipLoadType.Streaming;
        sample.compressionFormat = AudioCompressionFormat.Vorbis;
        sample.quality = 0.8f;
        audio.defaultSampleSettings = sample;
        audio.SaveAndReimport();
        _font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(UiPath + "IntroSerif.asset");
        if (_font == null)
        {
            var source = AssetDatabase.LoadAssetAtPath<Font>(UiPath + "OldStandard-Regular.ttf");
            _font = TMP_FontAsset.CreateFontAsset(source, 64, 6, GlyphRenderMode.SDFAA, 1024, 1024, AtlasPopulationMode.Dynamic);
            _font.name = "IntroSerif";
            string characters = string.Concat(Enumerable.Range(32, 95).Select(i => (char)i)) +
                string.Concat(Enumerable.Range(0x410, 64).Select(i => (char)i)) + "Ёё—–…";
            if (!_font.TryAddCharacters(characters, out string missing)) throw new Exception("Missing intro font characters: " + missing);
            _font.atlasPopulationMode = AtlasPopulationMode.Static;
            AssetDatabase.CreateAsset(_font, UiPath + "IntroSerif.asset");
            AssetDatabase.AddObjectToAsset(_font.material, _font);
            foreach (var atlas in _font.atlasTextures) AssetDatabase.AddObjectToAsset(atlas, _font);
            EditorUtility.SetDirty(_font);
        }
        _art = Material("Artwork", "DREAMS/Intro/Artwork");
        Material("Backdrop", "DREAMS/Intro/Backdrop");
        _line = AssetDatabase.LoadAssetAtPath<Texture2D>(UiPath + "Rule.asset");
        if (_line == null)
        {
            _line = new Texture2D(128, 1, TextureFormat.RGBA32, false) { name = "Rule", wrapMode = TextureWrapMode.Clamp, filterMode = FilterMode.Bilinear };
            for (int x = 0; x < 128; x++) _line.SetPixel(x, 0, new Color(0.7f, 0.76f, 0.8f, Mathf.Pow(Mathf.Sin(x / 127f * Mathf.PI), 1.3f) * 0.65f));
            _line.Apply();
            AssetDatabase.CreateAsset(_line, UiPath + "Rule.asset");
        }
    }

    private static Material Material(string name, string shaderName)
    {
        var result = AssetDatabase.LoadAssetAtPath<Material>(UiPath + name + ".mat");
        if (result != null) return result;
        var shader = Shader.Find(shaderName);
        if (shader == null) throw new Exception("Missing shader: " + shaderName);
        result = new Material(shader) { name = name };
        AssetDatabase.CreateAsset(result, UiPath + name + ".mat");
        return result;
    }

    private static void SetReference(SerializedObject target, string field, UnityEngine.Object value) => target.FindProperty(field).objectReferenceValue = value;

    private static void Card(SerializedProperty card, CanvasGroup group, float duration, params Reveal[] reveals)
    {
        card.FindPropertyRelative("Group").objectReferenceValue = group;
        card.FindPropertyRelative("Duration").floatValue = duration;
        var elements = card.FindPropertyRelative("Elements");
        elements.arraySize = reveals.Length;
        for (int i = 0; i < reveals.Length; i++)
        {
            var element = elements.GetArrayElementAtIndex(i);
            element.FindPropertyRelative("Group").objectReferenceValue = reveals[i].Group;
            element.FindPropertyRelative("Delay").floatValue = reveals[i].Delay;
            element.FindPropertyRelative("RevealDuration").floatValue = 1.15f;
            element.FindPropertyRelative("EnterOffset").vector2Value = new Vector2(0, -9);
            element.FindPropertyRelative("EnterScale").floatValue = 0.985f;
            element.FindPropertyRelative("FloatAmplitude").floatValue = reveals[i].Float;
        }
    }

    private static RectTransform Rect(string name, Transform parent, Vector2 position, Vector2 size)
    {
        var rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
        return rect;
    }

    private static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = rect.offsetMax = Vector2.zero;
    }

    private static CanvasGroup Group(string name, Transform parent)
    {
        var rect = Rect(name, parent, Vector2.zero, new Vector2(1280, 720));
        var group = rect.gameObject.AddComponent<CanvasGroup>();
        group.blocksRaycasts = false;
        return group;
    }

    private static TMP_Text Text(string name, Transform parent, string ru, string en, Vector2 position, Vector2 size,
        float fontSize, float spacing, TextAlignmentOptions alignment = TextAlignmentOptions.Center)
    {
        var rect = Rect(name, parent, position, size);
        var text = rect.gameObject.AddComponent<TextMeshProUGUI>();
        text.font = _font;
        text.fontSize = fontSize;
        text.characterSpacing = spacing;
        text.lineSpacing = -6;
        text.text = ru;
        text.color = new Color(0.68f, 0.73f, 0.75f);
        text.alignment = alignment;
        text.textWrappingMode = TextWrappingModes.NoWrap;
        text.overflowMode = TextOverflowModes.Overflow;
        text.raycastTarget = false;
        if (en != null) Captions.Add(new Caption { Text = text, Ru = ru, En = en });
        return text;
    }

    private static void Artwork(string name, Transform parent, string asset, Vector2 position, Vector2 size)
    {
        var image = Rect(name, parent, position, size).gameObject.AddComponent<UnityEngine.UI.RawImage>();
        image.texture = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Sprites/Intro/" + asset + ".png");
        image.material = _art;
        image.raycastTarget = false;
    }

    private static void Line(string name, Transform parent, Vector2 position, Vector2 size)
    {
        var image = Rect(name, parent, position, size).gameObject.AddComponent<UnityEngine.UI.RawImage>();
        image.texture = _line;
        image.raycastTarget = false;
    }

    private static void Divider(string name, Transform parent, Vector2 position, Vector2 size)
    {
        var image = Rect(name, parent, position, size).gameObject.AddComponent<UnityEngine.UI.Image>();
        image.color = new Color(0.4f, 0.48f, 0.5f, 0.5f);
        image.raycastTarget = false;
    }
}
