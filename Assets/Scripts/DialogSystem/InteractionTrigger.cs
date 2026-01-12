using System.Collections;
using UnityEngine;

public class InteractionTrigger : MonoBehaviour
{
    [TextArea(3, 10)]
    [SerializeField] private string _text;
    [SerializeField] private Dialog _dialog;
    [SerializeField] private Animator _animator;

    [SerializeField] private InputService _inputService;

    private bool _isEntered = false;
    private readonly float _delayTime = 5f;

    private void Update()
    {
        if (_inputService.EIsPressed && _isEntered)
        {
            _dialog.Enable(_text);
            _animator.SetTrigger("Reach");
            StartCoroutine(DisableDialog());
        }
    }

    private IEnumerator DisableDialog()
    {
        _isEntered = false;

        yield return new WaitForSeconds(_delayTime);

        _isEntered = true;

        _dialog.Disable();
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.CompareTag("Player"))
        {
            _isEntered = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collider)
    {
        if (collider.CompareTag("Player"))
        {
            _isEntered = false;
        }
    }
}