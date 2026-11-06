using TMPro;
using UnityEngine;

// Shared transitions for kitchen notices. A text change fades the previous
// message out before revealing the next; scaled time makes pausing consistent.
public class SmoothServiceNotice : MonoBehaviour
{
    [SerializeField] private TMP_Text _label;
    [SerializeField] private CanvasGroup _group;
    [SerializeField, Min(.01f)] private float _fadeIn = .18f;
    [SerializeField, Min(.01f)] private float _fadeOut = .16f;
    private string _requested = "";
    private float _progress;

    public void Show(string message) => _requested = message ?? "";
    public void Clear()
    {
        _requested = "";
        _progress = 0;
        if (_group != null) _group.alpha = 0;
        if (_label != null) _label.text = "";
    }
    private void Awake() => Clear();
    private void OnDisable() => Clear();
    private void Update() => Advance(Time.deltaTime);
    private void Advance(float delta)
    {
        bool change = _label.text != _requested;
        bool visible = !change && !string.IsNullOrEmpty(_requested);
        _progress = Mathf.MoveTowards(_progress, visible ? 1 : 0, delta / (visible ? _fadeIn : _fadeOut));
        if (_progress <= 0 && change) _label.text = _requested;
        _group.alpha = Mathf.SmoothStep(0, 1, _progress);
    }
}
