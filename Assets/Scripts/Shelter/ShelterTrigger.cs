using UnityEngine;

public class ShelterTrigger : MonoBehaviour
{
    [SerializeField] private Player _player;
    [SerializeField] private SpriteService _spriteService;
    [SerializeField] private InputService _inputService;
    [SerializeField] private Dialog _ctrlDialog;
    [SerializeField] private float _spriteFadeDuration = 1f;

    private bool _isEntered;   // Игрок зашел в укрытие
    private bool _isActivated; // Укрытие активирвано, игрок спрятался

    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.TryGetComponent(out Player player))
        {
            _isEntered = true;
            _ctrlDialog.Enable();
            player.SetActiveDialog(_ctrlDialog);
        }
    }

    private void OnTriggerExit2D(Collider2D collider)
    {
        if (collider.TryGetComponent(out Player player))
        {
            _isEntered = false;
            _ctrlDialog.Disable();
            player.ResetActiveDialog();
        }
    }

    private void Update()
    {
        if (_inputService.ControlIsHolding == true && 
            _isEntered == true &&
            _isActivated == false
            )
        {
            _isActivated = true;
            _ctrlDialog.Disable();
            _inputService.Block();
            //_shelterAnimator.SetTrigger(TriggerBool.Open.ToString());
            _player.ChangeShelterStatus(true);
            _spriteService.FadeSprite(_player.SpriteRenderer, FadeDirection.Out, _spriteFadeDuration);
        }
        else if (_inputService.ControlIsHolding == false &&
                _isEntered == true &&
                _isActivated == true
                )
        {
            _isActivated = false;
            _ctrlDialog.Enable();
            _inputService.Unblock();
            //_shelterAnimator.SetTrigger(TriggerBool.Close.ToString());
            _player.ChangeShelterStatus(false);
            _spriteService.FadeSprite(_player.SpriteRenderer, FadeDirection.In, _spriteFadeDuration);
        }
    }
}