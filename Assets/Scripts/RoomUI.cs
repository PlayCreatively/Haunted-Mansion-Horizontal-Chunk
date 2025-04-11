using UnityEngine;

[DefaultExecutionOrder(10)]
public class RoomUI : MonoBehaviour
{
    public Room room;
    public GameObject bookingUI;
    public GameObject checkInUI;
    public GameObject requirementsParent;

    void Awake()
    {
        bookingUI.SetActive(false);
        checkInUI.SetActive(false);
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

    public void UpdateUI(Room.Requirements requirements)
    {
        bookingUI.SetActive(room.IsBooked);
        //checkInUI.SetActive(room.IsOccupied);
        requirementsParent.SetActive(room.IsDirty);
        requirementsParent.transform.GetChild(0).gameObject.SetActive(false);

        if (room.IsDirty)
        {
            for (int i = 0; i < room.requirements.Count; i++)
            {
                requirementsParent.transform.GetChild(i).gameObject.SetActive(room.requirements[i] > 0);
            }
        }
    }
}
