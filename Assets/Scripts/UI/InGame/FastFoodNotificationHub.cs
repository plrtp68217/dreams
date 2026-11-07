using System.Collections.Generic;
using TMPro;
using UnityEngine;

// One presentation surface for transient messages; gameplay hints keep their own lifetime.
public class FastFoodNotificationHub : MonoBehaviour
{
    [SerializeField] private FacelessOrderService _service;
    [SerializeField] private FastFoodInventory _inventory;
    [SerializeField] private CanvasGroup _group;
    [SerializeField] private RectTransform _card;
    [SerializeField] private RectTransform _symbol;
    [SerializeField] private TMP_Text _title, _body, _symbolText;
    [SerializeField] private GameObject _successIcon;
    [SerializeField] private UnityEngine.UI.Image _progress, _accent;
    [SerializeField, Min(.1f)] private float _enterDuration = .46f;
    [SerializeField, Min(.1f)] private float _exitDuration = .3f;

    private class Message
    {
        public Object Owner;
        public string Title, Body;
        public float Duration;
        public bool Warning;
    }
    private readonly List<Message> _queue = new();
    private Message _current;
    private int _phase, _knownItems;
    private float _elapsed, _remaining;
    private bool _ready, _reward;
    private Vector2 _origin;
    private Vector2 _exitPosition;
    private float _exitAlpha, _exitScale;
    private readonly Color _gold = new(0.87f, .67f, .27f);
    private readonly Color _warning = new(.91f, .40f, .24f);
    public float Remaining => _remaining;
    public bool IsVisible => _current != null;

    private void Awake()
    {
        _origin = _card.anchoredPosition;
        _group.alpha = 0;
        _group.interactable = _group.blocksRaycasts = false;
    }
    private void OnEnable()
    {
        _inventory.Changed += InventoryChanged;
        _service.Changed += InventoryChanged;
        LocalizedDialogueSystem.LanguageChanged += LocalizeCurrent;
    }
    private void OnDisable()
    {
        _inventory.Changed -= InventoryChanged;
        _service.Changed -= InventoryChanged;
        LocalizedDialogueSystem.LanguageChanged -= LocalizeCurrent;
        _queue.Clear(); _current = null;
        if (_group != null) _group.alpha = 0;
    }
    private void InventoryChanged()
    {
        int items = 0, added = -1;
        for (int i = 0; i < 4; i++)
            if (_inventory.Has((FastFoodOrderItem)i)) { items |= 1 << i; if ((_knownItems & (1 << i)) == 0) added = i; }
        if (_inventory.HasMedicalRecord && !_reward)
            Post(_inventory, "service_served_title", "service_record_title", 5.5f, false, true);
        else if (_inventory.IsOrderReady && !_ready)
            Post(_inventory, "service_ready_title", "service_ready_body", 5.5f, false, true);
        else if (added >= 0)
            Post(_inventory, "service_item_added", "service_item_" + added, 3.8f);
        _knownItems = items; _ready = _inventory.IsOrderReady; _reward = _inventory.HasMedicalRecord;
    }
    public void Post(Object owner, string title, string body, float duration, bool warning = false, bool priority = false)
    {
        var next = new Message { Owner = owner, Title = title, Body = body, Duration = Mathf.Max(.5f, duration), Warning = warning };
        if (_current != null && _phase != 2 && _current.Owner == owner && _current.Title == title && _current.Body == body) return;
        _queue.RemoveAll(m => m.Owner == owner);
        if (priority || (_current != null && _current.Owner == owner))
        {
            _queue.Insert(0, next);
            if (_current != null) BeginExit();
        }
        else _queue.Add(next);
        if (_queue.Count > 4) _queue.RemoveAt(_queue.Count - 1);
        if (_current == null) BeginNext();
    }
    public void Cancel(Object owner)
    {
        _queue.RemoveAll(m => m.Owner == owner);
        if (_current != null && _current.Owner == owner) BeginExit();
    }
    private void BeginNext()
    {
        if (_queue.Count == 0) { _current = null; _group.alpha = 0; return; }
        _current = _queue[0]; _queue.RemoveAt(0);
        _phase = 0; _elapsed = 0; _remaining = _current.Duration;
        _group.alpha = 0;
        _progress.fillAmount = 1;
        _progress.color = _accent.color = _current.Warning ? _warning : _gold;
        _symbolText.text = _current.Warning ? "!" : "+";
        bool success = _current.Title == "service_ready_title" || _current.Title == "service_served_title";
        if (_successIcon != null) _successIcon.SetActive(success);
        _symbolText.gameObject.SetActive(!success);
        LocalizeCurrent();
    }
    private static string Translate(string key) => key.StartsWith("service_") ? FacelessOrderService.Text(key) : key;
    private void LocalizeCurrent()
    {
        if (_current == null) return;
        _title.text = Translate(_current.Title);
        _body.text = Translate(_current.Body);
    }
    private void BeginExit()
    {
        if (_phase == 2) return;
        _exitAlpha = _group.alpha;
        _exitPosition = _card.anchoredPosition;
        _exitScale = _card.localScale.x;
        _phase = 2; _elapsed = 0;
    }
    private void Update() => Advance(Time.deltaTime);
    private void Advance(float delta)
    {
        if (_current == null || delta <= 0) return;
        _elapsed += delta;
        if (_phase != 2)
        {
            _remaining = Mathf.Max(0, _remaining - delta);
            _progress.fillAmount = _remaining / _current.Duration;
        }
        if (_phase == 0)
        {
            float t = Mathf.Clamp01(_elapsed / _enterDuration);
            float ease = 1 - Mathf.Pow(1 - t, 4);
            float settle = Mathf.Sin(t * Mathf.PI) * (1 - t);
            _card.anchoredPosition = _origin + Vector2.up * (36 * (1 - ease) - 5 * settle);
            _card.localScale = Vector3.one * (.96f + .04f * ease + .012f * settle);
            _symbol.localScale = Vector3.one * (.7f + .3f * ease + .09f * settle);
            _group.alpha = Mathf.SmoothStep(0, 1, Mathf.Clamp01(t * 2));
            if (t >= 1) { _phase = 1; _elapsed = 0; }
        }
        else if (_phase == 1)
        {
            if (_remaining <= 0) BeginExit();
        }
        else
        {
            float t = Mathf.Clamp01(_elapsed / _exitDuration);
            _group.alpha = _exitAlpha * (1 - t * t);
            _card.anchoredPosition = Vector2.Lerp(_exitPosition, _origin + Vector2.up * 18, t * t);
            _card.localScale = Vector3.one * Mathf.Lerp(_exitScale, .975f, t);
            if (t >= 1) BeginNext();
        }
    }
}
