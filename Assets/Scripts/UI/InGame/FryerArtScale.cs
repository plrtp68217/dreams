using UnityEngine;

// Keep source pixel coordinates independent of window size and aspect ratio.
[ExecuteAlways]
public class FryerArtScale : MonoBehaviour
{
    private void OnRectTransformDimensionsChange() => Resize();
    private void OnEnable() => Resize();
    private void LateUpdate() => Resize();
    private void Resize()
    {
        if (transform.parent is RectTransform parent)
        {
            var scale = Vector3.one * (parent.rect.width / 1280f);
            if (transform.localScale != scale) transform.localScale = scale;
        }
    }
}
