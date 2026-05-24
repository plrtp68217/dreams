using UnityEngine;

public class InteractionTrigger : MonoBehaviour
{
    [SerializeField] private string _dialogId;
    [SerializeField] private Dialog _dialogCanvas;
    [SerializeField] private Dialog _dialogUI;

    [SerializeField] private Animator _animator;
    [SerializeField] private InputService _inputService;

    private bool _isEntered = false;
    private bool _canvasIsActivated = false;

    private string _dialogText;

    private void Start()
    {
        _dialogText = LocalizedDialogueSystem.Instance.GetDialogueText(_dialogId);
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.TryGetComponent(out Player player))
        {
            _isEntered = true;
            _dialogUI.Enable();
            player.SetActiveDialog(_dialogUI);
        }
    }

    private void OnTriggerExit2D(Collider2D collider)
    {
        if (collider.TryGetComponent(out Player player))
        {
            _isEntered = false;
            _dialogUI.Disable();
            player.ResetActiveDialog();
        }
    }

    private void Update()
    {
        if (_inputService.EIsPressed && _isEntered)
        {
            if (_canvasIsActivated)
            {
                _dialogUI.Enable();
                _dialogCanvas.Disable();
                _canvasIsActivated = false;
                _inputService.Unblock();
            }
            else
            {
                _animator.SetTrigger(AnimatorTrigger.Reach.ToString());
                _dialogUI.Disable();
                _dialogCanvas.Enable(_dialogText);
                _canvasIsActivated = true;
                _inputService.Block();
            }
        }
    }
}