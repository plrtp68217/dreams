using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class DialogCanvas : Dialog
{
    [SerializeField] private GraphicService _graphicService;

    [SerializeField] private Image[] _images;
    [SerializeField] private TextMeshProUGUI[] _textsMeshPro;

    [SerializeField] private int _editableTextIndex = 0;

    [Tooltip("Необязательно: если задан, текст печатается посимвольно")]
    [SerializeField] private TypewriterText _typewriter;

    [Tooltip("Default voice for this dialogue window. Leave empty for silent text.")]
    [SerializeField] private DialogueVoiceProfile _voiceProfile;

    public override void Enable(string text)
    {
        Enable(text, _voiceProfile);
    }

    public override void Enable(string text, DialogueVoiceProfile voiceProfile)
    {
        if (_typewriter != null)
        {
            _typewriter.Play(text, voiceProfile != null ? voiceProfile : _voiceProfile);
        }
        else if (_textsMeshPro.Length > 0)
        {
            _textsMeshPro[_editableTextIndex].text = text;
        }

        _graphicService.FadeGraphic(_images, 1f);
        _graphicService.FadeGraphic(_textsMeshPro, 1f);
    }

    public override void Disable()
    {
        if (_typewriter != null) _typewriter.Complete();
        _graphicService.FadeGraphic(_images, 0f);
        _graphicService.FadeGraphic(_textsMeshPro, 0f);
    }

    public override bool TryCompleteText()
    {
        if (_typewriter == null || _typewriter.IsTyping == false) return false;

        _typewriter.Complete();
        return true;
    }

    private void OnDisable()
    {
        if (_typewriter != null) _typewriter.Complete();
    }
}
