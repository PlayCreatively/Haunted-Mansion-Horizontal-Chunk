using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

#nullable enable

public interface IHasInventory
{
    public Inventory Inventory { get; }
}

public interface IInventory : IHasInventory
{
    public Vector3 ItemLocation => Inventory.ItemLocation;
    public int MaxSize => Inventory.MaxSize;
    public bool IsFull => Inventory.IsFull;
    public bool IsEmpty => Inventory.IsEmpty;
    public Carriable? this[int index] => Inventory[index];
    public bool InsertAt(int index, Carriable item) => Inventory.InsertAt(index, item);
    public int FindSpaceAndInsert(Carriable item) => Inventory.FindSpaceAndInsert(item);
    public int FindSpace() => Inventory.FindSpace();
    public int TransferItem(Carriable item, IInventory targetInventory) => Inventory.TransferItem(item, targetInventory);
    public bool TransferAll(IInventory targetInventory) => Inventory.TransferAll(targetInventory);
    public void RemoveItem(Carriable item) => Inventory.RemoveItem(item);
    public Carriable? RemoveAt(int index) => Inventory.RemoveAt(index);
}

public class Inventory : MonoBehaviour, IInventory
{
    [SerializeField]
    Vector3 itemLocation;
    public bool isItemsVisible = true; // Flag to show/hide items in the inventory
    public Vector3 ItemLocation => itemLocation;

    public CarriableTypeMask allowedTypes = (CarriableTypeMask)16-1; // Types of items that can be stored in the inventory

    [SerializeField]
    protected int maxSize = 1; // Maximum size of the inventory
    public int MaxSize => maxSize;
    public int Count => count;
    int count = 0;
    Inventory IHasInventory.Inventory => this;

    public Action<int, CarriableType>? OnInventoryUpdate;

    // Inventory class to manage items
    [SerializeField]
    protected ManagedArray<Carriable> items;

    protected virtual void Awake()
    {
        items = new (maxSize);
    }

    public Carriable? this[int index] => items[index];

    public virtual bool InsertAt(int index, Carriable item)
    {
        Assert.IsNotNull(item, "Item cannot be null");
        Assert.IsTrue(index >= 0 && index < items.size, "Index out of bounds");
        Assert.IsFalse(items.Contains(item), item + " is already in inventory");

        bool success = items.Insert(index, item);

        if (!success) // Space is already occupied
        {
            Debug.Log("InsertAt() failed: No space");
            return false;
        }
        else if(item.pickedUp)
        {
            Debug.LogWarning("InsertAt() failed: Item already held");
            return false; // Item is already picked up
        }

        item.pickedUp = true;

        count++;

        item.transform.SetParent(transform, false);
        item.transform.SetLocalPositionAndRotation(ItemLocation, Quaternion.identity);
        item.EnablePhysics(false);
        item.EnableVisibility(isItemsVisible);
        items[index] = item;
        OnInventoryUpdate?.Invoke(index, item.type);

        FMODAudioManager.Instance.TriggerItemPickedUpSfx();

        return true;
    }

    public virtual int FindSpaceAndInsert(Carriable item)
    {
        Assert.IsNotNull(item, "Item cannot be null");
        Assert.IsFalse(items.Contains(item), item + " is already in inventory");

        int freeSpace = items.FindSpace();
        if (freeSpace == -1)
        {
            Debug.Log("Inventory is full");
            return freeSpace;
        }

        InsertAt(freeSpace, item);

        return freeSpace;
    }

    public int FindSpace() => items.FindSpace();
    public int FindAny() => items.FindAny();
    public void Swap(int from, int to) => items.Swap(from, to);

    public int TransferItem(Carriable item, IInventory targetInventory)
    {
        Assert.IsNotNull(item, "Item cannot be null");

        int freeSpace = targetInventory.FindSpace();
        if (freeSpace == -1)
        {
            Debug.Log("Target inventory is full");
            return freeSpace;
        }

        int itemFound = items.Find(item);
        Assert.IsTrue(itemFound != -1, item.type + " is not in inventory");

        RemoveAt(itemFound);

        targetInventory.InsertAt(freeSpace, item);
        return freeSpace;
    }

    public bool TransferAll(IInventory targetInventory)
    {
        for (int i = 0; i < items.size; i++)
            if (items[i] != null)
                if (TransferItem(items[i]!, targetInventory) == -1)
                    return false;

        return true;
    }

    public bool IsFull => items.IsFull;

    public bool IsEmpty => items.IsEmpty;

    public void RemoveItem(Carriable item)
    {
        int itemFound = items.Find(item);
        if (itemFound == -1) return;
        RemoveAt(itemFound);
    }

    public virtual Carriable? RemoveAt(int index)
    {
        Assert.IsTrue(index >= 0 && index < items.size, "Index out of bounds");

        Carriable? item = items.RemoveAt(index);

        if(item == null)
            return null;

        item.pickedUp = false;

        count--;
        item.EnablePhysics(true);
        item.EnableVisibility(true);
        item.transform.SetParent(null, true);
        item.transform.localScale = Vector3.one;

        OnInventoryUpdate?.Invoke(index, (CarriableType)(-1));
        FMODAudioManager.Instance.TriggerItemDroppedSfx();

        return item;
    }
}

public class ManagedArray<T> where T : class
{
    public readonly int size;
    readonly T?[] array;

    public bool IsFull => FindSpace() == -1;
    public bool IsEmpty
    {
        get
        {
            for (int i = 0; i < size; i++)
                if (array[i] != null)
                    return false;
            return true;
        }
    }

    public ManagedArray(int size)
    {
        this.size = size;
        array = new T[size];
    }

    public T? this[int index]
    {
        get => array[index]; 
        set => array[index] = value;
    }

    public T? RemoveAt(int index)
    {
        T? item = array[index];
        array[index] = null;
        return item;
    }

    //public bool Remove(T item)
    //{
    //    for (int i = 0; i < size; i++)
    //        if (array[i] != null && array[i].Equals(item))
    //        {
    //            array[i] = default;
    //            return true; // Item removed successfully
    //        }

    //    return false; // Item not found
    //}

    public int FindAny()
    {
        for (int i = 0; i < size; i++)
            if (array[i] != null)
                return i;
        return -1; // No item found
    }

    public void Swap(int from, int to)
    {
        (array[to], array[from]) = (array[from], array[to]);
    }

    public int Find(T item)
    {
        for (int i = 0; i < size; i++)
            if (EqualityComparer<T>.Default.Equals(array[i], item))
                return i;
        return -1; // Item not found
    }

    public int FindSpace()
    {
        for (int i = 0; i < size; i++)
            if (array[i] == null)
                return i;
        
        return -1; // No space found
    }

    public int FindIndex(T item)
    {
        for (int i = 0; i < size; i++)
            if (EqualityComparer<T>.Default.Equals(array[i], item))
                return i;
        return -1; // Item not found
    }

    int? IsSpaceInDirection(int index)
    {
        int? dir = null;

        for (int i = 0; i < size; i++)
            if (array[i] == null)
            {
                int newDir = i - index;
                if(dir == null)
                {
                    //Debug.Log($"New direction: {newDir} < null");
                    dir = newDir;
                }
                else if (Math.Abs(newDir) < Math.Abs(dir.Value))
                {
                    //Debug.Log($"New direction: {newDir} < {dir}");
                    dir = newDir;
                }
            }

        return dir;
    }

    public bool Insert(int index, T item)
    {
        Assert.IsTrue(index >= 0 && index < size, "Index out of bounds");

        var spaceFromDistance = IsSpaceInDirection(index);
        if(spaceFromDistance == null) return false;
        else if (spaceFromDistance == 0)
        {
            array[index] = item;
            return true;
        }
        else
        {
            int dir = Math.Sign(spaceFromDistance.Value);
            int distance = spaceFromDistance.Value;

            while (distance != 0)
            {
                int oldIndex = index + distance;
                int newIndex = oldIndex - dir;
                array[oldIndex] = array[newIndex];
                Debug.Log($"Moving item from {oldIndex} to {newIndex}");
                distance -= dir;
            }

            array[index] = item;
            return true;
        }


    }

    public int FindSpaceAndInsert(T item)
    {
        int index = FindSpace();
        if (index != -1)
            array[index] = item;
        return index;
    }

    internal bool Contains(Carriable item)
    {
        for (int i = 0; i < size; i++)
            if (array[i] != null && array[i]!.Equals(item))
                return true;
        return false;
    }
}

#nullable disable