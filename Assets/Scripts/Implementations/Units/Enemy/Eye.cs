using UnityEngine;

public class Eye : MonoBehaviour
{
    [SerializeField] private LayerMask damageableLayers;
    [SerializeField] private int maxTargets = 5;
    [SerializeField] private float _rayCastLength = 10f;

    [SerializeField] private Transform _aimTarget;

    private EyeState _eyeState = EyeState.Swing;

    private float _currentAngle = 0f;

    private float _swingAngleDeviation = 45f;
    private float _swingTimer = 0f;
    private float _swingSpeed;

    private RaycastHit2D[] _hitBuffer;
    private ContactFilter2D _contactFilter;

    public Vector3 DirectionToTarget => _aimTarget.transform.position - transform.position;
    public Vector3 RaycastDirection => -transform.up;

    private void Awake()
    {
        _contactFilter = new()
        {
            useTriggers = false,
            useLayerMask = true,
            layerMask = damageableLayers
        };

        _hitBuffer = new RaycastHit2D[maxTargets];

        _swingSpeed = 1f + Random.Range(1f - 0.4f, 1f + 0.4f);
    }

    private void FixedUpdate()
    {
        if (_eyeState == EyeState.Swing)
        {
            Swing();
        }
        else if (_eyeState == EyeState.Aim)
        {
            Aim();
        }

        PerformRaycast();
    }

    public void SetState(EyeState state)
    {
        _eyeState = state;
    }

    private void Aim()
    {
        float rotationSmoothTime = 0.3f;
        Vector3 directionToTarget = _aimTarget.transform.position - transform.position;
        Quaternion targetRotation = Quaternion.FromToRotation(-transform.up, directionToTarget) * transform.rotation;
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime / rotationSmoothTime);
    }

    private void Swing()
    {
        _swingTimer += Time.deltaTime * _swingSpeed;
        _currentAngle = Mathf.Sin(_swingTimer) * _swingAngleDeviation;
        var rotation = Quaternion.Euler(0f, 0f, _currentAngle);
        transform.rotation = rotation;
    }

    private void SwingWithDebug()
    {
        Swing();

        Debug.DrawRay(transform.position, -transform.up * _rayCastLength, Color.red);

        Debug.DrawRay(
            transform.position,
            _aimTarget.position - transform.position,
            Color.red);
    }

    private void PerformRaycast()
    {
        int hitCount = Physics2D.Raycast(
            transform.position,
            RaycastDirection,
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
        if (hit.collider.TryGetComponent(out AEntity damageable))
        {
            if (damageable.IsInShelter == false && damageable.IsAlive == true)
            {
                damageable.Die();
            }
        }
    }
}