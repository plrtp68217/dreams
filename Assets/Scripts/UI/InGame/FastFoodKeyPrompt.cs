using TMPro;
using UnityEngine;

// Adapts existing text/image visibility to a shared keycap without changing input ownership.
public class FastFoodKeyPrompt : MonoBehaviour
{
    [SerializeField] private TMP_Text _source;
    [SerializeField] private UnityEngine.UI.Image _visibility;
    [SerializeField] private TMP_Text _caption, _key;
    [SerializeField] private RectTransform _panel, _keycap;
    [SerializeField] private CanvasGroup _group;
    [SerializeField] private string _fallbackKey = "E";
    [SerializeField] private string _captionId;
    [SerializeField] private FastFoodServiceView _view;
    [SerializeField] private InputService _input;
    [SerializeField] private bool _routeHint;
    private string _last;
    private void Awake()
    {
        if (_source != null) _source.enabled = false;
        if (_visibility != null) _visibility.enabled = false;
        _group.interactable = _group.blocksRaycasts = false;
    }
    private void LateUpdate()
    {
        string message = _source != null ? _source.text : FacelessOrderService.Text(_captionId);
        if (_last != message)
        {
            _last = message;
            string key = _fallbackKey;
            int separator = message.IndexOf('—');
            if (separator < 0) separator = message.IndexOf(" - ", System.StringComparison.Ordinal);
            if (separator > 0 && separator < 12)
            { key = message.Substring(0, separator).Trim(); message = message.Substring(separator + 1).Trim(' ', '-'); }
            _caption.text = message;
            _key.text = key;
            bool hasKey = !string.IsNullOrEmpty(key);
            _keycap.gameObject.SetActive(hasKey);
            float keyWidth = Mathf.Max(34, _key.GetPreferredValues(key).x + 12);
            _keycap.sizeDelta = new Vector2(keyWidth, 30);
            float inset = hasKey ? keyWidth + 26 : 16;
            float width = Mathf.Clamp(_caption.GetPreferredValues(message).x + inset + 16, 90, 660);
            _panel.sizeDelta = new Vector2(width, 44);
            _caption.rectTransform.offsetMin = new Vector2(inset, 4);
        }
        float alpha = _visibility != null ? _visibility.color.a : string.IsNullOrEmpty(_last) ? 0 : 1;
        if (Time.timeScale <= 0 || (_view != null && _view.IsOpen) ||
            (_routeHint && ((_input != null && _input.IsBlocked) || (_view != null && _view.HasPrompt)))) alpha = 0;
        _group.alpha = alpha;
    }
}
