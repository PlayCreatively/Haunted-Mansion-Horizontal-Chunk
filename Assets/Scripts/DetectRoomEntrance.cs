using UnityEngine;

public class DetectRoomEntrance : MonoBehaviour
{
    void Start()
    {
        // Get the MeshFilter component attached to this GameObject
        MeshFilter meshFilter = GetComponent<MeshFilter>();
        // Check if the MeshFilter is not null and has a mesh
        //if (meshFilter != null && meshFilter.mesh != null)
        //{
        //    // Create a new PolygonCollider2D
        //    PolygonCollider2D polygonCollider = gameObject.AddComponent<PolygonCollider2D>();
        //    // Set the points of the PolygonCollider2D to match the vertices of the mesh
        //    Vector2[] points = new Vector2[meshFilter.mesh.vertexCount];
        //    for (int i = 0; i < meshFilter.mesh.vertexCount; i++)
        //    {
        //        points[i] = new Vector2(meshFilter.mesh.vertices[i].x, meshFilter.mesh.vertices[i].z);
        //    }
        //    polygonCollider.points = points;
        //}
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(TryGetComponent(out Room room) && collision.gameObject.TryGetComponent(out Carriable item) && room.IsDirty)
        {
            room.ResourceEnter(item.type, true);
            item.Destroy();
        }
    }
    //private void OnCollisionExit(Collision collision)
    //{

    //    if (TryGetComponent(out Room room) && collision.gameObject.TryGetComponent(out Carriable item) && room.IsDirty)
    //    {
    //        room.ResourceEnter(item.type, false);
    //    }
    //}
}
