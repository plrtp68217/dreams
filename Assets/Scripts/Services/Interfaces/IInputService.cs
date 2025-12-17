using UnityEngine;

public interface IInputService
{
    float Direction { get; }
    bool SpaceIsPressed { get; }
    bool ShiftIsHolding { get; }
    bool ControlIsHolding { get; }
}