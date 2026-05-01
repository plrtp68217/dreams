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
        HandleRunning();
        HandleCrouching();
        HandleSitting();
        HandleJump();
    }

    private void HandleMove()
    {
        if (_inputService.Direction != 0 && 
            _inputService.ControlIsHolding == false)
        {
            _animator.SetBool(AnimatorBool.IsWalking.ToString(), true);
        }
        else
        {
            _animator.SetBool(AnimatorBool.IsWalking.ToString(), false);
        }
    }

    private void HandleRunning()
    {
        if (_inputService.Direction != 0 && 
            _inputService.ControlIsHolding == false &&
            _inputService.ShiftIsHolding == true)
        {
            _animator.SetBool(AnimatorBool.IsRunning.ToString(), true);
        }
        else
        {
            _animator.SetBool(AnimatorBool.IsRunning.ToString(), false);
        }
    }

    private void HandleCrouching()
    {
        if (_inputService.Direction != 0 && 
            _inputService.ControlIsHolding == true)
        {
            _animator.SetBool(AnimatorBool.IsCrouching.ToString(), true);
        }
        else
        {
            _animator.SetBool(AnimatorBool.IsCrouching.ToString(), false);
        }
    }

    private void HandleSitting()
    {
        if (_inputService.Direction == 0 && 
            _inputService.ControlIsHolding == true)
        {
            _animator.SetBool(AnimatorBool.IsSitting.ToString(), true);
        }
        else
        {
            _animator.SetBool(AnimatorBool.IsSitting.ToString(), false);
        }
    }

    private void HandleJump()
    {
        if (_wasOnGround && _inputService.SpaceIsPressed)
        {
            _animator.SetTrigger(AnimatorTrigger.Jump.ToString());
        }

        _wasOnGround = _entity.IsOnGround;
    }
}