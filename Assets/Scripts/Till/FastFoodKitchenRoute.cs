using System.Collections;
using TMPro;
using Unity.Cinemachine;
using UnityEngine;

// The kitchen is a separate area of FastFood, so the first order stays in memory.
public class FastFoodKitchenRoute : MonoBehaviour
{
    [SerializeField] private TillTrigger _till;
    [SerializeField] private Player _player;
    [SerializeField] private PlayerController _playerController;
    [SerializeField] private InputService _inputService;
    [SerializeField] private CinemachineCamera _camera;
    [SerializeField] private CinemachineConfiner2D _confiner;
    [SerializeField] private Collider2D _counterCameraBounds;
    [SerializeField] private Collider2D _kitchenCameraBounds;
    [SerializeField] private Transform _kitchenSpawn;
    [SerializeField] private Transform _counterSpawn;
    [SerializeField] private CanvasGroup _hint;
    [SerializeField] private TextMeshProUGUI _hintText;
    [SerializeField] private UnityEngine.UI.Image _blackMask;
    [SerializeField, Min(0.01f)] private float _fadeDuration = 0.4f;
    [SerializeField, Min(0f)] private float _blackHoldDuration = 0.1f;

    private bool _hasOrder;
    private bool _isInKitchen;
    private bool _isTransitioning;
    private Coroutine _transition;
    private RigidbodyType2D _bodyType;
    private bool _controllerWasEnabled;
    private bool _inputWasBlocked;

    private void Awake()
    {
        // The route always starts at the counter, even if the kitchen was previewed in the Editor.
        _confiner.BoundingShape2D = _counterCameraBounds;
        _confiner.InvalidateBoundingShapeCache();
        _camera.PreviousStateIsValid = false;
    }

    private void OnEnable()
    {
        _till.OrderCompleted += UnlockKitchen;
        LocalizedDialogueSystem.LanguageChanged += UpdateHintText;
        UpdateHintText();
        if (_till.IsCompleted) UnlockKitchen();
    }

    private void OnDisable()
    {
        _till.OrderCompleted -= UnlockKitchen;
        LocalizedDialogueSystem.LanguageChanged -= UpdateHintText;
        if (_transition != null) StopCoroutine(_transition);
        if (_isTransitioning) RestoreControls();
        _transition = null;
        _isTransitioning = false;
        _hint.alpha = 0f;
        SetBlackAlpha(0f);
    }

    private void UnlockKitchen()
    {
        _hasOrder = true;
        _hint.alpha = _isInKitchen ? 0f : 1f;
    }

    private void UpdateHintText()
    {
        _hintText.text = LocalizedDialogueSystem.CurrentLanguage == Language.Russian
            ? "На кухню: налево (A)"
            : "Kitchen: go left (A)";
    }

    public void TryTravel(Player player, bool toKitchen)
    {
        if (player != _player || !_hasOrder || _isTransitioning ||
            toKitchen == _isInKitchen || _inputService.IsBlocked || !_player.IsAlive)
            return;

        _isTransitioning = true;
        _transition = StartCoroutine(Travel(toKitchen));
    }

    private IEnumerator Travel(bool toKitchen)
    {
        _inputWasBlocked = _inputService.IsBlocked;
        _controllerWasEnabled = _playerController.enabled;
        _bodyType = _player.Rigidbody.bodyType;
        _inputService.Block();
        _playerController.enabled = false;
        _player.Rigidbody.linearVelocity = Vector2.zero;
        _player.Rigidbody.bodyType = RigidbodyType2D.Kinematic;
        _hint.alpha = 0f;

        yield return Fade(1f);

        Vector3 destination = (toKitchen ? _kitchenSpawn : _counterSpawn).position;
        Vector3 delta = destination - _player.transform.position;
        Vector3 cameraPosition = _camera.State.GetFinalPosition();
        _player.transform.position = destination;
        _player.Rigidbody.position = destination;
        _player.SpriteRenderer.flipX = toKitchen;
        _confiner.BoundingShape2D = toKitchen ? _kitchenCameraBounds : _counterCameraBounds;
        _confiner.InvalidateBoundingShapeCache();
        _confiner.BakeBoundingShape(_camera, 0.1f);
        _camera.OnTargetObjectWarped(_player.transform, delta);
        _camera.ForceCameraPosition(new Vector3(destination.x, cameraPosition.y, cameraPosition.z), _camera.transform.rotation);
        _camera.PreviousStateIsValid = false;
        _isInKitchen = toKitchen;
        Physics2D.SyncTransforms();

        // Apply the destination's framing while the mask is fully black. The first revealed
        // frame already contains Eva, rather than waiting for normal follow damping.
        _camera.InternalUpdateCameraState(Vector3.up, -1f);
        // Let the Brain apply this state in its normal update mode before revealing it.
        yield return null;

        yield return new WaitForSeconds(_blackHoldDuration);
        yield return Fade(0f);

        RestoreControls();
        _isTransitioning = false;
        _transition = null;
    }

    private IEnumerator Fade(float targetAlpha)
    {
        float startAlpha = _blackMask.color.a;
        float elapsed = 0f;
        while (elapsed < _fadeDuration)
        {
            elapsed += Time.deltaTime;
            SetBlackAlpha(Mathf.Lerp(startAlpha, targetAlpha, Mathf.SmoothStep(0f, 1f, elapsed / _fadeDuration)));
            yield return null;
        }
        SetBlackAlpha(targetAlpha);
    }

    private void SetBlackAlpha(float alpha)
    {
        _blackMask.color = new Color(0f, 0f, 0f, alpha);
    }

    private void RestoreControls()
    {
        _player.Rigidbody.linearVelocity = Vector2.zero;
        _player.Rigidbody.bodyType = _bodyType;
        _playerController.enabled = _controllerWasEnabled;
        if (!_inputWasBlocked) _inputService.Unblock();
    }
}
