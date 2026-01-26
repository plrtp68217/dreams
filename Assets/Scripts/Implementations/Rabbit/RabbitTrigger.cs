using System.Collections;
using UnityEngine;

public class RabbitTrigger : MonoBehaviour
{
    [SerializeField] private Rabbit _rabbit;
    [SerializeField] private RabbitState _state;
    [SerializeField] private float _reactionDelay;

    private bool _isActivated = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_isActivated == true) return;
        
        if (other.CompareTag("Rabbit") || other.CompareTag("Player"))
        {
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