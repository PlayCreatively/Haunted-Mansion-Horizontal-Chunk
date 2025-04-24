using TMPro;
using UnityEngine;

[DefaultExecutionOrder(-500)]
public class RoomUI : MonoBehaviour
{
    public Room room;
    public GameObject bookingUI;
    TextMeshProUGUI bookingTimeUI;
    [SerializeField]
    TextMeshProUGUI[] ResourceCountUI;
    //public GameObject checkInUI;
    public GameObject requirementsParent;

    void Start()
    {
        bookingUI.SetActive(false);
        //checkInUI.SetActive(false);

        bookingTimeUI = bookingUI.GetComponentInChildren<TextMeshProUGUI>();
        room.OnStateChange += UpdateRequirementsUI;
        room.OnRequirementsChange += UpdateRequirementsUI;
    }

    void Update()
    {
        (transform as RectTransform).position = GetRoomPosInScreenSpace();
    }

    public Vector2 GetRoomPosInScreenSpace()
    {
        Vector3 screenPos = Camera.main.WorldToScreenPoint(room.transform.position);
        return new (screenPos.x, screenPos.y);
    }

    public void UpdateRequirementsUI(Room.Requirements requirements)
    {
        Debug.Log($"Updating requirements UI for {room.gameObject.name}.\n" +
            $"State: {room.state}\n" +
            $"IsDirty: {room.IsDirty}\n" +
            $"IsOccupied: {room.IsOccupied}\n" +
            $"Requirements: {requirements}");

        //checkInUI.SetActive(room.IsOccupied);
        requirementsParent.SetActive(room.IsDirty);

        for (int i = 0; i < requirementsParent.transform.childCount; i++)
        {
            ResourceCountUI[i].text = requirements[i].ToString();
        }

        if (room.IsDirty)
        {
            for (int i = 0; i < ResourceCountUI.Length; i++)
            {
                ResourceCountUI[i].transform.parent.gameObject.SetActive(room.requirements[i] > 0);
                ResourceCountUI[i].text = requirements[i].ToString();
                //requirementsParent.transform.GetChild(i).gameObject.SetActive(room.requirements[i] > 0);
            }
        }

        gameObject.SetActive(!requirements.IsFulfilled());
    }

    public void UpdateRequirementsUI(RoomState state)
    {
        bookingUI.SetActive(state == RoomState.Booked);
        //checkInUI.SetActive(state == RoomState.Occupied);
    }

    public void UpdateBookingTimeUI(float time)
    {
        int minutes = Mathf.FloorToInt(time / 60);
        int seconds = Mathf.FloorToInt(time % 60);

        bookingTimeUI.text = $"{minutes:D2}:{seconds:D2}";
    }
}
