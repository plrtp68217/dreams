using TMPro;
using UnityEngine;

public class PauseSettingsPanel : MonoBehaviour
{
    [SerializeField] private UnityEngine.UI.Slider _volume;
    [SerializeField] private UnityEngine.UI.Slider _brightness;
    [SerializeField] private UnityEngine.UI.Toggle _fullscreen;
    [SerializeField] private TextMeshProUGUI _volumeValue;
    [SerializeField] private TextMeshProUGUI _brightnessValue;
    [SerializeField] private TextMeshProUGUI _languageValue;

    private void OnEnable()
    {
        GameSettings.Instance.Changed += Refresh;
        Refresh();
    }

    private void OnDisable()
    {
        if (GameSettings.Instance != null)
        {
            GameSettings.Instance.Changed -= Refresh;
            GameSettings.Instance.Save();
        }
    }

    public void ChangeVolume(float value) => GameSettings.Instance.SetVolume(value);
    public void ChangeBrightness(float value) => GameSettings.Instance.SetBrightness(value);
    public void ChangeFullscreen(bool value) => GameSettings.Instance.SetFullscreen(value);
    public void ChangeLanguage() => GameSettings.Instance.ToggleLanguage();
    public void ResetDefaults() => GameSettings.Instance.ResetDefaults();

    private void Refresh()
    {
        var settings = GameSettings.Instance;
        _volume.SetValueWithoutNotify(settings.Volume);
        _brightness.SetValueWithoutNotify(settings.Brightness);
        _fullscreen.SetIsOnWithoutNotify(settings.Fullscreen);
        _volumeValue.text = Mathf.RoundToInt(settings.Volume * 100) + "%";
        _brightnessValue.text = Mathf.RoundToInt(settings.Brightness * 100) + "%";
        _languageValue.text = LocalizedDialogueSystem.CurrentLanguage == Language.Russian ? "Русский" : "English";
    }
}
