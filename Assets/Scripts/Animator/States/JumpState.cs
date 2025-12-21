public class JumpState : IAnimatorState
{
    private readonly AnimatorController _controller;
    private AEntity _entity;

    public JumpState(AnimatorController controller)
    {
        _controller = controller;
    }

    public void Enter()
    {
        _entity = _controller.GetComponent<AEntity>();
        _controller.Animator.SetBool("isJumping", true);
    }

    public void Exit()
    {
        _controller.Animator.SetBool("isJumping", false);
    }

    public void Update()
    {
        if (_entity.IsOnGround == true)
        {
            _controller.ChangeState(AnimatorStateType.Walking);
        }
    }
}
