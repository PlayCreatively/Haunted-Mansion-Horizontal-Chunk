using GameManagers;
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
}
