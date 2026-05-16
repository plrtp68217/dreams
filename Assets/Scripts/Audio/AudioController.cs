using UnityEngine;

public class AudioController : MonoBehaviour
{
    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private AudioClip _backgroundClip;

    [SerializeField][Range(0f, 1f)] private float _volume = 0.5f;

    public float InitialVolume => _volume;
    public float SourceVolume => _audioSource.volume;

    public void StartAudio()
    {
        if (_audioSource == null || _backgroundClip == null)
        {
            return;
        }

        _audioSource.loop = true;
        _audioSource.resource = _backgroundClip;
        _audioSource.Play();
    }

    public void SetVolume(float volume)
    {
        if (_audioSource == null)
        {
            return;
        }

        _audioSource.volume = Mathf.Clamp01(volume);
    }
}