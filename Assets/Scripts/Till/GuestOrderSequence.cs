using System.Collections;
using UnityEngine;

public class GuestOrderSequence : MonoBehaviour
{
    [SerializeField] private InputService _inputService;

    [Header("Посетитель")]
    [SerializeField] private Guest _guest;
    [SerializeField] private Transform _spawnPoint;
    [SerializeField] private Transform _orderPoint;

    [Header("Диалог посетителя")]
    [SerializeField] private Dialog _subtitles;
    [SerializeField] private string[] _phraseIds;
    [SerializeField] private float _delayBeforeDialog = 0.3f;

    [Header("Заказ")]
    [SerializeField] private Dialog _orderPanel;
    [SerializeField] private string _orderId;

    public IEnumerator Play()
    {
        _guest.transform.position = _spawnPoint.position;
        _guest.gameObject.SetActive(true);

        yield return _guest.WalkTo(_orderPoint.position);
        yield return new WaitForSeconds(_delayBeforeDialog);

        foreach (string phraseId in _phraseIds)
        {
            _subtitles.Enable(GetText(phraseId));
            yield return _inputService.WaitForKeyE();
        }

        _subtitles.Disable();
        _orderPanel.Enable(GetText(_orderId));
    }

    private string GetText(string id)
    {
        return LocalizedDialogueSystem.Instance.GetDialogueText(id);
    }
}
