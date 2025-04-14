using UnityEngine;

public class Elevator : MonoBehaviour
{
    public float speed = 1.0f; // Speed of the sine wave
    public float amplitude = 1.0f; // Amplitude of the sine wave

    public Transform platform;

    float baseline = 0.0f; // Baseline position
    float openWindow = 0.2f; // Distance window for opening the elevator door
    ElevatorDoor[] doors;

    float GetY(float x) => x - Mathf.Sin(2 * Mathf.PI * x) / (Mathf.PI * 2);

    void Awake()
    {
        // Set the baseline to the current position
        baseline = platform.position.y;
        // Find all ElevatorDoor components in the children
        doors = GetComponentsInChildren<ElevatorDoor>();
    }

    void FixedUpdate()
    {
        float t = Mathf.PingPong(Time.time * speed, amplitude);

        platform.position = new Vector3(
            platform.position.x,
            baseline + GetY(t),
            platform.position.z
        );

        for (int i = 0; i < doors.Length; i++)
        {
            doors[i].Open(IsWithinWindow(i));
        }
    }

    bool IsWithinWindow(float x)
    {
        float v = x - (platform.position.y - baseline);
        return Mathf.Abs(v) < openWindow;
    }
}
