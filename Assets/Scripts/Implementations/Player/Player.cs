using UnityEngine;

public class Player : AEntity
{
    public Rigidbody2D Rigidbody { get; private set; }
    public SpriteRenderer SpriteRenderer { get; private set; }

    [SerializeField] private LayerMask _layerMask;

    private readonly float _checkDistance = .5f;

    private void Start()
    {
        Rigidbody = GetComponent<Rigidbody2D>();
        SpriteRenderer = GetComponent<SpriteRenderer>();
    }

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