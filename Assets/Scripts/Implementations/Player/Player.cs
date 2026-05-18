using UnityEngine;

public class Player : AEntity
{
    [Header("Слой, по которому ходит Игрок")]
    [SerializeField] private LayerMask _layerMask;

    private readonly float _circleCastRadius = 0.1f;
    private readonly float _circleCastDistance = 0.1f;

    private Collider2D _playerCollider;

    private void Awake()
    {
        _playerCollider = GetComponent<Collider2D>();
    }

    private void FixedUpdate()
    {
        Vector2 circleCastCenter = (Vector2)transform.position - new Vector2(0, _playerCollider.bounds.extents.y);

        RaycastHit2D hit = Physics2D.CircleCast(
            circleCastCenter,
            _circleCastRadius,
            Vector2.down,
            _circleCastDistance,
            _layerMask
        );

        IsOnGround = hit.collider != null;
    }
}