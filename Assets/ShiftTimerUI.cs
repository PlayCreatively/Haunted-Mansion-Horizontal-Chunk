using GameManagers;
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
        SetupShiftUI();
    }

    void Update()
    {
        float angle = startAngle + ShiftData.Instance.TimeIntoShiftAlpha * angleWidth;
        clockHandUI.transform.SetLocalPositionAndRotation(new Vector3(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad)) * timerSize, Quaternion.Euler(0, 0, angle));
        clockHandUI.color = Color.black;
    }

    public void SetupShiftUI()
    {
        var shiftData = ShiftData.Instance;
        bookingBookmarksUI = new Image[shiftData.CurrentBookingCount];


        for (int i = 0; i < shiftData.CurrentBookingCount; i++)
        {
            bookingBookmarksUI[i] = Instantiate(bookingBookmarkUIPrefab, transform);
            float a = shiftData.GetBookingTimeAlpha(i);
            float angle = startAngle + a * angleWidth;
            bookingBookmarksUI[i].transform.SetLocalPositionAndRotation(new Vector3(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad)) * timerSize, Quaternion.Euler(0, 0, angle));
        }

        clockHandUI = Instantiate(bookingBookmarkUIPrefab, transform);
    }
}
