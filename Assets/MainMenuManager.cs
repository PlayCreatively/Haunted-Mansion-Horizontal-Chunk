using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Windows;

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

        input.currentActionMap = new InputActionMap("UI");

        var player = input.GetComponent<Player>();
        player.enabled = false;
    }

    public void StartGame() => LoadScene(1);

    public void StartTutorial() => LoadScene(2);

    async void LoadScene(int i)
    {
        var playersObj = GameObject.Find("Players");
        await UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(i);
        foreach (var player in playersObj.GetComponentsInChildren<Player>())
        {
            player.enabled = true;
            player.GetComponent<PlayerInput>().currentActionMap = new InputActionMap("Player");
            Destroy(player.GetComponent<SkinSelector>());
        }
        playersObj.GetComponentInChildren<DynamicCamera>().followPlayers = true;

    }
}
