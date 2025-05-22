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
            Destroy(gameObject);
    }
}
