using UnityEngine;

[DisallowMultipleComponent]
public class FastFoodMirrorCloseUp : MonoBehaviour
{
    [SerializeField] private FacelessOrderService _service;
    [SerializeField] private Player _player;
    [SerializeField] private GameObject _root;
    [SerializeField] private UnityEngine.UI.Image _toy;
    [SerializeField] private Sprite[] _toyFrames;
    [SerializeField] private float[] _frameDurations;
    [SerializeField] private UnityEngine.UI.Image _pickup;
    [SerializeField] private UnityEngine.UI.Image _outline;
    [SerializeField] private SmoothServiceNotice _hint;
    [SerializeField] private AudioSource _audio;
    [SerializeField] private AudioClip _pickupSound;
    [SerializeField, Min(.1f)] private float _pickupDuration = .75f;

    private bool _standingUp, _collecting;
    private float _elapsed;
    private int _openedFrame;
    private Vector2 _pickupOrigin;
    public bool IsVisible => _root != null && _root.activeSelf;
    public bool IsBusy => _standingUp || _collecting;
    private bool CanControl => IsVisible && Time.timeScale > 0 && _service.CanInteract && _player.IsAlive;
    private bool Taken => _service.Inventory.Has(FastFoodOrderItem.Toy) || _service.IsCompleted;

    private void Awake() => _pickupOrigin = _pickup.rectTransform.anchoredPosition;
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
        _standingUp = _collecting = false;
        _elapsed = 0;
        if (_hint != null) _hint.Clear();
        if (_outline != null) _outline.enabled = false;
        if (_pickup != null)
        {
            _pickup.enabled = false;
            _pickup.rectTransform.anchoredPosition = _pickupOrigin;
            _pickup.rectTransform.localScale = Vector3.one;
            _pickup.rectTransform.localRotation = Quaternion.identity;
            _pickup.color = Color.white;
        }
        if (_audio != null) _audio.Stop();
    }
    private void Update()
    {
        if (!IsVisible) return;
        AdvanceAnimation(Time.deltaTime);
        ProcessInput(Input.mousePosition, Input.GetKeyDown(KeyCode.E));
    }
    private void ProcessInput(Vector2 screenPoint, bool interact)
    {
        bool available = CanControl && !IsBusy && !Taken;
        bool hover = available && _service.CanPerform(FacelessServiceAction.TakeToy) && HitToy(screenPoint);
        if (available && interact && Time.frameCount > _openedFrame)
        {
            if (_service.CanPerform(FacelessServiceAction.LookInMirror))
            {
                _standingUp = true;
                _elapsed = 0;
            }
            else if (hover)
            {
                _collecting = true;
                _elapsed = 0;
            }
        }
        Render(hover && !IsBusy);
    }
    private void AdvanceAnimation(float delta)
    {
        if (!CanControl || !IsBusy) return;
        _elapsed += delta;
        if (_standingUp && _elapsed >= StandingDuration())
        {
            _standingUp = false;
            _service.Perform(FacelessServiceAction.LookInMirror);
        }
        else if (_collecting && _elapsed >= _pickupDuration)
        {
            // Keep the modal alive through the inventory's synchronous Changed event.
            bool collected = _service.Perform(FacelessServiceAction.TakeToy);
            _collecting = false;
            if (collected && _audio != null && _pickupSound != null)
            {
                _audio.Stop();
                _audio.clip = _pickupSound;
                _audio.Play();
            }
        }
    }
    private float StandingDuration()
    {
        float total = 0;
        foreach (float duration in _frameDurations) total += duration;
        return total;
    }
    private int StandingFrame()
    {
        float time = _elapsed;
        for (int i = 0; i < _frameDurations.Length; ++i)
        {
            if (time < _frameDurations[i]) return i;
            time -= _frameDurations[i];
        }
        return _toyFrames.Length - 1;
    }
    private void Render(bool hover)
    {
        _toy.sprite = _toyFrames[_service.MirrorLooked ? _toyFrames.Length - 1 : _standingUp ? StandingFrame() : 0];
        _toy.enabled = !Taken && !_collecting;
        _outline.enabled = hover;
        string key = CanControl && !IsBusy && !Taken ? !_service.MirrorLooked ? "service_mirror_wake_hint" : hover ? "service_mirror_take_hint" : null : null;
        _hint.Show(key == null ? "" : FacelessOrderService.Text(key));
        _pickup.enabled = _collecting;
        if (!_collecting) return;
        float t = Mathf.Clamp01(_elapsed / _pickupDuration);
        float ease = Mathf.SmoothStep(0, 1, t);
        _pickup.rectTransform.anchoredPosition = _pickupOrigin + new Vector2(-130 * ease, 105 * Mathf.Sin(t * Mathf.PI) - 310 * ease);
        _pickup.rectTransform.localScale = Vector3.one * Mathf.Lerp(1, 1.7f, ease);
        _pickup.rectTransform.localRotation = Quaternion.Euler(0, 0, -9 * Mathf.Sin(t * Mathf.PI));
        _pickup.color = new Color(1, 1, 1, 1 - Mathf.SmoothStep(0, 1, Mathf.InverseLerp(.5f, 1, t)));
    }
    private bool HitToy(Vector2 screenPoint)
    {
        var canvas = _toy.GetComponentInParent<Canvas>();
        var camera = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(_toy.rectTransform, screenPoint, camera, out var point)) return false;
        var rect = _toy.rectTransform.rect;
        if (!rect.Contains(point)) return false;
        var sprite = _toy.sprite;
        int x = Mathf.Clamp((int)((point.x - rect.xMin) / rect.width * sprite.rect.width), 0, (int)sprite.rect.width - 1);
        int y = Mathf.Clamp((int)((point.y - rect.yMin) / rect.height * sprite.rect.height), 0, (int)sprite.rect.height - 1);
        return sprite.texture.GetPixel((int)sprite.rect.x + x, (int)sprite.rect.y + y).a > .1f;
    }
    private void OnDisable() => Hide();
}
