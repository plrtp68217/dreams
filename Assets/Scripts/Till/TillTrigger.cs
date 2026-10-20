using System.Collections;
using UnityEngine;

public class TillTrigger : MonoBehaviour
{
    [SerializeField] private Player _player;
    [SerializeField] private InputService _inputService;
    [SerializeField] private Dialog _dialogUI;
    [SerializeField] private GuestOrderSequence _orderSequence;

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

    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (_isBusy || _isCompleted) return;

        if (collider.TryGetComponent(out Player player))
        {
            _isEntered = true;
            ShowHint();
        }
    }

    private void OnTriggerExit2D(Collider2D collider)
    {
        if (_isBusy || _isCompleted) return;

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
    }

    private void Update()
    {
        if (_isEntered && _isBusy == false && _isCompleted == false && _inputService.EIsPressed)
        {
            _workCoroutine = StartCoroutine(WorkRoutine());
        }
    }

    private IEnumerator WorkRoutine()
    {
        _isBusy = true;

        HideHint();
        _inputService.Block();

        yield return StepBehindCounter();
        yield return _orderSequence.Play();

        // Подсказка: E — выйти из кассы
        ShowHint();
        yield return _inputService.WaitForKeyE();
        HideHint();

        yield return StepOutFromCounter();

        _inputService.Unblock();

        _isCompleted = true;
        _isBusy = false;
        _workCoroutine = null;
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
        _dialogUI.Enable();
        _player.SetActiveDialog(_dialogUI);
    }

    private void HideHint()
    {
        _dialogUI.Disable();
        _player.ResetActiveDialog();
    }
}
