using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public static class TransitionUtils
{
    public static IEnumerator FadeGraphic(Graphic graphic, float fadeTime, float targetAlpha)
    {
        float startAlpha = graphic.color.a;
        float elapsedTime = 0f;

        while (elapsedTime < fadeTime)
        {
            elapsedTime += Time.deltaTime;
            Color color = graphic.color;
            color.a = Mathf.Lerp(startAlpha, targetAlpha, elapsedTime / fadeTime);
            graphic.color = color;

            yield return null;
        }

        Color finalColor = graphic.color;
        finalColor.a = targetAlpha;
        graphic.color = finalColor;
    }
}