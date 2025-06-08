using GameManagers;
using MotionUtils;
using System;
using UnityEngine;
using UnityEngine.UI;

public class ShiftTimerUI : MonoBehaviour
{
    public Image bookingBookmarkUIPrefab;

    Image[] bookingBookmarksUI;
    Image clockHandUI;
    Image shiftTimerUI;

    const float startAngle = -180f, endAngle = -90f, angleWidth = endAngle - startAngle;
    const float timerSize = 200f;

    Player[] players;

    void Start()
    {
        CreateTimer();
        clockHandUI = transform.Find("ClockHand").GetComponent<Image>();
        shiftTimerUI = GetComponent<Image>();

        players = FindObjectsByType<Player>(0);
    }

    readonly Spring clockHandSpring = new(-90*3, 14f, .05f);
    float deltaMove = 0;
    void Update()
    {
        float angle = startAngle + ShiftData.Instance.TimeIntoShiftAlpha * angleWidth;
        clockHandSpring.equilibrium = angle - 90;
        float lastPosition = clockHandSpring.position;
        clockHandSpring.Step(Time.deltaTime);
        deltaMove =+ MathF.Abs(clockHandSpring.position - lastPosition);
        const float deltaTickTrigger = .25f;
        if (deltaMove > deltaTickTrigger)
        {
            FMODAudioManager.Instance.TriggerWindingSfx(deltaMove / (deltaTickTrigger * 4f));
            deltaMove -= deltaTickTrigger;
        }
        clockHandUI.transform.localRotation = Quaternion.Euler(0, 0, clockHandSpring.position);

        //// UPDATE OPACITY
        bool isBehindUI = false;
        foreach (var player in players)
        {
            isBehindUI |= IsBehindUI(player.transform.position);
        }
        UpdateOpacity(isBehindUI ? 0.3f : 1f);
    }

    void UpdateOpacity(float opacity)
    {
        clockHandUI.color = new Color(clockHandUI.color.r, clockHandUI.color.g, clockHandUI.color.b, opacity);
        shiftTimerUI.color = new Color(shiftTimerUI.color.r, shiftTimerUI.color.g, shiftTimerUI.color.b, opacity);
        for (int i = 0; i < bookingBookmarksUI.Length; i++)
        {
            bookingBookmarksUI[i].color = new Color(bookingBookmarksUI[i].color.r, bookingBookmarksUI[i].color.g, bookingBookmarksUI[i].color.b, opacity);
        }
    }

    public bool IsBehindUI(Vector3 worldPos)
    {
        Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPos);
        RectTransform rectTransform = transform as RectTransform;
        return RectTransformUtility.RectangleContainsScreenPoint(rectTransform, screenPos);
    }

    void OnEnable()
    {
        RoomManager.Instance.OnBookingCompleted += OnBookingCompleted;
    }

    void OnBookingCompleted(int bookingID)
    {
        bookingBookmarksUI[bookingID].color = Color.gray * .5f;
        UpdateColorByPriority();
    }

    void UpdateColorByPriority()
    {
        ShiftData shiftData = ShiftData.Instance;
        
        int o = 0;
        for (int i = 0; i < shiftData.BookingShiftSequence.Length && o < 3; i++)
            if (shiftData.BookingShiftSequence[i].done == false)
                bookingBookmarksUI[i].color = RoomUI.bookingPriorityColors[o++];
    }

    public void CreateTimer()
    {
        var shiftData = ShiftData.Instance;
        bookingBookmarksUI = new Image[shiftData.CurrentBookingCount];

        for (int i = 0; i < shiftData.CurrentBookingCount; i++)
        {
            bookingBookmarksUI[i] = Instantiate(bookingBookmarkUIPrefab, transform);
            bookingBookmarksUI[i].transform.SetAsFirstSibling();
            float a = shiftData.GetBookingTimeAlpha(i);
            float angle = startAngle + a * angleWidth;
            bookingBookmarksUI[i].transform.SetLocalPositionAndRotation(new Vector3(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad)) * timerSize, Quaternion.Euler(0, 0, angle - 90));
            bookingBookmarksUI[i].color = RoomUI.bookingPriorityColors[3];
        }

        UpdateColorByPriority();
    }
}
