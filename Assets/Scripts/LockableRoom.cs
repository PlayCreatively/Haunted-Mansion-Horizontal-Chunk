using GameManagers;
using UnityEngine;

public class LockableRoom : MonoBehaviour
{
    [SerializeField]
    Material fogMaterial;
    MeshRenderer meshRenderer;

    void Start()
    {
        if (meshRenderer == null)
            CreateCeiling();
    }

    void CreateCeiling()
    {
        var ceiling = new GameObject("FogCeiling");
        ceiling.transform.SetParent(transform);
        ceiling.transform.localPosition = Vector3.up * .95f;
        meshRenderer = ceiling.AddComponent<MeshRenderer>();
        meshRenderer.material = fogMaterial;
        meshRenderer.enabled = GetComponent<Room>().IsLocked;
    }

    public void AssignCeilingMesh(Mesh mesh)
    {
        if (meshRenderer == null)
            CreateCeiling();

        var meshFilter = meshRenderer.gameObject.AddComponent<MeshFilter>();
        meshFilter.mesh = mesh;
    }

    public void SetFogCeilingActive(bool active)
    {
        if (meshRenderer == null)
            CreateCeiling();

        meshRenderer.enabled = active;
    }

}
