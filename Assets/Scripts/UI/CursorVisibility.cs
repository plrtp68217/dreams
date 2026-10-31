using UnityEngine;

// Курсор доступен и в игре, и в меню.
public class CursorVisibility : MonoBehaviour
{
    private void Start()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }
}
