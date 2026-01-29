using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private Player _player;

    [SerializeField] private InputService _inputService;
    [SerializeField] private MovementService _movementService;

    [SerializeField] private float _speed = 8f;
    [SerializeField] private float _jumpForce = 12f;

    [SerializeField] private float _accelerationMultiplier = 2f;
    [SerializeField] private float _decelerationMultiplier = 0.5f;

    private readonly float defaultMultiplier = 1.0f;

    public float SpeedMultiplier
    {
        get
        {
            if (_inputService.ShiftIsHolding) return _accelerationMultiplier;
            if (_inputService.ControlIsHolding) return _decelerationMultiplier;
            return defaultMultiplier;
        }
    }

    private void Update()
    {
        if (_player.IsAlive == false) return;

        if (_inputService.Direction != 0)
        {
            _player.SpriteRenderer.flipX = _inputService.Direction < 0;
        }

        if (_player.IsOnGround)
        {
            _movementService.Move(
                    _player.Rigidbody,
                    _inputService.Direction,
                    _speed * SpeedMultiplier
                );
        }

        if (_inputService.SpaceIsPressed && _player.IsOnGround)
        {
            _movementService.Jump(_player.Rigidbody, _jumpForce);
        }
    }
}