using System.Collections;
using UnityEngine;

public class InteractionTrigger : MonoBehaviour
{
    [TextArea(3, 10)]
    [SerializeField] private string _text;

    [SerializeField] private Dialog _dialog;
    [SerializeField] private Animator _animator;
    [SerializeField] private InputService _inputService;

    private readonly float _delayTime = 5f;

    private bool _isEntered = false;
    private Coroutine _coroutine;
    private WaitForSeconds _waiter;

    private void Awake()
    {
        _waiter = new WaitForSeconds(_delayTime);
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.TryGetComponent(out Player _))
        {
            _isEntered = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collider)
    {
        if (collider.TryGetComponent(out Player _))
        {
            _isEntered = false;
        }
    }

    private void Update()
    {
        if (_inputService.EIsPressed && _isEntered)
        {
            _dialog.Enable(_text);
            _animator.SetTrigger(AnimatorTrigger.Reach.ToString());

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
    }
}