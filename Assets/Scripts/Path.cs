using UnityEngine;
using System.Collections.Generic;
using System;

[Serializable]
public enum LineType { Normal, Path, Shortcut }
[Serializable]
public enum WallType { Wall, Door, Handrail}

[Serializable]
public struct Connection
{
    public int nodeA;
    public int nodeB;
    public float width; // Default width for the connection
    public bool isCardinal; // If true, the connection is a straight line (cardinal direction)

    public int lineType;

    public Connection(int a, int b, float width = 0.5f, bool isCardinal = false)
    {
        nodeA = a;
        nodeB = b;
        this.width = width;
        this.isCardinal = isCardinal;
        lineType = 0; // Default to Normal
    }

    public void SetLineType(int lineType)
    {
        this.lineType = lineType;
    }

    public WallType WallType 
    {
        readonly get => (WallType)lineType;
        set => lineType = (int)value;
    }

    public LineType LineType
    {
        readonly get => (LineType)lineType;
        set => lineType = (int)value;
    }

    public Connection(int a, int b, LineType type = default): this()
    {
        nodeA = a;
        nodeB = b;
        LineType = type;
    }

    public Connection(int a, int b, WallType type = default): this()
    {
        nodeA = a;
        nodeB = b;
        WallType = type;
    }
}

public class Path : MonoBehaviour
{
    [HideInInspector]
    public bool editMode = false; // Toggle for edit mode
    public List<Vector2> nodes = new(); // Node positions stored as Vector2 (x, y mapped to world x, z)
    public List<Connection> connections = new (); // List of connections between nodes
#if UNITY_EDITOR
    public bool isPath = false; // Flag to indicate if this is a path or a floor editor
    public bool dirty = false;
#endif

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
    public void DrawConnections()
    {
        static void DrawLine(Vector3 a, Vector3 b, int type)
        {
            UnityEditor.Handles.color = type switch
            {
                0 => Color.white,
                1 => Color.red,
                2 => Color.yellow,
                _ => Color.white
            };

            switch (type)
            {
                case 0:
                    UnityEditor.Handles.DrawLine(a, b, 1f);
                break;
                case 1:
                    UnityEditor.Handles.DrawDottedLine(a, b, 1f);
                break;
                case 2:
                    UnityEditor.Handles.DrawDottedLine(a, b, 1f);
                break;
                default:
                    UnityEditor.Handles.DrawLine(a, b, 1f);
                break;
            }
        }

        // Draw connections.
        for (int i = 0; i < connections.Count; i++)
        {
            var con = connections[i];
            if (con.nodeA < nodes.Count && con.nodeB < nodes.Count)
            {
                Vector3 a = GetPos(con.nodeA);
                Vector3 b = GetPos(con.nodeB);

                DrawLine(a, b, (int)con.LineType);
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
