using System;
using System.Collections;
using UnityEditor;
using UnityEngine;
using static UnityEditor.Progress;

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

    bool highlighted = false;
    bool destroyed = false;

    protected virtual void Awake()
    {
        col = GetComponentInChildren<Collider>();
        rb = GetComponent<Rigidbody>();
        meshRend = GetComponentInChildren<MeshRenderer>();
    }

    public void Highlight(bool value, InteractiveHand interactiveHand)
    {
        highlighted = value;
        meshRend.material.color = value ? Color.yellow : Color.white;
    }

    public void Destroy()
    {
        EnablePhysics(false);
        destroyed = true;

        StartCoroutine(DestroyRoutine());
    }

    IEnumerator DestroyRoutine()
    {
        Timer shrinkTimer = new(.5f);

        while(!shrinkTimer.Finished)
        {
            yield return null;
            transform.localScale = Vector3.Lerp(Vector3.one, Vector3.zero, shrinkTimer);
        }

        EnableVisibility(false);

        while (highlighted)
        {
            yield return null;
        }
        Destroy(gameObject);
    }

    public void Interact(InteractiveHand hand)
    {
        if (destroyed) return; // if the object is invisible, don't interact with it

        hand.PickUp(this);
    }

    public void EnableCollider(bool value) => col.enabled = value;

    public void EnableRigidbody(bool value)
    {
        rb.detectCollisions = value;
        rb.isKinematic = !value;
    }

    public void EnablePhysics(bool value)
    {
        EnableCollider(value);
        EnableRigidbody(value);
    }

    public void SetVelocity(Vector3 velocity)
    {
        rb.linearVelocity = velocity;
    }

    public void SetAngularVelocity(Vector3 velocity)
    {
        rb.angularVelocity = velocity;
    }

    internal void EnableVisibility(bool isItemsVisible)
    {
        meshRend.enabled = isItemsVisible;
    }
}
