using System.Collections;
using UnityEngine;

public class CanvasGroupService : MonoBehaviour
{
    private readonly float _isOpened = 1f;
    private readonly float _isClosed = 0f;

    private bool isActivated = false;
    private Coroutine _coroutine;

    private void OnDisable()
    {
        if (_coroutine != null)
        {
            StopCoroutine(_coroutine);
            _coroutine = null;
        }
    }

    public void Open()
    {
    }

    public void Open(CanvasGroup canvasGroup, float delay = 0.5f)
    {
        if (isActivated) return;

        if (canvasGroup.alpha == _isOpened) return;

        _coroutine = StartCoroutine(FadeCanvasGroupRoutine(canvasGroup, 1f, delay));

        canvasGroup.interactable = true;
    }

    public void Close(CanvasGroup canvasGroup, float delay = 0.5f)
    {
        if (isActivated) return;

        if (canvasGroup.alpha == _isClosed) return;

        _coroutine = StartCoroutine(FadeCanvasGroupRoutine(canvasGroup, 0f, delay));

        canvasGroup.interactable = false;
    }


    private IEnumerator FadeCanvasGroupRoutine(CanvasGroup canvasGroup, float targetAlpha, float delay)
    {
        isActivated = true;

        float startAlpha = canvasGroup.alpha;
        float elapsedTime = 0f;

        while (elapsedTime < delay)
        {
            if (canvasGroup == null)
            {
                yield break;
            }

            elapsedTime += Time.deltaTime;

            canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsedTime / delay);

            yield return null;
        }

        if (canvasGroup != null)
        {
            canvasGroup.alpha = targetAlpha;
        }

        isActivated = false;

    }
}