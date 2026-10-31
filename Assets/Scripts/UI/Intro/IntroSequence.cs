using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>The startup cards, their staggered reveals, and the fade into the main menu.</summary>
public sealed class IntroSequence : MonoBehaviour
{
    [Serializable]
    private sealed class Element
    {
        public CanvasGroup Group;
        [Min(0)] public float Delay;
        [Min(0.01f)] public float RevealDuration = 1f;
        public Vector2 EnterOffset = new Vector2(0, -8);
        public float EnterScale = 0.98f;
        public float FloatAmplitude;
        [NonSerialized] public Vector2 Position;
        [NonSerialized] public Vector3 Scale;
    }

    [Serializable]
    private sealed class Card
    {
        public CanvasGroup Group;
        [Min(1)] public float Duration = 5f;
        public Element[] Elements;
    }

    [Serializable]
    private sealed class Caption
    {
        public TMP_Text Text;
        [TextArea] public string Russian;
        [TextArea] public string English;
    }

    [SerializeField] private Card[] _cards;
    [SerializeField] private Caption[] _captions;
    [SerializeField] private AudioSource _music;
    [SerializeField, Range(0, 1)] private float _musicVolume = 0.42f;
    [SerializeField] private CanvasGroup _skipHint;
    [SerializeField] private CanvasGroup _blackout;
    [SerializeField] private GameObject _presentation;
    [SerializeField] private RectTransform _composition;
    [SerializeField, Min(0.01f)] private float _cardFadeOut = 0.8f;
    [SerializeField, Min(0.01f)] private float _menuFadeIn = 0.8f;

    private float _elapsed;
    private bool _skipRequested;
    private bool _loading;

    private void Awake()
    {
        foreach (var card in _cards)
        {
            card.Group.alpha = 0;
            card.Group.gameObject.SetActive(false);
            foreach (var element in card.Elements)
            {
                var rect = (RectTransform)element.Group.transform;
                element.Position = rect.anchoredPosition;
                element.Scale = rect.localScale;
            }
        }
        _blackout.alpha = 1;
        _skipHint.alpha = 0;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        RefreshLanguage();
        FitComposition();
    }

    private void OnEnable() => LocalizedDialogueSystem.LanguageChanged += RefreshLanguage;
    private void OnDisable() => LocalizedDialogueSystem.LanguageChanged -= RefreshLanguage;

    private void Update()
    {
        FitComposition();
        _elapsed += Time.unscaledDeltaTime;
        if (!_loading && _elapsed > 0.6f &&
            (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return)))
            Skip();
    }

    public void Skip()
    {
        if (_elapsed > 0.6f && !_loading) _skipRequested = true;
    }

    private IEnumerator Start()
    {
        _music.volume = 0;
        _music.Play();
        yield return FadeBlack(0, 0.4f);
        foreach (var card in _cards)
        {
            if (_skipRequested) break;
            card.Group.gameObject.SetActive(true);
            card.Group.alpha = 1;
            float time = 0;
            while (time < card.Duration && !_skipRequested)
            {
                Animate(card, time);
                time += Time.unscaledDeltaTime;
                _music.volume = _musicVolume * Ease(Mathf.Clamp01(_elapsed / 2f));
                _skipHint.alpha = 0.65f * Ease(Mathf.Clamp01((_elapsed - 2f) / 1.2f));
                yield return null;
            }
            if (_skipRequested) break;
            float fade = 0;
            while (fade < _cardFadeOut && !_skipRequested)
            {
                fade += Time.unscaledDeltaTime;
                card.Group.alpha = 1 - Ease(Mathf.Clamp01(fade / _cardFadeOut));
                yield return null;
            }
            if (_skipRequested) break;
            card.Group.gameObject.SetActive(false);
            yield return new WaitForSecondsRealtime(0.2f);
        }
        yield return OpenMenu();
    }

    private static void Animate(Card card, float time)
    {
        foreach (var element in card.Elements)
        {
            float reveal = Mathf.Clamp01((time - element.Delay) / element.RevealDuration);
            float eased = Ease(reveal);
            element.Group.alpha = eased;
            var rect = (RectTransform)element.Group.transform;
            float drift = Mathf.Sin(Mathf.Max(0, time - element.Delay) * 0.8f) * element.FloatAmplitude * eased;
            rect.anchoredPosition = element.Position + element.EnterOffset * (1 - eased) + Vector2.up * drift;
            rect.localScale = element.Scale * Mathf.Lerp(element.EnterScale, 1, eased);
        }
    }

    private IEnumerator OpenMenu()
    {
        _loading = true;
        _skipHint.interactable = false;
        _blackout.blocksRaycasts = true;
        float volume = _music.volume;
        float hintAlpha = _skipHint.alpha;
        float startAlpha = _blackout.alpha;
        float time = 0;
        while (time < _cardFadeOut)
        {
            time += Time.unscaledDeltaTime;
            float amount = Ease(Mathf.Clamp01(time / _cardFadeOut));
            _blackout.alpha = Mathf.Lerp(startAlpha, 1, amount);
            _music.volume = volume * (1 - amount);
            _skipHint.alpha = hintAlpha * (1 - amount);
            yield return null;
        }
        _music.Stop();
        foreach (var card in _cards) card.Group.gameObject.SetActive(false);
        _presentation.SetActive(false);
        // Only the overlay and its controller survive. The intro camera/listener stay in the scene.
        DontDestroyOnLoad(gameObject);
        yield return SceneManager.LoadSceneAsync(SceneName.MainMenu.ToString());
        yield return FadeBlack(0, _menuFadeIn);
        Destroy(gameObject);
    }

    private IEnumerator FadeBlack(float target, float duration)
    {
        float initial = _blackout.alpha;
        float time = 0;
        while (time < duration)
        {
            time += Time.unscaledDeltaTime;
            _blackout.alpha = Mathf.Lerp(initial, target, Ease(Mathf.Clamp01(time / duration)));
            yield return null;
        }
        _blackout.alpha = target;
        _blackout.blocksRaycasts = target > 0;
    }

    private void RefreshLanguage()
    {
        bool english = LocalizedDialogueSystem.CurrentLanguage == Language.English;
        foreach (var caption in _captions)
            if (caption.Text != null) caption.Text.text = english ? caption.English : caption.Russian;
    }

    private void FitComposition()
    {
        var canvasRect = (RectTransform)_composition.parent;
        float scale = Mathf.Min(canvasRect.rect.width / 1280f, canvasRect.rect.height / 720f);
        _composition.localScale = Vector3.one * scale;
    }

    private static float Ease(float value) => value * value * (3 - 2 * value);
}
