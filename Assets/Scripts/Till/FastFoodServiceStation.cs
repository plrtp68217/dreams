using UnityEngine;

// Shares the scene InputService and existing E interaction convention. Distance
// selection avoids overlapping trigger prompts and does not change player physics.
public class FastFoodServiceStation : MonoBehaviour
{
    [SerializeField] private FacelessOrderService _service;
    [SerializeField] private FastFoodServiceView _view;
    [SerializeField] private Player _player;
    [SerializeField] private InputService _input;
    [SerializeField] private FastFoodStationKind _kind;
    [SerializeField, Min(.1f)] private float _reach = .43f;
    [SerializeField] private bool _onlyWhileTaskPending;
    public FastFoodStationKind Kind => _kind;
    public float Reach => _reach;
    public bool TaskPending
    {
        get
        {
            if (!_service.CanInteract) return false;
            switch (_kind)
            {
                case FastFoodStationKind.Fryer:
                    return _service.CanPerform(FacelessServiceAction.PowerOff) ||
                        _service.CanPerform(FacelessServiceAction.RemoveFuse) ||
                        _service.CanPerform(FacelessServiceAction.DrainOil) ||
                        _service.CanPerform(FacelessServiceAction.TakeBurger);
                case FastFoodStationKind.FuseBox:
                    return _service.CanPerform(FacelessServiceAction.RemoveFuse);
                case FastFoodStationKind.Fridge:
                    return !_service.Inventory.Has(FastFoodOrderItem.Fries);
                case FastFoodStationKind.Dispenser:
                    return !_service.Inventory.Has(FastFoodOrderItem.Drink);
                case FastFoodStationKind.Mirror:
                    return !_service.Inventory.Has(FastFoodOrderItem.Toy);
                case FastFoodStationKind.Guest:
                    return _service.Inventory.IsOrderReady;
                default: return false;
            }
        }
    }
    // Delivery belongs to TillTrigger; no competing guest interaction consumes the same E.
    public bool Available => _kind != FastFoodStationKind.Guest && _service.CanInteract && _player.IsAlive && (!_onlyWhileTaskPending || TaskPending) &&
        Mathf.Abs(_player.transform.position.y - transform.position.y) < 1.2f &&
        Mathf.Abs(_player.transform.position.x - transform.position.x) <= _reach;
    public bool CanHighlight => isActiveAndEnabled && TaskPending && Available &&
        Time.timeScale > 0 && !_input.IsBlocked && !_view.IsOpen && _view.NearestStation() == this;

    private void Update()
    {
        if (Time.timeScale <= 0 || _input.IsBlocked || _view.IsOpen || !Available) return;
        if (_view.NearestStation() != this) return;
        _view.ShowPrompt(this);
        if (_input.EIsPressed) _view.Open(this);
    }
}
