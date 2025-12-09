using UnityEngine;

public interface IInputService
{
    float Direction { get; }
    bool SpaceIsPressed { get; }
}