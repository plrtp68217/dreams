using UnityEngine;
using UnityEngine.Rendering.Universal;

// A receiver region in the painted floor; no collision or navigation changes.
public class CharacterShadowSurface : MonoBehaviour
{
    [SerializeField] private Rect _floorBounds = new Rect(-8.2f, -.9f, 13.2f, .41f);
    [SerializeField] private float _groundY = -.73f;
    [SerializeField] private Light2D[] _lights;
    [SerializeField, Min(.01f)] private float _referenceIntensity = 1f;

    public Rect FloorBounds => _floorBounds;
    public float GroundY => _groundY;
    public bool Contains(float x) => x >= _floorBounds.xMin && x <= _floorBounds.xMax;

    public void SampleLighting(float x, out Vector2 lightPosition, out float strength)
    {
        float total = 0, active = 0;
        Vector2 weightedPosition = Vector2.zero;
        if (_lights != null)
        foreach (var light in _lights)
        {
            if (light == null) continue;
            float distance = light.transform.position.x - x;
            float weight = 1f / Mathf.Pow(.12f + distance * distance, 2);
            total += weight;
            float intensity = light.isActiveAndEnabled ? Mathf.Clamp01(light.intensity / _referenceIntensity) : 0f;
            active += weight * intensity;
            weightedPosition += (Vector2)light.transform.position * (weight * intensity);
        }
        strength = total > 0 ? Mathf.Clamp01(active / total) : 1f;
        lightPosition = active > .00001f ? weightedPosition / active : new Vector2(x - .5f, _groundY + 1.4f);
    }
}
