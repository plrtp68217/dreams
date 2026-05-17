using UnityEngine;

public class AudioController : MonoBehaviour
{
    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private AudioClip _ambientClip;
    [SerializeField] private AudioClip _backgroundClip;

    [SerializeField][Range(0f, 1f)] private float _volume = 0.5f;

    public float InitialVolume => _volume;
    public float SourceVolume => _audioSource.volume;

    public void StartAudio()
    {
        if (_audioSource == null || _ambientClip == null)
        {
            return;
        }

        _audioSource.loop = true;
        _audioSource.resource = _ambientClip;
        _audioSource.Play();

        if (_backgroundClip != null)
        {
            _audioSource.PlayOneShot(_backgroundClip, _volume);
        }
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