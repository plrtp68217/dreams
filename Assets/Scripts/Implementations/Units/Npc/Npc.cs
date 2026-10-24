using System.Collections;
using UnityEngine;

public class Npc : MonoBehaviour
{
    [SerializeField] private SpriteRenderer _spriteRenderer;
    [SerializeField] private Animator _animator;
    [SerializeField] private float _speed = 0.6f;

    private readonly float _arrivalDistance = 0.01f;

    public IEnumerator WalkTo(Vector3 target)
    {
        // Спрайты персонажей смотрят вправо
        _spriteRenderer.flipX = target.x < transform.position.x;
        SetWalking(true);

        while (Vector2.Distance(transform.position, target) > _arrivalDistance)
        {
            transform.position = Vector3.MoveTowards(transform.position, target, _speed * Time.deltaTime);
            yield return null;
        }

        transform.position = target;
        SetWalking(false);
    }

    private void SetWalking(bool status)
    {
        if (_animator != null)
        {
            _animator.SetBool(AnimatorBool.IsWalking.ToString(), status);
        }
    }
}
