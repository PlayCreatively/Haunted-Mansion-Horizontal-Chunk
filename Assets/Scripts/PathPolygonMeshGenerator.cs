using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[ExecuteInEditMode]
[RequireComponent(typeof(Path))]
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshCollider))]
public class PathPolygonMeshGenerator : MonoBehaviour
{
#if UNITY_EDITOR
    public Material meshMaterial;

    private Path pathComponent;
    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;

    [Header("Debugging")]
    public bool enableDebugLogging = true;
    public bool visualizeTriangles = true;
    public Color triangleGizmoColor = Color.yellow;

    private List<Vector2> lastGenerated2DVertices = null;  // Store 2D vertices
    private List<int> lastGeneratedTriangles = null;

    void Awake()
    {
        pathComponent = GetComponent<Path>();
        meshFilter = GetComponent<MeshFilter>();
        meshRenderer = GetComponent<MeshRenderer>();
        MeshCollider meshCollider = GetComponent<MeshCollider>();
        meshCollider.sharedMesh = meshFilter.sharedMesh;

        meshRenderer.staticShadowCaster = true;
    }

    void Update()
    {
        if (meshFilter == null) meshFilter = GetComponent<MeshFilter>();
        if (meshFilter.sharedMesh == null)
        {
            Generate2DPolygonMesh();
        }
    }

    void OnDrawGizmosSelected()
    {
        if (pathComponent == null) pathComponent = GetComponent<Path>();

        // only if in edit mode
        if (!pathComponent.editMode) return;
        if (meshFilter == null) meshFilter = GetComponent<MeshFilter>();
        if (!pathComponent.dirty && meshFilter.sharedMesh != null) return;
        if (meshRenderer == null) meshRenderer = GetComponent<MeshRenderer>();
        if (pathComponent == null)
        {
            Debug.LogError("Path component not found.", this);
            return;
        }
        if (pathComponent.nodes == null || pathComponent.nodes.Count < 3)
        {
            //Debug.LogError("Not enough path nodes to form a polygon.", this);
            return;
        }
        Generate2DPolygonMesh();
    }

    [ContextMenu("Generate Polygon Mesh from Path (2D, then convert)")]
    public void GenerateMeshFromEditor()
    {
        lastGenerated2DVertices = null;
        lastGeneratedTriangles = null;

        if (pathComponent == null) pathComponent = GetComponent<Path>();
        if (meshFilter == null) meshFilter = GetComponent<MeshFilter>();
        if (meshRenderer == null) meshRenderer = GetComponent<MeshRenderer>();

        if (pathComponent == null)
        {
            Debug.LogError("Path component not found.", this);
            return;
        }
        if (pathComponent.nodes == null || pathComponent.nodes.Count < 3)
        {
            Debug.LogError("Not enough path nodes to form a polygon.", this);
            return;
        }

        Generate2DPolygonMesh();
    }

    /// <summary>
    /// Gathers the Path nodes (in 2D) by traversing the connections, runs 2D ear-clipping,
    /// then produces a 3D mesh by placing local2D.x -> x, y=0, local2D.y -> z.
    /// </summary>
    public void Generate2DPolygonMesh()
    {
        // 1. Build an ordered list of vertices by traversing the connections.
        List<int> orderedIndices = BuildOrderedIndicesFromConnections();
        if (orderedIndices == null || orderedIndices.Count < 3)
        {
            Debug.LogError("Not enough connected nodes to form a polygon.");
            return;
        }
        List<Vector2> vertices2D = new List<Vector2>();
        foreach (int idx in orderedIndices)
        {
            vertices2D.Add(pathComponent.nodes[idx]);
        }
        if (enableDebugLogging) Debug.Log($"--- Starting 2D Mesh Generation: {vertices2D.Count} Nodes (ordered via connections) ---", this);

        // 2. Ear clipping in 2D
        if (enableDebugLogging) Debug.Log("--- Starting 2D Triangulation ---", this);
        List<int> triangles = TriangulateEarClipping2D(vertices2D);
        if (triangles == null || triangles.Count < 3)
        {
            Debug.LogError("2D Triangulation failed; no triangles produced.", this);
            if (meshFilter.sharedMesh != null) meshFilter.sharedMesh.Clear();
            return;
        }
        if (enableDebugLogging) Debug.Log($"--- 2D Triangulation complete: {triangles.Count / 3} triangles ---", this);

        // Save for gizmos
        lastGenerated2DVertices = new List<Vector2>(vertices2D);
        lastGeneratedTriangles = new List<int>(triangles);

        // 3. Convert 2D -> 3D
        List<Vector3> finalVertices3D = new List<Vector3>(vertices2D.Count);
        for (int i = 0; i < vertices2D.Count; i++)
        {
            var p2D = vertices2D[i];
            finalVertices3D.Add(new Vector3(p2D.x, 0f, p2D.y));
        }

        // 4. Build the Mesh
        Mesh mesh = new Mesh { name = "Generated2DPolygonTo3D" };
        mesh.SetVertices(finalVertices3D);
        mesh.SetTriangles(triangles, 0);

        // 5. Generate simple planar UVs from the 2D data
        mesh.uv = CalculatePlanarUVs2D(vertices2D);

        // 6. Recalculate
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        // 7. Assign
        meshFilter.sharedMesh = mesh;
        if (meshMaterial != null) meshRenderer.sharedMaterial = meshMaterial;
        else meshRenderer.enabled = true;

        if (enableDebugLogging)
            Debug.Log($"Successfully generated mesh with {finalVertices3D.Count} vertices and {triangles.Count / 3} triangles.", this);
        pathComponent.dirty = false;

        SpawnConnectionPrefabs();
    }

    /// <summary>
    /// Builds an ordered list of node indices by traversing the connections.
    /// Assumes that the connections form a continuous loop (or chain).
    /// </summary>
    private List<int> BuildOrderedIndicesFromConnections()
    {
        // Build a connectivity dictionary: each node index maps to a list of its connected nodes.
        Dictionary<int, List<int>> connectivity = new Dictionary<int, List<int>>();
        for (int i = 0; i < pathComponent.connections.Count; i++)
        {
            var con = pathComponent.connections[i];
            // Only consider valid connections.
            if (con.nodeA < 0 || con.nodeA >= pathComponent.nodes.Count ||
                con.nodeB < 0 || con.nodeB >= pathComponent.nodes.Count)
                continue;
            if (!connectivity.ContainsKey(con.nodeA))
                connectivity[con.nodeA] = new List<int>();
            if (!connectivity.ContainsKey(con.nodeB))
                connectivity[con.nodeB] = new List<int>();
            connectivity[con.nodeA].Add(con.nodeB);
            connectivity[con.nodeB].Add(con.nodeA);
        }

        if (connectivity.Count == 0)
            return null;

        // Find a starting node.
        // For a proper polygon, every node would have degree 2.
        // If an endpoint exists (degree 1), start there; otherwise, choose any node.
        int startNode = -1;
        foreach (var kv in connectivity)
        {
            if (kv.Value.Count == 1)
            {
                startNode = kv.Key;
                break;
            }
        }
        if (startNode == -1)
        {
            // None has degree 1; pick any node.
            startNode = connectivity.Keys.First();
        }

        List<int> orderedIndices = new List<int>
        {
            startNode
        };
        int current = startNode;
        int previous = -1;

        // Traverse the connections.
        while (true)
        {
            List<int> neighbors = connectivity[current];
            int next = -1;
            foreach (int neighbor in neighbors)
            {
                if (neighbor != previous)
                {
                    next = neighbor;
                    break;
                }
            }
            if (next == -1)
            {
                break; // dead end
            }
            // If next equals start and we already have more than 2 nodes, we've completed the loop.
            if (next == startNode && orderedIndices.Count > 2)
            {
                break;
            }
            orderedIndices.Add(next);
            previous = current;
            current = next;
            // Safety break to prevent infinite loops.
            if (orderedIndices.Count > connectivity.Count)
                break;
        }
        return orderedIndices;
    }

    #region 2D Ear Clipping
    private List<int> TriangulateEarClipping2D(List<Vector2> vertices)
    {
        if (vertices == null || vertices.Count < 3)
        {
            if (enableDebugLogging) Debug.LogError("Ear Clipping Error: fewer than 3 vertices");
            return null;
        }

        List<int> triangles = new List<int>(vertices.Count * 3);
        List<int> remainingIndices = new List<int>(vertices.Count);
        for (int i = 0; i < vertices.Count; i++) remainingIndices.Add(i);

        // Check winding order (2D)
        bool isCCW = IsWindingOrderCCW2D(vertices);
        if (enableDebugLogging) Debug.Log($"Is CCW: {isCCW}. Initial indices: [{string.Join(", ", remainingIndices)}]", this);
        if (!isCCW)
        {
            remainingIndices.Reverse();
            if (enableDebugLogging) Debug.Log($"Reversed indices for CW input: [{string.Join(", ", remainingIndices)}]", this);
        }

        // Clip ears
        int currentIndex = 0;
        int loopSafetyBreak = remainingIndices.Count * remainingIndices.Count * 2;
        int loopCount = 0;
        int earsClipped = 0;

        while (remainingIndices.Count > 3 && loopCount < loopSafetyBreak)
        {
            loopCount++;

            int prevIndex = (currentIndex == 0) ? remainingIndices.Count - 1 : currentIndex - 1;
            int nextIndex = (currentIndex + 1) % remainingIndices.Count;

            int pi = remainingIndices[prevIndex];
            int ci = remainingIndices[currentIndex];
            int ni = remainingIndices[nextIndex];

            Vector2 p = vertices[pi];
            Vector2 c = vertices[ci];
            Vector2 n = vertices[ni];

            float cross = CrossProduct2D(p, c, n);

            // For CCW, cross > 0 means c is a convex corner
            if (!(cross >= 0f))
            {
                bool isEar = true;
                for (int j = 0; j < remainingIndices.Count; j++)
                {
                    if (j == prevIndex || j == currentIndex || j == nextIndex) continue;
                    int checkIdx = remainingIndices[j];
                    if (IsPointInTriangle2D(vertices[checkIdx], p, c, n))
                    {
                        isEar = false;
                        break;
                    }
                }

                if (isEar)
                {
                    if (enableDebugLogging) Debug.Log($"  >> Found Ear! Clipping triangle ({pi}, {ci}, {ni}).", this);
                    triangles.Add(pi);
                    triangles.Add(ci);
                    triangles.Add(ni);
                    earsClipped++;
                    remainingIndices.RemoveAt(currentIndex);
                    currentIndex = (currentIndex == 0) ? remainingIndices.Count - 1 : currentIndex - 1;
                    continue;
                }
            }
            currentIndex = (currentIndex + 1) % remainingIndices.Count;
        }

        //if (loopCount >= loopSafetyBreak)
        //{
        //    Debug.LogError($"Ear Clipping failed: Safety break triggered. Remaining: [{string.Join(", ", remainingIndices)}]. Ears clipped: {earsClipped}.", this);
        //    return null;
        //}

        if (remainingIndices.Count == 3)
        {
            triangles.Add(remainingIndices[0]);
            triangles.Add(remainingIndices[1]);
            triangles.Add(remainingIndices[2]);
            earsClipped++;
        }
        if (earsClipped != vertices.Count - 2)
        {
            if (enableDebugLogging) Debug.LogWarning($"Triangles generated ({earsClipped}) != expected ({vertices.Count - 2}).", this);
        }
        return triangles;
    }

    private bool IsWindingOrderCCW2D(List<Vector2> vertices2D)
    {
        float area = 0f;
        for (int i = 0; i < vertices2D.Count; i++)
        {
            Vector2 v1 = vertices2D[i];
            Vector2 v2 = vertices2D[(i + 1) % vertices2D.Count];
            area += (v2.x - v1.x) * (v2.y + v1.y);
        }
        return area > 0f;
    }

    private float CrossProduct2D(Vector2 a, Vector2 b, Vector2 c)
    {
        // Cross of AB x AC => (b.x - a.x)*(c.y - a.y) - (b.y - a.y)*(c.x - a.x)
        return ((b.x - a.x) * (c.y - a.y)) - ((b.y - a.y) * (c.x - a.x));
    }

    private bool IsPointInTriangle2D(Vector2 p, Vector2 a, Vector2 b, Vector2 c)
    {
        // Based on checking same winding sign with barycentric or cross method
        bool sideA = CrossProduct2D(a, b, p) >= 0f;
        bool sideB = CrossProduct2D(b, c, p) >= 0f;
        bool sideC = CrossProduct2D(c, a, p) >= 0f;
        return (sideA == sideB) && (sideB == sideC);
    }
    #endregion

    [Header("Room Trigger Settings")]
    public float triggerHeight = 3f; // How high the trigger volume should be

    [ContextMenu("Generate Room Trigger")]
    public void GenerateRoomTrigger()
    {
        // Ensure the floor mesh is generated and we have the 2D vertices.
        if (lastGenerated2DVertices == null || lastGenerated2DVertices.Count < 3)
        {
            Generate2DPolygonMesh();
            if (lastGenerated2DVertices == null || lastGenerated2DVertices.Count < 3)
            {
                Debug.LogError("Not enough vertices to generate room trigger.");
                return;
            }
        }

        // Create an extruded mesh from the 2D polygon.
        Mesh triggerMesh = new Mesh();
        List<Vector3> vertices = new List<Vector3>();
        int n = lastGenerated2DVertices.Count;

        // Bottom vertices (y = 0)
        for (int i = 0; i < n; i++)
        {
            Vector2 v = lastGenerated2DVertices[i];
            vertices.Add(new Vector3(v.x, 0f, v.y));
        }
        // Top vertices (y = triggerHeight)
        for (int i = 0; i < n; i++)
        {
            Vector2 v = lastGenerated2DVertices[i];
            vertices.Add(new Vector3(v.x, triggerHeight, v.y));
        }

        List<int> tris = new List<int>();

        // Bottom face: use the original triangulation (assumed to be in proper winding order).
        tris.AddRange(lastGeneratedTriangles);

        // Top face: reverse the winding order, offset by n (top vertices start at index n).
        for (int i = 0; i < lastGeneratedTriangles.Count; i += 3)
        {
            int a = lastGeneratedTriangles[i] + n;
            int b = lastGeneratedTriangles[i + 1] + n;
            int c = lastGeneratedTriangles[i + 2] + n;
            // Reverse order so normals point outward.
            tris.Add(c);
            tris.Add(b);
            tris.Add(a);
        }

        // Side faces: for each edge in the polygon, create a quad (two triangles).
        for (int i = 0; i < n; i++)
        {
            int next = (i + 1) % n;
            int bottomA = i;
            int bottomB = next;
            int topA = i + n;
            int topB = next + n;

            // First triangle of the side quad.
            tris.Add(bottomA);
            tris.Add(bottomB);
            tris.Add(topB);

            // Second triangle of the side quad.
            tris.Add(bottomA);
            tris.Add(topB);
            tris.Add(topA);
        }

        triggerMesh.SetVertices(vertices);
        triggerMesh.SetTriangles(tris, 0);
        triggerMesh.RecalculateNormals();
        triggerMesh.RecalculateBounds();

        // Create a child GameObject to hold the trigger collider.
        GameObject triggerObj = new GameObject("RoomTrigger");
        triggerObj.transform.SetParent(transform, false);
        triggerObj.transform.localPosition = Vector3.zero;

        // Add a MeshCollider, assign the extruded mesh, and set it as a trigger.
        MeshCollider mc = triggerObj.AddComponent<MeshCollider>();
        mc.sharedMesh = triggerMesh;
        mc.convex = false;  // Allows concave shape for static triggers.
        mc.isTrigger = true;

        Debug.Log("Room trigger generated.");
    }

    #region UV Calculation
    private Vector2[] CalculatePlanarUVs2D(List<Vector2> vertices2D)
    {
        if (vertices2D == null || vertices2D.Count == 0)
            return new Vector2[0];

        float minX = vertices2D[0].x, maxX = vertices2D[0].x;
        float minY = vertices2D[0].y, maxY = vertices2D[0].y;
        for (int i = 1; i < vertices2D.Count; i++)
        {
            if (vertices2D[i].x < minX) minX = vertices2D[i].x;
            if (vertices2D[i].x > maxX) maxX = vertices2D[i].x;
            if (vertices2D[i].y < minY) minY = vertices2D[i].y;
            if (vertices2D[i].y > maxY) maxY = vertices2D[i].y;
        }
        float rangeX = Mathf.Approximately(minX, maxX) ? 1f : (maxX - minX);
        float rangeY = Mathf.Approximately(minY, maxY) ? 1f : (maxY - minY);

        Vector2[] uvs = new Vector2[vertices2D.Count];
        for (int i = 0; i < vertices2D.Count; i++)
        {
            float u = (vertices2D[i].x - minX) / rangeX;
            float v = (vertices2D[i].y - minY) / rangeY;
            uvs[i] = new Vector2(u, v);
        }
        return uvs;
    }
    #endregion

    /// <summary>
    /// Spawns a prefab (with a plane mesh) for each connection in the Path.
    /// Each spawned prefab is positioned at the midpoint between node A and node B,
    /// rotated so its local X axis aligns with the connection, and stretched along X
    /// to match the distance between the nodes.
    /// 
    /// This function also clears previously spawned connection instances (identified by their name).
    /// </summary>
    [ContextMenu("Spawn Connection Prefabs")]
    public void SpawnConnectionPrefabs()
    {
        // Clear previously spawned connection instances.
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            Transform child = transform.GetChild(i);
            if (child.gameObject.name.StartsWith("ConnectionInstance"))
            {
#if UNITY_EDITOR
                DestroyImmediate(child.gameObject);
#else
                Destroy(child.gameObject);
#endif
            }
        }

        // Spawn a prefab instance for each connection.
        foreach (var con in pathComponent.connections)
        {
            if (con.nodeA < pathComponent.nodes.Count && con.nodeB < pathComponent.nodes.Count)
            {
                Vector3 posA = pathComponent.GetPos(con.nodeA);
                Vector3 posB = pathComponent.GetPos(con.nodeB);
                Vector3 midpoint = (posA + posB) * 0.5f;
                Vector3 direction = posB - posA;
                float length = direction.magnitude;
                if (length <= Mathf.Epsilon) continue;

                var connectionPrefabs = GameSettings.Instance.wallPrefabs;
                int wallIndex = (int)con.WallType;
                float baseXRotation = connectionPrefabs[wallIndex] ? connectionPrefabs[wallIndex].transform.localEulerAngles.x : 0;

                // First, create a base rotation that rotates the prefab’s local X (Vector3.right)
                // to align with the connection direction.
                Quaternion baseRotation =
                     Quaternion.FromToRotation(Vector3.right, direction.normalized);
                // At this point, the prefab’s default normal (assumed to be Vector3.up) becomes:
                // currentNormal = baseRotation * Vector3.up.
                Vector3 currentNormal = baseRotation * Vector3.up;
                // Compute how much to rotate about the connection direction so that the wall’s normal becomes Vector3.forward.
                float correctionAngle = Vector3.SignedAngle(currentNormal, Vector3.forward + Vector3.right, direction.normalized);
                Quaternion correction = Quaternion.AngleAxis(correctionAngle, Vector3.up);
                Quaternion finalRotation = correction * baseRotation * Quaternion.Euler(baseXRotation, -90, 0);

#if UNITY_EDITOR
                if (wallIndex >= connectionPrefabs.Length || connectionPrefabs[wallIndex] == null)
                {
                    Debug.LogWarning($"Connection prefab for ID {wallIndex} is not assigned!", this);
                    continue;
                }
                GameObject instance = (GameObject)UnityEditor.PrefabUtility.InstantiatePrefab(connectionPrefabs[wallIndex], transform);
#else
                GameObject instance = Instantiate(connectionPrefab, transform);
#endif
                instance.name = $"ConnectionInstance {con.nodeA}-{con.nodeB}";
                instance.transform.SetPositionAndRotation(midpoint + Vector3.up * .5f, finalRotation);

                // Stretch the prefab along its local X axis (the connection direction) to match the connection length.
                Vector3 originalScale = instance.transform.localScale;
                instance.transform.localScale = new Vector3(length, originalScale.y, originalScale.z);

                instance.hideFlags = HideFlags.HideInHierarchy | HideFlags.NotEditable;

            }
        }

        UnityEditor.EditorApplication.RepaintHierarchyWindow();
    }
#endif
}
