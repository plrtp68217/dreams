using UnityEngine;

public class StatementEye : MonoBehaviour
{
    [Header("Raycast Settings")]
    [SerializeField] private LayerMask damageableLayers;
    [SerializeField] private float raycastFrequency = 0.1f;
    [SerializeField] private int maxTargets = 5;
    [SerializeField] private float _rayCastLength = 10f;

    [SerializeField] private Transform _aimTarget;

    private float _currentAngle = 0f;

    private float _swingAngleDeviation = 45f;
    private float _swingTimer = 0f;
    private float _swingSpeed;

    private RaycastHit2D[] _hitBuffer;
    private ContactFilter2D _contactFilter;

    public float CurrentAngle => _currentAngle;
    public Vector3 RaycastDirection => transform.rotation * Vector3.down;

    private void Awake()
    {
        _swingSpeed = 1f + Random.Range(1f - 0.4f, 1f + 0.4f);
    }

    private void FixedUpdate()
    {
        RotateWidthDebug();
    }

    private void Aim()
    {

    }

    private void RotateWidthDebug()
    {
        Swing();

        Debug.DrawRay(transform.position, -transform.up * _rayCastLength, Color.red);

        Debug.DrawRay(
            transform.position,
            _aimTarget.position - transform.position,
            Color.red);
    }

    private void Swing()
    {
        _swingTimer += Time.deltaTime * _swingSpeed;
        _currentAngle = Mathf.Sin(_swingTimer) * _swingAngleDeviation;
        var rotation = Quaternion.Euler(0f, 0f, _currentAngle);
        transform.rotation = rotation;
    }

    private void PerformRaycast()
    {
        Vector2 startPoint = transform.position;
        Vector2 direction = RaycastDirection;

        int hitCount = Physics2D.Raycast(
            startPoint,
            direction,
            _contactFilter,
            _hitBuffer,
            _rayCastLength
        );

        for (int i = 0; i < hitCount; i++)
        {
            HandleHit(_hitBuffer[i]);
        }
    }

    private void HandleHit(RaycastHit2D hit)
    {
        hit.collider.TryGetComponent(out AEntity damageable);

        if (damageable != null && damageable.IsInShelter == false)
        {
            damageable.Die();
        }
    }
}