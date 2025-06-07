using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.InputSystem;
using UnityEngine.UI;

[DefaultExecutionOrder(ExecutionOrder.UI)]
public class InventoryUI : MonoBehaviour
{
    [SerializeField]
    Sprite[] iconSprites;

    Image[] icons;
    Transform target;

    void Awake()
    {
        transform.localScale = Vector3.zero;
    }

    public static bool CreateUI(int count, Transform target, out InventoryUI inventoryUI)
    {
        var inventoryCanvas = GameObject.FindAnyObjectByType<PlayerInputManager>().GetComponentInChildren<Canvas>();

        Assert.IsNotNull(inventoryCanvas, "Canvas not found in the scene.");
        inventoryUI = Instantiate(Resources.Load<InventoryUI>("InventoryUI"), inventoryCanvas.transform);
        inventoryUI.icons = new Image[count];
        inventoryUI.target = target;

        float rotSteps = 180f / (count - 1);

        GameObject slotUI = inventoryUI.transform.GetChild(0).gameObject;
        for (int i = 0; i < count; i++)
        {
            GameObject newSlotUI = Instantiate(slotUI, inventoryUI.transform, true);
            inventoryUI.icons[i] = newSlotUI.transform.GetChild(0).GetComponent<Image>();
            inventoryUI.icons[i].gameObject.SetActive(false);
            newSlotUI.name = $"ItemSlot_{i}";
            newSlotUI.SetActive(true);
            float distance = slotUI.transform.localPosition.magnitude;
            newSlotUI.transform.localPosition = Quaternion.Euler(0, 0, -rotSteps * i) * Vector2.up * distance;
        }

        return true;
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
            float brightness = (i == index) ? 1f : 0.4f;
            float scale = (i == index) ? 1.2f : 1f;
            var color = new Color(brightness, brightness, brightness, 1f);
            icons[i].transform.parent.localScale = new Vector3(scale, scale, scale);
            icons[i].transform.parent.GetComponent<Image>().color = color;
            icons[i].color = color;
        }
    }

    public void Setup(Inventory newInventory, int selected)
    {
        Assert.IsNotNull(newInventory, "Inventory cannot be null");
        Assert.IsTrue(newInventory.MaxSize == icons.Length, "Inventory size does not match UI slots");

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

        ////TODO: set last slot as bag
        //int bagSlot = icons.Length - 1;
        //UpdateSlot(bagSlot, CarriableType.Backpack);

        UpdateSelected(selected);
    }

    void Update()
    {
        (transform as RectTransform).position = GetTargetPosInScreenSpace();
    }

    public Vector2 GetTargetPosInScreenSpace()
    {
        Vector3 offset = new(0, .5f, 0);
        Vector3 screenPos = Camera.main.WorldToScreenPoint(target.position + offset);
        return new(screenPos.x, screenPos.y);
    }

    public void Activate(bool value, float scale)
    {
        gameObject.SetActive(value);
        transform.localScale = Vector3.one * scale;

        //StartCoroutine(transform.ScaleObject(0.075f, value ? 1f : 0f));
    }
}
