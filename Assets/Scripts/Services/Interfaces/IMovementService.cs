using UnityEngine;

public interface IMovementService
{
    void Move(Rigidbody2D rb, float direction, float speed);

    void Jump(Rigidbody2D rb, float force);
}