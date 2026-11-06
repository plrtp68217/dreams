using UnityEngine;
using UnityEngine.EventSystems;

// Standard Button.onClick handles a tap. A held +/- also repeats slowly,
// using the same ten-percent step as a single click.
[RequireComponent(typeof(UnityEngine.UI.Button))]
public class VendingButtonHold : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private bool _repeat = true;
    [SerializeField] private UnityEngine.UI.Outline _outline;
    private UnityEngine.UI.Button _button;
    private bool _held, _hovered;
    private float _remaining;
    private void Awake() => _button = GetComponent<UnityEngine.UI.Button>();
    public void OnPointerDown(PointerEventData data)
    {
        if (!_repeat || data.button != PointerEventData.InputButton.Left || !_button.IsInteractable() || Time.timeScale <= 0) return;
        _held = true;
        _remaining = .35f;
    }
    public void OnPointerUp(PointerEventData data) { if (data.button == PointerEventData.InputButton.Left) _held = false; }
    public void OnPointerEnter(PointerEventData data) => _hovered = true;
    public void OnPointerExit(PointerEventData data) { _held = _hovered = false; }
    private void OnDisable() { _held = _hovered = false; if (_outline != null) _outline.enabled = false; }
    private void OnApplicationFocus(bool focused) { if (!focused) _held = _hovered = false; }
    private void Update()
    {
        if (_outline != null) _outline.enabled = _hovered && _button.IsInteractable() && Time.timeScale > 0;
        if (!_held) return;
        if (Time.timeScale <= 0 || !_button.IsInteractable() || !Input.GetMouseButton(0)) { _held = false; return; }
        _remaining -= Time.deltaTime;
        if (_remaining <= 0) { _button.onClick.Invoke(); _remaining = .06f; }
    }
}
