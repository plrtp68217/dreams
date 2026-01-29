using System.Collections.Generic;
using UnityEngine;

public class CheckpointService : MonoBehaviour
{
    [SerializeField] private Player _playerPrefab;

    private readonly List<Transform> _checkpoints = new();

    public void AddCheckpoint (Transform checkpoint) => _checkpoints.Add(checkpoint);

    public Player RespawnPlayer()
    {
        if (_checkpoints.Count == 0) return null;

        var newPlayer = Instantiate(_playerPrefab, GetLastCheckpoint().position, Quaternion.identity);

        return newPlayer;
    }

    private Transform GetLastCheckpoint() => _checkpoints[^1];
}