using TMPro;
using UnityEngine;

public class EmployeeOfTheMonthManager : MonoBehaviour
{
    TextMeshProUGUI[] leaderBoard;

    void Awake()
    {
        leaderBoard = GetComponentsInChildren<TextMeshProUGUI>();
    }

    void OnEnable()
    {
        var leaderBoardData = LeaderBoardManager.leaderBoardDatas;
        for (int i = 0; i < leaderBoardData.Count && i < 10; i++)
        {
            leaderBoard[i].text = leaderBoardData[i].name + "\n" + leaderBoardData[i].score.ToString();
        }
    }
}
