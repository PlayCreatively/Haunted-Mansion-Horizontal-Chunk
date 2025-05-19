using UnityEngine;

public class FloatingObject : MonoBehaviour
{
    [SerializeField] private float floatSpeed = 1f;
    [SerializeField] private float floatHeight = 0.5f;
    [SerializeField] private float offset = 0f;
    private Vector3 startPosition;

    void Start()
    {
        startPosition = transform.position;
        
        // Apply random offset if none provided
        if (Mathf.Approximately(offset, 0f))
        {
            offset = Random.Range(0f, 2f * Mathf.PI);
        }
    }

    void Update()
    {
        float newY = startPosition.y + Mathf.Sin(Time.time * floatSpeed + offset) * floatHeight;
        transform.position = new Vector3(startPosition.x, newY, startPosition.z);
    }
}
