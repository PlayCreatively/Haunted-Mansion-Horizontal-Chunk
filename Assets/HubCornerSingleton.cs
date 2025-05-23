using UnityEngine;

[DefaultExecutionOrder(ExecutionOrder.Singleton)]
public class HubCornerSingleton : MonoBehaviour
{
    static HubCornerSingleton instance;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            instance.transform.SetPositionAndRotation(transform.position, transform.rotation);
            Destroy(gameObject);
        }
    }
}
