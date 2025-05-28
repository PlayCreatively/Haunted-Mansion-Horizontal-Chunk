using GameManagers;
using System;
using UnityEngine;
using UnityEngine.UI;

public class ShiftTimerUI : MonoBehaviour
{
    public Image bookingBookmarkUIPrefab;

    Image[] bookingBookmarksUI;
    Image clockHandUI;

    const float startAngle = -180f, endAngle = -90f, angleWidth = endAngle - startAngle;
    const float timerSize = 200f;

    void Start()
    {
        CreateTimer();
    }

    void Update()
    {
        float angle = startAngle + ShiftData.Instance.TimeIntoShiftAlpha * angleWidth;
        clockHandUI.transform.SetLocalPositionAndRotation(new Vector3(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad)) * timerSize, Quaternion.Euler(0, 0, angle));
        clockHandUI.color = Color.black;
    }

    void OnEnable()
    {
        RoomManager.Instance.OnBookingCompleted += OnBookingStateChange;
    }

    void OnDisable()
    {
        RoomManager.Instance.OnBookingCompleted -= OnBookingStateChange;
    }

    void OnBookingStateChange(int bookingID)
    {
        bookingBookmarksUI[bookingID].color = Color.gray * .5f;
        UpdateColorByPriority();
    }

    void UpdateColorByPriority()
    {
        ShiftData shiftData = ShiftData.Instance;

        Color[] priorityColors = new Color[]
        {
            Color.red,
            Color.yellow,
            Color.white
        };
        int o = 0;
        for (int i = 0; i < shiftData.BookingShiftSequence.Length && o < 3; i++)
            if (shiftData.BookingShiftSequence[i].done == false)
                bookingBookmarksUI[i].color = priorityColors[o++];
    }

    public void CreateTimer()
    {
        var shiftData = ShiftData.Instance;
        bookingBookmarksUI = new Image[shiftData.CurrentBookingCount];


        for (int i = 0; i < shiftData.CurrentBookingCount; i++)
        {
            bookingBookmarksUI[i] = Instantiate(bookingBookmarkUIPrefab, transform);
            float a = shiftData.GetBookingTimeAlpha(i);
            float angle = startAngle + a * angleWidth;
            bookingBookmarksUI[i].transform.SetLocalPositionAndRotation(new Vector3(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad)) * timerSize, Quaternion.Euler(0, 0, angle));
            bookingBookmarksUI[i].color = Color.blue;
        }

        UpdateColorByPriority();

        clockHandUI = Instantiate(bookingBookmarkUIPrefab, transform);
    }
}
