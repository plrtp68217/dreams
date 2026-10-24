using UnityEngine;

// Задаёт видимость курсора при загрузке сцены
public class CursorVisibility : MonoBehaviour
{
    [SerializeField] private bool _isVisible = true;

    private void Start()
    {
        Cursor.visible = _isVisible;
    }
}
