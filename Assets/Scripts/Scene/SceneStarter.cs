using UnityEngine;
using UnityEngine.UI;

public class SceneStarter : MonoBehaviour
{
    [SerializeField] GraphicService _graphicService;
    [SerializeField] AudioService _audioService;

    [SerializeField] AudioController _audioController;
    [SerializeField] AudioSource _audioSource;

    [SerializeField] Image[] _imagesMask;

    private readonly float _startVolumeValue = 0f;

    private void Start()
    {
        _audioController.SetVolume(_startVolumeValue);
        _audioController.StartAudio();

        _graphicService.FadeGraphic(_imagesMask, 0f);
        _audioService.FadeAudio(_audioSource, _audioController.InitialVolume);
    }
}
