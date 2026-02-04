using UnityEngine;

public class AnimatorController : MonoBehaviour
{
    [SerializeField] private InputService _inputService;
    [SerializeField] private Animator _animator;
    [SerializeField] private Rigidbody2D _rigidbody;

    private AEntity _entity;
    private bool _wasOnGround = true;

    private void Start()
    {
        _entity = GetComponent<AEntity>();
    }

    private void Update()
    {
        HandleMove();
        HandleCrouching();
        HandleSitting();
        HandleJump();
    }

    private void HandleMove()
    {
        if (_inputService.Direction != 0 && _inputService.ControlIsHolding == false)
        {
            _animator.SetBool("isWalking", true);
        }
        else
        {
            _animator.SetBool("isWalking", false);
        }
    }

    private void HandleCrouching()
    {
        if (_inputService.Direction != 0 && _inputService.ControlIsHolding == true)
        {
            _animator.SetBool("isCrouching", true);
        }
        else
        {
            _animator.SetBool("isCrouching", false);
        }
    }

    private void HandleSitting()
    {
        if (_inputService.Direction == 0 && _inputService.ControlIsHolding == true)
        {
            _animator.SetBool("isSitting", true);
        }
        else
        {
            _animator.SetBool("isSitting", false);
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


}