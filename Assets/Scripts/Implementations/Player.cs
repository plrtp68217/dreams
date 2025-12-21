using UnityEngine;

public class Player : AEntity
{
    public Rigidbody2D Rigidbody { get; private set; }
    public SpriteRenderer SpriteRenderer { get; private set; }

    private void Start()
    {
        Rigidbody = GetComponent<Rigidbody2D>();
        SpriteRenderer = GetComponent<SpriteRenderer>();
    }
}