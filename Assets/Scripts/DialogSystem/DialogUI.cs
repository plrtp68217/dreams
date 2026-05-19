using UnityEngine;
using UnityEngine.UI;

public class DialogUI : Dialog
{
    [SerializeField] private GraphicService _graphicService;
    [SerializeField] private Image[] _images;

    public override void Enable(string text)
    {
        _graphicService.FadeGraphic(_images, 1f);
    }

    public override void Disable()
    {
        _graphicService.FadeGraphic(_images, 0f);
    }
}