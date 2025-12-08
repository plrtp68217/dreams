using UnityEngine;

public interface IInputService
{
    float Direction { get; }
    bool JumpIsPressed { get; }
}