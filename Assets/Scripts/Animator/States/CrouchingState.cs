public class CrouchingState : IAnimatorState
{
    private readonly AnimatorController _controller;

    public CrouchingState(AnimatorController controller)
    {
        _controller = controller;
    }

    public void Enter()
    {
        _controller.Animator.SetBool("isCrouching", true);
    }

    public void Exit()
    {
        _controller.Animator.SetBool("isCrouching", false);
    }

    public void Update()
    {
        if (_controller.InputService.ControlIsHolding == false)
        {
            _controller.ChangeState(AnimatorStateType.Walking);
        }
    }
}