using UnityEngine;

public class GhostRotation : MonoBehaviour
{
    public float rotationSpeed = 1.0f; // Speed of rotation in degrees per second
    Renderer rend;

    void Awake()
    {
        rend = GetComponent<Renderer>();
    }

    void Update()
    {
        float rotation = Time.time * rotationSpeed;
        transform.localEulerAngles = new Vector3(0, rotation, 0);
        rend.material.SetFloat("_Rotation", rotation * Mathf.Deg2Rad);
    }
}
