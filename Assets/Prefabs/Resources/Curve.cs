using UnityEngine;

[CreateAssetMenu(fileName = "Curve", menuName = "Scriptable Objects/Curve")]
public class Curve : ScriptableObject
{
    public AnimationCurve curve;

    public float Evaluate(float t)
    {
        return curve.Evaluate(t);
    }
}
