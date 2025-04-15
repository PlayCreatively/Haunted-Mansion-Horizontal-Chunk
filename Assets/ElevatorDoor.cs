using UnityEngine;

public class ElevatorDoor : MonoBehaviour
{
    public float time = .2f;
    public float openWidth = .2f;
    public float closeWidth = .5f;
    float curWidth = 0f;
    bool isOpen = false;
    int floor;

    readonly Transform[] doors = new Transform[2];

    void Awake()
    {
        doors[0] = transform.GetChild(0);
        doors[1] = transform.GetChild(1);

        curWidth = closeWidth;

        floor = int.Parse(transform.parent.name[^1].ToString());
    }

    void Update()
    {
        curWidth = Mathf.Clamp(curWidth + (Time.deltaTime / time) * (isOpen ? -1f : 1f), openWidth, closeWidth);
        var localScale = doors[0].localScale;
        localScale.x = curWidth;
        doors[0].localScale = doors[1].localScale =  localScale;
    }

    public void Open(bool open)
    {
        isOpen = open;
    }

    void CallElevator()
    {
        GetComponentInParent<Elevator>().CallElevator(floor);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            CallElevator();
        }
    }
}
