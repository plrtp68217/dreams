using UnityEngine;

public class ShelterTrigger : MonoBehaviour
{
    [SerializeField] private SpriteService _spriteService;
    [SerializeField] private InputService _inputService;
    [SerializeField] private AEntity _entity;
    [SerializeField] private float _fadeDuration;

    private SpriteRenderer _spriteRenderer;

    private void OnTriggerStay2D(Collider2D other)
    {
        if (_inputService.ControlIsHolding)
        {
            HandleTriggerInside(other);
        }
        else
        {
            HandleTriggerOutside(other);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        HandleTriggerOutside(other);
    }

    private void HandleTriggerInside(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            _spriteRenderer = _entity.GetComponent<SpriteRenderer>();

            if (_spriteRenderer == null) return;

            _entity.ChangeShelterStatus(true);

            _spriteService.FadeSprite(_spriteRenderer, FadeDirection.Out, _fadeDuration);
        }
    }

    private void HandleTriggerOutside(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (_entity.IsInShelter == false) return;

            _entity.ChangeShelterStatus(false);

            _spriteService.FadeSprite(_spriteRenderer, FadeDirection.In, _fadeDuration);
        }
    }
}