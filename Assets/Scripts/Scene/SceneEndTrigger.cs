using UnityEngine;
using UnityEngine.UI;

public class SceneEndTrigger : MonoBehaviour
{
    [SerializeField] private InputService _inputService;
    [SerializeField] private Animator _titlesAnimator;
    [SerializeField] private Image _imageMask;
    [SerializeField] private float _fadeTime = 2f;

    private Coroutine _fadeMaskCoroutine;
    private bool isEntered = false; 

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent(out Player _) && isEntered == false)
        {
            _inputService.Block();

            isEntered = true;

            _fadeMaskCoroutine = StartCoroutine(TransitionUtils.FadeGraphic(_imageMask, _fadeTime, 1f));

            _titlesAnimator.SetTrigger(AnimatorTrigger.Show.ToString());
        }
    }

    private void OnDisable()
    {
        if (_fadeMaskCoroutine != null)
        {
            StopCoroutine(_fadeMaskCoroutine);
            _fadeMaskCoroutine = null;
        }   
    }
}
