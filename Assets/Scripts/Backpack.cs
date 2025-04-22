using System;
using UnityEngine;
using UnityEngine.Assertions;

# nullable enable

[RequireComponent(typeof(Inventory))]
public class Backpack : Carriable, IInventory
{
    Inventory inventory;
    public Inventory Inventory => inventory;
    int selected;
    public int Selected => selected;
    public Carriable? SelectedItem => inventory[selected];

    public int SetSelected(int index)
    {
        Assert.IsTrue(index >= 0 && index < inventory.MaxSize, "Index out of bounds");

        if(inventory[selected] != null)
            inventory[selected]!.EnableVisibility(false);
        if(inventory[index] != null)
            inventory[index]!.EnableVisibility(true);

        index = Math.Clamp(index, 0, inventory.MaxSize - 1);
        return selected = index;
    }
    public int MaxSize => inventory.MaxSize;

    protected override void Awake()
    {
        base.Awake();
        inventory = GetComponent<Inventory>();
        inventory.isItemsVisible = false;
    }

    public int FindSpace()
    {
        if(inventory[selected] == null) return selected;

        return inventory.FindSpace();
    }
    public int FindSpaceAndInsert(Carriable item) => inventory.FindSpaceAndInsert(item);
    public bool InsertAt(int index, Carriable item) => inventory.InsertAt(index, item);
    public int TransferItem(Carriable item, IInventory targetInventory) => inventory.TransferItem(item, targetInventory);

    public bool TransferAll(IInventory from) => inventory.TransferAll(from);

    public Carriable? RemoveAtSelected() => inventory.RemoveAt(selected);

    public bool IsFull => inventory.IsFull;
    public bool IsEmpty => inventory.IsEmpty;

    public int Count => inventory.Count;
}

#nullable disable