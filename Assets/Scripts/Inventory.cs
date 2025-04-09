using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class Inventory : MonoBehaviour
{
    public Vector3 ItemLocation => transform.position;

    [SerializeField]
    protected int maxSize = 1; // Maximum size of the inventory

    // Inventory class to manage items
    [SerializeField]
    protected List<InteractableItem> items;

    protected void Awake()
    {
        items = new List<InteractableItem>(maxSize);
    }

    public bool AddItem(InteractableItem item)
    {
        Assert.IsNotNull(item, "Item cannot be null");
        Assert.IsFalse(items.Contains(item), item + " is already in inventory");

        if (IsFull())
        {
            Debug.Log("Inventory is full");
            return false;
        }

        item.transform.SetParent(transform, false);
        item.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
        item.EnablePhysics(false);

        items.Add(item);
        return true;
    }

    public bool TransferItem(InteractableItem item, Inventory targetInventory)
    {
        Assert.IsNotNull(item, "Item cannot be null");
        Assert.IsFalse(items.Contains(item), item + " is not in inventory");

        if (targetInventory.IsFull())
        {
            Debug.Log("Target inventory is full");
            return false;
        }

        items.Remove(item);
        targetInventory.AddItem(item);
        return true;
    }

    public bool IsFull() => items.Count >= maxSize;

    public void Clear() => items.Clear();

    public bool IsEmpty() => items.Count == 0;

    public void RemoveItem(InteractableItem item)
    {
        items.Remove(item);
        item.EnablePhysics(true);
        item.transform.SetParent(null, true);
    }
}
