using System;
using UnityEngine;

public interface IInteractable : IHighlightable
{
    void Interact(InteractiveHand hand);
}

public interface IInteractableObject : IInteractable
{
}

public interface IHighlightable
{
    void Highlight(bool value, InteractiveHand interactiveHand);
}

public interface ICarriable : IInteractable
{
    public CarriableType Type { get; }
    public void SetParent(Transform parent);
    public void SetLocalPositionAndRotation(Vector3 position, Quaternion rotation);
    public void SetLocalScale(Vector3 scale);
    public void EnablePhysics(bool enable);
    public void EnableVisibility(bool enable);
    public void Carry(Transform parent, bool visible = false, Vector3 offset = new Vector3())
    {
        SetParent(parent);
        SetLocalPositionAndRotation(offset, Quaternion.identity);
        EnablePhysics(false);
        EnableVisibility(visible);
    }
    public void Drop()
    {
        SetParent(null);
        EnablePhysics(true);
        EnableVisibility(true);
    }
    //new void Interact(InteractiveHand hand)
    //{
    //    hand.AddItem(this);
    //}

    void SetVelocity(Vector3 newVelocity);
}

public enum CarriableType
{
    ToiletPaper,
    Towel,
    BedSheet,
    Backpack,
    Trash
}

[Flags]
public enum CarriableTypeMask
{
    ToiletPaper = 1,
    Towel = 2,
    BedSheet = 4,
    AllResources = ToiletPaper | Towel | BedSheet,
    Backpack = 8,
    Trash = 16
}

[RequireComponent(typeof(Rigidbody))]
public class Carriable : MonoBehaviour, IInteractable
{
    public CarriableType type;
    Collider col;
    Rigidbody rb;
    MeshRenderer meshRend;

    protected virtual void Awake()
    {
        col = GetComponentInChildren<Collider>();
        rb = GetComponent<Rigidbody>();
        meshRend = GetComponentInChildren<MeshRenderer>();
    }

    public void Highlight(bool value, InteractiveHand interactiveHand)
    {
        meshRend.material.color = value ? Color.yellow : Color.white;
    }

    public void Interact(InteractiveHand hand) => hand.PickUp(this);

    public void EnableCollider(bool value) => col.enabled = value;

    public void EnableRigidbody(bool value) => rb.isKinematic = !value;

    public void EnablePhysics(bool value)
    {
        EnableCollider(value);
        EnableRigidbody(value);
    }

    public void SetVelocity(Vector3 velocity)
    {
        rb.linearVelocity = velocity;
    }

    internal void EnableVisibility(bool isItemsVisible)
    {
        meshRend.enabled = isItemsVisible;
    }
}
