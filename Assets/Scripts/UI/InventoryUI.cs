using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour
{
    [SerializeField]
    Sprite[] iconSprites;

    Image[] icons;
    Transform target;

    public static InventoryUI CreateUI(int count, Transform target)
    {
        InventoryUI inventoryUI = Instantiate(Resources.Load<InventoryUI>("InventoryUI"));
        inventoryUI.transform.SetParent(GameObject.Find("Canvas").transform);

        inventoryUI.icons = new Image[count];
        inventoryUI.target = target;

        float rotSteps = 180f / (count - 1);

        GameObject slotUI = inventoryUI.transform.GetChild(0).gameObject;
        inventoryUI.icons[0] = slotUI.transform.GetChild(0).GetComponent<Image>();
        inventoryUI.icons[0].gameObject.SetActive(false);
        for (int i = 1; i < count; i++)
        {
            GameObject newSlotUI = Instantiate(slotUI, inventoryUI.transform, true);
            inventoryUI.icons[i] = newSlotUI.transform.GetChild(0).GetComponent<Image>();
            inventoryUI.icons[i].gameObject.SetActive(false);
            newSlotUI.name = $"ItemSlot_{i}";
            newSlotUI.SetActive(true);
            float distance = slotUI.transform.localPosition.magnitude;
            newSlotUI.transform.localPosition = Quaternion.Euler(0, 0, -rotSteps * i) * Vector2.up * distance;
        }

        return inventoryUI;
    }

    public void UpdateSlot(int index, CarriableType type)
    {
        Assert.IsTrue(iconSprites.Length > (int)type, $"Icon sprite array is not long enough for type {type}.");
        Assert.IsFalse(index < 0 || index >= icons.Length, "Index is out of bounds for the inventory slots.");

        bool isItemLeaving = (int)type == -1;
        icons[index].gameObject.SetActive(!isItemLeaving);

        if (!isItemLeaving)
        {
            icons[index].sprite = iconSprites[(int)type];
        }
    }

    public void UpdateSelected(int index)
    {
        Assert.IsFalse(index < 0 || index >= icons.Length, "Index is out of bounds for the inventory slots.");

        for (int i = 0; i < icons.Length; i++)
        {
            icons[i].transform.parent.GetComponent<Image>().color = (index == i) ? Color.white : Color.gray;
            //icons[i].color = (index == i) ? Color.white : Color.gray;
        }
    }

    public void Setup(Inventory newInventory, int selected)
    {
        Assert.IsNotNull(newInventory, "Inventory cannot be null");
        Assert.IsTrue(newInventory.MaxSize == icons.Length - 1, "Inventory size does not match UI slots");

        for (int i = 0; i < newInventory.MaxSize; i++)
        {
            if (newInventory[i] != null)
            {
                UpdateSlot(i, newInventory[i].type);
            }
            else
            {
                UpdateSlot(i, (CarriableType)(-1));
            }
        }

        //TODO: set last slot as bag
        int bagSlot = icons.Length - 1;
        UpdateSlot(bagSlot, CarriableType.Backpack);

        UpdateSelected(selected);
    }

    void Update()
    {
        (transform as RectTransform).position = GetTargetPosInScreenSpace();
    }

    public Vector2 GetTargetPosInScreenSpace()
    {
        Vector3 screenPos = Camera.main.WorldToScreenPoint(target.position);
        return new(screenPos.x, screenPos.y);
    }
}
