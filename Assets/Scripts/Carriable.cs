using System;
using System.Collections;
using UnityEngine;

#nullable enable
public interface IInteractable : IHighlightable
{
    bool Interact(InteractiveHand hand)
    {
        bool successful = hand.ItemInHand != null && Interact(hand.ItemInHand);
        if (successful) hand.DropFromHand();
        return successful;
    }
    bool Interact(Carriable carriable);
}
#nullable disable

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
    DirtyTowel,
    DirtyBedSheet,
}

[Flags]
public enum CarriableTypeMask
{
    ToiletPaper = 1,
    Towel = 2,
    BedSheet = 4,
    AllResources = ToiletPaper | Towel | BedSheet | DirtyTowel | DirtyBedsheet,
    Backpack = 8,
    DirtyTowel = 16,
    DirtyBedsheet = 32,
}

[RequireComponent(typeof(Rigidbody))]
public class Carriable : MonoBehaviour, IInteractable
{
    public CarriableType type;
    Collider col;
    Rigidbody rb;
    MeshRenderer meshRend;

    public bool pickedUp = false;
    public bool highlighted = false;
    bool destroyed = false;

    protected virtual void Awake()
    {
        col = GetComponentInChildren<Collider>();
        rb = GetComponent<Rigidbody>();
        meshRend = GetComponentInChildren<MeshRenderer>();
    }

    public Vector3 GetSize()
    {
        return col switch
        {
            BoxCollider box => box.size,
            SphereCollider sphere => new Vector3(sphere.radius, sphere.radius, sphere.radius) * 2f,
            CapsuleCollider capsule => new Vector3(capsule.radius * 2f, capsule.height, capsule.radius * 2f),
            _ => Vector3.one
        };
    }

    public void Highlight(bool value, InteractiveHand interactiveHand)
    {
        highlighted = value;
        if(meshRend != null) // TODO: find out why this is needed, ugly fix
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

    public bool Interact(InteractiveHand hand)
    {
        if (destroyed) return false; // if the object is invisible, don't interact with it

        if(!pickedUp)
        {
            hand.PickUp(this);
            pickedUp = true;
        }

        return pickedUp;
    }

    public bool Interact(Carriable carriable) { return false; }

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

    public void EnableVisibility(bool isItemsVisible)
    {
        meshRend.enabled = isItemsVisible;
    }

    void OnCollisionEnter(Collision collision)
    {
        Debug.Log($"{gameObject.name} colliding with {collision.gameObject.name}");
        const float minInteractionSpeed = .2f;

        if (!pickedUp && rb.angularVelocity.sqrMagnitude > minInteractionSpeed)
        {
            if (collision.rigidbody != null && collision.rigidbody.TryGetComponent(out IInteractable interactable))
            {
                interactable.Interact(this);
                Debug.Log($"Interacting with {collision.gameObject.name} using {gameObject.name}");
            }
            
            else if (collision.gameObject.TryGetComponent(out interactable))
            {
                interactable.Interact(this);
                Debug.Log($"Interacting with {collision.gameObject.name} using {gameObject.name}");

            }
        }
    }
}
