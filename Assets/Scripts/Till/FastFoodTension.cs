using System.Collections;
using TMPro;
using Unity.Cinemachine;
using UnityEngine;

public enum ClownEncounterKind { None, Watcher, KitchenReturn, Ready, IdleReminder }

// One order, three story beats. UI, transitions and other dialogues retain control priority.
public class FastFoodTension : MonoBehaviour
{
    [SerializeField] private FacelessOrderService _service;
    [SerializeField] private FastFoodServiceView _view;
    [SerializeField] private InputService _input;
    [SerializeField] private Player _player;
    [SerializeField] private SpriteRenderer _clown;
    [SerializeField] private TextMeshProUGUI _caption;
    [SerializeField] private TextMeshProUGUI _levelLabel;
    [SerializeField] private FastFoodKitchenRoute _route;
    [SerializeField] private TillTrigger _till;
    [SerializeField] private Npc _actor;
    [SerializeField] private Animator _animator;
    [SerializeField] private DialogCanvas _dialog;
    [SerializeField] private DialogueVoiceProfile _voice;
    [SerializeField] private Transform[] _hallPoints;
    [SerializeField] private Transform _finalPoint;
    [SerializeField] private CinemachineCameraOffset _cameraOffset;
    [SerializeField] private Camera _gameCamera;
    [SerializeField, Min(0)] private float _interactionGrace = 1.5f;
    [SerializeField, Min(0)] private float _encounterGap = 30f;
    [SerializeField, Min(1)] private float _idleDelay = 150f;
    [SerializeField, Min(.1f)] private float _silentWatchDuration = 3.5f;
    [SerializeField, Min(.1f)] private float _lingerDuration = 7f;
    [SerializeField, Min(.1f)] private float _cameraBlendTime = 1f;
    [SerializeField, Range(0, 1)] private float _cameraWeight = .32f;
    [SerializeField, Min(0)] private float _maxCameraShift = .55f;
    private Coroutine _appearance;
    private ClownEncounterKind _current;
    private int _completedMask;
    private bool _idleDone, _foodSeenInKitchen, _returnPending, _wasInKitchen;
    private bool _orderStarted, _ownsInput, _speaking, _deliveryFinal;
    private bool _ownsCamera, _focusActive;
    private Vector3 _baseCameraOffset;
    private float _cameraShift, _cameraVelocity;
    private float _quietRemaining, _cooldown, _idleElapsed;
    public bool IsAppearing => _appearance != null;
    public bool IsSpeaking => _speaking;
    public ClownEncounterKind CurrentEncounter => _current;
    public int CompletedMask => _completedMask;
    public int Level => (_completedMask & 4) != 0 ? 3 : (_completedMask & 2) != 0 ? 2 : 1;
    private bool InHall => !_route.IsInKitchen && !_route.IsInRestroom && !_route.IsTransitioning;
    private bool HasFood => _service.Inventory.Has(FastFoodOrderItem.Burger) ||
        _service.Inventory.Has(FastFoodOrderItem.Fries) || _service.Inventory.Has(FastFoodOrderItem.Drink);

    private void Awake()
    {
        _clown.gameObject.SetActive(false);
        if (_caption != null) _caption.gameObject.SetActive(false);
        if (_levelLabel != null) _levelLabel.gameObject.SetActive(false);
    }
    private void OnEnable()
    {
        _service.Changed += Progress;
        _till.OrderCompleted += LeftCounter;
    }
    private void Progress() => _idleElapsed = 0;
    private void LeftCounter() => _quietRemaining = .6f;
    private void Update()
    {
        if (Time.timeScale <= 0) return;
        AdvanceCamera(Time.deltaTime);
        if (_service.IsServing && _current == ClownEncounterKind.Ready && (_completedMask & 4) != 0)
            _deliveryFinal = true;
        if (!_player.IsAlive || _service.IsCompleted || (!_service.IsActive && !_deliveryFinal) ||
            (_service.IsServing && !_deliveryFinal))
        {
            AbortAppearance(false);
            return;
        }
        if (!_orderStarted) { _orderStarted = true; _quietRemaining = _interactionGrace; }
        _cooldown = Mathf.Max(0, _cooldown - Time.deltaTime);
        if (_appearance == null && !_view.IsOpen && !_input.IsBlocked && !_route.IsTransitioning)
            _idleElapsed += Time.deltaTime;
        if (_route.IsInKitchen && HasFood) _foodSeenInKitchen = true;
        if (_wasInKitchen && !_route.IsInKitchen && _foodSeenInKitchen) _returnPending = true;
        _wasInKitchen = _route.IsInKitchen;
        bool busyElsewhere = !InHall || _view.IsOpen || (_input.IsBlocked && !_ownsInput && !_deliveryFinal);
        if (busyElsewhere)
        {
            _quietRemaining = _interactionGrace;
            if (_appearance != null) AbortAppearance(true);
            return;
        }
        if (_appearance != null) return;
        if (!_till.IsCompleted || (_completedMask & 4) != 0) return;
        _quietRemaining = Mathf.Max(0, _quietRemaining - Time.deltaTime);
        if (_quietRemaining > 0) return;
        ClownEncounterKind next = NextEncounter();
        if (next == ClownEncounterKind.None) return;
        Transform point = ChoosePoint(next);
        if (point != null) _appearance = StartCoroutine(Appear(next, point));
    }
    private ClownEncounterKind NextEncounter()
    {
        if (_service.Inventory.IsOrderReady && Mathf.Abs(_player.transform.position.x - _finalPoint.position.x) <= 2.4f)
            return ClownEncounterKind.Ready;
        if (_cooldown > 0) return ClownEncounterKind.None;
        if ((_completedMask & 1) == 0) return ClownEncounterKind.Watcher;
        if (_returnPending && (_completedMask & 2) == 0) return ClownEncounterKind.KitchenReturn;
        if (!_idleDone && _idleElapsed >= _idleDelay) return ClownEncounterKind.IdleReminder;
        return ClownEncounterKind.None;
    }
    private Transform ChoosePoint(ClownEncounterKind kind)
    {
        if (kind == ClownEncounterKind.Ready) return _finalPoint;
        float preferred = kind == ClownEncounterKind.Watcher ? 1.9f : 1.3f;
        Transform selected = null;
        float best = float.PositiveInfinity;
        foreach (var point in _hallPoints)
        {
            float distance = Mathf.Abs(point.position.x - _player.transform.position.x);
            if (distance < .85f || distance > 2.4f) continue;
            float score = Mathf.Abs(distance - preferred);
            if (score < best) { selected = point; best = score; }
        }
        return selected;
    }
    private IEnumerator Appear(ClownEncounterKind kind, Transform point)
    {
        _current = kind;
        float side = Mathf.Sign(point.position.x - _gameCamera.transform.position.x);
        if (side == 0) side = 1;
        Vector3 entry = point.position;
        entry.x = _gameCamera.transform.position.x + side * (HalfCameraWidth() + .4f);
        _clown.transform.position = entry;
        _clown.color = Color.white;
        _clown.enabled = true;
        _clown.gameObject.SetActive(true);
        BeginFocus();
        yield return _actor.WalkTo(point.position);
        _clown.flipX = _player.transform.position.x < _clown.transform.position.x;
        if (kind == ClownEncounterKind.Watcher)
            yield return new WaitForSeconds(_silentWatchDuration);
        else
        {
            _ownsInput = !_input.IsBlocked;
            _input.Block();
            _player.Rigidbody.linearVelocity = Vector2.zero;
            _speaking = true;
            string phrase = kind == ClownEncounterKind.KitchenReturn ? "fastfood_clown_waiting" :
                kind == ClownEncounterKind.Ready ? "fastfood_clown_ready" : "fastfood_clown_forgotten";
            yield return DialogPhrases.Play(_dialog, _input, new[] { phrase }, _voice);
            _speaking = false;
            ReleaseInput();
        }
        MarkCompleted(kind);
        _focusActive = false;
        float elapsed = 0;
        float linger = _deliveryFinal ? .35f : _lingerDuration;
        while (elapsed < linger)
        {
            bool looksAway = (_player.transform.position.x < _clown.transform.position.x) == _player.SpriteRenderer.flipX;
            if (!Visible() || elapsed > 1 && looksAway) break;
            elapsed += Time.deltaTime;
            yield return null;
        }
        if (Visible())
        {
            Vector3 exit = _clown.transform.position;
            exit.x = _gameCamera.transform.position.x + side * (HalfCameraWidth() + .65f);
            yield return _actor.WalkTo(exit);
        }
        Cleanup(false);
        _deliveryFinal = false;
        _appearance = null;
        _cooldown = _encounterGap;
        _quietRemaining = _interactionGrace;
    }
    private void MarkCompleted(ClownEncounterKind kind)
    {
        if (kind == ClownEncounterKind.Watcher) _completedMask |= 1;
        else if (kind == ClownEncounterKind.KitchenReturn) _completedMask |= 2;
        else if (kind == ClownEncounterKind.Ready) _completedMask |= 7;
        else if (kind == ClownEncounterKind.IdleReminder) _idleDone = true;
        _idleElapsed = 0;
    }
    // Fast players still receive the final beat before the guest's completion dialogue.
    public IEnumerator PlayBeforeServing()
    {
        if (!isActiveAndEnabled || (_completedMask & 4) != 0 || !_service.Inventory.IsOrderReady || !_player.IsAlive) yield break;
        AbortAppearance(true);
        _deliveryFinal = true;
        _appearance = StartCoroutine(Appear(ClownEncounterKind.Ready, _finalPoint));
        while (_appearance != null) yield return null;
        _deliveryFinal = false;
    }
    private float HalfCameraWidth() => (_gameCamera.orthographic ? _gameCamera.orthographicSize :
        Mathf.Tan(_gameCamera.fieldOfView * Mathf.Deg2Rad * .5f) * Mathf.Abs(_gameCamera.transform.position.z - _clown.transform.position.z)) * _gameCamera.aspect;
    private bool Visible()
    {
        Vector3 p = _gameCamera.WorldToViewportPoint(_clown.bounds.center);
        return p.z > 0 && p.x > -.12f && p.x < 1.12f;
    }
    private void BeginFocus()
    {
        if (!_ownsCamera)
        {
            _baseCameraOffset = _cameraOffset.Offset;
            _cameraShift = _cameraVelocity = 0;
            _ownsCamera = true;
        }
        _focusActive = true;
    }
    private void AdvanceCamera(float delta)
    {
        if (!_ownsCamera) return;
        float desired = _focusActive ? Mathf.Clamp((_clown.transform.position.x - _player.transform.position.x) * _cameraWeight, -_maxCameraShift, _maxCameraShift) : 0;
        _cameraShift = Mathf.SmoothDamp(_cameraShift, desired, ref _cameraVelocity, _cameraBlendTime, float.PositiveInfinity, delta);
        _cameraOffset.Offset = _baseCameraOffset + Vector3.right * _cameraShift;
        if (!_focusActive && Mathf.Abs(_cameraShift) < .001f && Mathf.Abs(_cameraVelocity) < .001f) RestoreCamera();
    }
    private void RestoreCamera()
    {
        if (_ownsCamera && _cameraOffset != null) _cameraOffset.Offset = _baseCameraOffset;
        _ownsCamera = _focusActive = false;
        _cameraShift = _cameraVelocity = 0;
    }
    private void ReleaseInput()
    {
        if (_ownsInput && _input != null) _input.Unblock();
        _ownsInput = false;
    }
    private void Cleanup(bool immediateFocus)
    {
        if (_speaking && _dialog != null) _dialog.Disable();
        _speaking = false;
        ReleaseInput();
        _focusActive = false;
        if (immediateFocus) RestoreCamera();
        if (_animator != null && _animator.gameObject.activeInHierarchy) _animator.SetBool(nameof(AnimatorBool.IsWalking), false);
        if (_clown != null) _clown.gameObject.SetActive(false);
        _current = ClownEncounterKind.None;
    }
    private void AbortAppearance(bool immediateFocus)
    {
        if (_appearance != null) StopCoroutine(_appearance);
        _appearance = null;
        Cleanup(immediateFocus);
    }
    private void OnDisable()
    {
        _service.Changed -= Progress;
        _till.OrderCompleted -= LeftCounter;
        AbortAppearance(true);
        _deliveryFinal = false;
    }
}
