using TMPro;
using UnityEngine;

[DefaultExecutionOrder(-500)]
public class RoomUI : MonoBehaviour
{
    public Room room;
    public GameObject bookingUI;
    TextMeshProUGUI bookingTimeUI;
    public GameObject checkInUI;
    public GameObject requirementsParent;

    void Start()
    {
        bookingUI.SetActive(false);
        checkInUI.SetActive(false);

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
        //checkInUI.SetActive(room.IsOccupied);
        requirementsParent.SetActive(room.IsDirty);

        if (room.IsDirty)
        {
            for (int i = 0; i < room.requirements.Count; i++)
            {
                requirementsParent.transform.GetChild(i).gameObject.SetActive(room.requirements[i] > 0);
            }
        }
    }

    public void UpdateRequirementsUI(RoomState state)
    {
        bookingUI.SetActive(state == RoomState.Booked);
        checkInUI.SetActive(state == RoomState.Occupied);
    }

    public void UpdateBookingTimeUI(float time)
    {
        int minutes = Mathf.FloorToInt(time / 60);
        int seconds = Mathf.FloorToInt(time % 60);

        bookingTimeUI.text = $"{minutes:D2}:{seconds:D2}";
    }
}
