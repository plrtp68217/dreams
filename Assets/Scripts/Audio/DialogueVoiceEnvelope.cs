using System.Threading;
using UnityEngine;

// Audio-rate fades avoid cutting a voice at an arbitrary nonzero sample.
[DisallowMultipleComponent]
[RequireComponent(typeof(AudioSource))]
public sealed class DialogueVoiceEnvelope : MonoBehaviour
{
    private const float AttackSeconds = 0.008f;
    private const float ReleaseSeconds = 0.045f;
    private AudioSource _source;
    private float _gain;
    private float _target;
    private float _attackStep;
    private float _releaseStep;
    private float _bufferMargin;
    private bool _releasing;
    private double _stopAfter;

    public bool IsReleasing => _releasing;

    private void Awake()
    {
        _source = GetComponent<AudioSource>();
        ConfigureSampleRate();
    }

    private void OnEnable() => AudioSettings.OnAudioConfigurationChanged += AudioConfigurationChanged;

    private void AudioConfigurationChanged(bool deviceChanged) => ConfigureSampleRate();

    private void ConfigureSampleRate()
    {
        int rate = Mathf.Max(8000, AudioSettings.outputSampleRate);
        Volatile.Write(ref _attackStep, 1f / (rate * AttackSeconds));
        Volatile.Write(ref _releaseStep, 1f / (rate * ReleaseSeconds));
        AudioSettings.GetDSPBufferSize(out int bufferLength, out int bufferCount);
        _bufferMargin = (float)(bufferLength * bufferCount) / rate;
    }

    public void Play(AudioClip clip, float volume, float pitch)
    {
        if (_source == null || _source.isPlaying || _releasing) return;
        Volatile.Write(ref _gain, 0f);
        Volatile.Write(ref _target, 1f);
        _source.clip = clip;
        _source.loop = false;
        _source.volume = volume;
        _source.pitch = pitch;
        _source.Play();
    }

    public void Release()
    {
        if (_source == null) return;
        // Paused/stopped sources are already silent and need no audible tail.
        if (!_source.isPlaying || !isActiveAndEnabled || !_source.enabled)
        {
            StopNow();
            return;
        }
        if (_releasing) return;
        _releasing = true;
        _stopAfter = AudioSettings.dspTime + ReleaseSeconds + _bufferMargin;
        Volatile.Write(ref _target, 0f);
    }

    private void Update()
    {
        if (_releasing && (!_source.isPlaying || AudioSettings.dspTime >= _stopAfter)) StopNow();
    }

    private void StopNow()
    {
        if (_source != null) _source.Stop();
        _releasing = false;
        Volatile.Write(ref _target, 0f);
        Volatile.Write(ref _gain, 0f);
    }

    private void OnAudioFilterRead(float[] data, int channels)
    {
        float gain = Volatile.Read(ref _gain);
        float target = Volatile.Read(ref _target);
        float step = target > gain ? Volatile.Read(ref _attackStep) : Volatile.Read(ref _releaseStep);
        for (int i = 0; i < data.Length; i += channels)
        {
            gain = target > gain ? System.Math.Min(target, gain + step) : System.Math.Max(target, gain - step);
            for (int channel = 0; channel < channels; channel++) data[i + channel] *= gain;
        }
        Volatile.Write(ref _gain, gain);
    }

    private void OnDisable()
    {
        AudioSettings.OnAudioConfigurationChanged -= AudioConfigurationChanged;
        StopNow();
    }
}
