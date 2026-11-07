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
    [SerializeField] private FacelessOrderService _service;
    [SerializeField] private NpcVisit _completionVisit;
    [SerializeField] private FastFoodTension _clownEncounters;
    public event System.Action OrderReceived;
    public bool HasOrder => _hasOrder;

    private bool _hasOrder;
    private bool _receivingOrder;

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
        if (_hasOrder || _receivingOrder) yield break;
        _receivingOrder = true;
        try
        {
            yield return _guestVisit.Play();
            _hasOrder = true;
            if (_service != null) _service.AcceptOrder();
            RefreshOrder();
            if (_receipt != null) _receipt.ShowReceivedOrder();
            OrderReceived?.Invoke();
        }
        finally { _receivingOrder = false; }
    }

    public IEnumerator FinishOrder()
    {
        if (_clownEncounters != null) yield return _clownEncounters.PlayBeforeServing();
        if (_completionVisit != null) yield return _completionVisit.Play();
    }

    public void RefreshOrder()
    {
        if (_hasOrder)
            _orderPanel.Enable(_service != null ? _service.ReceiptText() : LocalizedDialogueSystem.Instance.GetDialogueText(_orderId));
    }
}
