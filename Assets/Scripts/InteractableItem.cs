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
    void Interact(object param = null);
}



public interface IHighlightable
{
    void Highlight(bool value);
}

public enum ResourceType
{
    ToiletPaper,
    Towel,
    BedSheet
}

[RequireComponent(typeof(Collider), typeof(Rigidbody))]
public class InteractableItem : MonoBehaviour, IInteractable
{
    public ResourceType type;
    Collider col;
    Rigidbody rb;

    private void Awake()
    {
        col = GetComponent<Collider>();
        rb = GetComponent<Rigidbody>();
    }

    public void Highlight(bool value)
    {
        //Debug.Log($"Highlight {gameObject.name} to {value}");
    }

    public void Interact(object param = null)
    {
        throw new System.NotImplementedException($"Interact run in item: {gameObject.name}.\nItems are not meant to be interacted with.");
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
}
