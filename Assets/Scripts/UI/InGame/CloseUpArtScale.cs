using UnityEngine;

[ExecuteAlways]
public class CloseUpArtScale : MonoBehaviour
{
    [SerializeField] private Vector2 _referenceSize = new Vector2(1176, 772);
    private void OnEnable() => Resize();
    private void OnRectTransformDimensionsChange() => Resize();
    private void LateUpdate() => Resize();
    private void Resize()
    {
        if (transform.parent is RectTransform parent && _referenceSize.x > 0)
        {
            var scale = Vector3.one * (parent.rect.width / _referenceSize.x);
            if (transform.localScale != scale) transform.localScale = scale;
        }
    }
}
