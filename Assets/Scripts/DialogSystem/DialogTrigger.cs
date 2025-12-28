using System;
using UnityEngine;

public class DialogTrigger : MonoBehaviour
{
    [TextArea(3, 10)]
    [SerializeField] private string _text;

    public static event Action<string> ActionDialogEnter;
    public static event Action ActionDialogExit;

    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.CompareTag("Player"))
        {
            ActionDialogEnter?.Invoke(_text);
        }
    }

    private void OnTriggerExit2D(Collider2D collider)
    {
        if (collider.CompareTag("Player"))
        {
            ActionDialogExit?.Invoke();
        }
    }
} 