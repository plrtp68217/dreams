using UnityEngine;

public class InputService : MonoBehaviour, IInputService
{
    public float Direction { get; private set; }
    public bool SpaceIsPressed { get; private set; }
    public bool ShiftIsHolding { get; private set; }
    public bool ControlIsHolding { get;  set; }

    private void Update()
    {
        Direction = Input.GetAxis("Horizontal");

        SpaceIsPressed = Input.GetButton(KeyCode.Space.ToString());
        ShiftIsHolding = Input.GetButton(KeyCode.LeftShift.ToString());
        ControlIsHolding = Input.GetButton(KeyCode.LeftControl.ToString());


    }
}