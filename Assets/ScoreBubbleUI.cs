using GameManagers;
using MotionUtils;
using System.Collections;
using TMPro;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public class ScoreBubbleUI : MonoBehaviour
{
    static ScoreBubbleUI _prefab;
    static ScoreBubbleUI Prefab
    {
        get
        {
            if (_prefab == null)
            {
                _prefab = Resources.Load<ScoreBubbleUI>("ScoreBubbleUI");
                Debug.Assert(_prefab != null, "ScoreBubbleUI prefab not found in Resources folder. Please ensure it is placed correctly.");
            }
            return _prefab;
        }
    }

    TextMeshProUGUI textMesh;

    void Awake()
    {
        textMesh = GetComponentInChildren<TextMeshProUGUI>();
    }

    IEnumerator DisplayRoutine(int score, Vector3 position)
    {
        Vector3 offset = new (0, 0, 0);
        void MoveUp()
        {
            offset += Time.deltaTime * Vector3.up;
            transform.localPosition += offset;
        }

        Spring spring = new(1f, 14f, .3f)
        {
            position = 0
        };

        Camera mainCamera = Camera.main;
        int lastScoreTick = 0;
        FMODAudioManager.Instance.ActivateCalculationSfx(true);
        yield return new Timer(.3f).GetRoutine(a =>
        {
            if (mainCamera == null)
            {
                Debug.LogWarning("Main camera not found. Score bubble will not be displayed.");
                Destroy(gameObject);
                return;
            }

            spring.Step(Time.deltaTime);
            transform.localScale = spring.position * Vector3.one;

            float powA = a * a;

            //textMesh.color = new Color(1f, 1f, 1f, powA);

            transform.position = mainCamera.WorldToScreenPoint(position);
            MoveUp();

            int curScoreTick = (int)(score * powA);
            if(curScoreTick == lastScoreTick) return;
            lastScoreTick = curScoreTick;

            textMesh.text = curScoreTick.ToString("0");
            FMODAudioManager.Instance.UpdateCalculationSfx(powA);
        });
        FMODAudioManager.Instance.ActivateCalculationSfx(false);


        yield return new Timer(.4f).GetRoutine(a =>
        {
            if (mainCamera == null)
            {
                Debug.LogWarning("Main camera not found. Score bubble will not be displayed.");
                Destroy(gameObject);
                return;
            }
            transform.position = mainCamera.WorldToScreenPoint(position);

            spring.Step(Time.deltaTime);
            transform.localScale = spring.position * Vector3.one;


        });
        var shiftScoreUI = GameLoopManager.Instance.shiftScoreUI;
        float originalRotation = transform.localEulerAngles.z;

        //spring = new(1f, 8f, .3f)
        //{
        //    position = 0,
        //    velocity = 0
        //};

        yield return new Timer(.15f).GetRoutine(a =>
        {
            if (mainCamera == null)
            {
                Debug.LogWarning("Main camera not found. Score bubble will not be displayed.");
                Destroy(gameObject);
                return;
            }

            spring.Step(Time.deltaTime);

            float aPow = a * a;

            float targetRotation = (-transform.position + shiftScoreUI.transform.position).z + 90;
            transform.localEulerAngles = Mathf.Lerp(originalRotation, targetRotation, aPow) * Vector3.forward;
            transform.localPosition -= 1800f * Time.deltaTime * (1f - aPow) * transform.up;
        });
        transform.up = -transform.position + shiftScoreUI.transform.position;

        yield return Spring.StepRoutine(transform.position, -transform.up * 1f, shiftScoreUI.transform.position, 3f, .0f, (a, data) =>
        {
            (Vector2 pos, Vector2 vel) = data;
            if (mainCamera == null)
            {
                Debug.LogWarning("Main camera not found. Score bubble will not be displayed.");
                Destroy(gameObject);
                return;
            }

            float powA = a * a * a;

            transform.position = pos;
            transform.Squash(Mathf.Lerp(vel.magnitude, 1f, .3f));
            transform.localScale *= (1f - a);
        });
        FMODAudioManager.Instance.TriggerScoreFlySfx();

        float prevScore = ShiftData.Instance.ShiftScore - score;
        Debug.Log($"{GameLoopManager.Instance.shiftScoreUI}", GameLoopManager.Instance.shiftScoreUI);

        shiftScoreUI.StartCoroutine(new Timer(score * .05f).GetRoutine(a =>
        {
            GameLoopManager.Instance.shiftScoreUI.text = (prevScore + score * a).ToString("0");
        }));

        float initFontSize = shiftScoreUI.fontSize;
        shiftScoreUI.StartCoroutine(Spring.StepRoutine(18f, .2f, a =>
        {
            GameLoopManager.Instance.shiftScoreUI.fontSize = initFontSize * a;
        }, 1.2f, 1f, 8f));

        Destroy(gameObject);
    }

    public void DisplayScore(int score, Vector3 position)
    {
        StartCoroutine(DisplayRoutine(score, position));
    }

    public static void SpawnScore(int score, Vector3 position)
    {
        ScoreBubbleUI scoreBubbleUI = Instantiate(Prefab, position, Quaternion.identity, Game.Canvas.transform);

        scoreBubbleUI.DisplayScore(score, position);
    }
}
