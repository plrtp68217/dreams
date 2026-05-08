using System.Collections;
using UnityEngine;

public class EyeTrigger : MonoBehaviour
{
    [SerializeField] private Eye _eye;
    [SerializeField] private CustomTag _targetTag = CustomTag.Unit;
    [SerializeField] private float _resetDelay = 2f;

    private bool _isEntered = false;
    private WaitForSeconds _waiter;
    private Coroutine _coroutine;

    private void Awake()
    {
        _waiter = new WaitForSeconds(_resetDelay);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        //if (_isEntered) return;

        if (collision.CompareTag(CustomTag.Unit.ToString()))
        {
            _eye.SetState(EyeState.Aim);

            _isEntered = true;

            ResetEyeWithDelay();
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

    private void ResetEyeWithDelay()
    {
        _coroutine = StartCoroutine(ResetEyeWithDelayRoutine());
    }

    private IEnumerator ResetEyeWithDelayRoutine()
    {
        yield return _waiter;

        _eye.SetState(EyeState.Swing);
    }
}