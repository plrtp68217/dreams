using UnityEngine;

// Keep dialogues centered on screen, above the inventory on narrow screens.
public class FastFoodHudLayout : MonoBehaviour
{
    [SerializeField] private RectTransform _canvas;
    [SerializeField] private RectTransform[] _dialogueArt;
    [SerializeField] private RectTransform[] _dialogueKeys;
    [SerializeField] private FastFoodInventory _inventory;
    [SerializeField] private UnityEngine.UI.Image _trayBorder;
    [SerializeField] private FastFoodServiceView _view;
    [SerializeField] private CanvasGroup _dialogueGroup;
    [SerializeField, Range(.5f, 1f)] private float _dialogueScale = .75f;
    private Vector2 _lastSize;
    private float _lastDialogueScale;
    private void LateUpdate()
    {
        // Invisible graphics must leave the input path as well as the rendered image.
        if (_dialogueGroup != null)
        {
            bool visible = _view == null || !_view.IsOpen;
            _dialogueGroup.alpha = visible ? 1 : 0;
            _dialogueGroup.blocksRaycasts = visible;
            _dialogueGroup.interactable = visible;
        }
        var size = _canvas.rect.size;
        if (size != _lastSize || _lastDialogueScale != _dialogueScale)
        {
            _lastSize = size;
            _lastDialogueScale = _dialogueScale;
            const float left = 376, right = 232;
            bool narrow = size.x < 640 * _dialogueScale + 2 * Mathf.Max(left, right);
            float bottom = narrow ? 144 : 24;
            for (int i = 0; i < _dialogueArt.Length; i++)
            {
                _dialogueArt[i].anchoredPosition = new Vector2(0, bottom);
                _dialogueArt[i].localScale = Vector3.one * _dialogueScale;
                _dialogueKeys[i].anchoredPosition = new Vector2(236 * _dialogueScale, bottom + 22);
            }
        }
        _trayBorder.color = _inventory.IsOrderReady ? new Color(.87f,.67f,.27f) : new Color(.55f,.18f,.16f);
    }
}
