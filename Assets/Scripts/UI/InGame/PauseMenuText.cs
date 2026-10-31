using TMPro;
using UnityEngine;

[RequireComponent(typeof(TextMeshProUGUI))]
public class PauseMenuText : MonoBehaviour
{
    [SerializeField] private string _russian;
    [SerializeField] private string _english;

    private void OnEnable()
    {
        LocalizedDialogueSystem.LanguageChanged += Refresh;
        Refresh();
    }

    private void OnDisable() => LocalizedDialogueSystem.LanguageChanged -= Refresh;

    private void Refresh()
    {
        GetComponent<TextMeshProUGUI>().text = LocalizedDialogueSystem.CurrentLanguage == Language.Russian ? _russian : _english;
    }
}
