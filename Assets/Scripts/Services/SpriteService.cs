using System.Collections;
using UnityEngine;

public enum FadeDirection { In, Out }

public class SpriteService : MonoBehaviour
{
    private Color _minColorValue = new(0.5f, 0.5f, 0.5f, 1f);
    private Color _maxColorValue = new(1f, 1f, 1f, 1f);

    private Coroutine _currentFadeCoroutine;

    public void FadeSprite(SpriteRenderer spriteRenderer, FadeDirection direction, float duration)
    {
        if (_currentFadeCoroutine != null)
        {
            StopCoroutine(_currentFadeCoroutine);
        }

        _currentFadeCoroutine = StartCoroutine(FadeSpriteRoutine(spriteRenderer, direction, duration));
    }

    private IEnumerator FadeSpriteRoutine(SpriteRenderer spriteRenderer, FadeDirection direction, float duration)
    {
        Color startColor = spriteRenderer.color;
        Color targetColor;

        if (direction == FadeDirection.Out)
        {
            targetColor = _minColorValue;
        }
        else
        {
            targetColor = _maxColorValue;
        }

        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;

            float t  = elapsedTime / duration;

            spriteRenderer.color = Color.Lerp(startColor, targetColor, t);

            yield return null;
        }

        _currentFadeCoroutine = null;
    }
}