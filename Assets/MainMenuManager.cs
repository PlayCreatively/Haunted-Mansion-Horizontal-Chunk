using GameManagers;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

[DefaultExecutionOrder(-10)]
public class MainMenuManager : MonoBehaviour
{
    [SerializeField]
    UnityEvent OnOpenLeaderBoard, OnCloseLeaderBoard;

    PlayerInputManager playerInputManager;
    void Awake()
    {
        playerInputManager = FindAnyObjectByType<PlayerInputManager>();

        if (Gamepad.all.Count < 2)
            playerInputManager.EnableJoining();

        playerInputManager.onPlayerJoined += OnPlayerJoined;

        playerInputManager.GetComponentInChildren<DynamicCamera>().followPlayers = false;
    }

    public void OpenLeaderBoard() => OnOpenLeaderBoard?.Invoke();
    public void CloseLeaderBoard() => OnCloseLeaderBoard?.Invoke();

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
