using System.Collections;
using UnityEngine;

public class JoinTrigger : MonoBehaviour
{
    [SerializeField] private Player _player;

    [SerializeField] private InputService _inputService;

    [Header("Выбираем одно из двух:")]

    [Header("Используется для отображения Canvas")]
    [SerializeField] private bool _disableWithKey = false;

    [Header("Используется для отображения UI над игроком")]
    [SerializeField] private bool _blockInputWidthDelay = false;
    [SerializeField] private float _blockDelay = 3f;

    [SerializeField] private Dialog _dialog;
    [SerializeField] private string _dialogId;

    private bool _isEntered = false;
    private bool _isVisited = false;

    private string _dialogText;

    private void Start()
    {
        _dialogText = LocalizedDialogueSystem.Instance.GetDialogueText(_dialogId);
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (_isVisited) return;

        if (collider.TryGetComponent(out Player player))
        {
            _isEntered = true;
            _dialog.Enable(_dialogText);
            player.SetActiveDialog(_dialog);


            if (_disableWithKey)
            {
                _inputService.Block();
            }

            if (_blockInputWidthDelay)
            {
                _inputService.BlockWithDelay(_blockDelay);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collider)
    {
        if (_isVisited) return;

        if (_disableWithKey) return;

        if (collider.TryGetComponent(out Player player))
        {
            _dialog.Disable();
            player.ResetActiveDialog();
            _isVisited = true;
        }

        _isEntered = false;
    }

    private void Update()
    {
        if (_disableWithKey && _isEntered && _inputService.EIsPressed)
        {
            _dialog.Disable();
            _inputService.Unblock();
            _player.ResetActiveDialog();
            _isVisited = true;
            _isEntered = false;
        }
    }
}