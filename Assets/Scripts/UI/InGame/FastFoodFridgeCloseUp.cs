using TMPro;
using UnityEngine;

public class FastFoodFridgeCloseUp : MonoBehaviour
{
    [SerializeField] private FacelessOrderService _service;
    [SerializeField] private Player _player;
    [SerializeField] private GameObject _root;
    [SerializeField] private CanvasGroup _controls;
    [SerializeField] private UnityEngine.UI.Button[] _digits;
    [SerializeField] private UnityEngine.UI.Button _backspace;
    [SerializeField] private TextMeshProUGUI _display;
    [SerializeField] private UnityEngine.UI.Image _doors;
    [SerializeField] private Sprite[] _doorFrames;
    [SerializeField] private UnityEngine.UI.Image _fries;
    [SerializeField] private UnityEngine.UI.Image _pickup;
    [SerializeField] private UnityEngine.UI.Image _outline;
    [SerializeField] private SmoothServiceNotice _hint;
    [SerializeField] private SmoothServiceNotice _warning;
    [SerializeField] private AudioSource _audio;
    [SerializeField] private AudioClip _openSound;
    [SerializeField] private AudioClip _pickupSound;
    [SerializeField, Min(.1f)] private float _openingDuration = 1.7f;
    [SerializeField, Min(.1f)] private float _pickupDuration = .7f;
    [SerializeField, Min(.1f)] private float _warningDuration = 2f;
    private string _entered = "";
    private bool _opening, _collecting;
    private float _elapsed, _warningRemaining;
    private int _openedFrame;
    private Vector2 _pickupOrigin;
    public bool IsVisible => _root != null && _root.activeSelf;
    public bool IsBusy => _opening || _collecting;
    private bool CanControl => IsVisible && Time.timeScale > 0 && _service.CanInteract && _player.IsAlive;
    private bool CanEnter => CanControl && !IsBusy && !_service.FridgeUnlocked;
    private static string T(string key) => FacelessOrderService.Text("service_fridge_" + key);

    private void Awake()
    {
        _pickupOrigin = _pickup.rectTransform.anchoredPosition;
        for (int i = 0; i < _digits.Length; i++)
        {
            int digit = i;
            _digits[i].onClick.AddListener(() => EnterDigit(digit));
        }
        _backspace.onClick.AddListener(Backspace);
    }
    public void Show()
    {
        if (IsVisible) return;
        ResetTransient();
        _root.SetActive(true);
        _openedFrame = Time.frameCount;
        Render(false);
    }
    public void Hide()
    {
        ResetTransient();
        if (_root != null) _root.SetActive(false);
    }
    private void ResetTransient()
    {
        _opening = _collecting = false;
        _elapsed = _warningRemaining = 0;
        _entered = "";
        if (_warning != null) _warning.Clear();
        if (_hint != null) _hint.Clear();
        if (_outline != null) _outline.enabled = false;
        if (_pickup != null)
        {
            _pickup.rectTransform.anchoredPosition = _pickupOrigin;
            _pickup.rectTransform.localScale = Vector3.one;
            _pickup.rectTransform.localRotation = Quaternion.identity;
            _pickup.color = Color.white;
        }
        if (_audio != null) _audio.Stop();
    }
    private void EnterDigit(int digit)
    {
        if (!CanEnter || digit < 0 || digit > 9 || _entered.Length >= _service.FridgeCode.Length) return;
        _warningRemaining = 0;
        _entered += digit.ToString();
        if (_entered.Length == _service.FridgeCode.Length) Submit();
    }
    private void Backspace()
    {
        if (!CanEnter) return;
        if (_entered.Length > 0) _entered = _entered.Substring(0, _entered.Length - 1);
        _warningRemaining = 0;
    }
    private void Submit()
    {
        if (!CanEnter || _entered.Length == 0) return;
        if (_entered != _service.FridgeCode)
        {
            _entered = "";
            _warningRemaining = _warningDuration;
            return;
        }
        _warningRemaining = 0;
        _opening = true;
        _elapsed = 0;
        Play(_openSound);
    }
    private void Update()
    {
        if (!IsVisible) return;
        AdvanceAnimation(Time.deltaTime);
        if (CanEnter)
        {
            for (int i = 0; i < 10; i++)
                if (Input.GetKeyDown(KeyCode.Alpha0 + i) || Input.GetKeyDown(KeyCode.Keypad0 + i)) EnterDigit(i);
            if (Input.GetKeyDown(KeyCode.Backspace)) Backspace();
            if (CanEnter && Input.GetKeyDown(KeyCode.Delete)) { _entered = ""; _warningRemaining = 0; }
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)) Submit();
        }
        ProcessInput(Input.mousePosition, Input.GetKeyDown(KeyCode.E));
    }
    private void ProcessInput(Vector2 screenPoint, bool interact)
    {
        bool hover = CanControl && !IsBusy && _service.CanPerform(FacelessServiceAction.TakeFries) && HitFries(screenPoint);
        if (hover && interact && Time.frameCount > _openedFrame)
        {
            _collecting = true;
            _elapsed = 0;
        }
        Render(hover && !_collecting);
    }
    private void AdvanceAnimation(float delta)
    {
        if (!CanControl) return;
        _warningRemaining = Mathf.Max(0, _warningRemaining - delta);
        if (!IsBusy) return;
        _elapsed += delta;
        if (_opening && _elapsed >= _openingDuration)
        {
            _service.EnterFridgeCode(_entered);
            _opening = false;
            if (_audio != null) _audio.Stop();
        }
        else if (_collecting && _elapsed >= _pickupDuration)
        {
            bool collected = _service.Perform(FacelessServiceAction.TakeFries);
            _collecting = false;
            if (collected) Play(_pickupSound);
        }
    }
    private void Render(bool hover)
    {
        _controls.interactable = _controls.blocksRaycasts = CanEnter;
        _display.text = _entered.PadRight(_service.FridgeCode.Length, '_');
        _warning.Show(_warningRemaining > 0 ? T("wrong") : "");
        _hint.Show(hover ? T("take") : "");
        _outline.enabled = hover;
        int frame = _service.FridgeUnlocked ? _doorFrames.Length - 1 : _opening ?
            Mathf.Min(_doorFrames.Length - 1, Mathf.FloorToInt(_elapsed / _openingDuration * _doorFrames.Length)) : 0;
        _doors.sprite = _doorFrames[frame];
        bool taken = _service.Inventory.Has(FastFoodOrderItem.Fries) || _service.IsCompleted;
        _fries.enabled = !taken && !_collecting;
        _pickup.enabled = _collecting;
        if (_collecting)
        {
            float t = Mathf.Clamp01(_elapsed / _pickupDuration);
            float ease = Mathf.SmoothStep(0, 1, t);
            _pickup.rectTransform.anchoredPosition = _pickupOrigin + new Vector2(130 * ease, 110 * Mathf.Sin(t * Mathf.PI) - 340 * ease);
            _pickup.rectTransform.localScale = Vector3.one * Mathf.Lerp(1, 1.8f, ease);
            _pickup.rectTransform.localRotation = Quaternion.Euler(0, 0, 8 * Mathf.Sin(t * Mathf.PI));
            _pickup.color = new Color(1, 1, 1, 1 - Mathf.SmoothStep(0, 1, Mathf.InverseLerp(.55f, 1, t)));
        }
    }
    private bool HitFries(Vector2 screenPoint)
    {
        var canvas = _fries.GetComponentInParent<Canvas>();
        var camera = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(_fries.rectTransform, screenPoint, camera, out var point)) return false;
        var rect = _fries.rectTransform.rect;
        if (!rect.Contains(point)) return false;
        var sprite = _fries.sprite;
        int x = Mathf.Clamp((int)((point.x - rect.xMin) / rect.width * sprite.rect.width), 0, (int)sprite.rect.width - 1);
        int y = Mathf.Clamp((int)((point.y - rect.yMin) / rect.height * sprite.rect.height), 0, (int)sprite.rect.height - 1);
        return sprite.texture.GetPixel((int)sprite.rect.x + x, (int)sprite.rect.y + y).a > .1f;
    }
    private void OnDisable() => Hide();
    private void Play(AudioClip clip)
    {
        if (_audio == null || clip == null) return;
        _audio.Stop(); _audio.clip = clip; _audio.Play();
    }
}
