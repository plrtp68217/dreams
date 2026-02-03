using System.Collections;
using UnityEngine;

public class RabbitTrigger : MonoBehaviour
{
    [SerializeField] private Rabbit _rabbit;
    [SerializeField] private RabbitState _state;
    [SerializeField] private float _reactionDelay;

    [SerializeField] private CustomTag _actor;

    private bool _isActivated = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_isActivated == true) return;
        
        if (other.CompareTag(_actor.ToString()))
        {
            Debug.Log(CustomTag.Rabbit.ToString());
            StartCoroutine(ChangeState());
            _isActivated = true;
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