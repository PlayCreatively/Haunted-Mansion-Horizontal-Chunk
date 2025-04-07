using UnityEngine;
using System.Collections.Generic;

[ExecuteInEditMode]
public class PathRasterDebugger : MonoBehaviour
{
    public Path path;
    public float tileSize = 1f;
    public int gridWidth = 64; // You can change this as needed

    private bool[] rasterData;

    void OnDrawGizmos()
    {
        if (path == null || path.connections == null || path.nodes == null)
            return;

        rasterData = new bool[gridWidth * gridWidth * 4]; // 4 triangles per tile (2x2 per tile)

        foreach (var con in path.connections)
        {
            if (con.LineType != LineType.Normal)
                continue;

            if (con.nodeA >= path.nodes.Count || con.nodeB >= path.nodes.Count)
                continue;

            Vector2 start = path.nodes[con.nodeA];
            Vector2 end = path.nodes[con.nodeB];

            RasterizeConnection(start, end);
        }

        DrawRasterizedTriangles();
    }

    void RasterizeConnection(Vector2 start, Vector2 end)
    {
        // Simple Bresenham-style rasterization into triangle tiles
        Vector2 dir = (end - start).normalized;
        float length = Vector2.Distance(start, end);
        float step = tileSize * 0.25f;
        int count = Mathf.CeilToInt(length / step);

        for (int i = 0; i <= count; i++)
        {
            Vector2 point = Vector2.Lerp(start, end, i / (float)count);

            int tileX = Mathf.FloorToInt(point.x / tileSize);
            int tileZ = Mathf.FloorToInt(point.y / tileSize);

            float localX = (point.x / tileSize) - tileX;
            float localZ = (point.y / tileSize) - tileZ;

            bool rightTriangle = (localX + localZ) > 1f;

            int triX = tileX * 2 + (rightTriangle ? 1 : 0);
            int triZ = tileZ;
            int index = triZ * gridWidth * 2 + triX;

            if (index >= 0 && index < rasterData.Length)
                rasterData[index] = true;
        }
    }

    void DrawRasterizedTriangles()
    {
        Gizmos.color = Color.red;

        for (int i = 0; i < rasterData.Length; i++)
        {
            if (!rasterData[i]) continue;

            int triX = i % (gridWidth * 2);
            int triZ = i / (gridWidth * 2);

            int tileX = triX / 2;
            int tileZ = triZ;

            bool isRightTriangle = (triX % 2) == 1;

            Vector3 tileOrigin = new Vector3(tileX * tileSize, 0, tileZ * tileSize);

            Vector3 a, b, c;

            if (isRightTriangle)
            {
                a = new Vector3(tileOrigin.x + tileSize, 0, tileOrigin.z);
                b = new Vector3(tileOrigin.x + tileSize, 0, tileOrigin.z + tileSize);
                c = new Vector3(tileOrigin.x, 0, tileOrigin.z + tileSize);
            }
            else
            {
                a = new Vector3(tileOrigin.x, 0, tileOrigin.z);
                b = new Vector3(tileOrigin.x + tileSize, 0, tileOrigin.z);
                c = new Vector3(tileOrigin.x, 0, tileOrigin.z + tileSize);
            }

            Gizmos.DrawLine(a, b);
            Gizmos.DrawLine(b, c);
            Gizmos.DrawLine(c, a);
        }
    }
}
