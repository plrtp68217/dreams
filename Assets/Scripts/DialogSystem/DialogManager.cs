using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class DialogManager : MonoBehaviour
{
    [SerializeField] private Image _image;
    [SerializeField] private TextMeshProUGUI _textMeshPro;

    [SerializeField] private float fadeTime = 1f;

    private void OnEnable()
    {
        DialogTrigger.ActionDialogEnter += OnDialogEnter;
        DialogTrigger.ActionDialogExit += OnDialogExit;

    }

    private void OnDisable()
    {
        DialogTrigger.ActionDialogEnter -= OnDialogEnter;
        DialogTrigger.ActionDialogExit -= OnDialogExit;
    }

    private void OnDialogEnter(string text)
    {
        _textMeshPro.text = text;
        StartCoroutine(Fade(1f));
    }

    private void OnDialogExit()
    {
        StartCoroutine(Fade(0f));
    }

    private IEnumerator Fade(float targetAlpha)
    {
        float startAlphaImage = _image.color.a;
        float startAlphaText = _textMeshPro.color.a;
        float elapsedTime = 0f;

        while (elapsedTime < fadeTime)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / fadeTime;

            Color imageColor = _image.color;
            imageColor.a = Mathf.Lerp(startAlphaImage, targetAlpha, progress);
            _image.color = imageColor;

            Color textColor = _textMeshPro.color;
            textColor.a = Mathf.Lerp(startAlphaText, targetAlpha, progress);
            _textMeshPro.color = textColor;

            yield return null;
        }

        Color finalImageColor = _image.color;
        finalImageColor.a = targetAlpha;
        _image.color = finalImageColor;

        Color finalTextColor = _textMeshPro.color;
        finalTextColor.a = targetAlpha;
        _textMeshPro.color = finalTextColor;
    }
}