using UnityEngine;

public class MainMenuManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
    }

    public void StartGame()
    {
        // Load the game scene
        UnityEngine.SceneManagement.SceneManager.LoadScene(0);

        var playersObj = GameObject.Find("Players");

        foreach (var player in playersObj.GetComponentsInChildren<Player>())
        {
            player.enabled = true;
        }

        playersObj.GetComponentInChildren<DynamicCamera>().followPlayers = true;
    }
}
