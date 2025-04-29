using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Assertions;

[ExecuteAlways, RequireComponent(typeof(BoxCollider))]
public class TilingObjects : MonoBehaviour
{
    public Vector3Int itemGrid = new(1, 1, 1); // Size of the grid for item placement
    public Vector3 padding; // Padding between items in the grid
    public Vector3 pivotOffset; // Offset for the grid position
    public GameObject objectPrefab;
    BoxCollider boxCollider;
    Mesh objectMesh;

    Vector3Int oldItemGrid;

    void Awake()
    {
        boxCollider = GetComponent<BoxCollider>();
        Assert.IsTrue(itemGrid.x > 0 && itemGrid.y > 0 && itemGrid.z > 0, "Item grid size must be greater than zero");

        objectMesh = objectPrefab.GetComponent<MeshFilter>().sharedMesh;
    }

    [ContextMenu("Respawn Objects")]
    public void RespawnObjects()
    {
        Vector3 meshSize = objectMesh.bounds.size;

        var itemFGrid = boxCollider.size.Divide(objectMesh.bounds.size);
        itemGrid = new Vector3Int(Mathf.Max(1, Mathf.RoundToInt(itemFGrid.x)), Mathf.Max(1, Mathf.RoundToInt(itemFGrid.y)), Mathf.Max(1, Mathf.RoundToInt(itemFGrid.z)));

        if (itemGrid == oldItemGrid) return;
        if (objectPrefab == null) return;

        oldItemGrid = itemGrid;

        CleanChildren();

        Vector3 cellSize = meshSize + padding;
        Vector3 stackSize = Vector3.Scale(itemGrid, cellSize);
        Vector3 offset = (stackSize - cellSize - ((meshSize.y + padding.y) * Vector3.up)) * 0.5f + pivotOffset;

        for (int x = 0; x < itemGrid.x; x++)
            for (int z = 0; z < itemGrid.z; z++)
                for (int y = 0; y < itemGrid.y; y++)
                {
                    Vector3 pos = Vector3.Scale(new Vector3(x, y, z), cellSize) - offset;
                    var item = Instantiate(objectPrefab, transform);
                    item.transform.SetLocalPositionAndRotation(pos, Quaternion.identity);

#if UNITY_EDITOR
                    item.hideFlags = HideFlags.HideInHierarchy;
#endif
                }

#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
        UnityEditor.EditorUtility.SetDirty(transform);

#endif
    }

    [ContextMenu("Destroy Objects")]
    void CleanChildren()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(transform.GetChild(i).gameObject);
        }
    }

    void Update()
    {
        RespawnObjects();
    }
}
