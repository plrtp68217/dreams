using UnityEngine;

public class Parallax : MonoBehaviour
{
    [SerializeField] private Camera _camera;
    [SerializeField] private Transform _subject;

    private Vector2 _startPosition;
    private float _startZ;

    private Vector2 Travel => (Vector2)_camera.transform.position - _startPosition;
    private float DistanceFromSubject => transform.position.z - _subject.position.z;
    private float ClippingPlane => (_camera.transform.position.z + (DistanceFromSubject > 0 ? _camera.farClipPlane : _camera.nearClipPlane));
    private float ParallaxFactor => Mathf.Abs(DistanceFromSubject) / ClippingPlane;

    private void Start()
    {
        _startPosition = transform.position;
        _startZ = transform.localPosition.z;
    }

    private void FixedUpdate()
    {
        if (_subject == null) return;

        Vector2 newPosition = _startPosition + Travel * ParallaxFactor;
        transform.position = new Vector3(newPosition.x, transform.position.y, _startZ); 
    }
}
