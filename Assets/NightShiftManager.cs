using GameManagers;
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
        countDown.transform.parent.gameObject.SetActive(true);

        const float panTime = 1.5f, duration = .5f;
        var shiftData = ShiftData.Instance;

        yield return ShowText(shiftComplete, shiftData.CurrentShift.ToString(), panTime, duration);
        yield return ShowText(shiftScore, shiftData.ShiftScore.ToString(), panTime, duration);
        yield return ShowText(pointsFromX, "x,x", panTime, duration);

        var directionalLight = GameObject.Find("Directional Light").GetComponent<Light>();
        yield return new Timer(2f).GetRoutine(a =>
        {
            directionalLight.colorTemperature = Mathf.Lerp(20000, 1500f, a);
        });

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
}
