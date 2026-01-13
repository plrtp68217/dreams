using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class DialogCanvas : Dialog
{
    [SerializeField] private Image _image;
    [SerializeField] private TextMeshProUGUI _textMeshPro;

    [SerializeField] private float fadeTime = 1f;

    public override void Enable(string text)
    {
        _textMeshPro.text = text;

        StartCoroutine(TransitionUtils.FadeGraphic(_image, fadeTime, 1f));
        StartCoroutine(TransitionUtils.FadeGraphic(_textMeshPro, fadeTime, 1f));
    }

    public override void Disable()
    {
        StartCoroutine(TransitionUtils.FadeGraphic(_image, fadeTime, 0f));
        StartCoroutine(TransitionUtils.FadeGraphic(_textMeshPro, fadeTime, 0f));
    }
}