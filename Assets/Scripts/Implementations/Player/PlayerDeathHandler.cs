using System.Collections;
using UnityEngine;

public class PlayerDeathHandler : MonoBehaviour
{
    [SerializeField] private InputService _inputService;
    [SerializeField] private CheckpointService _checkpointService;
    [SerializeField] private CameraController _cameraController;
    [SerializeField] private Player _player;

    [SerializeField] private float _deathDelay = 3f;

    private WaitForSeconds _waiter;
    private Coroutine _deathCoroutine;

    private void Awake()
    {
        _waiter = new WaitForSeconds(_deathDelay);
    }

    private void OnEnable()
    {
        _player.Died += OnDied;
    }

    private void OnDisable()
    {
        _player.Died -= OnDied;

        if (_deathCoroutine != null)
        {
            StopCoroutine(_deathCoroutine);
            _deathCoroutine = null;
        }
    }

    private void OnDied()
    {
        _deathCoroutine = StartCoroutine(OnDiedRoutine());
    }

    private IEnumerator OnDiedRoutine()
    {
        _inputService.Block();

        yield return _waiter;

        RespawnPlayer();

        _inputService.Unblock();
    }

    private void RespawnPlayer()
    {
        if (_checkpointService.CheckpointsCount == 0) return;

        var lastCheckpoint = _checkpointService.GetLastCheckpoint();
        _player.transform.position = lastCheckpoint.position;

        _player.StartInvincibility();
        _player.SetAlive(true);
        _player.SetVisibility(true);
    }
}