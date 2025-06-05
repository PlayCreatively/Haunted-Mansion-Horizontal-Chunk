using GameManagers;
using UnityEngine;

public class YoureFiredManager : MonoBehaviour
{
    [SerializeField]
    NameInput nameInput;
    [SerializeField]
    SkinnedMeshRenderer playerMesh;

    ShiftData ShiftData;

    void Start()
    {
        ShiftData = ShiftData.Instance;
        SkinSelector.SetSkin(playerMesh, SkinSelector.GetSkinIndex(ShiftData.playerScoreData.MVP));
    }

    public void OnSubmit()
    {
        ShiftData.playerScoreData.name = nameInput.GetName();
        LeaderBoardManager.AddLeaderBoardData(ShiftData.playerScoreData);
        Debug.Log($"You're Fired! {ShiftData.playerScoreData.name} has been added to the leaderboard with a score of {ShiftData.playerScoreData.score}.");
    }
}
