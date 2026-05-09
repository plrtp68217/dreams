using System.Collections;
using UnityEngine;

public class JoinTrigger : MonoBehaviour
{
    [TextArea(3, 10)]
    [SerializeField] private string _text;
    [SerializeField] private Dialog _dialog;
    [SerializeField] private float _delay = 3f;


    private bool _isDialogActive = false;
    private WaitForSeconds _waiter;
    private Coroutine _coroutine;

    private void Awake()
    {
        _waiter = new WaitForSeconds(_delay);
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.TryGetComponent(out Player _) && _isDialogActive == false)
        {
            _isDialogActive = true;

            _dialog.Enable(_text);

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
        yield return _waiter;

        _dialog.Disable();
        _isDialogActive = false;
    }
}