using UnityEngine;
using UnityEditor;
using System;

[CustomEditor(typeof(Path))]
public class PathEditor : Editor
{
    private Path path;
    private int selectedNode = -1;           // Currently selected node index
    private int selectedConnection = -1;     // Currently selected connection index

    private int hoveredNode = -1;            // Node under mouse pointer
    private int hoveredConnection = -1;      // Connection under mouse pointer

    private Vector3 previewNodePos;          // Preview position for new node
    private bool previewingNode = false;     // Whether we are in preview mode (holding CTRL)
    private bool insertingNode = false;      // Whether we’re previewing an insertion on an existing connection
    private int insertTargetNodeA = -1, insertTargetNodeB = -1; // The nodes of the connection to split

    void OnEnable()
    {
        path = (Path)target;
    }

    void OnDisable()
    {
        // Exit edit mode when the inspector is closed.
        if (path.editMode)
        {
            SetEditMode(false);
        }
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        // or pressed ESCape key to stop editing mode.
        if (GUILayout.Button(path.editMode ? "Stop Editing" : "Start Editing"))
        {
            SetEditMode(!path.editMode);
        }
    }

    void SetEditMode(bool editMode)
    {
        path.editMode = editMode;
        Tools.current = editMode ? Tool.None : Tool.Move;
        if (editMode)
        {
            // Show the Scene view and set the camera to the path's position.
            SceneView sceneView = SceneView.lastActiveSceneView;
            sceneView.LookAt(path.transform.position);
            sceneView.Repaint();
        }
        else
        {
            // Exit edit mode and clear selections.
            selectedNode = -1;
            selectedConnection = -1;
        }
        // Hide default transform gizmos.
        SceneView.RepaintAll();
    }

    private void OnSceneGUI()
    {
        if (path.editMode && Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Escape)
        {
            SetEditMode(false);
        }
        else if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.E)
        {
            SetEditMode(!path.editMode);
        }

        if (!path.editMode) return;

        Event e = Event.current;
        Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
        Vector3 mousePos = GetMouseWorldPosition(ray);

        // Update what the mouse is hovering over (node or connection)
        UpdateHoveredElements(mousePos);

        // If a connection is selected, allow arrow keys to cycle its type.
        if (e.type == EventType.KeyDown)
        {
            if (selectedConnection != -1)
                ProcessArrowKeys(e);

            HandleKeyInput(e);
        }

        HandleMouseInput(e, mousePos);
        DrawNodesAndConnections();
        SceneView.RepaintAll();
    }

    private void HandleKeyInput(Event e)
    {
        // If ESC is pressed, exit edit mode.
        if (e.keyCode == KeyCode.Escape)
        {
            SetEditMode(false);
            e.Use();
        }
        else if (e.keyCode == KeyCode.Delete && selectedNode != -1)
        {
            // Delete the selected node if any.
            DeleteNode(selectedNode);
            e.Use();
        }
        else if (e.keyCode == KeyCode.Delete && selectedConnection != -1)
        {
            // Delete the selected connection if any.
            Undo.RecordObject(path, "Delete Connection");
            path.connections.RemoveAt(selectedConnection);
            selectedConnection = -1;
            EditorUtility.SetDirty(path);
            path.dirty = true;
            e.Use();
        }
        else if (e.keyCode == KeyCode.F && selectedConnection != -1)
        {
            // Focus on the selected connection if any.
            var con = path.connections[selectedConnection];
            SceneView.lastActiveSceneView.Frame(new Bounds((path.GetPos(con.nodeA) + path.GetPos(con.nodeB)) / 2, Vector3.one), false);
            e.Use();
        }
        else if (e.keyCode == KeyCode.F && selectedNode != -1)
        {
            // Focus on the selected node if any.
            SceneView.lastActiveSceneView.Frame(new Bounds(path.GetPos(selectedNode), Vector3.one), false);
            e.Use();
        }
        else if (e.keyCode == KeyCode.C && selectedNode != -1)
        {
            // Copy the selected node's position to the clipboard.
            EditorGUIUtility.systemCopyBuffer = path.GetPos(selectedNode).ToString();
            e.Use();
        }
        else if (e.keyCode == KeyCode.V && selectedNode != -1)
        {
            // Paste a new node at the copied position.
            Vector3 pastePos = new Vector3(float.Parse(EditorGUIUtility.systemCopyBuffer.Split(',')[0]),
                                            0,
                                            float.Parse(EditorGUIUtility.systemCopyBuffer.Split(',')[1]));
            AddNodeConnectedToSelected(pastePos);
            e.Use();
        }
        //undo
        else if (e.keyCode == KeyCode.Z && e.control)
        {
            selectedNode = -1;
            Undo.PerformUndo();
            path.dirty = true;
            e.Use();
        }
    }

    // Update hovered node and connection based on current mouse position.
    private void UpdateHoveredElements(Vector3 mousePos)
    {
        hoveredNode = -1;
        hoveredConnection = -1;

        // Check for hovered node.
        for (int i = 0; i < path.nodes.Count; i++)
        {
            Vector3 nodeWorld = path.GetPos(i);
            if (Vector3.Distance(mousePos, nodeWorld) < 0.3f)
            {
                hoveredNode = i;
                // If a node is hovered, we prioritize it over connections.
                return;
            }
        }
        // Check for hovered connection.
        float minDistance = 0.2f;
        for (int i = 0; i < path.connections.Count; i++)
        {
            var con = path.connections[i];
            if (con.nodeA < path.nodes.Count && con.nodeB < path.nodes.Count)
            {
                Vector3 a = path.GetPos(con.nodeA);
                Vector3 b = path.GetPos(con.nodeB);
                Vector3 closest = ClosestPointOnSegment(a, b, mousePos);
                if (Vector3.Distance(mousePos, closest) < minDistance)
                {
                    hoveredConnection = i;
                    break;
                }
            }
        }
    }

    // Cycle through the enum value for the selected connection using arrow keys.
    private void ProcessArrowKeys(Event e)
    {
        var con = path.connections[selectedConnection];
        Array values = Enum.GetValues(typeof(LineType));
        int currentIndex = (int)con.LineType;
        if (e.keyCode == KeyCode.LeftArrow || e.keyCode == KeyCode.UpArrow)
        {
            currentIndex = (currentIndex - 1 + values.Length) % values.Length;
            con.LineType = (LineType)values.GetValue(currentIndex);
            EditorUtility.SetDirty(path);
            e.Use();
        }
        else if (e.keyCode == KeyCode.RightArrow || e.keyCode == KeyCode.DownArrow)
        {
            currentIndex = (currentIndex + 1) % values.Length;
            con.LineType = (LineType)values.GetValue(currentIndex);
            EditorUtility.SetDirty(path);
            e.Use();
        }
    }

    // Process mouse events (clicks, drags, CTRL-based additions, etc.)
    private void HandleMouseInput(Event e, Vector3 mousePos)
    {
        // If CTRL is held and a node is selected, show preview.
        previewingNode = e.control && selectedNode != -1;
        insertingNode = false;
        if (previewingNode)
        {
            previewNodePos = mousePos;
            CheckForLineInsertion(mousePos);
        }

        if (e.type == EventType.MouseDown && e.button == 0)
        {
            if (e.control)
            {
                // If hovering a node that is not the selected node, create a connection.
                if (hoveredNode != -1 && hoveredNode != selectedNode)
                {
                    if (!ConnectionExists(selectedNode, hoveredNode))
                    {
                        Undo.RecordObject(path, "Create Connection");
                        path.connections.Add(new Connection(selectedNode, hoveredNode, LineType.Normal));
                        EditorUtility.SetDirty(path);
                    }
                    e.Use();
                    return;
                }
                // If selecting the already selected node, delete it.
                else if (hoveredNode == selectedNode)
                {
                    DeleteNode(selectedNode);
                    e.Use();
                    return;
                }
                // CTRL+Click on a connection insertion preview.
                else if (insertingNode)
                {
                    InsertNodeOnLine();
                }
                // Otherwise, if a node is selected, add a new node connected to the selected node.
                else if (selectedNode != -1)
                {
                    AddNodeConnectedToSelected(mousePos);
                }
            }
            else
            {
                // Without CTRL, clicking on a connection selects it.
                if (hoveredConnection != -1)
                {
                    selectedConnection = hoveredConnection;
                    selectedNode = -1;
                }
                // Otherwise, if clicking on a node, select it.
                else if (hoveredNode != -1)
                {
                    selectedNode = hoveredNode;
                    selectedConnection = -1;
                }
                else
                {
                    // Click on empty space deselects both.
                    selectedNode = -1;
                    selectedConnection = -1;
                }
            }
            e.Use();
        }
        else if (e.type == EventType.MouseDrag && e.button == 0 && selectedNode != -1 && !e.control)
        {
            // Apply snapping using the editor's snap setting.
            Vector3 snapPos = mousePos;
            float snapX = EditorSnapSettings.move.x; // the grid snap value set in the editor
            float snapZ = EditorSnapSettings.move.z; // the grid snap value set in the editor
            snapPos.x = Mathf.Round(snapPos.x / snapX) * snapX;
            snapPos.z = Mathf.Round(snapPos.z / snapZ) * snapZ;

            // Drag to move the selected node.
            Undo.RecordObject(path, "Move Node");
            path.SetPos(selectedNode, snapPos);
            EditorUtility.SetDirty(path);
            path.dirty = true;
            e.Use();
        }
    }

    // Helper method to check if a connection between two nodes already exists.
    private bool ConnectionExists(int nodeA, int nodeB)
    {
        foreach (var con in path.connections)
        {
            if ((con.nodeA == nodeA && con.nodeB == nodeB) || (con.nodeA == nodeB && con.nodeB == nodeA))
                return true;
        }
        return false;
    }

    // Deletes a node and updates connections accordingly.
    private void DeleteNode(int nodeIndex)
    {
        Undo.RecordObject(path, "Delete Node");
        // Remove the node.
        path.nodes.RemoveAt(nodeIndex);
        // Remove connections referencing this node and adjust indices.
        for (int i = path.connections.Count - 1; i >= 0; i--)
        {
            var con = path.connections[i];
            if (con.nodeA == nodeIndex || con.nodeB == nodeIndex)
            {
                path.connections.RemoveAt(i);
            }
            else
            {
                if (con.nodeA > nodeIndex) con.nodeA--;
                if (con.nodeB > nodeIndex) con.nodeB--;
            }
        }
        // Deselect if needed.
        if (selectedNode == nodeIndex)
            selectedNode = -1;
        EditorUtility.SetDirty(path);
        path.dirty = true;
    }

    // Converts the current mouse ray to a world point.
    private Vector3 GetMouseWorldPosition(Ray ray)
    {
        if (Physics.Raycast(ray, out RaycastHit hit))
            return hit.point;

        Plane plane = new Plane(Vector3.up, Vector3.up * path.transform.position.y);
        return plane.Raycast(ray, out float distance) ? ray.GetPoint(distance) : Vector3.zero;
    }

    // Add a new node connected to the currently selected node.
    private void AddNodeConnectedToSelected(Vector3 mousePos)
    {
        Undo.RecordObject(path, "Add Node");
        path.dirty = true;
        int newIndex = path.nodes.Count;
        path.AddNode(mousePos);

        // Create a new connection from the selected node to the new node.
        path.connections.Add(new Connection(selectedNode, newIndex, LineType.Normal));

        // Update the selected node to the new one.
        selectedNode = newIndex;

        EditorUtility.SetDirty(path);
        path.dirty = true;
    }

    // When holding CTRL, check if the mouse is near a connection for node insertion.
    private void CheckForLineInsertion(Vector3 mousePos)
    {
        float minDistance = 0.3f;
        for (int i = 0; i < path.connections.Count; i++)
        {
            var con = path.connections[i];
            if (con.nodeA < path.nodes.Count && con.nodeB < path.nodes.Count)
            {
                Vector3 a = path.GetPos(con.nodeA);
                Vector3 b = path.GetPos(con.nodeB);
                Vector3 closest = ClosestPointOnSegment(a, b, mousePos);
                if (Vector3.Distance(mousePos, closest) < minDistance)
                {
                    insertTargetNodeA = con.nodeA;
                    insertTargetNodeB = con.nodeB;
                    previewNodePos = closest;
                    insertingNode = true;
                    return;
                }
            }
        }
    }

    // Insert a new node along a connection (splitting it into two connections).
    private void InsertNodeOnLine()
    {
        if (insertTargetNodeA == -1 || insertTargetNodeB == -1) return;

        Undo.RecordObject(path, "Insert Node");
        int newIndex = path.nodes.Count;
        path.AddNode(previewNodePos);

        // Remove the connection being split.
        int connectionToRemove = -1;
        for (int i = 0; i < path.connections.Count; i++)
        {
            var con = path.connections[i];
            if ((con.nodeA == insertTargetNodeA && con.nodeB == insertTargetNodeB) ||
                (con.nodeA == insertTargetNodeB && con.nodeB == insertTargetNodeA))
            {
                connectionToRemove = i;
                break;
            }
        }
        if (connectionToRemove != -1)
            path.connections.RemoveAt(connectionToRemove);

        // Add two new connections.
        path.connections.Add(new Connection(insertTargetNodeA, newIndex, LineType.Normal));
        path.connections.Add(new Connection(newIndex, insertTargetNodeB, LineType.Normal));

        insertTargetNodeA = -1;
        insertTargetNodeB = -1;

        // Update the selected node to the new one.
        selectedNode = newIndex;

        EditorUtility.SetDirty(path);
        path.dirty = true;
    }

    // Returns the closest point on a line segment.
    private Vector3 ClosestPointOnSegment(Vector3 a, Vector3 b, Vector3 p)
    {
        Vector3 ap = p - a, ab = b - a;
        float t = Mathf.Clamp01(Vector3.Dot(ap, ab) / Vector3.Dot(ab, ab));
        return a + t * ab;
    }

    // Draw nodes and connections, highlighting hovered/selected elements.
    private void DrawNodesAndConnections()
    {
        path.DrawConnections();

        // Redraw selected and hovered connection.
        if (selectedConnection != -1)
        {
            var con = path.connections[selectedConnection];
            if (con.nodeA < path.nodes.Count && con.nodeB < path.nodes.Count)
            {
                Vector3 a = path.GetPos(con.nodeA);
                Vector3 b = path.GetPos(con.nodeB);
                Handles.color = Color.red;
                Handles.DrawLine(a, b);
            }

        }
        if (hoveredConnection != -1 && hoveredConnection != selectedConnection)
        {
            var con = path.connections[hoveredConnection];
            if (con.nodeA < path.nodes.Count && con.nodeB < path.nodes.Count)
            {
                Vector3 a = path.GetPos(con.nodeA);
                Vector3 b = path.GetPos(con.nodeB);
                Handles.color = Color.yellow;
                Handles.DrawLine(a, b);
            }
        }

        // Draw nodes.
        for (int i = 0; i < path.nodes.Count; i++)
        {
            Vector3 nodePos = path.GetPos(i);
            if (i == selectedNode)
                Handles.color = Color.yellow;
            else if (i == hoveredNode)
                Handles.color = Color.cyan;
            else
                Handles.color = Color.green;

            Handles.DrawSolidDisc(nodePos, Vector3.up, 0.2f);

            // Label the node with its index.
            Handles.Label(nodePos + Vector3.up * 0.2f, i.ToString(), new GUIStyle
            {
                fontSize = 10,
                normal = new GUIStyleState { textColor = Color.white },
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Bold
            });
        }

        // If CTRL is held and a node is selected, handle preview drawing.
        if (Event.current.control && selectedNode != -1)
        {
            // If another node is hovered (and it's not the selected node), draw a preview connection.
            if (hoveredNode != -1 && hoveredNode != selectedNode)
            {
                Vector3 start = path.GetPos(selectedNode);
                Vector3 end = path.GetPos(hoveredNode);
                Handles.color = Color.magenta;
                Handles.DrawLine(start, end);
            }
            // Otherwise, do the regular preview (for adding a new node) if applicable.
            else
            {
                Handles.color = Color.cyan;
                Handles.DrawSolidDisc(previewNodePos, Vector3.up, 0.2f);
                if (!insertingNode)
                {
                    Vector3 selectedPos = path.GetPos(selectedNode);
                    Handles.DrawLine(selectedPos, previewNodePos);
                }
            }
        }
    }
}
