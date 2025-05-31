using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[DefaultExecutionOrder(-500)]
public class RoomUI : MonoBehaviour
{
    public IRoom room;
    public Image roomArrowUI;
    public GameObject bookingUI;
    public ResourceRequirementsUI[] resourceRequirementsUI;
    public TextMeshProUGUI bookingTimeUI;
    [SerializeField]
    Image[] ResourceUI;
    public GameObject requirementsParent;
    //public GameObject bookedIconUI;
    int priority = 0;

    void Awake()
    {
        bookingTimeUI = bookingUI.GetComponentInChildren<TextMeshProUGUI>(true);
    }

    public void AssignRoom(IRoom room)
    {
        Awake();

        this.room = room;
        room.OnStateChange += UpdateStateUI;
        room.OnRequirementsChange += UpdateRequirementsUI;
    }

    void Update()
    {
        (transform as RectTransform).position = GetRoomPosInScreenSpace();

        UpdateArrow();
        bookingUI.transform.parent.localScale = Vector3.one * (GetUrgencySine(priority, .05f) + 1f);
    }
    public static Color[] bookingPriorityColors = { new(214f / 255, 105f / 255, 107f / 255), new(214f / 255, 153f / 255, 105f / 255), new(218f / 255, 213f / 255, 202f / 255), new(0.1254902f, 0.1764706f, 0.1686275f) };

    public void UpdateRoomOrderColors(int priority)
    {
        this.priority = priority;

        var color = bookingPriorityColors[priority];
        roomArrowUI.color = color;
        bookingTimeUI.faceColor = color;
        Color darkerShade = color * .3f;
        darkerShade.a = 1f;
        bookingTimeUI.outlineColor = darkerShade;
    }

    public Vector2 GetRoomPosInScreenSpace() => Camera.main.WorldToScreenPoint(room.Transform.position + room.UIOffset.XZ());

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

        float sinOffset = (GetUrgencySine(priority, .5f) - .5f) * 70;

        roomArrowUI.rectTransform.anchoredPosition += roomArrowUI.rectTransform.anchoredPosition.normalized * sinOffset;

        var dif = Vector2.Distance(roomArrowUI.rectTransform.anchoredPosition, rectTrans.anchoredPosition * 0.885f);
        dif = Mathf.Min(dif, 200f);
        dif /= 200f;

        roomArrowUI.rectTransform.localScale = new Vector2(dif, dif);
    }

    float GetUrgencySine(int urgency, float magnitude = .5f)
    {
        int inverseUrgency = 2 - urgency;

        return Mathf.Sin(Time.time * inverseUrgency * 4f) * magnitude;
    }

    public void UpdateRequirementsUI(Room.Requirements requirements)
    {
        if (room.IsDirty)
            foreach (var requirementsUI in resourceRequirementsUI)
                requirementsUI.UpdateRequirements(requirements);
    }

    public void UpdateStateUI(RoomState state)
    {
        if (state == RoomState.Occupied)
        {
            transform.parent.gameObject.SetActive(false);
            //bookedIconUI.SetActive(false);
        }

        else if (state == RoomState.Booked)
        {
            transform.parent.gameObject.SetActive(true);
            bookingTimeUI.transform.parent.gameObject.SetActive(true);
            //StartCoroutine(bookedIconUI.transform.ScaleUpObject(.2f, true));
        }
    }

    public void UpdateBookingTimeUI(float time)
    {
        int minutes = Mathf.FloorToInt(time / 60);
        int seconds = Mathf.FloorToInt(time % 60);

        bookingTimeUI.text = $"{minutes:D2}:{seconds:D2}";
    }
}