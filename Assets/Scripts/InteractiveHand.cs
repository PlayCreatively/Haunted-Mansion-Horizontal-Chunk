using System.Linq;
using UnityEngine;

public class InteractiveHand : Inventory
{
    public InteractableItem Item => items.FirstOrDefault();

    IInteractable focusedInteractable;

    new void Awake()
    {
        maxSize = 1;
        base.Awake();
    }

    public void Interact()
    {
        Debug.Log("Interact");

        if (focusedInteractable != null)
            switch (focusedInteractable)
            {
                case InteractableItem item:
                    if(AddItem(item))
                    {
                        focusedInteractable = null; // reset focused interactable
                    }
                    break;

                case InteractableObject obj:
                    obj.Interact(items.First());
                    break;
                default:
                    break;
            }
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
                focusedInteractable = interactable;
                interactable.Highlight(true);
            }
    }
}