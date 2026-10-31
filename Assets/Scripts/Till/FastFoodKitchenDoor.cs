using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class FastFoodKitchenDoor : MonoBehaviour
{
    [SerializeField] private FastFoodKitchenRoute _route;
    [SerializeField] private bool _toKitchen;

    private void OnTriggerEnter2D(Collider2D other) => TryTravel(other);
    private void OnTriggerStay2D(Collider2D other) => TryTravel(other);

    private void TryTravel(Collider2D other)
    {
        if (other.TryGetComponent(out Player player))
            _route.TryTravel(player, _toKitchen);
    }
}
