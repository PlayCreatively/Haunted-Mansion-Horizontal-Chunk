using System;
using UnityEngine;

//Items | Objects

///Resources:
///-pick-up / drop
///- store in sports bag
///- placed in/on storage stations (toilet paper storage in bathroom, washing machine)

///Trashbag:
///-pick-up / drop
///- placed in/ on storage stations? (trash bin/shoot)

///Sports bag:
///-pick - up / drop
///-
///

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

public enum ResourceType
{
    ToiletPaper,
    Towel,
    BedSheet
}

[Flags]
public enum ResourceTypeMask
{
    ToiletPaper = 1,
    Towel = 2,
    BedSheet = 4
}

[RequireComponent(typeof(Collider), typeof(Rigidbody))]
public class InteractableItem : MonoBehaviour, IInteractable
{
    public ResourceType type;
    Collider col;
    Rigidbody rb;
    MeshRenderer meshRend;

    private void Awake()
    {
        col = GetComponent<Collider>();
        rb = GetComponent<Rigidbody>();
        meshRend = GetComponent<MeshRenderer>();
    }

    public void Highlight(bool value, InteractiveHand interactiveHand)
    {
        meshRend.material.color = value ? Color.yellow : Color.white;
    }

    public void Interact(InteractiveHand hand)
    {
        hand.AddItem(this);
    }

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
