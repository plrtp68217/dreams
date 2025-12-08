using UnityEngine;

public class MovementService : MonoBehaviour, IMovementService
{

    [Header("Settings")]
    [SerializeField] private float speed = 8f;
    [SerializeField] private float jumpForce = 12f;

    public void Move(Rigidbody2D rb, float direction)
    {
        if (rb == null) return;

        rb.linearVelocityX = direction * speed;
    }

    public void Jump(Rigidbody2D rb)
    {
        if (rb == null) return;

        rb.linearVelocityY = jumpForce;
    }
}
