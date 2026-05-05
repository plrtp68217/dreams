using UnityEngine;

public class AudioController : MonoBehaviour
{
    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private AudioClip _backgroundClip;

    [SerializeField][Range(0f, 1f)] private float _volume = 0.5f;

    private void Start()
    {
        if (_audioSource == null || _backgroundClip == null)
        {
            return;
        }

        _audioSource.volume = _volume;
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