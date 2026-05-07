using System.Collections;
using UnityEngine;

public class RabbitTrigger : MonoBehaviour
{
    [SerializeField] private Rabbit _rabbit;
    [SerializeField] private RabbitState _state;
    [SerializeField] private float _reactionDelay;

    [SerializeField] private CustomTag _actor;

    private bool _isActivated = false;
    private Coroutine _coroutine;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_isActivated == true) return;
        
        if (other.CompareTag(_actor.ToString()))
        {
            _coroutine = StartCoroutine(ChangeState());
            _isActivated = true;
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

    private IEnumerator ChangeState()
    {
        yield return new WaitForSeconds(_reactionDelay);
        
        if (_rabbit != null)
        {
            _rabbit.SetState(_state);
        }
    }
}