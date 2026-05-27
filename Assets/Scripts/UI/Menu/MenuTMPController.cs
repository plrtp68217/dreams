using TMPro;
using UnityEngine;

public class MenuTMPController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _textMeshProPlay;
    [SerializeField] private TextMeshProUGUI _textMeshProOptions;
    [SerializeField] private TextMeshProUGUI _textMeshProExit;
    [SerializeField] private TextMeshProUGUI _textMeshProLanguage;

    [SerializeField] private string _playTextId = "menu_button_play";
    [SerializeField] private string _optionsTextId = "menu_button_options";
    [SerializeField] private string _exitTextId = "menu_button_exit";
    [SerializeField] private string _languageTextId = "menu_language";

    private void OnEnable()
    {
        LocalizedDialogueSystem.LanguageChanged += OnLanguageChanged;
    }

    private void Start()
    {
        UpdateMenuText();
    }

    private void OnDisable()
    {
        LocalizedDialogueSystem.LanguageChanged -= OnLanguageChanged;
    }

    private void OnLanguageChanged()
    {
        UpdateMenuText();
    }

    private void UpdateMenuText()
    {
        _textMeshProPlay.text = LocalizedDialogueSystem.Instance.GetDialogueText(_playTextId);
        _textMeshProOptions.text = LocalizedDialogueSystem.Instance.GetDialogueText(_optionsTextId);
        _textMeshProExit.text = LocalizedDialogueSystem.Instance.GetDialogueText(_exitTextId);
        _textMeshProLanguage.text = LocalizedDialogueSystem.Instance.GetDialogueText(_languageTextId);
    }
}