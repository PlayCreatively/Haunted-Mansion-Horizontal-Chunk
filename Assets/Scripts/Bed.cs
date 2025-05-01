using UnityEngine;

public class Bed : MonoBehaviour, IInteractable
{
    [SerializeField]
    Transform[] bedsheets;

    void Start()
    {
        //MakeDirty(true);
    }

    // assuming there's no bedsheet when its dirty
    bool IsDirty()
    {
        foreach (var bedsheet in bedsheets)
        {
            if (bedsheet.gameObject.activeSelf) return false;
        }
        return true;
    }

    public void MakeDirty(bool dirty)
    {
        if(!dirty)
            FMODAudioManager.Instance.TriggerItemDroppedSfx();

        foreach (var bedsheet in bedsheets)
        {
            bedsheet.gameObject.SetActive(!dirty);
        }
    }

    public void Interact(InteractiveHand hand)
    {
        var itemInHand = hand.ItemInHand;
        if (itemInHand != null && hand.ItemInHand.type == CarriableType.BedSheet && IsDirty())
        {
            hand.DropFromHand();
            Destroy(itemInHand.gameObject);
            MakeDirty(false);
        }
    }

    public void Highlight(bool value, InteractiveHand interactiveHand)
    {
        if (!IsDirty()) return;

        // TODO: highlight bedsheet?
    }
}