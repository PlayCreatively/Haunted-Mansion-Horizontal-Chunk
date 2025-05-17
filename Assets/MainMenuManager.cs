using UnityEngine;

public class MainMenuManager : MonoBehaviour
{
    public async void StartGame()
    {
        var playersObj = GameObject.Find("Players");

        DontDestroyOnLoad(playersObj);

        await UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(1);

        foreach (var player in playersObj.GetComponentsInChildren<Player>())
        {
            player.enabled = true;
            Destroy(player.GetComponent<SkinSelector>());
        }

        playersObj.GetComponentInChildren<DynamicCamera>().followPlayers = true;

    }
}
