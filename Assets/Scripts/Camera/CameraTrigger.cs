using UnityEngine;
using System.Collections;

public class CameraTrigger : MonoBehaviour
{
    [SerializeField] private Transform _oldTarget;
    [SerializeField] private Transform _newTarget;
    [SerializeField] private CameraController _cameraController;

    [SerializeField] private float _delayDuration;

    private WaitForSeconds _waiter;
    private Coroutine _coroutine;

    private bool _isActivated = false;

    private void Awake()
    {
        _waiter = new WaitForSeconds(_delayDuration);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_isActivated == true) return;

        if (other.TryGetComponent(out Player _))
        {
            _coroutine = StartCoroutine(ChangeFollowTarget());
            _isActivated = true;
        }
    }

    private void OnDisable()
    {
        if (_coroutine != null)
        {
            StopCoroutine(_coroutine);
            _coroutine = null;
        }
    }

    private IEnumerator ChangeFollowTarget()
    {
        _cameraController.ChangeFollowTarget(_newTarget);
        yield return _waiter;
        _cameraController.ChangeFollowTarget(_oldTarget);
    }
}