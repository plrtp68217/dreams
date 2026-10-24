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

    private bool _wasBlocked = false;

    private void Start()
    {
        _textMeshProMenu.text = LocalizedDialogueSystem.Instance.GetDialogueText(_menuTextId);

        // В игре курсор нужен только в паузе
        Cursor.visible = false;
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
        Cursor.visible = true;

        // Пауза могла открыться во время катсцены, когда ввод уже заблокирован
        _wasBlocked = _inputService.IsBlocked;
        _inputService.Block();
    }

    public void CloseWindow()
    {
        _canvasGroupService.Close(_pauseCanvasGroup);
        Cursor.visible = false;

        if (_wasBlocked == false)
        {
            _inputService.Unblock();
        }
    }
}

