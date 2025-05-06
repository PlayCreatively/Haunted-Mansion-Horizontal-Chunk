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

    public bool Interact(Carriable carriable)
    {
        bool correct = carriable.type == CarriableType.BedSheet && IsDirty();

        if (correct)
        {
            Destroy(carriable);
            MakeDirty(false);
        }

        return correct;
    }

    public void Highlight(bool value, InteractiveHand interactiveHand)
    {
        if (!IsDirty()) return;

        // TODO: highlight bedsheet?
    }
}