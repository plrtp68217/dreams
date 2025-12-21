public class WalkingState : IAnimatorState
{
    private readonly AnimatorController _controller;

    public WalkingState(AnimatorController controller)
    {
        _controller = controller;
    }

    public void Enter()
    {
        _controller.Animator.SetBool("isWalking", true);
    }

    public void Exit()
    {
        _controller.Animator.SetBool("isWalking", false);
    }

    public void Update()
    {
        if (_controller.InputService.Direction == 0)
        {
            _controller.ChangeState(AnimatorStateType.Idle);
        }
        else if (_controller.InputService.SpaceIsPressed)
        {
            _controller.ChangeState(AnimatorStateType.Jumping);
        }
        else if (_controller.InputService.ControlIsHolding)
        {
            _controller.ChangeState(AnimatorStateType.Crouching);
        }
    }
}
