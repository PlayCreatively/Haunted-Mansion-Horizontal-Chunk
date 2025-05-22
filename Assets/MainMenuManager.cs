using UnityEngine;
using UnityEngine.InputSystem;

[DefaultExecutionOrder(-10)]
public class MainMenuManager : MonoBehaviour
{
    PlayerInputManager playerInputManager;
    void Awake()
    {
        playerInputManager = FindAnyObjectByType<PlayerInputManager>();

        playerInputManager.onPlayerJoined += OnPlayerJoined;

        foreach (var player in FindObjectsByType<PlayerInput>(0))
        {
            OnPlayerJoined(player);
        }

        playerInputManager.GetComponentInChildren<DynamicCamera>().followPlayers = false;
    }

    void OnDestroy()
    {
        playerInputManager.onPlayerJoined -= OnPlayerJoined;
    }

    public void OnPlayerJoined(PlayerInput input)
    {
        input.gameObject.AddComponent<SkinSelector>();

        var player = input.GetComponent<Player>();
        player.enabled = false;
        input.transform.parent.GetComponentInChildren<DynamicCamera>().followPlayers = false;
    }

    public async void StartGame()
    {
        var playersObj = GameObject.Find("Players");

        await UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(1);

        foreach (var player in playersObj.GetComponentsInChildren<Player>())
        {
            player.enabled = true;
            Destroy(player.GetComponent<SkinSelector>());
        }

        playersObj.GetComponentInChildren<DynamicCamera>().followPlayers = true;
    }

    public void StartTutorial()
    {
        var playersObj = GameObject.Find("Players");
        UnityEngine.SceneManagement.SceneManager.LoadScene(2);
        foreach (var player in playersObj.GetComponentsInChildren<Player>())
        {
            player.enabled = true;
            Destroy(player.GetComponent<SkinSelector>());
        }
        playersObj.GetComponentInChildren<DynamicCamera>().followPlayers = true;
    }
}
