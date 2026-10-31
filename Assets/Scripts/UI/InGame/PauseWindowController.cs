using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class PauseWindowController : MonoBehaviour
{
    [SerializeField] private InputService _inputService;
    [SerializeField] private CanvasGroup _pauseCanvasGroup;
    [SerializeField] private GameObject _mainPage;
    [SerializeField] private GameObject _settingsPage;
    [SerializeField] private UnityEngine.UI.Button _resumeButton;
    [SerializeField] private UnityEngine.UI.Slider _firstSetting;
    [SerializeField] private UnityEngine.UI.Image _dimmer;

    public bool IsOpen { get; private set; }
    public bool IsSettingsOpen => _settingsPage != null && _settingsPage.activeSelf;
    private bool _wasBlocked;
    private float _previousTimeScale;
    private Coroutine _fade;

    private void Awake()
    {
        if (_inputService == null) _inputService = FindFirstObjectByType<InputService>();
        _pauseCanvasGroup.alpha = 0;
        _pauseCanvasGroup.interactable = false;
        _pauseCanvasGroup.blocksRaycasts = false;
    }

    private void Update()
    {
        if (_inputService != null && _inputService.EscapeIsPressed) HandleEscape();
    }

    private void Start()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void HandleEscape()
    {
        if (!IsOpen) OpenWindow();
        else if (IsSettingsOpen) ShowMainPage();
        else CloseWindow();
    }

    public void OpenWindow()
    {
        if (IsOpen) return;
        IsOpen = true;
        _previousTimeScale = Time.timeScale;
        _wasBlocked = _inputService != null && _inputService.IsBlocked;
        if (_inputService != null)
        {
            if (!_wasBlocked) _inputService.Block();
            _inputService.SetPaused(true);
        }
        Time.timeScale = 0;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        _pauseCanvasGroup.interactable = true;
        _pauseCanvasGroup.blocksRaycasts = true;
        SetDimmer(0.78f);
        ShowMainPage();
        BeginFade(1);
    }

    public void CloseWindow()
    {
        if (!IsOpen) return;
        IsOpen = false;
        RestoreGameState();
        _pauseCanvasGroup.interactable = false;
        _pauseCanvasGroup.blocksRaycasts = false;
        if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(null);
        if (GameSettings.Instance != null) GameSettings.Instance.Save();
        BeginFade(0);
    }

    public void ShowSettings()
    {
        _mainPage.SetActive(false);
        _settingsPage.SetActive(true);
        if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(_firstSetting.gameObject);
    }

    public void ShowMainPage()
    {
        _settingsPage.SetActive(false);
        _mainPage.SetActive(true);
        if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(_resumeButton.gameObject);
    }

    public void ReturnToMainMenu()
    {
        CloseWindow();
        SceneManager.LoadScene(SceneName.MainMenu.ToString());
    }

    // Preserve existing scene button references to the original method name.
    public void ReturnToNainMenu() => ReturnToMainMenu();

    private void SetDimmer(float alpha)
    {
        var color = _dimmer.color;
        color.a = alpha;
        _dimmer.color = color;
    }

    private void BeginFade(float alpha)
    {
        if (_fade != null) StopCoroutine(_fade);
        _fade = StartCoroutine(Fade(alpha));
    }

    private IEnumerator Fade(float target)
    {
        float start = _pauseCanvasGroup.alpha;
        float elapsed = 0;
        while (elapsed < 0.18f)
        {
            elapsed += Time.unscaledDeltaTime;
            _pauseCanvasGroup.alpha = Mathf.Lerp(start, target, Mathf.SmoothStep(0, 1, elapsed / 0.18f));
            yield return null;
        }
        _pauseCanvasGroup.alpha = target;
        _fade = null;
    }

    private void RestoreGameState()
    {
        Time.timeScale = _previousTimeScale;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        if (_inputService != null)
        {
            _inputService.SetPaused(false);
            if (!_wasBlocked) _inputService.Unblock();
        }
    }

    private void OnDisable()
    {
        if (_fade != null) StopCoroutine(_fade);
        _fade = null;
        if (IsOpen) RestoreGameState();
        IsOpen = false;
    }
}
