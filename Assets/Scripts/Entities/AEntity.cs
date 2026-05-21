using System;
using System.Collections;
using UnityEngine;

public abstract class AEntity : MonoBehaviour, IDamageable
{
    public event Action Died;

    public Rigidbody2D Rigidbody { get; protected set; }
    public SpriteRenderer SpriteRenderer { get; protected set; }
    public Animator Animator { get; protected set; }
    public Collider2D Collider { get; protected set; }

    private readonly float _invincibilityTime = 0.5f;

    public bool IsOnGround { get; protected set; }
    public bool IsAlive { get; protected set; } = true;
    public bool IsInShelter { get; protected set; }
    public bool IsInvincible { get; protected set; } = false;

    private void Start()
    {
        Rigidbody = GetComponent<Rigidbody2D>();
        SpriteRenderer = GetComponent<SpriteRenderer>();
        Animator = GetComponent<Animator>();
        Collider = GetComponent<Collider2D>();
    }

    public void ChangeShelterStatus(bool inShelter)
    {
        IsInShelter = inShelter;
    }

    public virtual void Die()
    {
        if (IsAlive && IsInvincible == false)
        {
            SetAlive(false);
            SetVisibility(false);

            Died?.Invoke();
        }
    }

    public virtual void TakeDamage(float damage) { }

    public void SetAlive(bool alive)
    {
        IsAlive = alive;
    }

    public void SetVisibility(bool isVisible)
    {
        if (SpriteRenderer != null)
        {
            SpriteRenderer.enabled = isVisible;
        }
    }

    public void StartInvincibility()
    {
        StartCoroutine(StartInvincibilityRoutine());
    }

    private IEnumerator StartInvincibilityRoutine()
    {
        IsInvincible = true;
        yield return new WaitForSeconds(_invincibilityTime);
        IsInvincible = false;
    }
}