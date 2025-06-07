using GameManagers;
using TMPro;
using UnityEngine;

public class YoureFiredManager : MonoBehaviour
{
    [SerializeField]
    NameInput nameInput;
    [SerializeField]
    SkinnedMeshRenderer playerMesh;
    [SerializeField]
    TextTemplate scoreData;
    [SerializeField]
    TextMeshProUGUI date;

    ShiftData ShiftData;

    void Start()
    {
        FMODAudioManager.Instance.StopMenuLeaderboardTheme();
        FMODAudioManager.Instance.StopMainTheme();
        FMODAudioManager.Instance.StartLoseMenuTheme();
        ShiftData = ShiftData.Instance;
        var playerSoreData = ShiftData.Instance.playerScoreData;
        SkinSelector.SetSkin(playerMesh, SkinSelector.GetSkinIndex(playerSoreData.MVP + 1));
        scoreData.SetText(playerSoreData.shift, playerSoreData.score);
        // get current date and time
        var currentTime = System.DateTime.Now;
        // format the date and time as a string
        date.text = currentTime.ToString("dd/MM/yy");
    }

    public void OnSubmit()
    {
        ShiftData.playerScoreData.name = nameInput.GetName().Replace('.', ' ');
        LeaderBoardManager.AddLeaderBoardData(ShiftData.playerScoreData);
        Debug.Log($"You're Fired! {ShiftData.playerScoreData.name} has been added to the leaderboard with a score of {ShiftData.playerScoreData.score}.");
    }
}
