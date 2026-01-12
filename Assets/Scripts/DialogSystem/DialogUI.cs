using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class DialogUI : Dialog
{
    [SerializeField] private TextMeshProUGUI _textMeshPro;

    public override void Enable(string text)
    {
        _textMeshPro.text = text;
    }

    public override void Disable()
    {

    }
}