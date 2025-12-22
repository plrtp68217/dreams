using System.Collections.Generic;
using UnityEngine;

public class AnimatorController : MonoBehaviour
{
    [SerializeField] private InputService _inputService;

    private AnimatorStateType _currentState;
    private Dictionary<AnimatorStateType, IAnimatorState> _states;

    public Animator Animator { get; private set; }
    public InputService InputService => _inputService;

    public void ChangeState(AnimatorStateType newState)
    {
        if (_states.ContainsKey(_currentState))
            _states[_currentState].Exit();

        _currentState = newState;

        if (_states.ContainsKey(newState))
            _states[newState].Enter();
    }

    public void Update()
    {
        if (_states.ContainsKey(_currentState))
            _states[_currentState].Update();
    }
    
    private void Start()
    {
        Animator = GetComponent<Animator>();
        InitializeStates();
        ChangeState(AnimatorStateType.Idle);
    }

    private void InitializeStates()
    {
        _states = new Dictionary<AnimatorStateType, IAnimatorState>
        {
            { AnimatorStateType.Idle, new IdleState(this) },
            { AnimatorStateType.Walking, new WalkingState(this) },
            { AnimatorStateType.Jumping, new JumpState(this) },
            { AnimatorStateType.Crouching, new CrouchingState(this) },
        };
    }
}