using System;
using UnityEngine;

public abstract class AEntity : MonoBehaviour, IDamageable
{
    public event Action OnDied;

    public Rigidbody2D Rigidbody { get; protected set; }
    public SpriteRenderer SpriteRenderer { get; protected set; }
    public Animator Animator { get; protected set; }

    public bool IsOnGround { get; protected set; }
    public bool IsInShelter { get; protected set; }
    public bool IsAlive { get; protected set; } = true;

    private void Start()
    {
        Rigidbody = GetComponent<Rigidbody2D>();
        SpriteRenderer = GetComponent<SpriteRenderer>();
        Animator = GetComponent<Animator>();
    }

    public void ChangeShelterStatus(bool inShelter)
    {
        IsInShelter = inShelter;
    }

    public virtual void Die()
    {
        if (IsAlive)
        {
            Debug.Log("УМИРАЮЮЮЮ.");

            SetAlive(false);
            SetVisibility(false);

            OnDied?.Invoke();
        }
    }

    public virtual void TakeDamage(float damage)
    {
    }

    public void SetAlive(bool alive) => IsAlive = alive;
    public void SetVisibility(bool isVisible) => SpriteRenderer.enabled = isVisible;
}