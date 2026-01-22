using UnityEngine;

public class RabbitTrigger : MonoBehaviour
{
    [SerializeField] private Rabbit _rabbit;
    [SerializeField] private RabbitState _state;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Rabbit") || other.CompareTag("Player"))
        {
            _rabbit.SetState(_state);
        }
    }
}