using System;
using System.Collections;
using UnityEngine;

public class InputService : MonoBehaviour
{
    public const string Horizontal = "Horizontal";

    private bool isBlocked = false;
    private Coroutine _blockCoroutine;

    public float Direction { get; private set; }
    public bool EIsPressed { get; private set; }
    public bool SpaceIsPressed { get; private set; }
    public bool ShiftIsHolding { get; private set; }
    public bool ControlIsHolding { get; private set; }

    /*
     Input.GetKeyDown - один раз при нажатии
     Input.GetKey     - каждый кадр пока зажато
     Input.GetKeyUp   - один раз при отпускании
     */
    private void Update()
    {
        Direction = Input.GetAxis(Horizontal);

        EIsPressed = Input.GetKeyDown(KeyCode.E);
        SpaceIsPressed = Input.GetKeyDown(KeyCode.Space);

        ShiftIsHolding = Input.GetKey(KeyCode.LeftShift);
        ControlIsHolding = Input.GetKey(KeyCode.LeftControl);

        if (isBlocked == true)
        {
            Direction = 0;
            SpaceIsPressed = false;
        }
    }

    private void OnDisable()
    {
        if (_blockCoroutine != null)
        {
            StopCoroutine(_blockCoroutine);
            _blockCoroutine = null;
        }
    }
    public void Block()
    {
        if (_blockCoroutine != null)
        {
            StopCoroutine(_blockCoroutine);
            _blockCoroutine = null;
        }

        isBlocked = true;
    }

    public void Unblock()
    {
        if (_blockCoroutine != null)
        {
            StopCoroutine(_blockCoroutine);
            _blockCoroutine = null;
        }

        isBlocked = false;
    }

    public void BlockWithDelay(float delay)
    {
        _blockCoroutine = StartCoroutine(BLockWithDelayCoroutine(delay));
    }

    private IEnumerator BLockWithDelayCoroutine(float delay)
    {
        isBlocked = true;
        yield return new WaitForSeconds(delay);
        isBlocked = false;
    }
}