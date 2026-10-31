using System.Collections;
using UnityEngine;

public class GuestOrderSequence : MonoBehaviour
{
    [Header("Посетитель")]
    [SerializeField] private NpcVisit _guestVisit;

    [Header("Заказ")]
    [SerializeField] private Dialog _orderPanel;
    [SerializeField] private string _orderId;
    [SerializeField] private ReceiptHover _receipt;

    private bool _hasOrder;

    private void OnEnable()
    {
        LocalizedDialogueSystem.LanguageChanged += RefreshOrder;
        RefreshOrder();
    }

    private void OnDisable()
    {
        LocalizedDialogueSystem.LanguageChanged -= RefreshOrder;
    }

    public IEnumerator Play()
    {
        yield return _guestVisit.Play();

        _hasOrder = true;
        RefreshOrder();
        if (_receipt != null) _receipt.ShowReceivedOrder();
    }

    private void RefreshOrder()
    {
        if (_hasOrder)
            _orderPanel.Enable(LocalizedDialogueSystem.Instance.GetDialogueText(_orderId));
    }
}
