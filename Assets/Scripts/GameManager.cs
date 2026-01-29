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
        _player = _checkpointService.RespawnPlayer();

        if (_player == null) return;

        _cameraController.ChangeFollowTarget(_player.transform);
    }
}