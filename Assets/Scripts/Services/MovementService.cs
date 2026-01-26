using UnityEngine;

public class MovementService : MonoBehaviour
{
    public void Move(Rigidbody2D rb, float direction, float speed)
    {
        if (rb == null) return;

        rb.linearVelocityX = direction * speed;
    }

    public void Jump(Rigidbody2D rb, float force)
    {
        if (rb == null) return;

        rb.linearVelocityY = force;
    }
}