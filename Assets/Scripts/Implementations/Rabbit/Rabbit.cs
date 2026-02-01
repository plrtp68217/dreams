using UnityEngine;

public enum RabbitState
{
    Patrolling,
    Chasing,
}

public class Rabbit : AEntity
{
    [SerializeField] private MovementService _movementService;

    [SerializeField] private float _patrollingSpeed;
    [SerializeField] private float _patrollingFacingTime;

    [SerializeField] private float _chasingSpeed;
    [SerializeField] private float _jumpForce;

    private float _currentDirection = 1f;
    private float _elapsedTime = 0f;

    private RabbitState _currentState = RabbitState.Patrolling;

    public void Jump()
    {
        _movementService.Jump(Rigidbody, _jumpForce);
    }

    public void SetState(RabbitState state)
    {
        _currentState = state;
    }

    private void Update()
    {
        if (_currentState == RabbitState.Patrolling)
        {
            Patrolling();
        }
        else if (_currentState == RabbitState.Chasing)
        {
            Chasing();
        }
    }

    private void Patrolling()
    {
        _elapsedTime += Time.deltaTime;

        if (_elapsedTime >= _patrollingFacingTime)
        {
            _elapsedTime = 0f;

            _currentDirection *= -1f;

            HandleFlip();
        }

        Move(_patrollingSpeed);
    }

    private void Chasing()
    {
        _currentDirection = 1f;
        HandleFlip();
        Move(_chasingSpeed);
    }

    private void Move(float speed)
    {
        _movementService.Move(Rigidbody, _currentDirection, speed);
    }

    private void HandleFlip()
    {
        Vector3 scale = transform.localScale;
        scale.x = Mathf.Abs(scale.x) * _currentDirection;
        transform.localScale = scale;
    }
}