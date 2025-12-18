using UnityEngine;

public class Player : AEntity
{
    private Rigidbody2D _rigidBody;
    public Rigidbody2D Rigidbody => _rigidBody;

    private void Start()
    {
        _rigidBody = GetComponent<Rigidbody2D>();
    }
}