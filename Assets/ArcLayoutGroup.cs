using UnityEngine;

[AddComponentMenu("Layout/Circle Layout Group")]
[ExecuteAlways]
public class ArcLayoutGroup : MonoBehaviour
{
    public float stepAngle = 15;
    public float radius = 100;
    public float angle = 0;

    void OnValidate()
    {
        UpdateArc();
    }

    void Update()
    {
        foreach (RectTransform child in transform)
            child.transform.up = Vector2.up;
    }

    public void UpdateArc()
    {
        int count = 0;

        foreach (Transform child in transform)
        {
            if (child.gameObject.activeSelf) count++;
        }

        float angleStep = stepAngle * Mathf.Deg2Rad;
        float maxAngle = angleStep * (count - 1);
        float offset = (angle * Mathf.Deg2Rad) - (maxAngle * .5f);

        int i = 0;
        foreach (RectTransform child in transform)
        {
            if (!child.gameObject.activeSelf) continue;

            float angle = offset + i * angleStep;
            float x = radius * Mathf.Cos(angle);
            float y = radius * Mathf.Sin(angle);
            child.anchoredPosition = new Vector2(x, y);
            child.transform.up = Vector2.up;
            i++;
        }
    }
}
