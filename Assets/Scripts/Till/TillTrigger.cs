using System.Collections;
using UnityEngine;

public class TillTrigger : MonoBehaviour
{
    public event System.Action OrderCompleted;

    public bool IsCompleted => _isCompleted;
    public bool IsBehindCounter { get; private set; }
    public bool IsBusy => _isBusy;

    [SerializeField] private Player _player;
    [SerializeField] private InputService _inputService;
    [SerializeField] private Dialog _dialogUI;

    [Tooltip("Необязательно: кто подходит к кассе до посетителя")]
    [SerializeField] private NpcVisit _briefing;
    [SerializeField] private GuestOrderSequence _orderSequence;
    [SerializeField] private FacelessOrderService _service;
    [SerializeField] private FastFoodServiceView _serviceView;
    [SerializeField] private FastFoodKeyPrompt _keyPrompt;

    [Header("Место кассира за стойкой")]
    [SerializeField] private Transform _cashierPoint;
    [SerializeField] private string _cashierSortingLayer = "Objects";
    [SerializeField] private int _cashierSortingOrder = 1;
    [SerializeField] private float _fadeDuration = 0.3f;

    private bool _isEntered = false;
    private bool _isBusy = false;
    private bool _isCompleted = false;

    private string _defaultSortingLayer;
    private int _defaultSortingOrder;
    private RigidbodyType2D _defaultBodyType;
    private Vector3 _positionBeforeCounter;

    private Coroutine _workCoroutine;
    private bool _hintShown, _ownsInput;
    private bool CanEnter => !_isCompleted || (_service != null && _service.CanInteract && _service.Inventory.IsOrderReady);

    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.TryGetComponent(out Player player))
        {
            _isEntered = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collider)
    {
        if (collider.TryGetComponent(out Player _))
        {
            _isEntered = false;
            HideHint();
        }
    }

    private void OnDisable()
    {
        if (_workCoroutine != null)
        {
            StopCoroutine(_workCoroutine);
            _workCoroutine = null;
        }
        if (_orderSequence != null) _orderSequence.CancelAfterGuestLeft();
        RestoreCounterState();
        HideHint();
    }

    private void Update()
    {
        bool available = _isEntered && !_isBusy && CanEnter && _player.IsAlive &&
            !_inputService.IsBlocked && Time.timeScale > 0 && (_serviceView == null || !_serviceView.IsOpen);
        if (!_isBusy)
        {
            if (available) ShowHint(); else HideHint();
        }
        if (available && _inputService.EIsPressed)
        {
            _workCoroutine = StartCoroutine(WorkRoutine(_isCompleted));
        }
    }

    private IEnumerator WorkRoutine(bool returningWithOrder)
    {
        _isBusy = true;
        HideHint();
        _ownsInput = !_inputService.IsBlocked;
        _inputService.Block();
        _player.Rigidbody.linearVelocity = Vector2.zero;

        yield return StepBehindCounter();

        if (returningWithOrder)
        {
            if (_service.HandOverAtCounter())
            {
                // No interaction prompt or E wait between entering and handing over.
                yield return _orderSequence.FinishOrder();
                yield return _orderSequence.AfterGuestLeft();
            }
        }
        else
        {
            if (_briefing != null) yield return _briefing.Play();
            yield return _orderSequence.Play();

            ShowHint();
            yield return _inputService.WaitForKeyE();
            HideHint();
        }

        yield return StepOutFromCounter();
        RestoreCounterState();
        _workCoroutine = null;
        if (!returningWithOrder)
        {
            _isCompleted = true;
            OrderCompleted?.Invoke();
        }
    }

    private IEnumerator StepBehindCounter()
    {
        SpriteRenderer spriteRenderer = _player.SpriteRenderer;

        _defaultSortingLayer = spriteRenderer.sortingLayerName;
        _defaultSortingOrder = spriteRenderer.sortingOrder;

        yield return FadePlayer(0f);

        _positionBeforeCounter = _player.transform.position;

        // За стойкой Ева стоит выше пола, поэтому отключаем гравитацию
        _defaultBodyType = _player.Rigidbody.bodyType;
        _player.Rigidbody.linearVelocity = Vector2.zero;
        _player.Rigidbody.bodyType = RigidbodyType2D.Kinematic;

        MovePlayer(new Vector3(_cashierPoint.position.x, _cashierPoint.position.y, _positionBeforeCounter.z));
        IsBehindCounter = true;

        // Ева смотрит в зал, на посетителя
        spriteRenderer.flipX = false;
        spriteRenderer.sortingLayerName = _cashierSortingLayer;
        spriteRenderer.sortingOrder = _cashierSortingOrder;

        yield return FadePlayer(1f);
    }

    private IEnumerator StepOutFromCounter()
    {
        yield return FadePlayer(0f);

        _player.SpriteRenderer.sortingLayerName = _defaultSortingLayer;
        _player.SpriteRenderer.sortingOrder = _defaultSortingOrder;

        MovePlayer(_positionBeforeCounter);
        _player.Rigidbody.bodyType = _defaultBodyType;
        IsBehindCounter = false;

        yield return FadePlayer(1f);
    }

    private void MovePlayer(Vector3 position)
    {
        _player.transform.position = position;
        _player.Rigidbody.position = position;
    }

    private IEnumerator FadePlayer(float targetAlpha)
    {
        SpriteRenderer spriteRenderer = _player.SpriteRenderer;

        float startAlpha = spriteRenderer.color.a;
        float elapsedTime = 0f;

        while (elapsedTime < _fadeDuration)
        {
            elapsedTime += Time.deltaTime;

            Color color = spriteRenderer.color;
            color.a = Mathf.Lerp(startAlpha, targetAlpha, elapsedTime / _fadeDuration);
            spriteRenderer.color = color;

            yield return null;
        }

        Color finalColor = spriteRenderer.color;
        finalColor.a = targetAlpha;
        spriteRenderer.color = finalColor;
    }

    private void ShowHint()
    {
        if (_hintShown) return;
        _hintShown = true;
        if (_keyPrompt != null) _keyPrompt.SetCaptionId(_isBusy ? "service_counter_exit" : "service_counter_enter");
        _dialogUI.Enable();
        _player.SetActiveDialog(_dialogUI);
    }

    private void HideHint()
    {
        if (!_hintShown) return;
        _hintShown = false;
        if (_dialogUI != null) _dialogUI.Disable();
        if (_keyPrompt != null) _keyPrompt.SetCaptionId("service_interact");
        if (_player != null) _player.ResetActiveDialog();
    }

    private void RestoreCounterState()
    {
        if (IsBehindCounter && _player != null && _player.Rigidbody != null)
        {
            _player.SpriteRenderer.sortingLayerName = _defaultSortingLayer;
            _player.SpriteRenderer.sortingOrder = _defaultSortingOrder;
            MovePlayer(_positionBeforeCounter);
            _player.Rigidbody.bodyType = _defaultBodyType;
            IsBehindCounter = false;
        }
        if (_isBusy && _player != null && _player.SpriteRenderer != null)
        {
            var color = _player.SpriteRenderer.color; color.a = 1; _player.SpriteRenderer.color = color;
        }
        if (_service != null) _service.FinishCounterVisit();
        if (_ownsInput) _inputService.Unblock();
        _ownsInput = false;
        _isBusy = false;
    }
}
