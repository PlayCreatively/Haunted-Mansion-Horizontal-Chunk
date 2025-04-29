using TMPro;
using UnityEngine;
using UnityEngine.UI;

[DefaultExecutionOrder(+500)]
public class RoomUI : MonoBehaviour
{
    public Room room;
    public Image roomArrowUI;
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
        room.OnStateChange += UpdateStateUI;
        room.OnRequirementsChange += UpdateRequirementsUI;

        gameObject.SetActive(false);
    }

    void Update()
    {
        (transform as RectTransform).position = GetRoomPosInScreenSpace();

        UpdateArrow();
    }

    public Vector2 GetRoomPosInScreenSpace() => Camera.main.WorldToScreenPoint(room.transform.position);

    public void UpdateArrow()
    {
        var viewport = Camera.main.WorldToViewportPoint(room.transform.position) - (Vector3.one * .5f);
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
        Debug.Log($"Updating requirements UI for {room.gameObject.name}.\n" +
            $"State: {room.state}\n" +
            $"IsDirty: {room.IsDirty}\n" +
            $"IsOccupied: {room.IsOccupied}\n" +
            $"Requirements: {requirements}");

        //checkInUI.SetActive(room.IsOccupied);
        requirementsParent.SetActive(room.IsDirty);

        if (room.IsDirty)
        {
            for (int i = 0; i < ResourceCountUI.Length; i++)
            {
                ResourceCountUI[i].transform.parent.gameObject.SetActive(room.requirements[i] > 0);
                ResourceCountUI[i].text = requirements[i].ToString();
            }
        }

        gameObject.SetActive(!requirements.IsFulfilled());
    }

    public void UpdateStateUI(RoomState state)
    {
        bookingUI.SetActive(state == RoomState.Booked);
    }

    public void UpdateBookingTimeUI(float time)
    {
        int minutes = Mathf.FloorToInt(time / 60);
        int seconds = Mathf.FloorToInt(time % 60);

        bookingTimeUI.text = $"{minutes:D2}:{seconds:D2}";

        const float YELLOW_MARK = 60F, RED_MARK = 30F;

        bookingTimeUI.color 
            = time < RED_MARK
            ? new Color(1, .2f, .2f, 1) // red
                : time < YELLOW_MARK
                ? new Color(1, .8f, .2f, 1) // yellow
                    : Color.white;

        roomArrowUI.color = bookingTimeUI.color;
    }
}
