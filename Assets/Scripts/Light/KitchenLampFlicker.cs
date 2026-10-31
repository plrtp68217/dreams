using System;
using UnityEngine;
using UnityEngine.Rendering.Universal;

// One frame clock drives both the Aseprite image and every lamp's physical light.
public class KitchenLampFlicker : MonoBehaviour
{
    [Serializable]
    private class Frame
    {
        public Sprite sprite;
        public float duration;
        public float[] lampLevels;
    }

    [SerializeField] private SpriteRenderer _background;
    [SerializeField] private Light2D[] _lamps;
    [SerializeField] private Frame[] _frames;
    [SerializeField] private Transform _player;
    [SerializeField] private Collider2D _kitchenBounds;
    [SerializeField, Min(1f)] private float _minInterval = 6f;
    [SerializeField, Min(1f)] private float _maxInterval = 12f;

    private float[] _intensities;
    private float _cooldown;
    private float _remainingFrameTime;
    private bool _wasInKitchen;
    private bool _playing;
    private int _frameIndex;

    public int CurrentFrame => _frameIndex;
    public bool IsFlickering => _playing;
    public float TimeUntilFlicker => _cooldown;

    private void Awake()
    {
        _intensities = new float[_lamps.Length];
        for (int i = 0; i < _lamps.Length; i++) _intensities[i] = _lamps[i].intensity;
        ApplyFrame(0);
        ScheduleNext();
    }

    private void Update()
    {
        Bounds bounds = _kitchenBounds.bounds;
        Vector3 position = _player.position;
        bool inKitchen = position.x >= bounds.min.x && position.x <= bounds.max.x &&
                         position.y >= bounds.min.y && position.y <= bounds.max.y;
        if (!inKitchen)
        {
            if (_wasInKitchen)
            {
                _playing = false;
                ApplyFrame(0);
                ScheduleNext();
            }
            _wasInKitchen = false;
            return;
        }
        _wasInKitchen = true;

        if (!_playing)
        {
            _cooldown -= Time.deltaTime;
            if (_cooldown <= 0) PlayFlicker();
            return;
        }

        _remainingFrameTime -= Time.deltaTime;
        while (_playing && _remainingFrameTime <= 0)
        {
            if (_frameIndex + 1 >= _frames.Length)
            {
                _playing = false;
                ApplyFrame(0);
                ScheduleNext();
            }
            else
            {
                ApplyFrame(_frameIndex + 1);
                _remainingFrameTime += _frames[_frameIndex].duration;
            }
        }
    }

    public void PlayFlicker()
    {
        if (_playing) return;
        _playing = true;
        ApplyFrame(0);
        _remainingFrameTime = _frames[0].duration;
    }

    private void ApplyFrame(int index)
    {
        _frameIndex = index;
        var frame = _frames[index];
        _background.sprite = frame.sprite;
        for (int i = 0; i < _lamps.Length; i++)
        {
            float level = frame.lampLevels[i];
            _lamps[i].intensity = _intensities[i] * level;
            _lamps[i].enabled = level > 0.001f;
        }
    }

    private void ScheduleNext() => _cooldown = UnityEngine.Random.Range(_minInterval, Mathf.Max(_minInterval, _maxInterval));

    private void OnDisable()
    {
        if (_intensities == null) return;
        _playing = false;
        _wasInKitchen = false;
        ApplyFrame(0);
        ScheduleNext();
    }
}
