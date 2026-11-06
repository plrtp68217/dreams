using TMPro;
using UnityEngine;

// The art uses the same fixed 1280x720 coordinate space as the fryer.
public class FastFoodDrinkCloseUp : MonoBehaviour
{
    [SerializeField] private FacelessOrderService _service;
    [SerializeField] private Player _player;
    [SerializeField] private GameObject _root;
    [SerializeField] private RectTransform _canvas;
    [SerializeField] private CanvasGroup _controls;
    [SerializeField] private UnityEngine.UI.Button[] _buttons;
    [SerializeField] private UnityEngine.UI.Image[] _flasks;
    [SerializeField] private Sprite[] _redLevels;
    [SerializeField] private Sprite[] _amberLevels;
    [SerializeField] private Sprite[] _greenLevels;
    [SerializeField] private UnityEngine.UI.Image _bay;
    [SerializeField] private Sprite[] _drinkFrames;
    [SerializeField] private Sprite _emptyBay;
    [SerializeField] private UnityEngine.UI.Image _drink;
    [SerializeField] private UnityEngine.UI.Image _outline;
    [SerializeField] private TextMeshProUGUI _recipeTitle;
    [SerializeField] private TextMeshProUGUI[] _recipeValues;
    [SerializeField] private TextMeshProUGUI[] _values;
    [SerializeField] private TextMeshProUGUI _total;
    [SerializeField] private TextMeshProUGUI _prepare;
    [SerializeField] private TextMeshProUGUI _hint;
    [SerializeField] private CanvasGroup _warning;
    [SerializeField] private TextMeshProUGUI _warningText;
    [SerializeField] private SmoothServiceNotice _warningNotice;
    [SerializeField] private SmoothServiceNotice _hintNotice;
    [SerializeField] private AudioSource _audio;
    [SerializeField] private AudioClip _pourSound;
    [SerializeField] private AudioClip _pickupSound;
    [SerializeField, Min(.1f)] private float _pourDuration = 3f;
    [SerializeField, Min(.1f)] private float _pickupDuration = .7f;
    [SerializeField, Min(.1f)] private float _warningDuration = 1.8f;

    private readonly int[] _selected = new int[3];
    private Vector3 _poured;
    private bool _pouring, _collecting;
    private float _elapsed, _warningRemaining;
    private int _openedFrame;
    private Vector2 _drinkOrigin;
    public bool IsVisible => _root != null && _root.activeSelf;
    public bool IsBusy => _pouring || _collecting;
    private bool CanControl => IsVisible && Time.timeScale > 0 && _player.IsAlive && _service.CanInteract;
    private bool CanMix => CanControl && !IsBusy && _service.CanPerform(FacelessServiceAction.PourDrink);
    private static string T(string key) => FacelessOrderService.Text("service_vending_" + key);

    private void Awake()
    {
        _drinkOrigin = _drink.rectTransform.anchoredPosition;
        for (int i = 0; i < 6; i++)
        {
            int index = i;
            _buttons[i].onClick.AddListener(() => Adjust(index / 2, index % 2 == 0 ? -10 : 10));
        }
        _buttons[6].onClick.AddListener(Prepare);
    }
    private void OnEnable() => LocalizedDialogueSystem.LanguageChanged += Localize;
    private void OnDisable()
    {
        LocalizedDialogueSystem.LanguageChanged -= Localize;
        Hide();
    }
    public void Show()
    {
        if (IsVisible) return;
        ResetTransient();
        _root.SetActive(true);
        _openedFrame = Time.frameCount;
        Localize();
        Render(false);
    }
    public void Hide()
    {
        ResetTransient();
        if (_root != null) _root.SetActive(false);
    }
    private void ResetTransient()
    {
        _pouring = _collecting = false;
        _elapsed = _warningRemaining = 0;
        if (_warningNotice != null) _warningNotice.Clear();
        else if (_warning != null) _warning.alpha = 0;
        if (_hintNotice != null) _hintNotice.Clear();
        else if (_hint != null) _hint.gameObject.SetActive(false);
        if (_outline != null) _outline.enabled = false;
        if (_drink != null)
        {
            _drink.rectTransform.anchoredPosition = _drinkOrigin;
            _drink.rectTransform.localScale = Vector3.one;
            _drink.rectTransform.localRotation = Quaternion.identity;
            _drink.color = Color.white;
        }
        if (_audio != null) _audio.Stop();
    }
    private void Localize()
    {
        if (_recipeTitle == null) return;
        _recipeTitle.text = T("recipe");
        _prepare.text = FacelessOrderService.Text("service_pour");
        if (_hintNotice == null) _hint.text = T("take");
        if (_warningNotice == null) _warningText.text = T("wrong");
        for (int i = 0; i < 3; i++) _recipeValues[i].text = Mathf.RoundToInt(_service.Recipe[i]) + "%";
        UpdateReadouts();
    }
    private void UpdateReadouts()
    {
        for (int i = 0; i < 3; i++) _values[i].text = _selected[i] + "%";
        _total.text = T("total") + "\n100%";
    }
    private void Adjust(int channel, int delta)
    {
        if (!CanMix) return;
        _selected[channel] = Mathf.Clamp(_selected[channel] + delta, 0, 100);
        _warningRemaining = 0;
        UpdateReadouts();
    }
    private void Prepare()
    {
        if (!CanMix) return;
        var values = new Vector3(_selected[0], _selected[1], _selected[2]);
        if (!_service.CorrectRecipe(values))
        {
            _warningRemaining = _warningDuration;
            if (_warningNotice == null) _warningText.text = T("wrong");
            return;
        }
        _poured = values;
        _warningRemaining = 0;
        _pouring = true;
        _elapsed = 0;
        Play(_pourSound);
    }
    private void Update()
    {
        if (!IsVisible) return;
        AdvanceAnimation(Time.deltaTime);
        ProcessInput(Input.mousePosition, Input.GetKeyDown(KeyCode.E));
    }
    private void ProcessInput(Vector2 screenPoint, bool interact)
    {
        bool overDrink = CanControl && !IsBusy && _service.CanPerform(FacelessServiceAction.TakeDrink) && HitDrink(screenPoint);
        if (overDrink && Time.frameCount > _openedFrame && interact) BeginPickup();
        Render(overDrink && !_collecting);
    }
    private void BeginPickup()
    {
        if (!CanControl || IsBusy || !_service.CanPerform(FacelessServiceAction.TakeDrink)) return;
        _collecting = true;
        _elapsed = 0;
    }
    private void AdvanceAnimation(float delta)
    {
        if (!CanControl) return;
        _warningRemaining = Mathf.Max(0, _warningRemaining - delta);
        if (!IsBusy) return;
        _elapsed += delta;
        if (_pouring && _elapsed >= _pourDuration)
        {
            // Keep IsBusy true while Changed callbacks run, so Refresh cannot
            // reopen/reset the modal during the completion event.
            _service.Perform(FacelessServiceAction.PourDrink);
            _pouring = false;
            if (_audio != null) _audio.Stop();
        }
        else if (_collecting && _elapsed >= _pickupDuration)
        {
            bool collected = _service.Perform(FacelessServiceAction.TakeDrink);
            _collecting = false;
            if (collected) Play(_pickupSound);
        }
    }
    private void Render(bool overDrink)
    {
        _controls.interactable = CanMix;
        _controls.blocksRaycasts = CanMix;
        _outline.enabled = overDrink;
        if (_hintNotice != null) _hintNotice.Show(overDrink ? T("take") : "");
        else _hint.gameObject.SetActive(overDrink);
        if (_warningNotice != null) _warningNotice.Show(_warningRemaining > 0 ? T("wrong") : "");
        else _warning.alpha = _warningRemaining > 0 ? Mathf.Clamp01(_warningRemaining / .22f) : 0;
        if (!CanControl)
        {
            if (_warningNotice == null) _warning.alpha = 0;
            if (_hintNotice == null) _hint.gameObject.SetActive(false);
            _outline.enabled = false;
        }
        float progress = _pouring ? Mathf.Clamp01(_elapsed / _pourDuration) : _service.DrinkReady ? 1 : 0;
        Vector3 percentages = _service.DrinkReady ? _poured : _pouring ? _poured : Vector3.zero;
        // The authored first twelve cels drain the flasks; later cels fill
        // the cup. Each flask stops independently at its selected percentage.
        float drainPercent = Mathf.Clamp01(progress * 22f / 12f) * 100;
        SetLevel(0, _redLevels, Mathf.Min(percentages.x, drainPercent));
        SetLevel(1, _amberLevels, Mathf.Min(percentages.y, drainPercent));
        SetLevel(2, _greenLevels, Mathf.Min(percentages.z, drainPercent));
        bool taken = _service.Inventory.Has(FastFoodOrderItem.Drink) || _service.IsCompleted;
        _bay.sprite = _collecting || taken ? _emptyBay : _drinkFrames[Mathf.Min(_drinkFrames.Length - 1, Mathf.FloorToInt(progress * _drinkFrames.Length))];
        _drink.enabled = _collecting;
        if (_collecting)
        {
            float t = Mathf.Clamp01(_elapsed / _pickupDuration);
            float ease = t * t * (3 - 2 * t);
            // Lift towards the player, then carry the glass down out of view.
            _drink.rectTransform.anchoredPosition = _drinkOrigin + new Vector2(-100 * ease, 110 * Mathf.Sin(t * Mathf.PI) - 230 * ease);
            _drink.rectTransform.localScale = Vector3.one * Mathf.Lerp(1, 1.35f, ease);
            _drink.rectTransform.localRotation = Quaternion.Euler(0, 0, -7 * Mathf.Sin(t * Mathf.PI));
            _drink.color = new Color(1, 1, 1, 1 - Mathf.SmoothStep(0, 1, Mathf.InverseLerp(.55f, 1, t)));
        }
    }
    private void SetLevel(int channel, Sprite[] levels, float percent)
        => _flasks[channel].sprite = levels[Mathf.Clamp(Mathf.RoundToInt(percent), 0, levels.Length - 1)];
    private bool HitDrink(Vector2 screenPoint)
    {
        var canvas = _canvas.GetComponentInParent<Canvas>();
        var camera = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(_drink.rectTransform, screenPoint, camera, out var local)) return false;
        var rect = _drink.rectTransform.rect;
        if (!rect.Contains(local)) return false;
        var sprite = _drink.sprite;
        int x = Mathf.Clamp(Mathf.FloorToInt((local.x - rect.xMin) / rect.width * sprite.rect.width), 0, (int)sprite.rect.width - 1);
        int y = Mathf.Clamp(Mathf.FloorToInt((local.y - rect.yMin) / rect.height * sprite.rect.height), 0, (int)sprite.rect.height - 1);
        return sprite.texture.GetPixel((int)sprite.rect.x + x, (int)sprite.rect.y + y).a > .1f;
    }
    private void Play(AudioClip clip)
    {
        if (_audio == null || clip == null) return;
        _audio.Stop(); _audio.clip = clip; _audio.Play();
    }
}
