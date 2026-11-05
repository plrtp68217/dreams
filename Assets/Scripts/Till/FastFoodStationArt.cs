using UnityEngine;

public class FastFoodStationArt : MonoBehaviour
{
    [SerializeField] private FacelessOrderService _service;
    [SerializeField] private SpriteRenderer _renderer;
    [SerializeField] private FastFoodStationKind _kind;
    [SerializeField] private Sprite _initial;
    [SerializeField] private Sprite _finished;
    private void OnEnable() { _service.Changed += Refresh; Refresh(); }
    private void Start() => Refresh();
    private void OnDisable() => _service.Changed -= Refresh;
    private void Refresh()
    {
        bool finished = _kind == FastFoodStationKind.Fryer ? _service.OilDrained :
            _kind == FastFoodStationKind.Fridge ? _service.FridgeUnlocked :
            _kind == FastFoodStationKind.Dispenser ? _service.DrinkReady : _service.MirrorLooked;
        _renderer.sprite = finished && _finished != null ? _finished : _initial;
    }
}
