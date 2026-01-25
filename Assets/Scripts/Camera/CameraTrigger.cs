using UnityEngine;
using System.Collections;

public class CameraTrigger : MonoBehaviour
{
    [SerializeField] private Transform _oldTarget;
    [SerializeField] private Transform _newTarget;
    [SerializeField] private CameraController _cameraController;

    [SerializeField] private float _delayDuration;


    private IEnumerator OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            _cameraController.ChangeFollowTarget(_newTarget);

            yield return new WaitForSeconds(_delayDuration);

            _cameraController.ChangeFollowTarget(_oldTarget);
        }
    }
}