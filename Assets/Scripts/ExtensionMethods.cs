using UnityEngine;

public static class ExtensionMethods
{
    public static Vector2 XZ(this Vector3 v)
    {
        return new(v.x, v.z);
    }
}
