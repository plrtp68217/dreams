using UnityEngine;

public class JoinTrigger : MonoBehaviour
{
    [TextArea(3, 10)]
    [SerializeField] private string _text;
    [SerializeField] private Dialog _dialog;

    private bool _isShowed = false;

    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.CompareTag("Player") && _isShowed == false)
        {
            _dialog.Enable(_text);
            _isShowed = true;
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