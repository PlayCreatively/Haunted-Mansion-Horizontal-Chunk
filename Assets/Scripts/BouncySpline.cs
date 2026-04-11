using MotionUtils;
using UnityEngine;

[DefaultExecutionOrder(10000)]
public class BouncySpline : MonoBehaviour
{
    public float angularFrequency = 6f, dampingRatio = 1f;

    [HideInInspector]
    public Vector3 position, velocity, equalibrium;

    Vector3 lastPos;
    Vector3 lastDisplacement;
    void LateUpdate()
    {
        if(transform.localEulerAngles != lastPos)
        {
            equalibrium = transform.eulerAngles;
        }
        Vector3 displacement = transform.position - lastDisplacement;
        velocity += displacement;

        DampedSpring.Step(ref position, ref velocity, equalibrium, DampedSpring.CalcCoefficients(Time.deltaTime, angularFrequency, dampingRatio));

        
        Vector3 lookPos = position + Vector3.up;

        transform.rotation = new Quaternion(lookPos.x, lookPos.y, lookPos.z, 0);
        lastPos = position;
        lastDisplacement = transform.position;
    }

    /// <summary>
    /// Converts a displacement vector into a rotation that "leans" in the direction opposite to the movement,
    /// simulating a head or object lagging behind the movement of its parent.
    /// </summary>
    /// <param name="displacement">The movement vector in world or local space.</param>
    /// <param name="maxAngle">Maximum lean angle in degrees.</param>
    /// <returns>A Vector3 representing Euler angles for the lean rotation.</returns>
    Vector3 DisplacementToRotation(Vector3 displacement, float maxAngle = 30f)
    {
        if (displacement.sqrMagnitude < 1e-6f)
            return Vector3.zero;

        // Calculate the lean direction (opposite to movement)
        Vector3 leanDir = -displacement.normalized;

        // Map the lean direction to pitch (x) and yaw (y) rotations
        float pitch = Mathf.Clamp(leanDir.z, -1f, 1f) * maxAngle; // Forward/backward lean
        float yaw = Mathf.Clamp(leanDir.x, -1f, 1f) * maxAngle;   // Sideways lean

        // No roll (z) for head-like behavior
        return new Vector3(pitch, yaw, 0f);
    }
}
