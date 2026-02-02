using UnityEngine;

public class TestEye : AEntity
{
    [Header("Raycast Settings")]
    [SerializeField] private LayerMask damageableLayers;
    [SerializeField] private float raycastFrequency = 0.1f;
    [SerializeField] private int maxTargets = 5;
    [SerializeField] private float _rayCastLength = 10f;

    [Header("Variation Settings")]
    [SerializeField] private float speedVariation = 0.3f;
    [SerializeField] private float phaseOffsetRange = 2f;

    private readonly float _swingSpeed = 1f;
    private readonly float _swingAngle = 45f;
    private float _mySwingSpeed;

    private readonly float _startAngle = 0f;
    private float _currentAngle;

    private float _timer;
    private float _phaseOffset;

    private Vector3 _raycastDirection;
    private float _raycastTimer;
    private RaycastHit2D[] _hitBuffer;
    private ContactFilter2D _contactFilter;

    private void Start()
    {
        _timer = _phaseOffset;

        _contactFilter = new()
        {
            useTriggers = false,
            useLayerMask = true,
            layerMask = damageableLayers
        };

        _hitBuffer = new RaycastHit2D[maxTargets];

        _mySwingSpeed = _swingSpeed * Random.Range(1f - speedVariation, 1f + speedVariation);
        _phaseOffset = Random.Range(0f, phaseOffsetRange * Mathf.PI);
    }

    private void FixedUpdate()
    {
        _timer += Time.deltaTime * _mySwingSpeed;
        _currentAngle = _startAngle + Mathf.Sin(_timer) * _swingAngle;

        Quaternion rotation = Quaternion.Euler(0, 0, _currentAngle);

        gameObject.transform.rotation = rotation;

        _raycastDirection = rotation * Vector3.down;
        _raycastTimer -= Time.deltaTime;

        if (_raycastTimer <= 0f)
        {
            PerformOptimizedRaycast();
            _raycastTimer = raycastFrequency;
        }
    }

    void PerformOptimizedRaycast()
    {
        Vector2 startPoint = transform.position;
        Vector2 direction = _raycastDirection;

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