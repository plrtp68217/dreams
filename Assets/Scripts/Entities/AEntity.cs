using UnityEngine;

public abstract class AEntity : MonoBehaviour
{
    public bool IsOnGround { get; protected set; }

    //private void OnCollisionEnter2D(Collision2D collision)
    //{
    //    if (collision.gameObject.CompareTag("Ground"))
    //    {
    //        IsOnGround = true;
    //    }
    //}

    //private void OnCollisionExit2D(Collision2D collision)
    //{
    //    if (collision.gameObject.CompareTag("Ground"))
    //    {
    //        IsOnGround = false;
    //    }
    //}
}
