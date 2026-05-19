using UnityEngine;
using UnityEngine.UI;

public class SceneEndTrigger : MonoBehaviour
{
    [SerializeField] private InputService _inputService;
    [SerializeField] private GraphicService _graphicService;

    [SerializeField] private Animator _titlesAnimator;
    [SerializeField] private Image[] _imagesMask;

    private bool isEntered = false; 

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent(out Player _) && isEntered == false)
        {
            isEntered = true;

            _inputService.Block();
            _graphicService.FadeGraphic(_imagesMask, 1f);
            _titlesAnimator.SetTrigger(AnimatorTrigger.Show.ToString());
        }
    }
}