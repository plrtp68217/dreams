using UnityEngine;
using System.Collections;

public class CameraTrigger : MonoBehaviour
{
    [SerializeField] private Transform _oldTarget;
    [SerializeField] private Transform _newTarget;
    [SerializeField] private CameraController _cameraController;

    [SerializeField] private float _delayDuration;

    private bool _isActivated = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_isActivated == true) return;

        if (other.CompareTag("Player"))
        {
            StartCoroutine(ChangeFollowTarget());
            _isActivated = true;
        }
    }

    private IEnumerator ChangeFollowTarget()
    {
        _cameraController.ChangeFollowTarget(_newTarget);
        yield return new WaitForSeconds(_delayDuration);
        _cameraController.ChangeFollowTarget(_oldTarget);
    }
}