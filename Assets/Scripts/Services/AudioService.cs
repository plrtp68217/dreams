using System.Collections;
using UnityEngine;

public class AudioService : MonoBehaviour
{
    [SerializeField] private float _fadeTime = 1f;

    private Coroutine _fadeCoroutine;
    public bool CoroutineIsInProgress => _fadeCoroutine != null;

    public void FadeAudio(AudioSource audioSource, float targetVolume)
    {
        if (CoroutineIsInProgress)
        {
            StopCoroutine(_fadeCoroutine);
        }
        
        _fadeCoroutine = StartCoroutine(FadeAudioRoutine(audioSource, targetVolume));
    }

    private IEnumerator FadeAudioRoutine(AudioSource audioSource, float targetVolume)
    {
        float startVolume = audioSource.volume;
        float elapsedTime = 0f;

        while (elapsedTime < _fadeTime)
        {
            elapsedTime += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(startVolume, targetVolume, elapsedTime / _fadeTime);
            yield return null;
        }

        audioSource.volume = targetVolume;

        _fadeCoroutine = null;
    }
}
