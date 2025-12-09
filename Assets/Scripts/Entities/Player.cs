using UnityEngine;

public class Player : MonoBehaviour
{
    public bool IsOnGround { get; private set; }
    public Rigidbody2D Rigidbody { get; private set; }

    private void Start()
    {
        Rigidbody = GetComponent<Rigidbody2D>();

    }

    private void Update()
    {
        Debug.Log(IsOnGround);
    }

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