using UnityEngine;

public class InputService : MonoBehaviour, IInputService
{
    public float Direction { get; private set; }
    public bool JumpIsPressed { get; private set; }

    private void Update()
    {
        Direction = Input.GetAxis("Horizontal");

        JumpIsPressed = Input.GetButtonDown("Jump");
    }
}