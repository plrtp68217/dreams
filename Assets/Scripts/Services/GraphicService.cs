using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GraphicService : MonoBehaviour
{
    [SerializeField] private float _fadeTime = 1f;

    private readonly Dictionary<Graphic, Coroutine> _activeFades = new();

    public void FadeGraphic(Graphic[] graphic, float targetAlpha)
    {
        if (this == null || gameObject == null) return;

        foreach (Graphic graphicItem in graphic)
        {
            if (_activeFades.TryGetValue(graphicItem, out Coroutine existingCoroutine))
            {
                StopCoroutine(existingCoroutine);
                _activeFades.Remove(graphicItem);
            }

            var coroutine = StartCoroutine(FadeGraphicRoutine(graphicItem, targetAlpha));

            _activeFades.Add(graphicItem, coroutine);
        }
    }

    private IEnumerator FadeGraphicRoutine(Graphic graphic, float targetAlpha)
    {
        float startAlpha = graphic.color.a;
        float elapsedTime = 0f;

        while (elapsedTime < _fadeTime)
        {
            if (graphic == null)
            {
                _activeFades.Remove(graphic);
                yield break;
            }

            elapsedTime += Time.deltaTime;
            Color color = graphic.color;
            color.a = Mathf.Lerp(startAlpha, targetAlpha, elapsedTime / _fadeTime);
            graphic.color = color;

            yield return null;
        }

        if (graphic != null)
        {
            Color finalColor = graphic.color;
            finalColor.a = targetAlpha;
            graphic.color = finalColor;
        }

        _activeFades.Remove(graphic);
    }
}
