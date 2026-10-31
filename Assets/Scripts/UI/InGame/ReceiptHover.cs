using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(RectTransform), typeof(UnityEngine.UI.Image))]
public class ReceiptHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField, Min(0)] private float _readDuration = 5f;
    [SerializeField, Range(0.2f, 1f)] private float _collapsedScale = 0.55f;
    [SerializeField, Min(0.01f)] private float _animationDuration = 0.35f;
    [SerializeField] private UnityEngine.UI.Shadow _shadow;

    private RectTransform _rect;
    private UnityEngine.UI.Image _image;
    private Vector3 _expandedScale;
    private float _remainingReadTime;
    private bool _shown;
    private bool _hovered;
    private Coroutine _animation;

    private void Awake()
    {
        _rect = GetComponent<RectTransform>();
        _image = GetComponent<UnityEngine.UI.Image>();
        _expandedScale = _rect.localScale;
        _image.raycastTarget = false;
    }

    public void ShowReceivedOrder()
    {
        _shown = true;
        _remainingReadTime = _readDuration;
        _image.raycastTarget = true;
        Animate(true);
    }

    private void Update()
    {
        if (!_shown || _remainingReadTime <= 0) return;
        _remainingReadTime -= Time.deltaTime;
        if (_remainingReadTime <= 0 && !_hovered) Animate(false);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!_shown) return;
        _hovered = true;
        Animate(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _hovered = false;
        if (_shown && _remainingReadTime <= 0) Animate(false);
    }

    private void Animate(bool expanded)
    {
        if (_animation != null) StopCoroutine(_animation);
        _animation = StartCoroutine(AnimateRoutine(expanded));
    }

    private IEnumerator AnimateRoutine(bool expanded)
    {
        Vector3 start = _rect.localScale;
        Vector3 target = _expandedScale * (expanded ? 1f : _collapsedScale);
        float elapsed = 0;
        while (elapsed < _animationDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / _animationDuration);
            // A small overshoot gives the expanded paper a soft lift, without a hard snap.
            float shifted = t - 1f;
            float eased = expanded
                ? 1f + 1.8f * shifted * shifted * shifted + 0.8f * shifted * shifted
                : Mathf.SmoothStep(0, 1, t);
            _rect.localScale = Vector3.LerpUnclamped(start, target, eased);
            if (_shadow != null)
            {
                float lift = Mathf.InverseLerp(_collapsedScale, 1f, _rect.localScale.x / _expandedScale.x);
                _shadow.effectDistance = Vector2.Lerp(new Vector2(1, -2), new Vector2(4, -6), lift);
            }
            yield return null;
        }
        _rect.localScale = target;
        _animation = null;
    }

    private void OnDisable()
    {
        if (_animation != null) StopCoroutine(_animation);
        _animation = null;
        _hovered = false;
        if (_image != null) _image.raycastTarget = false;
    }

    private void OnEnable()
    {
        if (_shown) _image.raycastTarget = true;
    }
}
