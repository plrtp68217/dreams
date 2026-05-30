using UnityEngine;

public class AudioController : MonoBehaviour
{
    [SerializeField] private AudioSource _audioSourceMain;
    [SerializeField] private AudioClip _ambientClip;

    [SerializeField] private AudioSource _audioSourceBG;
    [SerializeField] private AudioClip _backgroundClip;

    [SerializeField][Range(0f, 1f)] private float _volume = 0.5f;

    public float InitialVolume => _volume;
    public float SourceVolume => _audioSourceMain.volume;

    public void StartAudio()
    {
        if (_audioSourceMain != null && _ambientClip != null)
        {
            _audioSourceMain.loop = true;
            _audioSourceMain.resource = _ambientClip;
            _audioSourceMain.Play();
        }

        if (_audioSourceBG != null && _backgroundClip != null)
        {
            _audioSourceBG.loop = true;
            _audioSourceBG.resource = _backgroundClip;
            _audioSourceBG.Play();
        }
    }

    public void SetVolume(float volume)
    {
        if (_audioSourceMain == null)
        {
            return;
        }

        _audioSourceMain.volume = Mathf.Clamp01(volume);
    }
}