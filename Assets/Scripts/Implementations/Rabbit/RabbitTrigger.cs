using System.Collections;
using UnityEngine;

public class RabbitTrigger : MonoBehaviour
{
    [SerializeField] private Rabbit _rabbit;
    [SerializeField] private RabbitState _state;

    [SerializeField] private float _reactionDelay;

    private IEnumerator OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Rabbit") || other.CompareTag("Player"))
        {
            yield return new WaitForSeconds(_reactionDelay);
            _rabbit.SetState(_state);
        }
    }
}