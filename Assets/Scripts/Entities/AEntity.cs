using UnityEngine;

public abstract class AEntity : MonoBehaviour
{
    public bool IsOnGround { get; protected set; }
    public bool IsInShelter { get; protected set; }

    public void ChangeShelterStatus(bool inShelter)
    {
        IsInShelter = inShelter;
    }
}
