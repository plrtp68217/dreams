using System.Collections.Generic;
using UnityEngine;

public class CheckpointService : MonoBehaviour
{
    private readonly List<Transform> _checkpoints = new();

    public int CheckpointsCount => _checkpoints.Count;

    public void AddCheckpoint (Transform checkpoint) => _checkpoints.Add(checkpoint);

    public Transform GetLastCheckpoint() => _checkpoints[^1];
}