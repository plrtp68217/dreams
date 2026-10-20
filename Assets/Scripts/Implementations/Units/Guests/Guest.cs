using System.Collections;
using UnityEngine;

public class Guest : MonoBehaviour
{
    [SerializeField] private SpriteRenderer _spriteRenderer;
    [SerializeField] private float _speed = 0.6f;

    private readonly float _arrivalDistance = 0.01f;

    public IEnumerator WalkTo(Vector3 target)
    {
        // Спрайт посетителя смотрит вправо
        _spriteRenderer.flipX = target.x < transform.position.x;

        while (Vector2.Distance(transform.position, target) > _arrivalDistance)
        {
            transform.position = Vector3.MoveTowards(transform.position, target, _speed * Time.deltaTime);
            yield return null;
        }

        transform.position = target;
    }
}
