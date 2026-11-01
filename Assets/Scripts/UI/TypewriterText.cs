using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

// Печатает текст TextMeshPro посимвольно. Вешается на объект с любым TMP_Text
// и запускается через Play(text); Complete() сразу показывает текст целиком.
[RequireComponent(typeof(TMP_Text))]
public class TypewriterText : MonoBehaviour
{
    [SerializeField] private float _charactersPerSecond = 30f;

    [Header("Пауза после знаков препинания")]
    [SerializeField] private float _punctuationDelay = 0.15f;
    [SerializeField] private string _punctuation = ".,!?;:…";

    [SerializeField] private bool _useUnscaledTime = false;

    [Tooltip("Dedicated non-looping voice source. The profile comes from the current speaker.")]
    [FormerlySerializedAs("_typingAudio")]
    [SerializeField] private AudioSource _voiceAudio;

    public event Action Finished;

    private TMP_Text _text;
    private Coroutine _typingCoroutine;
    private bool _isTyping;
    private bool _audioPaused;
    private DialogueVoiceProfile _voiceProfile;
    private DialogueVoiceProfile _bagProfile;
    private readonly List<AudioClip> _voiceBag = new List<AudioClip>();
    private readonly List<AudioClip> _recentClips = new List<AudioClip>();
    private float _speechWindow;
    private float _minimumStartDelay;
    private float _gapRemaining;
    private bool _voiceWasPlaying;
    private DialogueVoiceEnvelope _voiceEnvelope;
    private float _voiceTimeRemaining;

    public bool IsTyping => _isTyping;

    private TMP_Text Text
    {
        get
        {
            if (_text == null)
            {
                _text = GetComponent<TMP_Text>();
            }

            return _text;
        }
    }

    public void Play(string text, DialogueVoiceProfile voiceProfile = null)
    {
        StopTyping();
        _voiceProfile = voiceProfile;
        if (_bagProfile != voiceProfile)
        {
            _voiceBag.Clear();
            _recentClips.Clear();
            _bagProfile = voiceProfile;
        }

        Text.text = text;
        Text.maxVisibleCharacters = 0;

        if (isActiveAndEnabled == false)
        {
            ShowAll();
            return;
        }

        _isTyping = true;
        _typingCoroutine = StartCoroutine(TypeRoutine());
        // Пустая/односимвольная реплика может завершиться до возврата StartCoroutine.
        if (!_isTyping) _typingCoroutine = null;
    }

    public void Complete()
    {
        if (IsTyping == false) return;

        StopTyping();
        ShowAll();
    }

    private void OnDisable()
    {
        // Корутина остановится вместе с объектом, поэтому дописываем текст сразу
        Complete();
    }

    private void Update()
    {
        if (!IsTyping || _voiceAudio == null || _voiceProfile == null) return;
        bool paused = !_useUnscaledTime && Time.timeScale <= 0f;
        if (paused != _audioPaused)
        {
            if (paused) _voiceAudio.Pause();
            else _voiceAudio.UnPause();
            _audioPaused = paused;
        }
        if (paused) return;

        float dt = _useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
        _voiceTimeRemaining -= dt;
        _speechWindow -= dt;
        _minimumStartDelay -= dt;
        if (_voiceEnvelope != null && _voiceEnvelope.IsReleasing) return;
        if (_voiceAudio.isPlaying) return;
        if (_voiceWasPlaying)
        {
            _voiceWasPlaying = false;
            Vector2 pause = _voiceProfile.PauseRange;
            _gapRemaining = UnityEngine.Random.Range(pause.x, pause.y);
        }
        _gapRemaining -= dt;
        if (_speechWindow <= 0f || _gapRemaining > 0f || _minimumStartDelay > 0f) return;

        Vector2 pitch = _voiceProfile.PitchRange;
        float playbackPitch = UnityEngine.Random.Range(pitch.x, pitch.y);
        // Do not start a syllable that would have to be cut off by the last letter.
        AudioClip clip = NextVoiceClip(Mathf.Max(0f, _voiceTimeRemaining - 0.06f) * playbackPitch);
        if (clip == null) return;
        if (_voiceEnvelope == null)
        {
            _voiceEnvelope = _voiceAudio.GetComponent<DialogueVoiceEnvelope>();
            if (_voiceEnvelope == null) _voiceEnvelope = _voiceAudio.gameObject.AddComponent<DialogueVoiceEnvelope>();
        }
        _voiceEnvelope.Play(clip, _voiceProfile.Volume, playbackPitch);
        _voiceWasPlaying = true;
        _minimumStartDelay = 1f / _voiceProfile.FragmentsPerSecond;
    }

    private AudioClip NextVoiceClip(float maxLength)
    {
        if (_voiceBag.Count == 0)
        {
            if (_voiceProfile.Clips == null) return null;
            foreach (AudioClip clip in _voiceProfile.Clips)
                if (clip != null && !_voiceBag.Contains(clip)) _voiceBag.Add(clip);
            ShuffleVoiceBag();
        }
        if (_voiceBag.Count == 0) return null;

        int selected = -1;
        // Avoid the last two clips across bag boundaries too; small sets degrade gracefully.
        for (int history = _recentClips.Count; history >= 0; history--)
        {
            int candidate = _voiceBag.FindLastIndex(c =>
                c.length <= maxLength && !WasRecentlyPlayed(c, history));
            if (candidate < 0) continue;
            selected = candidate;
            break;
        }
        if (selected < 0)
        {
            // Long unused fragments must not leave successive short lines permanently silent.
            bool added = false;
            if (_voiceProfile.Clips != null)
            foreach (AudioClip clip in _voiceProfile.Clips)
            {
                if (clip == null || clip.length > maxLength || _voiceBag.Contains(clip)) continue;
                _voiceBag.Add(clip);
                added = true;
            }
            if (!added) return null;
            ShuffleVoiceBag();
            return NextVoiceClip(maxLength);
        }
        AudioClip result = _voiceBag[selected];
        _voiceBag.RemoveAt(selected);
        _recentClips.Add(result);
        if (_recentClips.Count > 2) _recentClips.RemoveAt(0);
        return result;
    }

    private void ShuffleVoiceBag()
    {
        for (int i = _voiceBag.Count - 1; i > 0; i--)
        {
            int j = UnityEngine.Random.Range(0, i + 1);
            AudioClip swap = _voiceBag[i];
            _voiceBag[i] = _voiceBag[j];
            _voiceBag[j] = swap;
        }
    }

    private bool WasRecentlyPlayed(AudioClip clip, int count)
    {
        for (int i = _recentClips.Count - count; i < _recentClips.Count; i++)
            if (_recentClips[i] == clip) return true;
        return false;
    }

    private IEnumerator TypeRoutine()
    {
        Text.ForceMeshUpdate();

        TMP_TextInfo textInfo = Text.textInfo;
        int characterCount = textInfo.characterCount;
        float delay = 0f;
        float remainingVoiceTime = 0f;
        for (int i = 0; i < characterCount - 1; i++)
        {
            remainingVoiceTime += 1f / Mathf.Max(1f, _charactersPerSecond);
            if (_punctuation.IndexOf(textInfo.characterInfo[i].character) >= 0)
                remainingVoiceTime += _punctuationDelay;
        }

        for (int i = 0; i < characterCount; i++)
        {
            while (delay > 0f)
            {
                yield return null;
                delay -= _useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            }

            Text.maxVisibleCharacters = i + 1;
            char character = textInfo.characterInfo[i].character;
            _voiceTimeRemaining = remainingVoiceTime;
            remainingVoiceTime -= 1f / Mathf.Max(1f, _charactersPerSecond);
            if (_punctuation.IndexOf(character) >= 0) remainingVoiceTime -= _punctuationDelay;
            if (char.IsLetterOrDigit(character))
                _speechWindow = Mathf.Max(0.12f, 2f / Mathf.Max(1f, _charactersPerSecond));
            else if (_punctuation.IndexOf(character) >= 0)
                _speechWindow = 0f;

            // Остаток задержки переносится, поэтому за кадр может появиться несколько символов
            delay += 1f / Mathf.Max(1f, _charactersPerSecond);

            if (_punctuation.IndexOf(textInfo.characterInfo[i].character) >= 0)
            {
                delay += _punctuationDelay;
            }
        }

        StopVoice();
        _isTyping = false;
        _typingCoroutine = null;
        ShowAll();
    }

    private void StopTyping()
    {
        StopVoice();
        _isTyping = false;
        if (_typingCoroutine != null)
        {
            StopCoroutine(_typingCoroutine);
            _typingCoroutine = null;
        }
    }

    private void StopVoice()
    {
        if (_voiceAudio != null)
        {
            if (_voiceEnvelope != null) _voiceEnvelope.Release();
            else _voiceAudio.Stop();
        }
        _audioPaused = false;
        _voiceWasPlaying = false;
        _speechWindow = 0f;
        _minimumStartDelay = 0f;
        _gapRemaining = 0f;
        _voiceTimeRemaining = 0f;
    }

    private void ShowAll()
    {
        Text.maxVisibleCharacters = int.MaxValue;
        Finished?.Invoke();
    }
}
