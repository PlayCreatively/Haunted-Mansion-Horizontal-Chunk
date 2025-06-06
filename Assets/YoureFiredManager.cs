using GameManagers;
using UnityEngine;

public class YoureFiredManager : MonoBehaviour
{
    [SerializeField]
    NameInput nameInput;
    [SerializeField]
    SkinnedMeshRenderer playerMesh;
    [SerializeField]
    TextTemplate scoreData;

    ShiftData ShiftData;

    void Start()
    {
        ShiftData = ShiftData.Instance;
        var playerSoreData = ShiftData.Instance.playerScoreData;
        SkinSelector.SetSkin(playerMesh, SkinSelector.GetSkinIndex(playerSoreData.MVP + 1));
        scoreData.SetText(playerSoreData.shift, playerSoreData.score);
    }

    public void OnSubmit()
    {
        ShiftData.playerScoreData.name = nameInput.GetName();
        LeaderBoardManager.AddLeaderBoardData(ShiftData.playerScoreData);
        Debug.Log($"You're Fired! {ShiftData.playerScoreData.name} has been added to the leaderboard with a score of {ShiftData.playerScoreData.score}.");
    }
}
