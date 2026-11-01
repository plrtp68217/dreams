using UnityEngine;

[CreateAssetMenu(fileName = "DialogueVoice", menuName = "DREAMS/Dialogue Voice Profile")]
public class DialogueVoiceProfile : ScriptableObject
{
    [SerializeField] private AudioClip[] _clips;
    [SerializeField, Range(0f, 1f)] private float _volume = 0.2f;
    [Tooltip("Maximum fragment starts per second. Fragments always finish before the next starts.")]
    [SerializeField, Range(0.2f, 10f)] private float _fragmentsPerSecond = 3f;
    [Tooltip("Random silence in seconds after each fragment.")]
    [SerializeField] private Vector2 _pauseRange = new Vector2(0.08f, 0.18f);
    [Tooltip("Random playback pitch; 1 is the original voice.")]
    [SerializeField] private Vector2 _pitchRange = new Vector2(0.94f, 1.04f);

    public AudioClip[] Clips => _clips;
    public float Volume => Mathf.Clamp01(_volume);
    public float FragmentsPerSecond => Mathf.Clamp(_fragmentsPerSecond, 0.2f, 10f);
    public Vector2 PauseRange => OrderedRange(_pauseRange, 0f, 5f);
    public Vector2 PitchRange => OrderedRange(_pitchRange, 0.25f, 3f);

    private static Vector2 OrderedRange(Vector2 value, float min, float max) =>
        new Vector2(Mathf.Clamp(Mathf.Min(value.x, value.y), min, max),
                    Mathf.Clamp(Mathf.Max(value.x, value.y), min, max));

    private void OnValidate()
    {
        _pauseRange = PauseRange;
        _pitchRange = PitchRange;
    }
}
