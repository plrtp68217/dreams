using UnityEngine;

public class RabbitSensor : MonoBehaviour
{
    [SerializeField] private Rabbit _rabbit;
    [SerializeField] private CustomTag _triggerTag = CustomTag.Obstacle;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(_triggerTag.ToString()))
        {
            _rabbit.Jump();
        }
    }
}