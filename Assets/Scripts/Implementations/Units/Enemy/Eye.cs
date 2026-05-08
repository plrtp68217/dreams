using UnityEngine;

public class Eye : AEntity
{
    [Header("Raycast Settings")]
    [SerializeField] private LayerMask damageableLayers;
    [SerializeField] private float raycastFrequency = 0.1f;
    [SerializeField] private int maxTargets = 5;
    [SerializeField] private float _rayCastLength = 10f;

    [Header("Variation Settings")]
    [SerializeField] private float speedVariation = 0.4f;

    [SerializeField] private Transform _aimTarget;

    private readonly float _swingSpeed = 1f;
    private readonly float _swingAngle = 45f;

     private float _mySwingSpeed;
    private float _currentAngle;
    private float _swingTimer;

    private EyeState _eyeState = EyeState.Swing;

    private Vector3 _raycastDirection;
    private float _raycastTimer;
    private RaycastHit2D[] _hitBuffer;
    private ContactFilter2D _contactFilter;

    private void Awake()
    {
        _contactFilter = new()
        {
            useTriggers = false,
            useLayerMask = true,
            layerMask = damageableLayers
        };

        _hitBuffer = new RaycastHit2D[maxTargets];

        _mySwingSpeed = _swingSpeed * Random.Range(1f - speedVariation, 1f + speedVariation);
    }

    private void FixedUpdate()
    {
        if (_eyeState == EyeState.Swing)
        {
            UpdateSwingRotation();
        }
        else if (_eyeState == EyeState.Aim)
        {
            UpdateAimRotation();
        }

        //UpdateRaycast();
    }

    public void SetState(EyeState state)
    {
        _eyeState = state;
    }

    private void UpdateAimRotation()
    {
        if (_aimTarget == null)
        {
            _eyeState = EyeState.Swing;
            return;
        }

        Vector3 directionToTarget = (_aimTarget.transform.position - transform.position).normalized;

        float angleToTarget = Vector3.SignedAngle(-transform.up, directionToTarget, transform.forward);

        float lerpedAngle = Mathf.Lerp(Vector3.down.z, angleToTarget, 1);

        transform.Rotate(Vector3.forward, lerpedAngle);
    }

    private void UpdateSwingRotation()
    {
        _swingTimer += Time.deltaTime * _mySwingSpeed;

        _currentAngle = Mathf.Sin(_swingTimer) * _swingAngle;

        Quaternion rotation = Quaternion.Euler(0, 0, _currentAngle);
        gameObject.transform.rotation = rotation;

        _raycastDirection = rotation * Vector3.down;
    }

    private void UpdateRaycast()
    {
        _raycastTimer -= Time.deltaTime;

        if (_raycastTimer <= 0f)
        {
            PerformRaycast();
            _raycastTimer = raycastFrequency;
        }
    }

    private void PerformRaycast()
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