using UnityEngine;

public static class ExtensionMethods
{
    public static Vector2 XZ(this Vector3 v)
    {
        return new(v.x, v.z);
    }

    public static Vector3 XZ(this Vector3 v, float y)
    {
        return new(v.x, y, v.z);
    }

    public static Vector3 XZ(this Vector2 v, float y = 0)
    {
        return new(v.x, y, v.y);
    }
}
