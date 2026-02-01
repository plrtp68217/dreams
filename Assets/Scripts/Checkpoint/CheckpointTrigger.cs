using UnityEngine;

public class CheckpointTrigger : MonoBehaviour
{
    [SerializeField] private CheckpointService _checkpointService;

    private bool _isAdded = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_isAdded == true) return;

        if (other.CompareTag("Player"))
        {
            _checkpointService.AddCheckpoint(transform);

            _isAdded = true;
        }
    }
}