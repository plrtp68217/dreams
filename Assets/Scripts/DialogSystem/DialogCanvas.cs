using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class DialogCanvas : Dialog
{
    [SerializeField] private Image _image;
    [SerializeField] private TextMeshProUGUI _textMeshPro;

    [SerializeField] private float fadeTime = 1f;

    private Coroutine _textCoroutine;
    private Coroutine _imageCoroutine;

    private void OnDisable()
    {
        if (_textCoroutine != null)
        {
            StopCoroutine(_textCoroutine);
            _textCoroutine = null;
        }

        if (_imageCoroutine != null)
        {
            StopCoroutine(_imageCoroutine);
            _imageCoroutine = null;
        }
    }

    public override void Enable(string text)
    {
        _textMeshPro.text = text;

        _textCoroutine = StartCoroutine(TransitionUtils.FadeGraphic(_image, fadeTime, 1f));
        _imageCoroutine = StartCoroutine(TransitionUtils.FadeGraphic(_textMeshPro, fadeTime, 1f));
    }

    public override void Disable()
    {
        _textCoroutine = StartCoroutine(TransitionUtils.FadeGraphic(_image, fadeTime, 0f));
        _imageCoroutine = StartCoroutine(TransitionUtils.FadeGraphic(_textMeshPro, fadeTime, 0f));
    }
}