using UnityEngine;

// The mask is extracted from the same Aseprite cel as the visible backdrop.
// Only its outer pixels are drawn; the original artwork stays in the backdrop.
[DisallowMultipleComponent]
public sealed class FastFoodStationOutline : MonoBehaviour
{
    [SerializeField] private FastFoodServiceStation _station;
    [SerializeField] private SpriteRenderer _outline;

    private void Awake() => Hide();

    private void LateUpdate()
    {
        if (_outline != null) _outline.enabled = _station != null && _station.CanHighlight;
    }

    private void OnDisable() => Hide();

    private void Hide()
    {
        if (_outline != null) _outline.enabled = false;
    }
}
