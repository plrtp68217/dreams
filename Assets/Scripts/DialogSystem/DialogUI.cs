using UnityEngine;
using UnityEngine.UI;

public class DialogUI : Dialog
{
    [SerializeField] private float fadeTime = 1f;

    private Image[] _images;

    private Coroutine _coroutine;

    private void OnDisable()
    {
        if (_coroutine != null)
        {
            StopCoroutine(_coroutine);
            _coroutine = null;
        }
    }

    public override void Enable(string text)
    {
        _images = GetComponentsInChildren<Image>();

        foreach (Image image in _images)
        {
            _coroutine = StartCoroutine(TransitionUtils.FadeGraphic(image, fadeTime, 1f));
        }
    }

    public override void Disable()
    {
        foreach (Image image in _images)
        {
            _coroutine = StartCoroutine(TransitionUtils.FadeGraphic(image, fadeTime, 0f));
        }
    }
}