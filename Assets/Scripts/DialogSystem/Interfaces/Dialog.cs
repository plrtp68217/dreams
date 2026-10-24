using UnityEngine;

public abstract class Dialog: MonoBehaviour 
{
    public abstract void Enable(string text = "");
    public abstract void Disable();

    // true, если текст ещё печатался и теперь показан целиком
    public virtual bool TryCompleteText() => false;
}