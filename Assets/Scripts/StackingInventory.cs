using UnityEngine;
using UnityEngine.Assertions;

public class StackingInventory : Inventory, IInteractable
{
    public Vector3Int itemGrid = new(1, 1, 1); // Size of the grid for item placement
    public Vector3 padding; // Padding between items in the grid
    [SerializeField]
    Material grayedOutMaterial; // Material to use when items are grayed out
    [SerializeField]
    Material highlight; // Material to use when items are grayed out
    MeshRenderer[] itemMeshRends;
    Material defaultMaterial; // Default material for items
    int count = 0;

    protected override void Awake()
    {
        Assert.IsTrue(itemGrid.x > 0 && itemGrid.y > 0 && itemGrid.z > 0, "Item grid size must be greater than zero");
        Assert.IsTrue(maxSize <= itemGrid.x * itemGrid.y * itemGrid.z, "Max size must be less than or equal to item grid size");

        base.Awake();

        itemMeshRends = new MeshRenderer[maxSize];

        PlaceItems();
    }

    void PlaceItems()
    {
        var itemInfo = GameSettings.Instance.GetResourceInfo(allowedTypes);
        GameObject pf = itemInfo.visualPrefab;
        defaultMaterial = pf.GetComponent<MeshRenderer>().sharedMaterial;
        Vector3 meshSize = itemInfo.mesh.bounds.size;

        Vector3 cellSize = meshSize + padding;

        Vector3 stackSize = Vector3.Scale(itemGrid, cellSize);

        Vector3 offset = (stackSize - cellSize - ((meshSize.y + padding.y) * Vector3.up)) * 0.5f;

        int i = 0;
        for (int x = 0; x < itemGrid.x; x++)
            for (int z = 0; z < itemGrid.z; z++)
                for (int y = 0; y < itemGrid.y; y++)
                {

                    Vector3 pos = Vector3.Scale(new Vector3(x, y, z), cellSize) - offset;

                    var item = Instantiate(pf, transform, false);
                    item.transform.SetLocalPositionAndRotation(pos, Quaternion.identity);

                    var rend = item.GetComponent<MeshRenderer>();
                    itemMeshRends[i] = rend;
                    rend.sharedMaterial = grayedOutMaterial;

                    if (++i > maxSize) break;
                }
    }


    void OnDrawGizmosSelected()
    {
        var itemInfo = GameSettings.Instance.GetResourceInfo(allowedTypes);
        Vector3 meshSize = itemInfo.mesh.bounds.size;
        Vector3 cellSize = meshSize + padding;
        Vector3 stackSize = Vector3.Scale(itemGrid, cellSize);
        Vector3 offset = (stackSize - cellSize - ((meshSize.y + padding.y) * Vector3.up)) * 0.5f;

        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position, stackSize);

        int i = 0;
        for (int x = 0; x < itemGrid.x; x++)
            for (int z = 0; z < itemGrid.z; z++)
                for (int y = 0; y < itemGrid.y; y++)
                {
                    if (++i >= maxSize) break;

                    Vector3 pos = Vector3.Scale(new Vector3(x, y, z), cellSize) - offset;
                    Gizmos.DrawMesh(itemInfo.mesh, transform.position + pos, Quaternion.identity);
                }
    }

    public override int FindSpaceAndInsert(Carriable item)
    {
        if(count >= maxSize)
            return -1;

        InsertAt(count, item);

        return count - 1;
    }

    public override bool InsertAt(int index, Carriable item)
    {
        if (count >= maxSize)
            return false;

        count++;

        itemMeshRends[index].sharedMaterial = defaultMaterial;
        return base.InsertAt(index, item);
    }

    public bool Interact(InteractiveHand hand)
    {
        bool successful = hand.ItemInHand != null && allowedTypes.IsInMask(hand.ItemInHand.type);

        if (successful)
            hand.Inventory.TransferItem(hand.ItemInHand, this);

        return successful;
    }

    public bool Interact(Carriable carriable)
    {
        bool successful = carriable != null && allowedTypes.IsInMask(carriable.type);
        if (successful)
            InsertAt(count, carriable);
        return successful;
    }

    public void Highlight(bool value, InteractiveHand hand)
    {
        if (count >= maxSize || hand.ItemInHand == null || !allowedTypes.IsInMask(hand.ItemInHand.type))
        {
            itemMeshRends[count].sharedMaterial = grayedOutMaterial;
            return;
        }

        itemMeshRends[count].sharedMaterial = value ? highlight : grayedOutMaterial;
    }
}
