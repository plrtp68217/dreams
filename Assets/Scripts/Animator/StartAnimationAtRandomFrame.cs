using UnityEngine;

public class StartAnimationAtRandomFrame : MonoBehaviour
{
    void Start()
    {
        var animator = GetComponent<Animator>();
        var stateInfo = animator.GetCurrentAnimatorStateInfo(0);

        animator.Play(stateInfo.fullPathHash, 0, Random.Range(0f, 1f));
    }
}