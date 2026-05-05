using UnityEngine;

public class AudioMovement : MonoBehaviour
{
    [SerializeField] private AudioSource _audioSource;

    [SerializeField] private AudioClip[] _audioFootstepClips;
    [SerializeField] private AudioClip[] _audioJumpClips;
    [SerializeField] private AudioClip _audioInteractionClip;

    [SerializeField] private float _volume = 0.5f;

    public int AudioFootstepClipIndex => Random.Range(0, _audioFootstepClips.Length);
    public int AudioJumpClipIndex => Random.Range(0, _audioJumpClips.Length);

    public void PlayFootStepSound()
    {
        if (_audioSource == null || _audioFootstepClips.Length == 0)
        {
            return;
        }

        _audioSource.PlayOneShot(_audioFootstepClips[AudioFootstepClipIndex], _volume);
    }

    public void PlayJumpSound()
    {
        if (_audioSource == null || _audioJumpClips.Length == 0)
        {
            return;
        }

        _audioSource.PlayOneShot(_audioJumpClips[AudioJumpClipIndex], _volume);
    }

    public void PlayIntreractionSound()
    {
        if (_audioSource == null || _audioInteractionClip == null)
        {
            return;
        }

        _audioSource.PlayOneShot(_audioInteractionClip, _volume);
    }
}