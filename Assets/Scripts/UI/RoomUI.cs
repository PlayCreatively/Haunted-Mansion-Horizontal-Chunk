using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static Room;

[DefaultExecutionOrder(+500)]
public class RoomUI : MonoBehaviour
{
    public Room room;
    public Image roomArrowUI;
    public GameObject bookingUI;
    TextMeshProUGUI bookingTimeUI;
    [SerializeField]
    TextMeshProUGUI[] ResourceCountUI;
    public GameObject requirementsParent;

    void Awake()
    {
        bookingTimeUI = bookingUI.GetComponentInChildren<TextMeshProUGUI>();
    }

    public void AssignRoom(Room room)
    {
        this.room = room;
        room.OnStateChange += UpdateStateUI;
        room.OnRequirementsChange += UpdateRequirementsUI;
    }

    void Update()
    {
        (transform as RectTransform).position = GetRoomPosInScreenSpace();

        UpdateArrow();
    }

    public Vector2 GetRoomPosInScreenSpace() => Camera.main.WorldToScreenPoint(room.transform.position);

    public void UpdateArrow()
    {
        var rectTrans = transform as RectTransform;
        var parentRect = (rectTrans.parent.parent as RectTransform).rect;
        
        var extents = parentRect.size * .525f;

        bool isOutsideScreenX = rectTrans.anchoredPosition.x < -extents.x || rectTrans.anchoredPosition.x > extents.x;
        bool isOutsideScreenY = rectTrans.anchoredPosition.y < -extents.y || rectTrans.anchoredPosition.y > extents.y;
        bool isOutsideScreen = isOutsideScreenX || isOutsideScreenY;

        roomArrowUI.gameObject.SetActive(isOutsideScreen);
        if (!isOutsideScreen) return;

        roomArrowUI.transform.up = rectTrans.anchoredPosition;

        extents *= .9f;

        roomArrowUI.rectTransform.anchoredPosition = new Vector2(
            Mathf.Clamp(rectTrans.anchoredPosition.x, -extents.x, extents.x),
            Mathf.Clamp(rectTrans.anchoredPosition.y, -extents.y, extents.y)
        );

        var dif = Vector2.Distance(roomArrowUI.rectTransform.anchoredPosition, rectTrans.anchoredPosition * 0.885f);
        dif = Mathf.Min(dif, 200f);
        dif /= 200f;

        roomArrowUI.rectTransform.localScale = new Vector2(dif, dif);
    }

    public void UpdateRequirementsUI(Room.Requirements requirements)
    {
        if (room.IsDirty)
            for (int i = 0; i < ResourceCountUI.Length; i++)
            {
                ResourceCountUI[i].transform.parent.gameObject.SetActive(room.requirements[i] > 0);
                ResourceCountUI[i].text = requirements[i].ToString();
            }
    }

    public void UpdateStateUI(RoomState state)
    {
        Debug.Log($"Updating state UI to {state}.");

        if (state == RoomState.Occupied)
        {
            transform.parent.gameObject.SetActive(false);
        }

        else if (state == RoomState.PreBooked)
        {
            transform.parent.gameObject.SetActive(true);
            bookingTimeUI.transform.parent.gameObject.SetActive(false);
            bookingTimeUI.color = Color.white;
            roomArrowUI.color = Color.white;
        }

        else if (state == RoomState.Booked)
        {
            bookingTimeUI.transform.parent.gameObject.SetActive(true);
        }
    }

    public void OnUrgencyUpdated(int urgency)
    {
        bookingTimeUI.color = roomArrowUI.color = urgency switch
        {
            0 => Color.white,
            1 => new Color(1, .8f, .2f, 1),
            2 => new Color(1, .2f, .2f, 1),
            _ => throw new System.ArgumentOutOfRangeException(nameof(urgency), urgency, null)
        };
    }

    public void UpdateBookingTimeUI(float time)
    {
        int minutes = Mathf.FloorToInt(time / 60);
        int seconds = Mathf.FloorToInt(time % 60);

        bookingTimeUI.text = $"{minutes:D2}:{seconds:D2}";
    }
}
