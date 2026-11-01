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
    public FastFoodStationKind Kind => _kind;
    public float Reach => _reach;
    public bool Available => _service.CanInteract && _player.IsAlive &&
        Mathf.Abs(_player.transform.position.y - transform.position.y) < 1.2f &&
        Mathf.Abs(_player.transform.position.x - transform.position.x) <= _reach;

    private void Update()
    {
        if (Time.timeScale <= 0 || _input.IsBlocked || _view.IsOpen || !Available) return;
        if (_view.NearestStation() != this) return;
        _view.ShowPrompt(this);
        if (_input.EIsPressed) _view.Open(this);
    }
}
