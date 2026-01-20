using UnityEngine;

public class RabbitSensor : MonoBehaviour
{
    [SerializeField] private Rabbit _rabbit;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Obstacle"))
        {
            _rabbit.Jump();
        }
    }
}