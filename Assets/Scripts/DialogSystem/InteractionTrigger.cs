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

    private Coroutine _coroutine;

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

    private void Update()
    {
        if (_inputService.EIsPressed && _isEntered)
        {
            _dialog.Enable(_text);
            _animator.SetTrigger("Reach");

            _coroutine = StartCoroutine(DisableDialog());
        }
    }

    private void OnDisable()
    {
        if (_coroutine != null)
        {
            StopCoroutine(_coroutine);
            _coroutine = null;
        }
    }

    private IEnumerator DisableDialog()
    {
        yield return new WaitForSeconds(_delayTime);

        _dialog.Disable();
    }
}