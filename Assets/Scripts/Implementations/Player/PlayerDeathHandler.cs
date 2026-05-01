using System.Collections;
using UnityEngine;

public class PlayerDeathHandler : MonoBehaviour
{
    [SerializeField] private CheckpointService _checkpointService;
    [SerializeField] private CameraController _cameraController;
    [SerializeField] private Player _player;

    [SerializeField] private float _deathDelay = 3f;

    private WaitForSeconds _waiter;

    private void Awake()
    {
        _waiter = new WaitForSeconds(_deathDelay);
    }

    private void OnEnable()
    {
        _player.Died += HandlePlayerDeath;
    }

    private void OnDisable()
    {
        _player.Died -= HandlePlayerDeath;
    }

    private void HandlePlayerDeath()
    {
        StartCoroutine(HandlePlayerDeathCoroutine());
    }

    private IEnumerator HandlePlayerDeathCoroutine()
    {
        yield return _waiter;

        RespawnPlayer();

        _cameraController.ChangeFollowTarget(_player.transform);
    }

    private void RespawnPlayer()
    {
        if (_checkpointService.CheckpointsCount == 0) return;

        var lastCheckpoint = _checkpointService.GetLastCheckpoint();

        _player.transform.position = lastCheckpoint.position;
        _player.SetAlive(true);
        _player.SetVisibility(true);
    }
}