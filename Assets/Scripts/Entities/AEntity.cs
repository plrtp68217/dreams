using UnityEngine;

public abstract class AEntity : MonoBehaviour
{
    public bool IsOnGround { get; private set; }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            Debug.Log("OnCollisionEnter2D");
            IsOnGround = true;
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            Debug.Log("OnCollisionExit2D");
            IsOnGround = false;
        }
    }
}
