using System.Collections;
using Unity.Cinemachine;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private CinemachineCamera _camera;
    [SerializeField] private float transitionDuration = 1f;

    private Coroutine _transitionCoroutine;

    public void ChangeFollowTarget(Transform target, bool smoothTransition = true)
    {
        if (target == null) return;

        if (_transitionCoroutine != null)
        {
            StopCoroutine(_transitionCoroutine);
        }

        if (smoothTransition)
        {
            _transitionCoroutine = StartCoroutine(SmoothTransition(target));
        }
        else
        {
            _camera.Follow = target;
        }
    }

    private IEnumerator SmoothTransition(Transform target)
    {
        Transform oldTarget = _camera.Follow;

        if (oldTarget == null)
        {
            _camera.Follow = target;
            yield break;
        }

        GameObject tempTarget = new("CameraTempTarget");
        tempTarget.transform.position = oldTarget.position;
        _camera.Follow = tempTarget.transform;

        float elapsedTime = 0f;
        Vector3 startPosition = tempTarget.transform.position;

        while (elapsedTime < transitionDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / transitionDuration;

            float smoothT = Mathf.SmoothStep(0f, 1f, t);
            tempTarget.transform.position = Vector3.Lerp(startPosition, target.position, smoothT);

            yield return null;
        }

        _camera.Follow = target;

        Destroy(tempTarget);
    }
}