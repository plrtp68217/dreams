using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(AudioSource), typeof(Animator))]
public class NpcFootstepAudio : MonoBehaviour
{
    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private Animator _animator;
    [SerializeField] private AudioClip[] _clips;
    [SerializeField, Range(0f, 1f)] private float _volume = 0.08f;
    [SerializeField] private Vector2 _pitchRange = new Vector2(0.98f, 1.02f);

    private static readonly int Walking = Animator.StringToHash(nameof(AnimatorBool.IsWalking));
    private Vector3 _lastPosition;
    private bool _moving;
    private bool _paused;
    private bool _resume;
    private AudioClip _lastClip;

    private bool WalkingAnimation => _animator != null && _animator.enabled && _animator.GetBool(Walking);

    private void Awake()
    {
        if (_audioSource == null) _audioSource = GetComponent<AudioSource>();
        if (_animator == null) _animator = GetComponent<Animator>();
    }

    private void OnEnable()
    {
        _lastPosition = transform.position;
        _moving = false;
    }

    private void LateUpdate()
    {
        float distanceSquared = (transform.position - _lastPosition).sqrMagnitude;
        _lastPosition = transform.position;
        if (_audioSource == null) return;
        if (Time.timeScale <= 0f)
        {
            if (!_paused)
            {
                _resume = _audioSource.isPlaying;
                if (_resume) _audioSource.Pause();
                _paused = true;
            }
            return;
        }

        // Ignore standing animation, canceled movement and spawn/route teleports.
        // The first resumed frame can still have zero deltaTime before WalkTo ticks.
        bool resumed = _paused;
        _moving = distanceSquared > 0.00000001f && distanceSquared < 0.25f;
        if (!WalkingAnimation || distanceSquared >= 0.25f || (!_moving && !resumed))
        {
            _audioSource.Stop();
        }
        else if (_paused && _resume)
        {
            _audioSource.UnPause();
        }
        _paused = false;
        _resume = false;
    }

    // Called by the existing walk clip at each foot contact.
    public void PlayFootStepSound()
    {
        if (!isActiveAndEnabled || !_moving || _paused || !WalkingAnimation || Time.timeScale <= 0f ||
            _audioSource == null || _audioSource.isPlaying || _clips == null) return;

        AudioClip selected = null;
        int candidates = 0;
        foreach (AudioClip clip in _clips)
        {
            if (clip == null || clip == _lastClip) continue;
            if (Random.Range(0, ++candidates) == 0) selected = clip;
        }
        if (selected == null)
        {
            foreach (AudioClip clip in _clips)
                if (clip != null) { selected = clip; break; }
        }
        if (selected == null) return;

        _audioSource.clip = selected;
        _audioSource.volume = Mathf.Clamp01(_volume);
        _audioSource.pitch = Random.Range(
            Mathf.Clamp(Mathf.Min(_pitchRange.x, _pitchRange.y), 0.5f, 1.5f),
            Mathf.Clamp(Mathf.Max(_pitchRange.x, _pitchRange.y), 0.5f, 1.5f));
        _audioSource.Play();
        _lastClip = selected;
    }

    private void OnDisable()
    {
        if (_audioSource != null) _audioSource.Stop();
        _paused = _resume = _moving = false;
    }
}
