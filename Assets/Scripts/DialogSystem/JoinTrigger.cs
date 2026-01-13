using UnityEngine;

public class JoinTrigger : MonoBehaviour
{
    [TextArea(3, 10)]
    [SerializeField] private string _text;
    [SerializeField] private Dialog _dialog;

    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.CompareTag("Player"))
        {
            _dialog.Enable(_text);
        }
    }

    private void OnTriggerExit2D(Collider2D collider)
    {
        if (collider.CompareTag("Player"))
        {
            _dialog.Disable();
        }
    }
}