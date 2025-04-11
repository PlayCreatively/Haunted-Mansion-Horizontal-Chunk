using UnityEngine;

public class SineTransform : MonoBehaviour
{
    public float speed = 1.0f; // Speed of the sine wave
    public float amplitude = 1.0f; // Amplitude of the sine wave

    float baseline = 0.0f; // Baseline position

    float GetY(float x) => x - Mathf.Sin(2 * Mathf.PI * x) / (Mathf.PI * 2);

    void Start()
    {
        // Set the baseline to the current position
        baseline = transform.position.y;
    }

    void FixedUpdate()
    {
        float t = Mathf.PingPong(Time.time * speed, amplitude);

        transform.transform.position = new Vector3(
            transform.position.x,
            baseline + GetY(t),
            transform.position.z
        );
    }
}
