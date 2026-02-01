using UnityEngine;

public class Player : AEntity
{
    [Header("Слой, по которому ходит Игрок")]
    [SerializeField] private LayerMask _layerMask;

    private void FixedUpdate()
    {
        Collider2D playerCollider = GetComponent<Collider2D>();

        Vector2 circleCenter = (Vector2)transform.position - new Vector2(0, playerCollider.bounds.extents.y);
        float circleRadius = 0.1f;
        float castDistance = 0.1f;

        RaycastHit2D hit = Physics2D.CircleCast(
            circleCenter,
            circleRadius,
            Vector2.down,
            castDistance,
            _layerMask
        );

        IsOnGround = hit.collider != null;
    }

}