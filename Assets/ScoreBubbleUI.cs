using GameManagers;
using System.Collections;
using TMPro;
using UnityEngine;

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
        Camera mainCamera = Camera.main;
        yield return new Timer(5).GetSpringRoutine(14f, .2f, 8f, a =>
        {

            transform.localScale = (a * Vector3.one);
            transform.position = mainCamera.WorldToScreenPoint(position);
            transform.rotation = Quaternion.Euler(0, 0, (a * 180f) - 180f);

            textMesh.text = score.ToString();
        });

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
