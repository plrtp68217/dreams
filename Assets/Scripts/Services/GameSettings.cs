using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

public class GameSettings : MonoBehaviour
{
    private const string VolumeKey = "Dreams.Settings.Volume";
    private const string BrightnessKey = "Dreams.Settings.Brightness";
    private const string LanguageKey = "Dreams.Settings.Language";
    private const string FullscreenKey = "Dreams.Settings.Fullscreen";

    public static GameSettings Instance { get; private set; }
    public event Action Changed;
    public float Volume { get; private set; }
    public float Brightness { get; private set; }
    public bool Fullscreen { get; private set; }

    private Volume _volume;
    private VolumeProfile _profile;
    private ColorAdjustments _exposure;
    private float _sceneExposure;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Initialize()
    {
        if (Instance == null) new GameObject("GameSettings").AddComponent<GameSettings>();
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        Volume = Mathf.Clamp01(PlayerPrefs.GetFloat(VolumeKey, 1f));
        Brightness = Mathf.Clamp(PlayerPrefs.GetFloat(BrightnessKey, 1f), 0.5f, 1.5f);
        Fullscreen = PlayerPrefs.GetInt(FullscreenKey, Screen.fullScreen ? 1 : 0) != 0;
        LocalizedDialogueSystem.CurrentLanguage = (Language)Mathf.Clamp(PlayerPrefs.GetInt(LanguageKey, 0), 0, 1);

        _volume = gameObject.AddComponent<Volume>();
        _volume.isGlobal = true;
        _volume.priority = 1000;
        _profile = ScriptableObject.CreateInstance<VolumeProfile>();
        _profile.name = "Player brightness (runtime)";
        _exposure = _profile.Add<ColorAdjustments>();
        _exposure.postExposure.overrideState = true;
        _volume.sharedProfile = _profile;
        AudioListener.volume = Volume;
        ApplyFullscreen();
        SceneManager.sceneLoaded += OnSceneLoaded;
        LocalizedDialogueSystem.LanguageChanged += RememberLanguage;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        _sceneExposure = 0;
        float highestPriority = float.NegativeInfinity;
        foreach (var other in FindObjectsByType<Volume>(FindObjectsSortMode.None))
        {
            if (other == _volume || !other.isGlobal || !other.enabled || other.weight <= 0 ||
                other.sharedProfile == null || other.priority < highestPriority) continue;
            if (other.sharedProfile.TryGet<ColorAdjustments>(out var color) && color.active && color.postExposure.overrideState)
            {
                _sceneExposure = color.postExposure.value;
                highestPriority = other.priority;
            }
        }
        var camera = Camera.main;
        if (camera != null && camera.TryGetComponent<UniversalAdditionalCameraData>(out var data))
        {
            data.renderPostProcessing = true;
            data.volumeLayerMask |= 1 << gameObject.layer;
        }
        ApplyBrightness();
    }

    public void SetVolume(float value)
    {
        Volume = Mathf.Clamp01(value);
        AudioListener.volume = Volume;
        PlayerPrefs.SetFloat(VolumeKey, Volume);
        Changed?.Invoke();
    }

    public void SetBrightness(float value)
    {
        Brightness = Mathf.Clamp(value, 0.5f, 1.5f);
        ApplyBrightness();
        PlayerPrefs.SetFloat(BrightnessKey, Brightness);
        Changed?.Invoke();
    }

    private void ApplyBrightness() => _exposure.postExposure.value = _sceneExposure + Mathf.Log(Brightness, 2);

    public void SetFullscreen(bool value)
    {
        Fullscreen = value;
        ApplyFullscreen();
        PlayerPrefs.SetInt(FullscreenKey, value ? 1 : 0);
        Changed?.Invoke();
    }

    private void ApplyFullscreen()
    {
        if (!Application.isEditor)
            Screen.fullScreenMode = Fullscreen ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed;
    }

    public void ToggleLanguage()
    {
        var language = LocalizedDialogueSystem.CurrentLanguage == Language.Russian ? Language.English : Language.Russian;
        if (LocalizedDialogueSystem.Instance != null) LocalizedDialogueSystem.Instance.SetLanguage(language);
    }

    private void RememberLanguage()
    {
        PlayerPrefs.SetInt(LanguageKey, (int)LocalizedDialogueSystem.CurrentLanguage);
        Changed?.Invoke();
    }

    public void ResetDefaults()
    {
        SetVolume(1f);
        SetBrightness(1f);
        SetFullscreen(true);
        Save();
    }

    public void Save() => PlayerPrefs.Save();

    private void OnApplicationQuit() => Save();

    private void OnDestroy()
    {
        if (Instance != this) return;
        SceneManager.sceneLoaded -= OnSceneLoaded;
        LocalizedDialogueSystem.LanguageChanged -= RememberLanguage;
        if (_profile != null)
        {
            foreach (var component in _profile.components) Destroy(component);
            Destroy(_profile);
        }
        Instance = null;
    }
}
