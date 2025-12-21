public class IdleState : IAnimatorState
{
    private readonly AnimatorController _controller;

    public IdleState(AnimatorController controller)
    {
        _controller = controller;
    }

    public void Enter()
    {
    }

    public void Exit()
    {
    }

    public void Update()
    {
        if (_controller.InputService.SpaceIsPressed)
        {
            _controller.ChangeState(AnimatorStateType.Jumping);
        }
        else if (_controller.InputService.Direction != 0)
        {
            _controller.ChangeState(AnimatorStateType.Walking);
        }
    }
}