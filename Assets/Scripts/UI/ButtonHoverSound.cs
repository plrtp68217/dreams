using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ButtonHoverSound : MonoBehaviour, IPointerEnterHandler
{
    [SerializeField] private AudioClip _hoverClip;
    [SerializeField] private AudioSource _audioSource;

    [SerializeField][Range(0f, 1f)] private float _volume = 0.5f;

    private Button _button;

    private void Awake()
    {
        if (_audioSource != null)
        {
            _audioSource.playOnAwake = false;
            _audioSource.loop = false;
        }

        _button = GetComponent<Button>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (_button.IsInteractable() == false) return;

        if (_hoverClip != null && _audioSource != null)
        {
            _audioSource.PlayOneShot(_hoverClip, _volume);
        }
    }
}