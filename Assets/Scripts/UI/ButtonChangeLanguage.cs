using UnityEngine;
using UnityEngine.UI;

public class ButtonChangeLanguage : MonoBehaviour
{
    [SerializeField] private Sprite _spriteChecked;
    [SerializeField] private Sprite _spriteUnchecked;

    // true для Русского, false для English
    private bool _buttonState = true;
    private Image _image;


    private void Start()
    {
        _image = GetComponent<Image>();
    }

    public void ChangeLanguage()
    {
        _buttonState = !_buttonState;

        if (_buttonState == true)
        {
            LocalizedDialogueSystem.Instance.SetLanguage(Language.Russian);
            _image.sprite = _spriteUnchecked;
        }
        else
        {
            LocalizedDialogueSystem.Instance.SetLanguage(Language.English);
            _image.sprite = _spriteChecked;    

        }
    }
}