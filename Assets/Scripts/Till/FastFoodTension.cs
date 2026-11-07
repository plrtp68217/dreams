using System.Collections;
using TMPro;
using Unity.Cinemachine;
using UnityEngine;

public enum ClownEncounterKind { None, Watcher, KitchenReturn, AfterOrder, IdleReminder }

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
    [SerializeField, Min(.1f)] private float _lingerDuration = .25f;
    [SerializeField, Min(1)] private float _exitSpeedMultiplier = 2.5f;
    [SerializeField, Min(.1f)] private float _cameraBlendTime = 1f;
    [SerializeField, Range(0, 1)] private float _cameraWeight = .32f;
    [SerializeField, Min(0)] private float _maxCameraShift = .55f;
    private Coroutine _appearance;
    private ClownEncounterKind _current;
    private int _completedMask;
    private bool _idleDone, _foodSeenInKitchen, _returnPending, _wasInKitchen;
    private bool _orderStarted, _ownsInput, _speaking, _postService;
    private bool _ownsCamera, _focusActive;
    private Vector3 _baseCameraOffset;
    private float _cameraShift, _cameraVelocity;
    private float _quietRemaining, _cooldown, _idleElapsed;
    private float _exitAnimatorSpeed;
    private bool _ownsExitSpeed;
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
        if (!_player.IsAlive || (_service.IsCompleted && !_postService) || (!_service.IsActive && !_postService) ||
            (_service.IsServing && !_postService))
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
        bool busyElsewhere = !InHall || _view.IsOpen || (_input.IsBlocked && !_ownsInput && !_postService);
        if (busyElsewhere)
        {
            _quietRemaining = _interactionGrace;
            if (_appearance != null) AbortAppearance(true);
            return;
        }
        if (_postService || _appearance != null) return;
        if (!_till.IsCompleted || (_completedMask & 4) != 0) return;
        _quietRemaining = Mathf.Max(0, _quietRemaining - Time.deltaTime);
        if (_quietRemaining > 0) return;
        ClownEncounterKind next = NextEncounter();
        if (next == ClownEncounterKind.None) return;
        if (TryChoosePoint(next, out var point)) _appearance = StartCoroutine(Appear(next, point));
    }
    private ClownEncounterKind NextEncounter()
    {
        if (_cooldown > 0) return ClownEncounterKind.None;
        if ((_completedMask & 1) == 0) return ClownEncounterKind.Watcher;
        if (_returnPending && (_completedMask & 2) == 0) return ClownEncounterKind.KitchenReturn;
        if (!_idleDone && _idleElapsed >= _idleDelay) return ClownEncounterKind.IdleReminder;
        return ClownEncounterKind.None;
    }
    private bool TryChoosePoint(ClownEncounterKind kind, out Vector3 position)
    {
        position = _finalPoint.position;
        if (kind == ClownEncounterKind.AfterOrder && CanStandInView(position)) return true;
        float preferred = kind == ClownEncounterKind.Watcher ? 1.9f : 1.3f;
        Transform selected = null;
        float best = float.PositiveInfinity;
        foreach (var point in _hallPoints)
        {
            float distance = Mathf.Abs(point.position.x - _player.transform.position.x);
            if (distance < .85f || distance > 2.4f || !CanStandInView(point.position)) continue;
            float score = Mathf.Abs(distance - preferred);
            if (score < best) { selected = point; best = score; }
        }
        if (selected != null) { position = selected.position; return true; }

        // A fast turn or a narrow view can leave every authored point outside the shot.
        // Use the same floor/depth, placing the visitor inside the actual camera projection.
        Vector3 floor = kind == ClownEncounterKind.AfterOrder || _hallPoints.Length == 0 ? _finalPoint.position : _hallPoints[0].position;
        Vector3 centerOffset = SpriteCenterOffset();
        bool found = false;
        best = float.PositiveInfinity;
        for (int side = -1; side <= 1; side += 2)
        {
            Vector3 candidate = floor;
            candidate.x = _player.transform.position.x + side * preferred;
            Vector3 viewport = _gameCamera.WorldToViewportPoint(candidate + centerOffset);
            if (viewport.z <= 0) continue;
            var left = _gameCamera.ViewportToWorldPoint(new Vector3(.22f, viewport.y, viewport.z));
            var right = _gameCamera.ViewportToWorldPoint(new Vector3(.78f, viewport.y, viewport.z));
            float minimumDistance = Mathf.Min(.6f, Mathf.Abs(right.x - left.x) * .35f);
            viewport.x = Mathf.Clamp(viewport.x, .22f, .78f);
            candidate.x = (_gameCamera.ViewportToWorldPoint(viewport) - centerOffset).x;
            float distance = Mathf.Abs(candidate.x - _player.transform.position.x);
            if (distance < minimumDistance || distance > 2.4f || !CanStandInView(candidate)) continue;
            float score = Mathf.Abs(distance - preferred) + (side == (_player.SpriteRenderer.flipX ? -1 : 1) ? 0 : .1f);
            if (score < best) { position = candidate; best = score; found = true; }
        }
        return found;
    }
    private Vector3 SpriteCenterOffset()
    {
        if (_clown.sprite == null) return Vector3.up * .5f;
        var center = _clown.sprite.bounds.center;
        if (_clown.flipX) center.x = -center.x;
        if (_clown.flipY) center.y = -center.y;
        return _clown.transform.TransformVector(center);
    }
    private bool CanStandInView(Vector3 position)
    {
        Vector3 viewport = _gameCamera.WorldToViewportPoint(position + SpriteCenterOffset());
        return viewport.z > 0 && viewport.x >= .15f && viewport.x <= .85f && viewport.y >= .08f && viewport.y <= .92f;
    }
    private IEnumerator Appear(ClownEncounterKind kind, Vector3 point)
    {
        // This encounter owns movement until the actor has left the screen.
        // A surrounding sequence may already own input; never release its block.
        _ownsInput = !_input.IsBlocked;
        _input.Block();
        _player.Rigidbody.linearVelocity = Vector2.zero;
        _current = kind;
        float side = Mathf.Sign(point.x - _gameCamera.transform.position.x);
        if (side == 0) side = 1;
        Vector3 entry = point;
        float actorHalfWidth = _clown.sprite != null ? _clown.sprite.bounds.extents.x * Mathf.Abs(_clown.transform.lossyScale.x) : .3f;
        entry.x = _gameCamera.transform.position.x + side * (HalfCameraWidth() + actorHalfWidth + .2f);
        _clown.transform.position = entry;
        _clown.color = Color.white;
        _clown.enabled = true;
        _clown.gameObject.SetActive(true);
        BeginFocus();
        yield return _actor.WalkTo(point);
        // Camera follow can still be settling after the player's last movement.
        // Recheck at arrival; never open a dialogue for a visitor outside the shot.
        while (!CanStandInView(_clown.transform.position))
        {
            if (!TryChoosePoint(kind, out point))
            {
                _focusActive = false;
                yield return Depart(side);
                Cleanup(true);
                _appearance = null;
                _quietRemaining = _interactionGrace;
                yield break;
            }
            yield return _actor.WalkTo(point);
        }
        _clown.flipX = _player.transform.position.x < _clown.transform.position.x;
        if (kind == ClownEncounterKind.Watcher)
            yield return new WaitForSeconds(_silentWatchDuration);
        else
        {
            _speaking = true;
            string phrase = kind == ClownEncounterKind.KitchenReturn ? "fastfood_clown_waiting" :
                kind == ClownEncounterKind.AfterOrder ? "fastfood_clown_after_order" : "fastfood_clown_forgotten";
            yield return DialogPhrases.Play(_dialog, _input, new[] { phrase }, _voice);
            _speaking = false;
        }
        MarkCompleted(kind);
        _focusActive = false;
        float elapsed = 0;
        float linger = _lingerDuration;
        while (elapsed < linger)
        {
            bool looksAway = (_player.transform.position.x < _clown.transform.position.x) == _player.SpriteRenderer.flipX;
            if (!Visible() || elapsed > 1 && looksAway) break;
            elapsed += Time.deltaTime;
            yield return null;
        }
        if (Visible())
        {
            yield return Depart(side);
        }
        Cleanup(false);
        _appearance = null;
        _cooldown = _encounterGap;
        _quietRemaining = _interactionGrace;
    }
    private IEnumerator Depart(float fallbackSide)
    {
        float side = Mathf.Sign(_clown.transform.position.x - _gameCamera.transform.position.x);
        if (side == 0) side = fallbackSide;
        Vector3 exit = _clown.transform.position;
        exit.x = _gameCamera.transform.position.x + side * (HalfCameraWidth() + _clown.bounds.extents.x + _maxCameraShift + .15f);
        _exitAnimatorSpeed = _animator.speed;
        _ownsExitSpeed = true;
        _animator.speed = _exitAnimatorSpeed * _exitSpeedMultiplier;
        yield return _actor.WalkTo(exit, _exitSpeedMultiplier);
    }
    private void MarkCompleted(ClownEncounterKind kind)
    {
        if (kind == ClownEncounterKind.Watcher) _completedMask |= 1;
        else if (kind == ClownEncounterKind.KitchenReturn) _completedMask |= 2;
        else if (kind == ClownEncounterKind.AfterOrder) _completedMask |= 4;
        else if (kind == ClownEncounterKind.IdleReminder) _idleDone = true;
        _idleElapsed = 0;
    }
    // Called only by the counter sequence after the guest's exit coroutine completes.
    public IEnumerator PlayAfterServing()
    {
        if (!isActiveAndEnabled || (_completedMask & 4) != 0 || !_service.IsCompleted || !_player.IsAlive) yield break;
        AbortAppearance(true);
        _postService = true;
        yield return new WaitForSeconds(.45f);
        if (!_postService || !isActiveAndEnabled || !_player.IsAlive) yield break;
        Vector3 point;
        while (!TryChoosePoint(ClownEncounterKind.AfterOrder, out point))
        {
            if (!_postService || !isActiveAndEnabled || !_player.IsAlive) yield break;
            yield return null;
        }
        _appearance = StartCoroutine(Appear(ClownEncounterKind.AfterOrder, point));
        while (_appearance != null) yield return null;
        _postService = false;
    }
    public void CancelAfterServing()
    {
        if (!_postService) return;
        AbortAppearance(true);
        _postService = false;
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
        _focusActive = false;
        if (immediateFocus) RestoreCamera();
        if (_ownsExitSpeed && _animator != null) _animator.speed = _exitAnimatorSpeed;
        _ownsExitSpeed = false;
        if (_animator != null && _animator.gameObject.activeInHierarchy) _animator.SetBool(nameof(AnimatorBool.IsWalking), false);
        if (_clown != null) _clown.gameObject.SetActive(false);
        ReleaseInput();
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
        _postService = false;
    }
}
