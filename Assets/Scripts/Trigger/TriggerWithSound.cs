using UnityEngine;

public class TriggerWithSound : MonoBehaviour
{
    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private AudioClip[] _clips;

    [SerializeField][Range(0f, 1f)] private float _volume = 0.3f;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (_audioSource == null || _clips.Length == 0)
        {
            return;
        }

        if (collision.TryGetComponent(out Player _)) 
        {
            AudioClip clip = GetRandomClip();
            _audioSource.PlayOneShot(clip, _volume);
        }
    }

    private AudioClip GetRandomClip()
    {
        int randomClipIndex = Random.Range(0, _clips.Length);

        return _clips[randomClipIndex];
    }
}