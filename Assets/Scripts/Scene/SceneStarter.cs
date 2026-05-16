using UnityEngine;
using UnityEngine.UI;

public class SceneStarter : MonoBehaviour
{
    [SerializeField] AudioController _audioController;
    [SerializeField] AudioSource _audioSource;
    [SerializeField] Image _imageMask;
    [SerializeField] float fadeTime = 1.5f;

    private readonly float _startVolumeValue = 0f;

    private Coroutine _fadeAudioCoroutine;
    private Coroutine _fadeImageCoroutine;

    private void Start()
    {
        _audioController.SetVolume(_startVolumeValue);
        _audioController.StartAudio();

        _fadeImageCoroutine = StartCoroutine(TransitionUtils.FadeGraphic(_imageMask, fadeTime, 0f));
        _fadeAudioCoroutine = StartCoroutine(TransitionUtils.FadeAudio(_audioSource, fadeTime, _audioController.InitialVolume));
    }

    private void OnDisable()
    {
        if (_fadeImageCoroutine != null)
        {
            StopCoroutine(_fadeImageCoroutine);
            _fadeImageCoroutine = null;
        }

        if (_fadeImageCoroutine != null)
        {
            StopCoroutine(_fadeAudioCoroutine);
            _fadeAudioCoroutine = null;
        }
    }


}
