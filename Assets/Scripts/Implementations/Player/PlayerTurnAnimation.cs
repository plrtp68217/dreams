using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Player), typeof(Animator), typeof(SpriteRenderer))]
public class PlayerTurnAnimation : MonoBehaviour
{
    private static readonly int TurnState = Animator.StringToHash("Base Layer.Turn");
    private static readonly int TurnSpeed = Animator.StringToHash(AnimatorFloat.TurnSpeed.ToString());

    [SerializeField] private InputService _inputService;
    [SerializeField] private PlayerController _controller;
    [SerializeField, Min(1f)] private float _rapidTurnSpeed = 3f;
    [SerializeField, Min(0f)] private float _rapidTurnWindow = 0.35f;

    private Player _player;
    private Animator _animator;
    private SpriteRenderer _renderer;
    private bool _startFacingLeft;
    private bool _targetFacingLeft;
    private bool _hasTurnSpeed;
    private float _lastTurnTime = float.NegativeInfinity;

    public bool IsTurning { get; private set; }

    private void Awake()
    {
        _player = GetComponent<Player>();
        _animator = GetComponent<Animator>();
        _renderer = GetComponent<SpriteRenderer>();
        foreach (var parameter in _animator.parameters)
            if (parameter.nameHash == TurnSpeed && parameter.type == AnimatorControllerParameterType.Float)
                _hasTurnSpeed = true;
    }

    public void FaceMovement(float direction)
    {
        if (Mathf.Abs(direction) < 0.01f) return;
        bool faceLeft = direction < 0f;

        // A standing turn must never replace a sitting/crouching pose.
        if (_inputService.ControlIsHolding)
        {
            if (IsTurning) CancelTurn();
            _renderer.flipX = faceLeft;
            return;
        }

        // Keep airborne poses and jumps responsive instead of showing a standing turn.
        if (!_player.IsOnGround || _inputService.SpaceIsPressed || !_animator.HasState(0, TurnState))
        {
            IsTurning = false;
            SetTurnSpeed(1f);
            _renderer.flipX = faceLeft;
            return;
        }
        if (IsTurning)
        {
            if (faceLeft != _targetFacingLeft)
            {
                // Reverse from the matching pose instead of queueing another full turn.
                var state = _animator.GetCurrentAnimatorStateInfo(0);
                float progress = state.fullPathHash == TurnState ? Mathf.Clamp01(state.normalizedTime) : 0f;
                _startFacingLeft = _targetFacingLeft;
                _targetFacingLeft = faceLeft;
                _lastTurnTime = Time.time;
                SetTurnSpeed(_rapidTurnSpeed);
                _animator.Play(TurnState, 0, Mathf.Clamp01(1f - progress - 0.001f));
            }
            return;
        }
        if (_renderer.flipX == faceLeft || _animator.speed <= 0f) return;

        _startFacingLeft = _renderer.flipX;
        _targetFacingLeft = faceLeft;
        SetTurnSpeed(Time.time - _lastTurnTime <= _rapidTurnWindow ? _rapidTurnSpeed : 1f);
        _lastTurnTime = Time.time;
        IsTurning = true;
        _animator.Play(TurnState, 0, 0f);
    }

    private void LateUpdate()
    {
        if (!IsTurning || Time.timeScale <= 0f) return;
        if (!_player.IsAlive || _inputService.IsBlocked || !_controller.enabled)
        {
            // Routes and cutscenes own the facing direction while controls are blocked.
            CancelTurn();
            return;
        }
        if (_inputService.ControlIsHolding)
        {
            _renderer.flipX = _targetFacingLeft;
            CancelTurn();
            return;
        }

        if (_animator.GetCurrentAnimatorStateInfo(0).fullPathHash == TurnState)
        {
            // The strip starts facing right and ends facing left. Mirror the whole
            // strip for the opposite direction, then flip once locomotion resumes.
            _renderer.flipX = _startFacingLeft;
        }
        else
        {
            _renderer.flipX = _targetFacingLeft;
            IsTurning = false;
            SetTurnSpeed(1f);
        }
    }

    private void CancelTurn()
    {
        IsTurning = false;
        SetTurnSpeed(1f);
        if (_animator.GetCurrentAnimatorStateInfo(0).fullPathHash == TurnState)
        {
            string pose = _inputService.ControlIsHolding
                ? (Mathf.Abs(_inputService.Direction) > 0.01f ? "Crouching" : "Sitting")
                : "Idle";
            _animator.Play(Animator.StringToHash("Base Layer." + pose), 0, 0f);
        }
    }

    private void SetTurnSpeed(float speed)
    {
        if (_hasTurnSpeed) _animator.SetFloat(TurnSpeed, speed);
    }

    private void OnDisable()
    {
        if (IsTurning) CancelTurn();
    }
}
