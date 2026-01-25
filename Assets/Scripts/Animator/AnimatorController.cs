using UnityEngine;

public class AnimatorController : MonoBehaviour
{
    [SerializeField] private InputService _inputService;
    [SerializeField] private Animator _animator;
    [SerializeField] private Rigidbody2D _rigidbody;

    private AEntity _entity;
    private bool _wasOnGround = true;

    public bool VelocityXIsZero => Mathf.Abs(_rigidbody.linearVelocityX) <= 0.01f;

    private void Start()
    {
        _entity = GetComponent<AEntity>();
    }

    private void Update()
    {
        HandleMove();
        HandleJump();
        HandleCrouching();
    }

    private void HandleMove()
    {
        if (_inputService.Direction != 0)
        {
            _animator.SetBool("isWalking", true);
        }
        else
        {
            _animator.SetBool("isWalking", false);
        }
    }

    private void HandleJump()
    {
        if (_wasOnGround && _inputService.SpaceIsPressed)
        {
            _animator.SetTrigger("Jump");
        }

        _wasOnGround = _entity.IsOnGround;
    }

    private void HandleCrouching()
    {
        if (VelocityXIsZero)
        {
            _animator.SetBool("isCrouching", false);
            return;
        }

        if (_inputService.ControlIsHolding)
        {
            _animator.SetBool("isCrouching", true);
        }
        else
        {
            _animator.SetBool("isCrouching", false);
        }
    }
}