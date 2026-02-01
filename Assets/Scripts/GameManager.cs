using System.Collections;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private CheckpointService _checkpointService;
    [SerializeField] private CameraController _cameraController;
    [SerializeField] private Player _player;

    private void OnEnable()
    {
        _player.OnDied += HandlePlayerDeath;
    }

    private void OnDisable()
    {
        _player.OnDied -= HandlePlayerDeath;
    }

    private void HandlePlayerDeath()
    {
        StartCoroutine(HandlePlayerDeathCoroutine());
    }

    private IEnumerator HandlePlayerDeathCoroutine()
    {
        yield return new WaitForSeconds(3f);

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