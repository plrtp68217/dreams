using TMPro;
using UnityEngine;

// Wall lettering uses the same language state and string database as the existing UI/dialogues.
public class LocalizedPosterText : MonoBehaviour
{
    [SerializeField] private TMP_Text _title;
    [SerializeField] private TMP_Text _body;
    [SerializeField] private string _titleId;
    [SerializeField] private string _bodyId;

    private void OnEnable()
    {
        LocalizedDialogueSystem.LanguageChanged += Refresh;
        Refresh();
    }

    private void Start() => Refresh();
    private void OnDisable() => LocalizedDialogueSystem.LanguageChanged -= Refresh;

    public void Refresh()
    {
        var database = LocalizedDialogueSystem.Instance;
        if (database == null) return;
        if (_title != null) _title.text = database.GetDialogueText(_titleId);
        if (_body != null) _body.text = database.GetDialogueText(_bodyId);
    }
}
