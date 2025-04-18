using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

public class InteractiveHand : Inventory
{
    public InteractableItem Item => items.FirstOrDefault();

    IInteractable focusedInteractable;

    new void Awake()
    {
        maxSize = 1;
        base.Awake();
    }

    public void Throw()
    {
        if (Item != null)
        {
            var item = Item;
            RemoveItem(item);
            var newVelocity = (Quaternion.AngleAxis(-GameSettings.Instance.playerThrowAngle, transform.right) * transform.forward) * GameSettings.Instance.playerThrowForce;
            Debug.DrawLine(transform.position, transform.position + newVelocity, Color.red, 1f);
            item.SetVelocity(newVelocity);
        }
    }

    public void Interact()
    {
        Debug.Log($"Interact with {focusedInteractable} using {items.FirstOrDefault()}");
        focusedInteractable?.Interact(this);
    }

    public override bool AddItem(InteractableItem item)
    {
        if (item == focusedInteractable as InteractableItem)
        {
            focusedInteractable.Highlight(false, this);
            focusedInteractable = null;
        }

        if (Item != null) RemoveItem(Item);

        return base.AddItem(item);
    }

    bool IsCloserThanFocused(IInteractable other)
    {
        if(focusedInteractable is MonoBehaviour focused) // not null
        {
            var otherMono = other as MonoBehaviour;

            float distanceToOther = Vector3.Distance(otherMono.transform.position, transform.position);
            float distanceToFocused = Vector3.Distance(focused.transform.position, transform.position);
            return distanceToOther < distanceToFocused;
        }
        else 
            return true;
    }

    void OnTriggerStay(Collider other)
    {
        if (other.TryGetComponent(out IInteractable interactable))
            if (IsCloserThanFocused(interactable))
            {
                focusedInteractable?.Highlight(false, this);
                focusedInteractable = interactable;
                focusedInteractable.Highlight(true, this);
            }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent(out IInteractable interactable))
        {
            if (focusedInteractable == interactable)
            {
                interactable.Highlight(false, this);
                focusedInteractable = null;
            }
        }
    }
}