using UnityEngine;

public class ShelterTrigger : MonoBehaviour
{
    [SerializeField] private SpriteService _spriteService;
    [SerializeField] private InputService _inputService;
    [SerializeField] private AEntity _entity;
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

    private void OnTriggerStay2D(Collider2D collider)
    {
        if (collider.TryGetComponent(out Player _))
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
                _entity.ChangeShelterStatus(true);
                _spriteService.FadeSprite(_entity.SpriteRenderer, FadeDirection.Out, _spriteFadeDuration);
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
                _entity.ChangeShelterStatus(false);
                _spriteService.FadeSprite(_entity.SpriteRenderer, FadeDirection.In, _spriteFadeDuration);
            }
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
}