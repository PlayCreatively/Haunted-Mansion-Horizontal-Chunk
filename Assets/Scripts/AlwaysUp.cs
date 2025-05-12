using UnityEngine;

[DefaultExecutionOrder(+200)]
public class AlwaysUp : MonoBehaviour
{

    void Update()
    {
        transform.up = Vector3.up;
    }
}
