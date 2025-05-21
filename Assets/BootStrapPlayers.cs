using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;

public class BootstrapPlayers : MonoBehaviour
{
    [SerializeField] GameObject playerPrefab;

    void Awake()
    {
        var mgr = PlayerInputManager.instance;

        for (int i = 0; i < Gamepad.all.Count; i++)
        {
            Debug.Log("Gamepad " + i + Gamepad.all[i].name + " connected out of " + Gamepad.all.Count);
            var player = PlayerInput.Instantiate(
                         playerPrefab,
                         playerIndex: i,
                         controlScheme: "Gamepad",
                         pairWithDevice: Gamepad.all[i]);

            Vector3 offset = new (0, 0, (i * 2) - 1); // offset the player position
            player.transform.localPosition = offset;
            player.transform.SetParent(transform, false);
            player.gameObject.name = "Player " + (i + 1); // rename the player object
            player.uiInputModule = FindAnyObjectByType<InputSystemUIInputModule>();
        }

        if(Gamepad.all.Count < 2)
            mgr.EnableJoining();
    }
}
