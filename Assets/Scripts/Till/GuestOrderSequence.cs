using System.Collections;
using UnityEngine;

public class GuestOrderSequence : MonoBehaviour
{
    [Header("Посетитель")]
    [SerializeField] private NpcVisit _guestVisit;

    [Header("Заказ")]
    [SerializeField] private Dialog _orderPanel;
    [SerializeField] private string _orderId;

    public IEnumerator Play()
    {
        yield return _guestVisit.Play();

        _orderPanel.Enable(LocalizedDialogueSystem.Instance.GetDialogueText(_orderId));
    }
}
