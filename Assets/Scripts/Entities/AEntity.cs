using System;
using UnityEngine;
using UnityEngine.Splines;

public abstract class AEntity : MonoBehaviour, IDamageable
{
    private Animator _animator;

    public event Action OnDied;

    public bool IsOnGround { get; protected set; }
    public bool IsInShelter { get; protected set; }
    public bool IsAlive { get; protected set; } = true;

    private void Start()
    {
        TryGetComponent<Animator>(out _animator);
    }

    private void Update()
    {
        Debug.Log(IsAlive);
    }

    public void ChangeShelterStatus(bool inShelter)
    {
        IsInShelter = inShelter;
    }

    public virtual void Die()
    {
        if (IsAlive)
        {
            //_animator.SetTrigger("die");
            Destroy(gameObject);
            IsAlive = false;
            OnDied?.Invoke();
        }
    }

    public virtual void TakeDamage(float damage)
    {
        //_animator.SetTrigger("damage");
    }
}
