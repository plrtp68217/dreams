using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private Player _player;
    [SerializeField] private InputService _inputService;
    [SerializeField] private MovementService _movementService;

    private void Update()
    {
        float direction = _inputService.Direction;
        bool jumpIsPressed = _inputService.JumpIsPressed;

        _movementService.Move(_player.Rigidbody, direction);

        if (jumpIsPressed && _player.IsOnGround)
        {
            _movementService.Jump(_player.Rigidbody);
        }
    }
}