using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Player), typeof(Animator), typeof(SpriteRenderer))]
public class PlayerTurnAnimation : MonoBehaviour
{
    private static readonly int TurnState = Animator.StringToHash("Base Layer.Turn");

    [SerializeField] private InputService _inputService;
    [SerializeField] private PlayerController _controller;

    private Player _player;
    private Animator _animator;
    private SpriteRenderer _renderer;
    private bool _startFacingLeft;
    private bool _targetFacingLeft;

    public bool IsTurning { get; private set; }

    private void Awake()
    {
        _player = GetComponent<Player>();
        _animator = GetComponent<Animator>();
        _renderer = GetComponent<SpriteRenderer>();
    }

    public void FaceMovement(float direction)
    {
        if (Mathf.Abs(direction) < 0.01f) return;
        bool faceLeft = direction < 0f;

        // Keep airborne poses and jumps responsive instead of showing a standing turn.
        if (!_player.IsOnGround || _inputService.SpaceIsPressed || !_animator.HasState(0, TurnState))
        {
            IsTurning = false;
            _renderer.flipX = faceLeft;
            return;
        }
        if (IsTurning || _renderer.flipX == faceLeft || _animator.speed <= 0f) return;

        _startFacingLeft = _renderer.flipX;
        _targetFacingLeft = faceLeft;
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
        }
    }

    private void CancelTurn()
    {
        IsTurning = false;
        if (_animator.GetCurrentAnimatorStateInfo(0).fullPathHash == TurnState)
            _animator.Play(Animator.StringToHash("Base Layer.Idle"), 0, 0f);
    }

    private void OnDisable()
    {
        if (IsTurning) CancelTurn();
    }
}
