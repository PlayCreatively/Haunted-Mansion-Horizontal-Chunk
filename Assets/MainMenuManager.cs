using UnityEngine;
using UnityEngine.InputSystem;

[DefaultExecutionOrder(-10)]
public class MainMenuManager : MonoBehaviour
{
    PlayerInputManager playerInputManager;
    void Awake()
    {
        playerInputManager = FindAnyObjectByType<PlayerInputManager>();

        if (Gamepad.all.Count < 2)
            playerInputManager.EnableJoining();

        playerInputManager.onPlayerJoined += OnPlayerJoined;

        foreach (var player in FindObjectsByType<PlayerInput>(0))
        {
            OnPlayerJoined(player);
        }

        playerInputManager.GetComponentInChildren<DynamicCamera>().followPlayers = false;
    }

    void OnDestroy()
    {
        playerInputManager.DisableJoining();
        playerInputManager.joinBehavior = PlayerJoinBehavior.JoinPlayersManually;
        playerInputManager.onPlayerJoined -= OnPlayerJoined;
    }

    public void OnPlayerJoined(PlayerInput input)
    {
        input.gameObject.AddComponent<SkinSelector>();

        var player = input.GetComponent<Player>();
        player.enabled = false;
        input.transform.parent.GetComponentInChildren<DynamicCamera>().followPlayers = false;
    }

    public void StartGame() => LoadScene(1);

    public void StartTutorial() => LoadScene(2);

    void LoadScene(int i)
    {
        var playersObj = GameObject.Find("Players");
        UnityEngine.SceneManagement.SceneManager.LoadScene(i);
        foreach (var player in playersObj.GetComponentsInChildren<Player>())
        {
            player.enabled = true;
            Destroy(player.GetComponent<SkinSelector>());
        }
        playersObj.GetComponentInChildren<DynamicCamera>().followPlayers = true;

    }
}
