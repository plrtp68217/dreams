using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Начало уровня: чёрный экран с названием сна, затем Ева просыпается
// и (необязательно) звучат субтитры. Всё это время управление заблокировано.
public class SceneIntro : MonoBehaviour
{
    [SerializeField] private InputService _inputService;
    [SerializeField] private Animator _playerAnimator;

    [Header("Название сна")]
    [SerializeField] private Image _blackMask;
    [SerializeField] private TextMeshProUGUI _titleText;
    [SerializeField] private string _titleId;
    [SerializeField] private float _delayBeforeTitle = 0.5f;
    [SerializeField] private float _titleFadeInTime = 1.5f;
    [SerializeField] private float _titleHoldTime = 3f;
    [SerializeField] private float _fadeOutTime = 1.5f;

    [Header("Пробуждение (длина анимации WakeUp)")]
    [SerializeField] private float _wakeUpTime = 1f;

    [Header("Субтитры после пробуждения")]
    [SerializeField] private Dialog _subtitles;
    [SerializeField] private string[] _phraseIds;
    [SerializeField] private float _delayBeforeDialog = 0.5f;

    private IEnumerator Start()
    {
        _inputService.Block();

        // Пока идёт заставка, Ева лежит на первом кадре WakeUp
        _playerAnimator.speed = 0f;

        _titleText.text = LocalizedDialogueSystem.Instance.GetDialogueText(_titleId);
        SetAlpha(_blackMask, 1f);
        SetAlpha(_titleText, 0f);

        yield return new WaitForSeconds(_delayBeforeTitle);
        yield return Fade(_titleFadeInTime, 1f, _titleText);
        yield return new WaitForSeconds(_titleHoldTime);
        yield return Fade(_fadeOutTime, 0f, _titleText, _blackMask);

        _playerAnimator.speed = 1f;
        yield return new WaitForSeconds(_wakeUpTime);

        if (_subtitles != null && _phraseIds.Length > 0)
        {
            yield return new WaitForSeconds(_delayBeforeDialog);
            yield return DialogPhrases.Play(_subtitles, _inputService, _phraseIds);
        }

        _inputService.Unblock();
    }

    private IEnumerator Fade(float duration, float targetAlpha, params Graphic[] graphics)
    {
        float[] startAlphas = new float[graphics.Length];

        for (int i = 0; i < graphics.Length; i++)
        {
            startAlphas[i] = graphics[i].color.a;
        }

        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;

            for (int i = 0; i < graphics.Length; i++)
            {
                SetAlpha(graphics[i], Mathf.Lerp(startAlphas[i], targetAlpha, elapsedTime / duration));
            }

            yield return null;
        }

        foreach (Graphic graphic in graphics)
        {
            SetAlpha(graphic, targetAlpha);
        }
    }

    private void SetAlpha(Graphic graphic, float alpha)
    {
        Color color = graphic.color;
        color.a = alpha;
        graphic.color = color;
    }
}
