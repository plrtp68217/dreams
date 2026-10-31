using UnityEngine;

// Pause scene audio without muting the menu's UI source or changing AudioListener state.
public class FastFoodAudioPause : MonoBehaviour
{
    [SerializeField] private AudioSource[] _sources;
    private bool[] _resume;
    private bool _paused;

    private void Awake() => _resume = new bool[_sources.Length];

    private void LateUpdate()
    {
        bool paused = Time.timeScale <= 0f;
        if (paused == _paused) return;
        if (paused)
        {
            for (int i = 0; i < _sources.Length; i++)
            {
                var source = _sources[i];
                _resume[i] = source != null && source.isPlaying;
                if (_resume[i]) source.Pause();
            }
        }
        else Resume();
        _paused = paused;
    }

    private void Resume()
    {
        for (int i = 0; i < _sources.Length; i++)
        {
            if (_resume[i] && _sources[i] != null) _sources[i].UnPause();
            _resume[i] = false;
        }
    }

    private void OnDisable()
    {
        if (_paused) Resume();
        _paused = false;
    }
}
