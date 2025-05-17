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
            //Debug.Log($"Backpack set to {__backpack}");
        }
    }

    GameObject backpackVisual;
    Vector3 defaultLocalPosition;

    public Carriable? ItemInHand => backpack.SelectedItem;
    //public bool HasBackpack => backpack != null;
    bool __displayBackpack = false;
    public void DisplayInventoryUI(float scale)
    {
        backpackUI.gameObject.SetActive(scale > .1f);
        backpackUI.transform.localScale = Vector3.one * scale;
    }

    public Inventory Inventory => backpack.Inventory;

    IInteractable? focusedInteractable;

    /// TODO: validate
    void Start()
    {
        defaultLocalPosition = transform.localPosition;
        backpackUI = InventoryUI.CreateUI(5, transform.parent);
        backpackUI.gameObject.SetActive(false);

        CreateNewBackpack();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) SetSelection(0);
        if (Input.GetKeyDown(KeyCode.Alpha2)) SetSelection(1);
        if (Input.GetKeyDown(KeyCode.Alpha3)) SetSelection(2);
        if (Input.GetKeyDown(KeyCode.Alpha4)) SetSelection(3);
        if (Input.GetKeyDown(KeyCode.Alpha5)) SetSelection(4);
        if (Input.GetKeyDown(KeyCode.Alpha6)) SetSelection(5);
        if (Input.GetKeyDown(KeyCode.B))
            DestroyBackpack();

        UpdatePositionOfHand();
    }

    public void UpdateSelection(int index)
    {
        if (index == backpack.Selected) return; // same selection, do nothing

        FMODAudioManager.Instance.TriggerItemSelectionInTheBagSfx();
        backpack.SetSelected(index);
        backpackUI.UpdateSelected(index);
    }

    public void Throw(float force)
    {
        var item = DropFromHand();

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
    public Carriable? DropFromHand()
    {
        if(ItemInHand != null)
        {
            var removedItem = backpack.RemoveAtSelected();

            if(Inventory.Count == 1)
            {
                // move last item to selection

                int itemFound = Inventory.FindAny();
                if(itemFound != -1)
                {
                    Debug.Log($"Swapping {ItemInHand} with {Inventory[itemFound]}");
                    SwapItems(itemFound, backpack.Selected);
                }

                return removedItem;
            }
            else return removedItem;
        }
        else return null;

    }

    void SwapItems(int from, int to)
    {
        var fromRef = backpack.Inventory[from];
        var toRef = backpack.Inventory[to];

        backpackUI.UpdateSlot(from, toRef != null ? toRef!.type : (CarriableType)(-1));
        backpackUI.UpdateSlot(to, fromRef != null ? fromRef!.type : (CarriableType)(-1));

        if(fromRef != null)
            fromRef!.EnableVisibility(to == backpack.Selected);
        if (toRef != null)
            toRef!.EnableVisibility(from == backpack.Selected);

        backpack.Inventory.Swap(from, to);
    }

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

    /// <summary>
    /// Moves hands backwards to avoid clipping with other colliders
    /// </summary>
    void UpdatePositionOfHand()
    {
        if(ItemInHand == null) return;

        Vector3 itemSize = ItemInHand.GetSize();

        const float rayBackDistance = .3f;
        float rayCastDistance = rayBackDistance + itemSize.z;
        Vector3 rayBackOffset = Vector3.forward * rayBackDistance;
        Debug.DrawRay(transform.parent.position + transform.TransformVector(defaultLocalPosition - rayBackOffset), transform.forward * rayCastDistance);
        if(Physics.Raycast(transform.parent.position + transform.TransformVector(defaultLocalPosition - rayBackOffset), transform.forward, out RaycastHit hit, rayCastDistance, ~LayerMask.GetMask("Player", "Enemy", "EnemyBlocker"), QueryTriggerInteraction.Ignore))
        {
            float zScale = (hit.distance - rayBackDistance) / itemSize.z;
            float zOffset = ((1f - zScale) * .5f * itemSize.z);
            transform.localScale = new (1, 1, (zScale * .5f) + .5f);
            transform.localPosition = defaultLocalPosition + Vector3.forward * (hit.distance - rayBackDistance - itemSize.z + zOffset);
        }
        else
        {
            transform.localPosition = defaultLocalPosition;
            transform.localScale = Vector3.one;
        } 
    }

    void CreateNewBackpack()
    {
        if(backpack != null)
            backpack.Inventory.OnInventoryUpdate = null;

        backpack = Instantiate(backpackPrefab, transform);
        
        // physical
        backpack.EnablePhysics(false);
        backpack.EnableCollider(false);
        backpack.EnableVisibility(false);
        backpack.transform.SetLocalPositionAndRotation(Vector3.back * .75f, Quaternion.identity);

        // UI
        backpackUI.gameObject.SetActive(false);
        backpack.Inventory.OnInventoryUpdate += backpackUI.UpdateSlot;
        backpackUI.Setup(backpack.Inventory, backpack.Selected);
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

        if (!(backpack.Count > 1))
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

        UpdateAllItemsVisibility();

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
        int newSelection = Mathf.Clamp(selection, 0, backpack.MaxSize);

        if (newSelection == backpack.Selected) return; // same selection, do nothing

        //Debug.Log($"SetSelection from {backpack.Selected} to {newSelection}");

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

        
        if (newSelection == backPackIndex && false /*disabled*/) // selected backpack
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

        UpdateSelection(newSelection);
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
                bool canHighlight = interactable.Highlight(true, this);

                if(canHighlight)
                {
                    if (focusedInteractable is MonoBehaviour mb && mb != null)
                        focusedInteractable.Highlight(false, this);
                    focusedInteractable = interactable;
                }
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

        public Carriable? Selected => hand.Inventory[selected];

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