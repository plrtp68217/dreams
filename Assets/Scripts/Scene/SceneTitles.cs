using TMPro;
using UnityEngine;

public class SceneTitles : MonoBehaviour
{
    [SerializeField] private string _gameNameId;
    [SerializeField] private string _authorsId;
    [SerializeField] private string _copyrightId;
    [SerializeField] private string _toByContinuedId;

    [SerializeField] private TextMeshProUGUI _gameNameTMP;
    [SerializeField] private TextMeshProUGUI _authorsTMP;
    [SerializeField] private TextMeshProUGUI _copyrightTMP;
    [SerializeField] private TextMeshProUGUI _toByContinuedTMP;

    private void Start()
    {
        _gameNameTMP.text = LocalizedDialogueSystem.Instance.GetDialogueText(_gameNameId);
        _authorsTMP.text = LocalizedDialogueSystem.Instance.GetDialogueText(_authorsId);
        _copyrightTMP.text = LocalizedDialogueSystem.Instance.GetDialogueText(_copyrightId);
        _toByContinuedTMP.text = LocalizedDialogueSystem.Instance.GetDialogueText(_toByContinuedId);
    }
}