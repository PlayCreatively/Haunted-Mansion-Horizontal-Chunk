using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NightShiftManager : MonoBehaviour
{
    IEnumerator Start()
    {
        var dynamicCamera = FindAnyObjectByType<DynamicCamera>();
        dynamicCamera.enabled = false;

        SetRandomAngle();

        var directionalLight = GameObject.Find("Directional Light").GetComponent<Light>();
        yield return new Timer(10f).GetRoutine(a =>
        {
            directionalLight.colorTemperature = Mathf.Lerp(20000, 1500f, a);
        });

        dynamicCamera.enabled = true;


        SceneManager.LoadScene(1);
    }

    void SetRandomAngle()
    {
        Transform[] angles = transform.Find("Angles").GetComponentsInChildren<Transform>();

        if (angles.Length == 0)
        {
            Debug.LogWarning("No angles found in Angles object.");
            return;
        }

        int randomIndex = Random.Range(1, angles.Length);

        Camera.main.transform.SetPositionAndRotation(
            angles[randomIndex].position,
            angles[randomIndex].rotation
        );
    }
}
