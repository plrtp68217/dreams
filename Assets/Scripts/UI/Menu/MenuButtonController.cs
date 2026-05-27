using UnityEngine;

public class MenuButtonController : MonoBehaviour
{
    [SerializeField] private CanvasGroupService _canvasGroupService;
    [SerializeField] private CanvasGroup _optionsCanvasGroup;

    public void OpenOptions()
    {
        _canvasGroupService.Open(_optionsCanvasGroup);
    }

    public void CloseOptions()
    {
        _canvasGroupService.Close(_optionsCanvasGroup);
    }
}