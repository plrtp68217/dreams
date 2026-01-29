using UnityEngine;

public class CheckpointTrigger : MonoBehaviour
{
    [SerializeField] private CheckpointService _checkpointService;

    private bool _isEnabled = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_isEnabled == true) return;

        if (other.CompareTag("Player"))
        {
            _checkpointService.AddCheckpoint(transform);

            _isEnabled = true;

            Debug.Log("Checkpoint added");
        }
    }
}
