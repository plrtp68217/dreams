using UnityEngine;
using System.Collections;

public class CameraTrigger : MonoBehaviour
{
    [SerializeField] private InputService _inputService;
    [SerializeField] private bool _blockInput = false;
    [SerializeField] private float _blockDelay = 3f;

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
            _coroutine = StartCoroutine(ChangeFollowTargetByTime());
            _isActivated = true;

            if (_blockInput == true)
            {
                _inputService.BlockWithDelay(_blockDelay);
            }
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

    private IEnumerator ChangeFollowTargetByTime()
    {
        _cameraController.ChangeFollowTarget(_newTarget);
        yield return _waiter;
        _cameraController.ChangeFollowTarget(_oldTarget);
    }
}