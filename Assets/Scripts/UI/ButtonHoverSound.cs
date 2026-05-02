using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonHoverSound : MonoBehaviour, IPointerEnterHandler
{
    [SerializeField] private AudioClip hoverClip;
    [SerializeField] private AudioSource audioSource;

    [SerializeField][Range(0f, 1f)] private float volume = 0.5f;

    private void Awake()
    {
        if (audioSource != null)
        {
            audioSource.playOnAwake = false;
            audioSource.loop = false;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (hoverClip != null && audioSource != null)
        {
            audioSource.PlayOneShot(hoverClip, volume);
        }
    }
}