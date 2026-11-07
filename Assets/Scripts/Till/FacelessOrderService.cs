using System;
using UnityEngine;

public enum FastFoodStationKind { Fryer, FuseBox, Fridge, Dispenser, Mirror, Guest }
public enum FacelessServiceAction { PowerOff, RemoveFuse, DrainOil, TakeBurger, TakeFries, PourDrink, TakeDrink, LookInMirror, TakeToy }

[DisallowMultipleComponent]
public class FacelessOrderService : MonoBehaviour
{
    [SerializeField] private FastFoodInventory _inventory;
    [SerializeField] private InputService _input;
    [SerializeField] private GuestOrderSequence _orderSequence;
    [SerializeField] private FastFoodServiceView _view;
    [SerializeField] private Player _player;
    [SerializeField] private TillTrigger _counter;
    [SerializeField] private string _fridgeCode = "0417";
    [SerializeField] private Vector3 _recipe = new Vector3(30, 20, 50);

    public event Action Changed;
    public event Action OrderAccepted;
    public bool IsActive { get; private set; }
    public bool IsServing { get; private set; }
    public bool IsCompleted { get; private set; }
    public bool PowerOff { get; private set; }
    public bool FuseRemoved { get; private set; }
    public bool OilDrained { get; private set; }
    public bool FridgeUnlocked { get; private set; }
    public bool DrinkReady { get; private set; }
    public bool MirrorLooked { get; private set; }
    public string FridgeCode => _fridgeCode;
    public Vector3 Recipe => _recipe;
    public FastFoodInventory Inventory => _inventory;

    public void AcceptOrder()
    {
        if (IsActive || IsCompleted) return;
        IsActive = true;
        OrderAccepted?.Invoke();
        Notify();
    }

    public bool EnterFridgeCode(string code)
    {
        if (!CanInteract || code != _fridgeCode) return false;
        FridgeUnlocked = true;
        Notify();
        return true;
    }

    public bool CorrectRecipe(Vector3 values) => values == _recipe;
    public bool CanInteract => IsActive && !IsCompleted && !IsServing;
    public bool CanPerform(FacelessServiceAction action)
    {
        if (!CanInteract) return false;
        switch (action)
        {
            case FacelessServiceAction.PowerOff: return !PowerOff;
            case FacelessServiceAction.RemoveFuse: return PowerOff && !FuseRemoved;
            case FacelessServiceAction.DrainOil: return FuseRemoved && !OilDrained;
            case FacelessServiceAction.TakeBurger: return OilDrained && !_inventory.Has(FastFoodOrderItem.Burger);
            case FacelessServiceAction.TakeFries: return FridgeUnlocked && !_inventory.Has(FastFoodOrderItem.Fries);
            case FacelessServiceAction.PourDrink: return !DrinkReady && !_inventory.Has(FastFoodOrderItem.Drink);
            case FacelessServiceAction.TakeDrink: return DrinkReady && !_inventory.Has(FastFoodOrderItem.Drink);
            case FacelessServiceAction.LookInMirror: return !MirrorLooked;
            case FacelessServiceAction.TakeToy: return MirrorLooked && !_inventory.Has(FastFoodOrderItem.Toy);
            default: return false;
        }
    }

    // Called after the corresponding one-shot animation completes, so closing a view
    // midway through a pour/drain cannot award an unfinished item.
    public bool Perform(FacelessServiceAction action)
    {
        if (!CanPerform(action)) return false;
        switch (action)
        {
            case FacelessServiceAction.PowerOff: PowerOff = true; break;
            case FacelessServiceAction.RemoveFuse: FuseRemoved = true; break;
            case FacelessServiceAction.DrainOil: OilDrained = true; break;
            case FacelessServiceAction.TakeBurger: _inventory.Collect(FastFoodOrderItem.Burger); break;
            case FacelessServiceAction.TakeFries: _inventory.Collect(FastFoodOrderItem.Fries); break;
            case FacelessServiceAction.PourDrink: DrinkReady = true; break;
            case FacelessServiceAction.TakeDrink: _inventory.Collect(FastFoodOrderItem.Drink); break;
            case FacelessServiceAction.LookInMirror: MirrorLooked = true; break;
            case FacelessServiceAction.TakeToy: _inventory.Collect(FastFoodOrderItem.Toy); break;
        }
        Notify();
        return true;
    }

    // TillTrigger owns the counter visit and its input. A UI panel cannot hand over an order.
    public bool HandOverAtCounter()
    {
        if (!CanInteract || !_inventory.IsOrderReady || _player == null || !_player.IsAlive ||
            _counter == null || !_counter.IsBehindCounter) return false;
        _view.Close();
        IsServing = true;
        if (!_inventory.DeliverOrder())
        {
            IsServing = false;
            return false;
        }
        IsCompleted = true;
        IsActive = false;
        _orderSequence.DismissReceipt();
        Notify();
        return true;
    }

    public void FinishCounterVisit() => IsServing = false;

    public string ReceiptText()
    {
        string result = Text(IsCompleted ? "service_order_served" : "service_order_title") + "\n";
        for (int i = 0; i < 4; i++)
        {
            string mark = _inventory.Has((FastFoodOrderItem)i) || IsCompleted ? "+ " : "- ";
            result += mark + Text("service_item_" + i) + "\n";
        }
        return result + "\n" + Text("service_fridge_code") + "\n" + _fridgeCode;
    }

    public static string Text(string key)
    {
        return LocalizedDialogueSystem.Instance != null ? LocalizedDialogueSystem.Instance.GetDialogueText(key) : key;
    }

    private void Notify()
    {
        Changed?.Invoke();
        if (_orderSequence != null) _orderSequence.RefreshOrder();
    }

    private void OnDisable()
    {
        IsServing = false;
    }
}
