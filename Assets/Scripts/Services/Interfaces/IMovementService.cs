using UnityEngine;

public interface IMovementService
{
    void Move(Rigidbody2D rb, float direction);

    void Jump(Rigidbody2D rb);
}