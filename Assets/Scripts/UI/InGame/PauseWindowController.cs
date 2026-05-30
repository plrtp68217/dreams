using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseWindowController: MonoBehaviour
{
    [SerializeField] private InputService _inputService;
    [SerializeField] private CanvasGroupService _canvasGroupService;
    [SerializeField] private CanvasGroup _pauseCanvasGroup;

    [SerializeField] private TextMeshProUGUI _textMeshProMenu;
    [SerializeField] private string _menuTextId = "ingame_button_menu";

    private void Start()
    {
        _textMeshProMenu.text = LocalizedDialogueSystem.Instance.GetDialogueText(_menuTextId);
    }

    private void Update()
    {
        if (_inputService.EscapeIsPressed)
        {
            if (_pauseCanvasGroup.alpha == 0f)
            {
                OpenWindow();
            }
            else
            {
                CloseWindow();
            }
        }
    }

    public void ReturnToNainMenu()
    {
        SceneManager.LoadScene(SceneName.MainMenu.ToString());
    }

    public void OpenWindow()
    {
        _canvasGroupService.Open(_pauseCanvasGroup);
        _inputService.Block();
    }

    public void CloseWindow()
    {
        _canvasGroupService.Close(_pauseCanvasGroup);
        _inputService.Unblock();
    }
}

