using UnityEngine;
using System.Collections.Generic;
using System;



public class Floor : MonoBehaviour
{
    public enum LineType { Normal, Path, Shortcut }

    [Serializable]
    public class Connection
    {
        public int nodeA;
        public int nodeB;
        public LineType lineType = LineType.Normal;
        public float width = 1f; // Default width for the connection
        public bool isCardinal = true; // If true, the connection is a straight line (cardinal direction)

        public Connection(int a, int b, LineType type = LineType.Normal)
        {
            nodeA = a;
            nodeB = b;
            lineType = type;
        }
    }

    [HideInInspector]
    public bool editMode = false; // Toggle for edit mode
    public List<Vector2> nodes = new(); // Node positions stored as Vector2 (x, y mapped to world x, z)
    public List<Connection> connections = new (); // List of connections between nodes


    // Helper: Convert stored Vector2 to world space Vector3 (using transform.position.y as the height)
    public Vector3 GetPos(int i)
    {
        if (i < 0 || i >= nodes.Count)
            throw new IndexOutOfRangeException("Index out of range");

        var pos = nodes[i];

        // Add the stored offset to the GameObject's position.
        return new Vector3(transform.position.x + pos.x,
                           transform.position.y,
                           transform.position.z + pos.y);
    }

    public void SetPos(int i, Vector3 pos)
    {
        if (i < 0 || i >= nodes.Count)
            throw new IndexOutOfRangeException("Index out of range");
        // Set the stored offset based on the GameObject's position.
        nodes[i] = new Vector2(pos.x - transform.position.x,
                               pos.z - transform.position.z);
    }

    public void AddNode(Vector3 pos)
    {
        nodes.Add(new Vector2(pos.x - transform.position.x,
                               pos.z - transform.position.z));
    }

#if UNITY_EDITOR
    // Visualize nodes and connections in the Scene view.
    void OnDrawGizmos()
    {
        DrawConnections();

        UnityEditor.Handles.Label(transform.position, string.Format("Floor {0:F0}", transform.position.y), style: new GUIStyle
        {
            fontSize = 10,
            normal = new GUIStyleState { textColor = Color.white },
            alignment = TextAnchor.MiddleCenter,
            fontStyle = FontStyle.Bold

        });
    }

    // Draw connections.
    void DrawConnections()
    {
        // Draw connections.
        for (int i = 0; i < connections.Count; i++)
        {
            var con = connections[i];
            if (con.nodeA < nodes.Count && con.nodeB < nodes.Count)
            {
                Vector3 a = GetPos(con.nodeA);
                Vector3 b = GetPos(con.nodeB);

                UnityEditor.Handles.color = Color.green;

                if (con.lineType == LineType.Path || con.lineType == LineType.Shortcut)
                {
                    UnityEditor.Handles.color = con.lineType == LineType.Path
                    ? Color.red
                    : Color.yellow;

                    UnityEditor.Handles.DrawDottedLine(a, b, 1f);
                }
                else
                    UnityEditor.Handles.DrawLine(a, b);
            }
        }
    }
#endif

    // Allow index access for convenience.
    public Vector3 this[int index]
    {
        get => GetPos(index);
        set
        {
            if (index >= 0 && index < nodes.Count)
                nodes[index] = new Vector2(value.x, value.z);
            else
                throw new IndexOutOfRangeException("Index out of range");
        }
    }
}
