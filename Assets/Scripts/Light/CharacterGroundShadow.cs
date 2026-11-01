using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[ExecuteAlways, DisallowMultipleComponent]
public class CharacterGroundShadow : MonoBehaviour
{
    [SerializeField] private SpriteRenderer _source;
    [SerializeField] private Material _material;
    [SerializeField] private CharacterShadowSurface[] _surfaces;
    [SerializeField] private Color _shadowColor = new Color(.18f, .11f, .14f, 1f);
    [SerializeField, Range(0, 1)] private float _contactOpacity = .3f;
    [SerializeField, Range(0, 1)] private float _projectionOpacity = .2f;
    [SerializeField] private Vector2 _contactSize = new Vector2(.28f, .06f);
    [SerializeField, Min(0)] private float _projectionLength = .85f;
    [SerializeField, Range(-.3f, -.02f)] private float _projectionDepth = -.16f;
    [SerializeField] private float _directionBias = .3f;
    [SerializeField, Min(.01f)] private float _directionSmoothing = .14f;
    [SerializeField, Min(.1f)] private float _jumpFadeHeight = 1.2f;
    [SerializeField] private float _footOffset = .02f;
    [SerializeField] private string _sortingLayer = "ForeGround";
    [SerializeField] private int _sortingOrder = 6;

    private sealed class Frame
    {
        public Vector2[] Vertices;
        public Vector2[] UV;
        public int[] Triangles;
    }

    private readonly Dictionary<Sprite, Frame> _frames = new Dictionary<Sprite, Frame>();
    private MeshRenderer _contact, _projection;
    private Mesh _contactMesh, _projectionMesh;
    private MaterialPropertyBlock _contactProperties, _projectionProperties;
    private Vector3[] _projectedVertices;
    private readonly Vector3[] _contactVertices = new Vector3[4];
    private CharacterShadowSurface _currentSurface;
    private Sprite _currentSprite;
    private float _shear, _shearVelocity;

    private void OnEnable() => Refresh();
    private void LateUpdate() => Refresh();

    public void Refresh()
    {
        if (_source == null) _source = GetComponent<SpriteRenderer>();
        CharacterShadowSurface surface = null;
        if (_source != null && _surfaces != null)
            foreach (var candidate in _surfaces)
                if (candidate != null && candidate.Contains(_source.bounds.center.x)) { surface = candidate; break; }
        if (_source == null || _source.sprite == null || !_source.enabled || _source.color.a <= .001f ||
            !isActiveAndEnabled || !_source.gameObject.activeInHierarchy || _material == null || surface == null)
        {
            SetVisible(false);
            return;
        }
        EnsureRenderers(surface);
        SetVisible(true);

        float ground = surface.GroundY + _footOffset;
        float x = _source.bounds.center.x;
        float height = Mathf.Max(0f, _source.bounds.min.y - surface.GroundY);
        float jump = Mathf.Clamp01(height / _jumpFadeHeight);
        surface.SampleLighting(x, out Vector2 light, out float strength);
        float desired = Mathf.Clamp((x - light.x) / Mathf.Max(.5f, light.y - surface.GroundY) + _directionBias, -1.2f, 1.2f)
            * _projectionLength;
        if (_currentSurface != surface || !Application.isPlaying)
        {
            _shear = desired;
            _shearVelocity = 0;
        }
        else if (Time.timeScale > 0f && Time.deltaTime > 0f)
            _shear = Mathf.SmoothDamp(_shear, desired, ref _shearVelocity, _directionSmoothing, Mathf.Infinity, Time.deltaTime);
        _currentSurface = surface;

        Vector3 anchor = new Vector3(x, ground, 0f);
        _contact.transform.position = _projection.transform.position = anchor;
        float width = _contactSize.x * Mathf.Lerp(1, .55f, jump);
        float depth = _contactSize.y * Mathf.Lerp(1, .7f, jump);
        _contactVertices[0] = _contact.transform.InverseTransformPoint(anchor + new Vector3(-width/2, -depth/2));
        _contactVertices[1] = _contact.transform.InverseTransformPoint(anchor + new Vector3(-width/2, depth/2));
        _contactVertices[2] = _contact.transform.InverseTransformPoint(anchor + new Vector3(width/2, depth/2));
        _contactVertices[3] = _contact.transform.InverseTransformPoint(anchor + new Vector3(width/2, -depth/2));
        _contactMesh.vertices = _contactVertices;
        _contactMesh.RecalculateBounds();

        Sprite sprite = _source.sprite;
        if (!_frames.TryGetValue(sprite, out Frame frame))
        {
            var triangles = sprite.triangles;
            frame = new Frame { Vertices = sprite.vertices, UV = sprite.uv, Triangles = new int[triangles.Length] };
            for (int i = 0; i < triangles.Length; i++) frame.Triangles[i] = triangles[i];
            _frames.Add(sprite, frame);
        }
        if (_projectedVertices == null || _projectedVertices.Length != frame.Vertices.Length)
            _projectedVertices = new Vector3[frame.Vertices.Length];
        for (int i = 0; i < frame.Vertices.Length; i++)
        {
            Vector2 vertex = frame.Vertices[i];
            if (_source.flipX) vertex.x = -vertex.x;
            if (_source.flipY) vertex.y = -vertex.y;
            Vector3 world = _source.transform.TransformPoint(vertex);
            float aboveFeet = Mathf.Max(0, world.y - _source.bounds.min.y);
            var projected = new Vector3(world.x + aboveFeet * _shear, ground + aboveFeet * _projectionDepth, 0);
            _projectedVertices[i] = _projection.transform.InverseTransformPoint(projected);
        }
        if (_currentSprite != sprite)
        {
            _projectionMesh.Clear();
            _projectionMesh.vertices = _projectedVertices;
            _projectionMesh.uv = frame.UV;
            _projectionMesh.triangles = frame.Triangles;
            _currentSprite = sprite;
        }
        else _projectionMesh.vertices = _projectedVertices;
        _projectionMesh.RecalculateBounds();

        Rect bounds = surface.FloorBounds;
        var floor = new Vector4(bounds.xMin, bounds.yMin, bounds.xMax, bounds.yMax);
        SetProperties(_contactProperties, floor, _contactOpacity * Mathf.Lerp(1, .3f, jump) * Mathf.Lerp(.65f, 1, strength));
        _contactProperties.SetFloat("_Contact", 1);
        _contactProperties.SetVector("_ContactCenter", anchor);
        _contactProperties.SetVector("_ContactRadii", new Vector4(width/2, depth/2, 0, 0));
        _contact.SetPropertyBlock(_contactProperties);
        SetProperties(_projectionProperties, floor, _projectionOpacity * Mathf.Lerp(1, .4f, jump) * strength);
        _projectionProperties.SetFloat("_Contact", 0);
        _projectionProperties.SetTexture("_MainTex", sprite.texture);
        _projection.SetPropertyBlock(_projectionProperties);
        int layer = SortingLayer.NameToID(_sortingLayer);
        _projection.sortingLayerID = _contact.sortingLayerID = layer;
        _projection.sortingOrder = _sortingOrder;
        _contact.sortingOrder = _sortingOrder + 1;
    }

    private void SetProperties(MaterialPropertyBlock properties, Vector4 floor, float opacity)
    {
        properties.SetColor("_ShadowColor", _shadowColor);
        properties.SetFloat("_Opacity", opacity * _source.color.a);
        properties.SetVector("_FloorRect", floor);
        properties.SetFloat("_PixelsPerUnit", _source.sprite.pixelsPerUnit);
    }

    private void EnsureRenderers(CharacterShadowSurface surface)
    {
        if (_contact == null)
        {
            _contact = CreateRenderer("ContactShadow", surface.transform, out _contactMesh);
            _contactMesh.vertices = new Vector3[4];
            _contactMesh.triangles = new[] { 0, 1, 2, 0, 2, 3 };
        }
        if (_projection == null)
        {
            _projection = CreateRenderer("ProjectedShadow", surface.transform, out _projectionMesh);
            _currentSprite = null;
        }
        // Unity retains native renderer references during a domain reload, but not property blocks.
        if (_contactProperties == null) _contactProperties = new MaterialPropertyBlock();
        if (_projectionProperties == null) _projectionProperties = new MaterialPropertyBlock();
        // Keep receivers off the actor hierarchy: Rigidbody interpolation can move children after LateUpdate.
        if (_contact.transform.parent != surface.transform) _contact.transform.SetParent(surface.transform, true);
        if (_projection.transform.parent != surface.transform) _projection.transform.SetParent(surface.transform, true);
        _contact.sharedMaterial = _projection.sharedMaterial = _material;
    }

    private MeshRenderer CreateRenderer(string name, Transform parent, out Mesh mesh)
    {
        var child = new GameObject(name) { hideFlags = HideFlags.HideAndDontSave, layer = gameObject.layer };
        child.transform.SetParent(parent, false);
        mesh = new Mesh { name = name + " mesh", hideFlags = HideFlags.HideAndDontSave };
        mesh.MarkDynamic();
        child.AddComponent<MeshFilter>().sharedMesh = mesh;
        var renderer = child.AddComponent<MeshRenderer>();
        renderer.shadowCastingMode = ShadowCastingMode.Off;
        renderer.receiveShadows = false;
        renderer.lightProbeUsage = LightProbeUsage.Off;
        renderer.reflectionProbeUsage = ReflectionProbeUsage.Off;
        return renderer;
    }

    private void SetVisible(bool visible)
    {
        if (_contact != null) _contact.enabled = visible;
        if (_projection != null) _projection.enabled = visible;
    }

    private void OnDisable() => SetVisible(false);
    private void OnDestroy()
    {
        if (_contact != null) Dispose(_contact.gameObject);
        if (_projection != null) Dispose(_projection.gameObject);
        if (_contactMesh != null) Dispose(_contactMesh);
        if (_projectionMesh != null) Dispose(_projectionMesh);
    }
    private static void Dispose(Object value)
    {
        if (Application.isPlaying) Destroy(value);
        else DestroyImmediate(value);
    }
}
