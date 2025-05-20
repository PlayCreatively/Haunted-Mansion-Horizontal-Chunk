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
    public GameObject Visual { get; }
    public int DefaultLayer { get; }
    public bool Highlight(bool value, InteractiveHand interactiveHand)
    {
        bool successful = value && CanHighlight(value, interactiveHand);
        Visual.SetLayerRecursively(value && successful ? LayerMask.NameToLayer("Highlight") : DefaultLayer);
        return successful;
    }

    bool CanHighlight(bool value, InteractiveHand interactiveHand);
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
    Soap,
    Backpack,
    DirtyTowel,
    DirtyBedSheet,
    Goo,
}

[Flags]
public enum CarriableTypeMask
{
    ToiletPaper = 1,
    Towel = 2,
    BedSheet = 4,
    AllResources = ToiletPaper | Towel | BedSheet | DirtyTowel | DirtyBedsheet | Goo | Soap,
    Soap = 8,
    Backpack = 16,
    DirtyTowel = 32,
    DirtyBedsheet = 64,
    Goo = 128,
}

[RequireComponent(typeof(Rigidbody))]
public class Carriable : MonoBehaviour, IInteractable
{
    public CarriableType type;
    Collider col;
    Rigidbody rb;
    Renderer meshRend;
    public GameObject Visual { get; private set; }

    public int DefaultLayer { get; private set; }
    public bool pickedUp = false;
    public bool highlighted = false;
    public bool destroyed = false;

    protected virtual void Awake()
    {
        col = GetComponentInChildren<Collider>();
        rb = GetComponent<Rigidbody>();
        meshRend = GetComponentInChildren<Renderer>();
        var visualFound = transform.Find("Visual");
        if(visualFound != null)
            Visual = visualFound.gameObject;
        else
            Visual = gameObject;
        DefaultLayer = Visual.layer;
    }

    void FixedUpdate()
    {
        bool isMoving = rb.linearVelocity.sqrMagnitude > 20f;
        col.material.bounciness = isMoving ? 0.7f : 0f;
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

    public bool CanHighlight(bool value, InteractiveHand interactiveHand)
    {
        return value;
    }

    public void Destroy()
    {
        Debug.Log($"{type} destroyed", gameObject);
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
        const float minInteractionSpeed = 100f;

        if (!destroyed && !pickedUp && (rb.angularVelocity.sqrMagnitude > minInteractionSpeed) || rb.angularVelocity.y < float.Epsilon)
        {

            if (collision.rigidbody != null && collision.rigidbody.TryGetComponent(out IInteractable interactable))
                interactable.Interact(this);
            
            else if (collision.gameObject.TryGetComponent(out interactable))
                interactable.Interact(this);
        }
    }
}
