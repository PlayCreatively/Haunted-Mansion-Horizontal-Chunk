using System;
using UnityEngine;
using UnityEngine.Assertions;

#nullable enable

public class InteractiveHand : MonoBehaviour, IInventory
{
    [SerializeField]
    Backpack backpackPrefab;

    InventoryUI backpackUI;
    Backpack __backpack;
    Backpack backpack { 
        get => __backpack; 
        set
        {
            __backpack = value;
            Debug.Log($"Backpack set to {__backpack}");
        }
    }

    GameObject backpackVisual;

    public Carriable? ItemInHand => backpack.SelectedItem;
    //public bool HasBackpack => backpack != null;
    public bool DisplayBackpack => backpack.Count > 1;

    public Inventory Inventory => backpack.Inventory;

    IInteractable? focusedInteractable;

    /// TODO: validate
    void Awake()
    {
        backpackUI = InventoryUI.CreateUI(4, transform.parent);
        backpackUI.gameObject.SetActive(false);

        CreateNewBackpack();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) SetSelection(0);
        if (Input.GetKeyDown(KeyCode.Alpha2)) SetSelection(1);
        if (Input.GetKeyDown(KeyCode.Alpha3)) SetSelection(2);
        if (Input.GetKeyDown(KeyCode.Alpha4)) SetSelection(3);
        if (Input.GetKeyDown(KeyCode.Alpha5)) 
            SetSelection(4);
        if (Input.GetKeyDown(KeyCode.Alpha6)) SetSelection(5);
        if (Input.GetKeyDown(KeyCode.B))
            DestroyBackpack();
    }

    public void Throw(float force)
    {
        var item = backpack.RemoveAtSelected();

        if (item != null)
        {
            var newVelocity = (Quaternion.AngleAxis(-GameSettings.Instance.playerThrowAngle, transform.right) * transform.forward) * force;
            Debug.DrawLine(transform.position, transform.position + newVelocity, Color.red, 1f);
            item.SetVelocity(newVelocity);
        }
    }

    public void Interact()
    {
        Debug.Log($"Interact with {(focusedInteractable != null ? focusedInteractable : "nothing")} using {(backpack.Inventory[0] != null ? backpack.Inventory[0] : "nothing")}");
        focusedInteractable?.Interact(this);
    }

    public bool InsertToHand(Carriable carriable) => InsertAt(backpack.Selected, carriable);
    public Carriable? DropFromHand() => backpack.RemoveAtSelected();

    /// TODO: validate
    bool IsCloserThanFocused(IInteractable other)
    {
        if(focusedInteractable is MonoBehaviour focused && focused != null) // not null
        {
            var otherMono = other as MonoBehaviour;

            float distanceToOther = Vector3.Distance(otherMono!.transform.position, transform.position);
            float distanceToFocused = Vector3.Distance(focused.transform.position, transform.position);
            return distanceToOther < distanceToFocused;
        }
        else 
            return true;
    }

    void CreateNewBackpack()
    {
        if(backpack != null)
            backpack.Inventory.OnInventoryUpdate = null;

        Debug.Log("creating new backpack");

        backpack = Instantiate(backpackPrefab, transform);
        
        // physical
        backpack.EnablePhysics(false);
        backpack.EnableCollider(false);
        backpack.EnableVisibility(false);
        backpack.transform.SetLocalPositionAndRotation(Vector3.back * .75f, Quaternion.identity);

        // UI
        backpackUI.gameObject.SetActive(false);
        backpack.Inventory.OnInventoryUpdate += backpackUI.UpdateSlot;
        backpack.Inventory.OnInventoryUpdate += (_,__) => UpdateBackpackVisibility();
        backpackUI.Setup(backpack.Inventory, backpack.Selected);
    }
    void UpdateBackpackVisibility()
    {
        backpackUI.gameObject.SetActive(DisplayBackpack);
        backpack.EnableVisibility(DisplayBackpack);
    }

    public bool InsertAt(int index, Carriable carriable)
    {
        Debug.Log($"Picked up {carriable.type}");

        if (carriable is Backpack carriableBackpack) 
            return InsertAt(index, carriableBackpack);

        else if (CarriableTypeMask.AllResources.IsInMask(carriable.type))
            return InsertResourceAt(index, carriable);

        else // if item is not a backpack or item
        {
            Debug.Log($"Item {carriable.type} not allowed in backpack");
            return false;
        }
    }

    /// TODO: validate
    bool InsertAt(int index, Backpack carriableBackpack)
    {
        Assert.IsTrue(carriableBackpack != backpack, "Cannot insert backpack into itself");

        if (!DisplayBackpack)
            return MergeBackpack(carriableBackpack);

        else // we already have a backpack
        {
            throw new System.NotImplementedException("Backpack to backpack transfer not implemented yet");
        }
    }

    bool MergeBackpack(Backpack carriableBackpack)
    {
        Debug.Log($"Merging backpack {carriableBackpack} into backpack {backpack}");
        Assert.IsTrue(carriableBackpack != backpack, "Cannot merge backpack into itself");

        bool cleanMerge = carriableBackpack.TransferAll(backpack);

        if(cleanMerge)
            Destroy(carriableBackpack.gameObject);

        return cleanMerge;
    }

    bool InsertResourceAt(int index, Carriable resource)
    {
        bool foundSpace = backpack.InsertAt(index, resource); // insert item to backpack

        if (foundSpace)
        {
            /// TODO: validate
            backpackUI.Setup(backpack.Inventory, index); // update backpack UI
        }

        return foundSpace;
    }

    public void PickUp(Carriable item)
    {
        if (item == null) return;
        bool success = InsertAt(backpack.Selected, item); // insert item to backpack

        if (!success) // not enough space
        {
            DropFromHand();
            InsertAt(backpack.Selected, item);
        }
        else
        {
            UpdateAllItemsVisibility();
        }

        // if item picked up is highlighted
        if (item == focusedInteractable as Carriable)
        {
            focusedInteractable.Highlight(false, this);
            focusedInteractable = null;
        }
    }

    void UpdateAllItemsVisibility()
    {
        for (int i = 0; i < backpack.MaxSize; i++)
            if (backpack.Inventory[i] != null)
                backpack.Inventory[i]!.EnableVisibility(backpack.Selected == i);
    }

    public void IncrementSelection(int increment) => SetSelection(backpack.Selected + increment);

    /// TODO: validate
    void SetSelection(int selection)
    {
        if(!DisplayBackpack) return;

        int newSelection = Mathf.Clamp(selection, 0, backpack.MaxSize);

        if (newSelection == backpack.Selected) return; // same selection, do nothing

        Debug.Log($"SetSelection from {backpack.Selected} to {newSelection}");

        int backPackIndex = backpack.MaxSize;

        if (ItemInHand != null) // appropriately handle item in hand
        {
            if (CarriableTypeMask.AllResources.IsInMask(ItemInHand.type))
            {
                ItemInHand.EnableVisibility(false);
            }
            else
            {
                Debug.Log("Cannot store non-resources in backpack");
                DropFromHand();
            }

        }

        
        if (newSelection == backPackIndex) // selected backpack
        {
            // this removes the backpack and creates a new empty backpack and places the old one in the new backpack (at hand index)
            var oldBackpack = backpack;
            CreateNewBackpack();

            InsertToHand(oldBackpack);
            return;
        }
        else if(backpack.Inventory[selection] != null)
        {
            backpack.Inventory[selection]!.EnableVisibility(true);
        }

        backpack.SetSelected(newSelection);
        backpackUI.UpdateSelected(newSelection);
    }

    /// TODO: validate
    void DestroyBackpack()
    {
        Assert.IsTrue(backpack.IsEmpty, "Backpack destroyed when not empty");

        Debug.Log("Destroying backpack");

        Destroy(backpack);
        backpack = null;

        backpackUI.gameObject.SetActive(false);
    }

    void OnTriggerStay(Collider other)
    {
        if (other.attachedRigidbody != null ? other.attachedRigidbody.TryGetComponent(out IInteractable interactable) : other.TryGetComponent(out interactable))
            if (IsCloserThanFocused(interactable))
            {
                focusedInteractable?.Highlight(false, this);
                focusedInteractable = interactable;
                focusedInteractable.Highlight(true, this);
            }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.attachedRigidbody != null ? other.attachedRigidbody.TryGetComponent(out IInteractable interactable) : other.TryGetComponent(out interactable))
        {
            if (focusedInteractable == interactable)
            {
                interactable.Highlight(false, this);
                focusedInteractable = null;
            }
        }
    }

    class SelectiveHand
    {
        readonly InteractiveHand hand;
        readonly int count;
        public int selected = 0;

        public Carriable Selected => hand.Inventory[selected];

        const float selectionEndDegree = 180f;
        const float selectionStartDegree = 0f;
        readonly float selectionWidth;

        public SelectiveHand(InteractiveHand hand)
        {
            this.hand = hand;
            count = hand.Inventory.MaxSize + 1;
            selectionWidth = (selectionEndDegree - selectionStartDegree) / count;
        }

        public void SetSelected(int index)
        {
            if (index < 0 || index >= count) return;
            selected = index;
        }

        public void SetSelected(Vector2 dir)
        {
            Vector2 selectionStartAngle = new(Mathf.Cos(selectionStartDegree), Mathf.Sin(selectionStartDegree));
            float angle = Vector2.SignedAngle(selectionStartAngle, dir);
            angle = Mathf.Clamp(angle, 0, 180f);
            int index = (int)((angle + selectionWidth * .5f) / selectionWidth);
            SetSelected(index);
        }
    }
}

#nullable disable