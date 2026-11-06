using TMPro;
using UnityEngine;

// Coordinates and drag distances use the original 320x180 Aseprite canvas.
// Progress is owned by FacelessOrderService and survives closing this view.
public class FastFoodFryerCloseUp : MonoBehaviour
{
    [SerializeField] private FacelessOrderService _service;
    [SerializeField] private Player _player;
    [SerializeField] private GameObject _root;
    [SerializeField] private RectTransform _canvas;
    [SerializeField] private UnityEngine.UI.Image _foreground;
    [SerializeField] private UnityEngine.UI.Image _switchOff;
    [SerializeField] private UnityEngine.UI.Image _switchOutline;
    [SerializeField] private UnityEngine.UI.Image _burger;
    [SerializeField] private UnityEngine.UI.Image _burgerOutline;
    [SerializeField] private UnityEngine.UI.Image _tumbler;
    [SerializeField] private UnityEngine.UI.Image _tumblerOutline;
    [SerializeField] private Sprite _switchHit;
    [SerializeField] private Sprite[] _drainFrames;
    [SerializeField] private Sprite _idleForeground;
    [SerializeField] private Sprite[] _switchFrames;
    [SerializeField] private TextMeshProUGUI _hint;
    [SerializeField] private SmoothServiceNotice _hintNotice;
    [SerializeField] private AudioSource _audio;
    [SerializeField] private AudioClip _drainSound;
    [SerializeField] private AudioClip _pickupSound;
    [SerializeField, Min(1)] private float _pullDistance = 28;
    [SerializeField, Min(.1f)] private float _flightDuration = .65f;
    [SerializeField, Min(.1f)] private float _drainDuration = 2.1f;
    [SerializeField, Min(.1f)] private float _switchDuration = .3f;
    [SerializeField, Min(.1f)] private float _burgerFadeDuration = .45f;

    private readonly Vector2 _origin = new Vector2(285, 108);
    private Vector2 _grabPoint, _offset, _releaseOffset;
    private bool _dragging, _flying, _draining, _switching, _collecting, _audioPaused;
    private float _elapsed, _warningUntil;
    private int _openedFrame;
    public bool IsVisible => _root != null && _root.activeSelf;
    public bool IsBusy => _dragging || _flying || _draining || _switching || _collecting;

    public void Show()
    {
        _root.SetActive(true);
        ResetTransient();
        _openedFrame = Time.frameCount;
        Render(new Vector2(-1000, -1000));
    }
    public void Hide()
    {
        ResetTransient();
        if (_root != null) _root.SetActive(false);
    }
    private void ResetTransient()
    {
        _dragging = _flying = _draining = _switching = _collecting = _audioPaused = false;
        _elapsed = _warningUntil = 0;
        if (_hintNotice != null) _hintNotice.Clear();
        _offset = Vector2.zero;
        if (_tumbler != null) _tumbler.rectTransform.localRotation = Quaternion.identity;
        if (_burger != null) SetBurgerAlpha(1);
        if (_audio != null) _audio.Stop();
    }
    private bool CanControl => IsVisible && _service.CanInteract && _player.IsAlive && Time.timeScale > 0;
    private static string T(string key) => FacelessOrderService.Text("service_fryer_" + key);

    private void Update()
    {
        if (!IsVisible) return;
        if (!CanControl)
        {
            CancelDrag();
            SetOutlines(false, false, false);
            if (_audio != null && _audio.isPlaying) { _audio.Pause(); _audioPaused = true; }
            return;
        }
        if (_audioPaused && _audio != null) { _audio.UnPause(); _audioPaused = false; }
        AdvanceAnimation(Time.deltaTime);
        Vector2 local;
        var canvas = _canvas.GetComponentInParent<Canvas>();
        var camera = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(_canvas, Input.mousePosition, camera, out local);
        var rect = _canvas.rect;
        var point = new Vector2((local.x - rect.xMin) / rect.width * 320,
            (rect.yMax - local.y) / rect.height * 180);
        ProcessInput(point, Input.GetMouseButtonDown(0), Input.GetMouseButton(0),
            Input.GetMouseButtonUp(0), Time.frameCount > _openedFrame && Input.GetKeyDown(KeyCode.E));
        Render(point);
    }

    private void ProcessInput(Vector2 point, bool down, bool held, bool up, bool interact)
    {
        if (!CanControl || _flying || _draining || _switching || _collecting) return;
        bool onTumbler = !_service.FuseRemoved && Hit(_tumbler.sprite, point - _origin - _offset + new Vector2(8, 18));
        if (_dragging)
        {
            _offset = Vector2.ClampMagnitude(point - _grabPoint, _pullDistance + 10);
            if (up)
            {
                _dragging = false;
                if (_offset.magnitude >= _pullDistance)
                {
                    _flying = true;
                    _releaseOffset = _offset;
                    _elapsed = 0;
                }
                else _offset = Vector2.zero;
            }
            else if (!held) CancelDrag();
            return;
        }
        if (down && onTumbler)
        {
            if (!_service.PowerOff) _warningUntil = Time.time + 3;
            else { _dragging = true; _grabPoint = point; _warningUntil = 0; }
            return;
        }
        if (down && !_service.PowerOff && Hit(_switchHit, point))
        {
            _switching = true;
            _elapsed = 0;
            _warningUntil = 0;
            return;
        }
        // A mouse release and E in the same frame cannot skip the flight.
        if (interact && _service.CanPerform(FacelessServiceAction.DrainOil))
        {
            _draining = true;
            _elapsed = 0;
            Play(_drainSound);
        }
        else if (interact && _service.CanPerform(FacelessServiceAction.TakeBurger) && Hit(_burger.sprite, point))
        {
            _collecting = true;
            _elapsed = 0;
        }
    }

    private void AdvanceAnimation(float delta)
    {
        if (!_flying && !_draining && !_switching && !_collecting) return;
        _elapsed += delta;
        if (_switching)
        {
            if (_elapsed >= _switchDuration)
            {
                _switching = false;
                _service.Perform(FacelessServiceAction.PowerOff);
            }
        }
        else if (_collecting)
        {
            if (_elapsed >= _burgerFadeDuration)
            {
                _collecting = false;
                if (_service.Perform(FacelessServiceAction.TakeBurger)) Play(_pickupSound);
            }
        }
        else if (_flying)
        {
            float t = Mathf.Clamp01(_elapsed / _flightDuration);
            _offset = _releaseOffset + new Vector2(130 * t, -95 * t - 35 * Mathf.Sin(t * Mathf.PI));
            _tumbler.rectTransform.localRotation = Quaternion.Euler(0, 0, -500 * t);
            if (t >= 1)
            {
                _flying = false;
                _service.Perform(FacelessServiceAction.RemoveFuse);
            }
        }
        else if (_elapsed >= _drainDuration)
        {
            _draining = false;
            _service.Perform(FacelessServiceAction.DrainOil);
            if (_audio != null) _audio.Stop();
        }
    }

    private void Render(Vector2 point)
    {
        _switchOff.enabled = _switching || _service.PowerOff;
        if (_switchFrames != null && _switchFrames.Length > 0)
        {
            int switchFrame = _switching ? Mathf.Min(_switchFrames.Length - 1,
                (int)(_elapsed / _switchDuration * _switchFrames.Length)) : _service.PowerOff ? _switchFrames.Length - 1 : 0;
            _switchOff.sprite = _switchFrames[switchFrame];
        }
        _burger.enabled = !_service.Inventory.Has(FastFoodOrderItem.Burger);
        SetBurgerAlpha(_collecting ? 1 - Mathf.SmoothStep(0, 1, Mathf.Clamp01(_elapsed / _burgerFadeDuration)) : 1);
        _tumbler.enabled = !_service.FuseRemoved;
        var position = _origin + _offset;
        _tumbler.rectTransform.anchoredPosition = new Vector2(position.x * 4 - 640, 360 - position.y * 4);
        _tumblerOutline.rectTransform.anchoredPosition = _tumbler.rectTransform.anchoredPosition;
        int frame = _service.OilDrained ? _drainFrames.Length - 1 :
            _draining ? Mathf.Min(_drainFrames.Length - 1, (int)(_elapsed / _drainDuration * _drainFrames.Length)) : 0;
        _foreground.sprite = !_draining && !_service.OilDrained && _idleForeground != null ? _idleForeground : _drainFrames[frame];
        bool onSwitch = !IsBusy && !_service.PowerOff && Hit(_switchHit, point);
        bool onTumbler = (_dragging || !IsBusy) && !_service.FuseRemoved && Hit(_tumbler.sprite, point - _origin - _offset + new Vector2(8, 18));
        bool onBurger = !IsBusy && _service.CanPerform(FacelessServiceAction.TakeBurger) && Hit(_burger.sprite, point);
        SetOutlines(onSwitch, onTumbler, onBurger);
        string hint = T(Time.time < _warningUntil ? "power_first" : _switching ? "switching" :
            _collecting ? "collecting" : _flying ? "flying" :
            _draining ? "draining" : _dragging ? (_offset.magnitude >= _pullDistance ? "release" : "pull") :
            !_service.PowerOff ? "switch_hint" : !_service.FuseRemoved ? "tumbler_hint" :
            !_service.OilDrained ? "drain_hint_new" : onBurger ? "take_hint" :
            _burger.enabled ? "hover_burger" : "collected");
        if (_hintNotice != null) _hintNotice.Show(hint); else _hint.text = hint;
    }

    private void SetOutlines(bool power, bool tumbler, bool burger)
    {
        _switchOutline.enabled = power;
        _tumblerOutline.enabled = tumbler;
        _burgerOutline.enabled = burger;
    }
    private void SetBurgerAlpha(float alpha)
    {
        var color = _burger.color;
        color.a = alpha;
        _burger.color = color;
    }
    private static bool Hit(Sprite sprite, Vector2 point)
    {
        var rect = sprite.rect;
        int x = Mathf.FloorToInt(point.x), y = Mathf.FloorToInt(point.y);
        if (x < 0 || y < 0 || x >= rect.width || y >= rect.height) return false;
        return sprite.texture.GetPixel((int)rect.x + x, (int)rect.y + (int)rect.height - 1 - y).a > .1f;
    }
    private void CancelDrag() { if (_dragging) { _dragging = false; _offset = Vector2.zero; } }
    private void OnApplicationFocus(bool focused) { if (!focused) CancelDrag(); }
    private void OnDisable() { Hide(); }
    private void Play(AudioClip clip)
    {
        if (_audio == null || clip == null) return;
        _audio.Stop(); _audio.clip = clip; _audio.Play(); _audioPaused = false;
    }
}
