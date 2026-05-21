using System.Collections;
using UnityEngine;

public class JoinTrigger : MonoBehaviour
{
    [SerializeField] private InputService _inputService;
    [SerializeField] private bool _blockInput = false;
    [SerializeField] private float _blockDelay = 3f;

    [TextArea(3, 10)]
    [SerializeField] private string _dialogText;
    [SerializeField] private Dialog _dialog;

    private bool _isVisited = false;

    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (_isVisited) return;

        if (collider.TryGetComponent(out Player player))
        {
            _dialog.Enable(_dialogText);
            player.SetActiveDialog(_dialog);

            if (_blockInput)
            {
                _inputService.BlockWithDelay(_blockDelay);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collider)
    {
        if (_isVisited) return;

        if (collider.TryGetComponent(out Player player))
        {
            _dialog.Disable();
            player.ResetActiveDialog();
            _isVisited = true;
        }

    }
}