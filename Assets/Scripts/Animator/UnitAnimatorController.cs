using UnityEngine;

public class UnitAnimatorController : MonoBehaviour
{
    [SerializeField] private Animator _entityAnimator;

    public void SetWalking(bool status)
    {
        _entityAnimator.SetBool(AnimatorBool.IsWalking.ToString(), status);
    }

    public void SetRunning(bool status)
    {
        _entityAnimator.SetBool(AnimatorBool.IsRunning.ToString(), status);
    }

    public void SetJumping()
    {
        _entityAnimator.SetTrigger(AnimatorTrigger.Jump.ToString());
    }
}