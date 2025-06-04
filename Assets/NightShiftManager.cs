using GameManagers;
using MotionUtils;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NightShiftManager : MonoBehaviour
{
    [SerializeField]
    TextTemplate shiftComplete, shiftScore, pointsFromX;
    [SerializeField]
    TextMeshProUGUI countDown;

    IEnumerator Start()
    {
        var dynamicCamera = FindAnyObjectByType<DynamicCamera>();
        dynamicCamera.enabled = false;

        shiftComplete.gameObject.SetActive(false);
        shiftScore.gameObject.SetActive(false);
        pointsFromX.gameObject.SetActive(false);
        var clock = countDown.transform.parent as RectTransform;
        clock.gameObject.SetActive(false);

        const float panTime = 1.5f, duration = .5f;
        var shiftData = ShiftData.Instance;

        yield return ShowText(shiftComplete, shiftData.CurrentShift.ToString(), panTime, duration);
        yield return ShowText(shiftScore, shiftData.ShiftScore.ToString(), panTime, duration);
        yield return ShowText(pointsFromX, "x,x", panTime, duration);

        const int countDownDuration = 5;

        var directionalLight = GameObject.Find("Directional Light").GetComponent<Light>();
        var sunsetRoutine = new Timer(countDownDuration).GetRoutine(a => directionalLight.colorTemperature = Mathf.Lerp(20000, 1500f, a));
        StartCoroutine(sunsetRoutine);

        clock.gameObject.SetActive(true);
        countDown.text = countDownDuration.ToString();
        yield return new Timer(panTime).GetMoveRoutine(new Vector3(clock.anchoredPosition.x, -1000), clock.anchoredPosition, pos => clock.anchoredPosition = pos);

        yield return CountDown(countDownDuration);

        yield return new Timer(panTime).GetMoveRoutine(clock.anchoredPosition, new Vector3(clock.anchoredPosition.x, -1000), pos => clock.anchoredPosition = pos);


        dynamicCamera.enabled = true;

        SceneManager.LoadScene(1);
    }

    IEnumerator ShowText(TextTemplate textTemplate, string message, float panTime, float duration)
    {
        textTemplate.gameObject.SetActive(true);
        textTemplate.SetText(message);
        var rect = textTemplate.GetComponent<RectTransform>();
        yield return new Timer(panTime).GetRoutine(a =>
        {
            float aInverse = 1f - a;
            aInverse *= aInverse; // Squaring to ease out
            rect.anchoredPosition = new Vector2(-1000f * aInverse, rect.anchoredPosition.y);
        });
        yield return new WaitForSeconds(duration);
    }

    IEnumerator CountDown(int duration)
    {
        while (duration >= 0)
        {
            countDown.StartCoroutine(Spring.StepRoutine(30f, .15f, a =>
            {
                countDown.transform.parent.localEulerAngles = new Vector3(0, 0, a * 45);
            }, 0, 0, 15));

            if(duration != 0)
                countDown.text = duration.ToString();
            else
                countDown.text = "GO!";

            yield return new WaitForSeconds(1f);
            duration--;
        }
    }
}
