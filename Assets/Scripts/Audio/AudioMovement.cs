using UnityEngine;

public class AudioMovement : MonoBehaviour
{
    [SerializeField] private AudioSource _audioSource;

    [SerializeField] private AudioClip[] _audioFootstepClips;
    [SerializeField] private AudioClip[] _audioRunningClips;
    [SerializeField] private AudioClip[] _audioJumpClips;
    [SerializeField] private AudioClip _audioInteractionClip;
    [SerializeField] private InputService _inputService;

    [SerializeField] private float _volume = 0.5f;

    public int AudioFootstepClipIndex => Random.Range(0, _audioFootstepClips.Length);
    public int AudioJumpClipIndex => Random.Range(0, _audioJumpClips.Length);

    private Player _player;

    private void Awake() => _player = GetComponent<Player>();

    private bool CanPlay => _audioSource != null && Time.timeScale > 0f &&
                            (_player == null || _player.IsAlive);

    public void PlayFootStepSound()
    {
        if (!CanPlay || (_inputService != null &&
            (_inputService.IsBlocked || Mathf.Abs(_inputService.Direction) < 0.01f ||
             (_player != null && !_player.IsOnGround))))
        {
            return;
        }

        bool running = _inputService != null && _inputService.ShiftIsHolding &&
                       !_inputService.ControlIsHolding;
        var clips = running && _audioRunningClips != null && _audioRunningClips.Length > 0
            ? _audioRunningClips : _audioFootstepClips;
        if (clips == null || clips.Length == 0) return;
        var clip = clips[Random.Range(0, clips.Length)];
        if (clip != null) _audioSource.PlayOneShot(clip, _volume);
    }

    public void PlayJumpSound()
    {
        if (!CanPlay || _audioJumpClips == null || _audioJumpClips.Length == 0)
        {
            return;
        }

        var clip = _audioJumpClips[AudioJumpClipIndex];
        if (clip != null) _audioSource.PlayOneShot(clip, _volume);
    }

    public void PlayIntreractionSound()
    {
        if (!CanPlay || _audioInteractionClip == null)
        {
            return;
        }

        _audioSource.PlayOneShot(_audioInteractionClip, _volume);
    }
}
