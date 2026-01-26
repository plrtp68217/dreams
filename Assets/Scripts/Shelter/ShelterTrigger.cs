using UnityEngine;

public class ShelterTrigger : MonoBehaviour
{
    [SerializeField] private SpriteService _spriteService;
    [SerializeField] private float _fadeDuration;

    private SpriteRenderer _enteredSpriteRenderer;

    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.CompareTag("Player"))
        {
            _enteredSpriteRenderer = collider.gameObject.GetComponent<SpriteRenderer>();

            Debug.Log(_enteredSpriteRenderer);

            if (_enteredSpriteRenderer == null) return;

            _spriteService.FadeSprite(_enteredSpriteRenderer, FadeDirection.Out, _fadeDuration);
        }
    }
    private void OnTriggerExit2D(Collider2D collider)
    {
        if (collider.CompareTag("Player"))
        {
            Debug.Log("exit");
            _spriteService.FadeSprite(_enteredSpriteRenderer, FadeDirection.In, _fadeDuration);
        }
    }
}