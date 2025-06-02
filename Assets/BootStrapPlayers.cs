using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;

[DefaultExecutionOrder(-11)]
public class BootstrapPlayers : MonoBehaviour
{
    [SerializeField] GameObject playerPrefab;
    PlayerInputManager PlayerInputManager;

    void Awake()
    {
        PlayerInputManager = GetComponent<PlayerInputManager>();
        PlayerInputManager.onPlayerJoined += OnPlayerJoin;

        if (Gamepad.all.Count == 0)
        {
            PlayerInput player = PlayerInputManager.JoinPlayer(
                playerIndex: 0, 
                controlScheme: "Keyboard&Mouse", 
                pairWithDevice: Keyboard.current);
        }
        else 
            for (int i = 0; i < Gamepad.all.Count; i++)
            {
                Debug.Log("Gamepad " + i + " " + Gamepad.all[i].name + " connected out of " + Gamepad.all.Count);
                PlayerInput player = PlayerInputManager.JoinPlayer(
                                playerIndex: i,
                                controlScheme: "Gamepad",
                                pairWithDevice: Gamepad.all[i]);
            }
    }

    void OnPlayerJoin(PlayerInput player)
    {
        const float offsetAmount = .65f;

        Vector3 offset = new(0, 0, (player.playerIndex == 0 ? 1f : -1f) * offsetAmount); // offset the player position
        player.transform.SetParent(transform, false);
        var rb = player.GetComponent<Rigidbody>();
        rb.position = transform.TransformPoint(offset);
        rb.rotation = Quaternion.Euler(0, -90f, 0);
        player.gameObject.name = "Player " + (player.playerIndex + 1); // rename the player object
        SkinSelector.ReassignDefaultSkin(player.GetComponent<Player>());
        //player.uiInputModule = FindAnyObjectByType<InputSystemUIInputModule>();
        StartCoroutine(ActivateControllerRoutine(player));
    }

    IEnumerator ActivateControllerRoutine(PlayerInput player)
    {
        yield return new WaitForEndOfFrame(); // Wait for the next frame to ensure the player is fully initialized
        player.SwitchCurrentActionMap("Player");
        player.ActivateInput();
    }
}
