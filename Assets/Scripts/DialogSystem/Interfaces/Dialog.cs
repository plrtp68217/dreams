using UnityEngine;

public abstract class Dialog: MonoBehaviour 
{
    public abstract void Enable(string text);
    public abstract void Disable();
}