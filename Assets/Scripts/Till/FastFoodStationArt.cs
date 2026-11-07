using UnityEngine;

public class FastFoodStationArt : MonoBehaviour
{
    [SerializeField] private FacelessOrderService _service;
    [SerializeField] private SpriteRenderer _renderer;
    [SerializeField] private FastFoodStationKind _kind;
    [SerializeField] private Sprite _initial;
    [SerializeField] private Sprite _finished;
    [SerializeField] private Sprite _collected;
    private void OnEnable() { _service.Changed += Refresh; Refresh(); }
    private void Start() => Refresh();
    private void OnDisable() => _service.Changed -= Refresh;
    private void Refresh()
    {
        if (_kind == FastFoodStationKind.Mirror && _collected != null &&
            (_service.Inventory.Has(FastFoodOrderItem.Toy) || _service.IsCompleted))
        {
            _renderer.sprite = _collected;
            return;
        }
        bool finished = _kind == FastFoodStationKind.Fryer ? _service.OilDrained :
            _kind == FastFoodStationKind.Fridge ? _service.FridgeUnlocked :
            _kind == FastFoodStationKind.Dispenser ? _service.DrinkReady : _service.MirrorLooked;
        _renderer.sprite = finished && _finished != null ? _finished : _initial;
    }
}
