using System;
using System.Collections;
using TMPro;
using UnityEngine;

// Печатает текст TextMeshPro посимвольно. Вешается на объект с любым TMP_Text
// и запускается через Play(text); Complete() сразу показывает текст целиком.
[RequireComponent(typeof(TMP_Text))]
public class TypewriterText : MonoBehaviour
{
    [SerializeField] private float _charactersPerSecond = 30f;

    [Header("Пауза после знаков препинания")]
    [SerializeField] private float _punctuationDelay = 0.15f;
    [SerializeField] private string _punctuation = ".,!?;:…";

    [SerializeField] private bool _useUnscaledTime = false;

    public event Action Finished;

    private TMP_Text _text;
    private Coroutine _typingCoroutine;

    public bool IsTyping => _typingCoroutine != null;

    private TMP_Text Text
    {
        get
        {
            if (_text == null)
            {
                _text = GetComponent<TMP_Text>();
            }

            return _text;
        }
    }

    public void Play(string text)
    {
        StopTyping();

        Text.text = text;
        Text.maxVisibleCharacters = 0;

        if (isActiveAndEnabled == false)
        {
            ShowAll();
            return;
        }

        _typingCoroutine = StartCoroutine(TypeRoutine());
    }

    public void Complete()
    {
        if (IsTyping == false) return;

        StopTyping();
        ShowAll();
    }

    private void OnDisable()
    {
        // Корутина остановится вместе с объектом, поэтому дописываем текст сразу
        Complete();
    }

    private IEnumerator TypeRoutine()
    {
        Text.ForceMeshUpdate();

        TMP_TextInfo textInfo = Text.textInfo;
        int characterCount = textInfo.characterCount;
        float delay = 0f;

        for (int i = 0; i < characterCount; i++)
        {
            while (delay > 0f)
            {
                yield return null;
                delay -= _useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            }

            Text.maxVisibleCharacters = i + 1;

            // Остаток задержки переносится, поэтому за кадр может появиться несколько символов
            delay += 1f / _charactersPerSecond;

            if (_punctuation.IndexOf(textInfo.characterInfo[i].character) >= 0)
            {
                delay += _punctuationDelay;
            }
        }

        _typingCoroutine = null;
        ShowAll();
    }

    private void StopTyping()
    {
        if (_typingCoroutine != null)
        {
            StopCoroutine(_typingCoroutine);
            _typingCoroutine = null;
        }
    }

    private void ShowAll()
    {
        Text.maxVisibleCharacters = int.MaxValue;
        Finished?.Invoke();
    }
}
