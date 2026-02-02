using UnityEngine;

public class Eye : AEntity
{
    [Header("Performance Settings")]
    [SerializeField] private LayerMask damageableLayers;
    [SerializeField] private float raycastFrequency = 0.1f;
    [SerializeField] private int maxTargets = 5;

    [Header("Line swing Settings")]
    [SerializeField] private float _swingAngle = 45f;
    [SerializeField] private float _startAngle = 0f;
    [SerializeField] private float _swingSpeed = 1f;

    [Header("Variation Settings")]
    [SerializeField] private float angleOffsetRange = 30f;
    [SerializeField] private float speedVariation = 0.3f;
    [SerializeField] private float phaseOffsetRange = 2f;

    [Header("Line Settings")]
    [SerializeField] private float _lineLength = 10f;
    [SerializeField] private Color _lineColor = Color.red;
    [SerializeField] private float _lineWidth = 0.1f;
    [SerializeField] private float _lineOffsetY;


    private LineRenderer _lineRenderer;
    private float _currentAngle;
    private float _timer;
    private float _mySwingSpeed;
    private float _phaseOffset;
    private Vector3 _lineDirection;

    private float _raycastTimer;
    private RaycastHit2D[] _hitBuffer;
    private ContactFilter2D _contactFilter;

    private void Start()
    {
        _timer = _phaseOffset;
        ConfigureLineRenderer();

        _contactFilter = new()
        {
            useTriggers = false,
            useLayerMask = true,
            layerMask = damageableLayers
        };

        _hitBuffer = new RaycastHit2D[maxTargets];
    }

    private void FixedUpdate()
    {
        //DrawLaser();

        _raycastTimer -= Time.deltaTime;

        if (_raycastTimer <= 0f)
        {
            PerformOptimizedRaycast();
            _raycastTimer = raycastFrequency;
        }

        UpdateLineAngle();
    }

    private void ConfigureLineRenderer()
    {
        TryGetComponent<LineRenderer>(out _lineRenderer);

        if (_lineRenderer == null) return;

        _lineRenderer.positionCount = 2;
        _lineRenderer.startWidth = _lineWidth;
        _lineRenderer.endWidth = _lineWidth;
        _lineRenderer.material = new Material(Shader.Find("Unlit/Color"));
        _lineRenderer.startColor = _lineColor;
        _lineRenderer.endColor = _lineColor;

        _mySwingSpeed = _swingSpeed * Random.Range(1f - speedVariation, 1f + speedVariation);
        _phaseOffset = Random.Range(0f, phaseOffsetRange * Mathf.PI);
    }

    private void UpdateLineAngle()
    {
        _timer += Time.deltaTime * _mySwingSpeed;
        _currentAngle = _startAngle + Mathf.Sin(_timer) * _swingAngle;
    }

    private void DrawLaser()
    {
        Vector3 startPoint = transform.position + new Vector3(0, _lineOffsetY, 0);

        Quaternion rotation = Quaternion.Euler(0, 0, _currentAngle);
        _lineDirection = rotation * Vector3.down;

        Vector3 endPoint = startPoint + _lineDirection * _lineLength;

        _lineRenderer.SetPosition(0, startPoint);
        _lineRenderer.SetPosition(1, endPoint);
    }

    void PerformOptimizedRaycast()
    {
        Vector2 startPoint = transform.position;
        Vector2 direction = _lineDirection;

        int hitCount = Physics2D.Raycast(
            startPoint,
            direction,
            _contactFilter,
            _hitBuffer,
            _lineLength
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