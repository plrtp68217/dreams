using System;
using UnityEngine;

public enum FastFoodOrderItem { Burger, Fries, Drink, Toy }

[DisallowMultipleComponent]
public class FastFoodInventory : MonoBehaviour
{
    [SerializeField] private Sprite[] _orderIcons = new Sprite[4];
    [SerializeField] private Sprite _medicalRecordIcon;
    private int _tray;
    private bool _hasMedicalRecord;

    public event Action Changed;
    public int TrayCount
    {
        get { int count = 0; for (int i = 0; i < 4; i++) if (Has((FastFoodOrderItem)i)) count++; return count; }
    }
    public bool IsOrderReady => _tray == 15;
    public bool HasMedicalRecord => _hasMedicalRecord;
    public Sprite MedicalRecordIcon => _medicalRecordIcon;
    public Sprite Icon(FastFoodOrderItem item) => _orderIcons[(int)item];
    public bool Has(FastFoodOrderItem item) => (_tray & (1 << (int)item)) != 0;

    public bool Collect(FastFoodOrderItem item)
    {
        if (Has(item)) return false;
        _tray |= 1 << (int)item;
        Changed?.Invoke();
        return true;
    }

    public bool DeliverOrder()
    {
        if (!IsOrderReady || _hasMedicalRecord) return false;
        _tray = 0;
        _hasMedicalRecord = true;
        Changed?.Invoke();
        return true;
    }
}
