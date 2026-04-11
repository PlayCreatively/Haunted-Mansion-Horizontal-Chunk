using UnityEngine;

public class TransformRot : MonoBehaviour
{
    public Vector3 position;
    public float height;

    public float Magnitude => position.magnitude - height + 1f;
    public Quaternion Rotation
    {
        get
        {
            Vector3 norm = (-transform.parent.position + position).normalized;
            return new Quaternion(norm.x, norm.y, norm.z, 1);
        }
    }
}

#if UNITY_EDITOR

[UnityEditor.CustomEditor(typeof(TransformRot))]
public class TransformRotEditor : UnityEditor.Editor
{    
    public void OnSceneGUI()
    {
        UnityEditor.Tools.current = UnityEditor.Tool.Custom;
        TransformRot tr = (TransformRot)target;
        Vector3 heightOffset = tr.transform.parent.up * tr.height;
        tr.position = UnityEditor.Handles.DoPositionHandle(tr.position + heightOffset, tr.transform.rotation) - heightOffset;
        tr.transform.rotation = tr.Rotation;
    }
}
#endif