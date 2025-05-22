using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;

[DefaultExecutionOrder(-11)]
public class BootstrapPlayers : MonoBehaviour
{
    [SerializeField] GameObject playerPrefab;

    void Awake()
    {
        PlayerInput player;

        const float offsetAmount = .65f;

        for (int i = 0; i < Gamepad.all.Count; i++)
        {
            Debug.Log("Gamepad " + i + Gamepad.all[i].name + " connected out of " + Gamepad.all.Count);
            player = PlayerInput.Instantiate(
                         playerPrefab,
                         playerIndex: i,
                         controlScheme: "Gamepad",
                         pairWithDevice: Gamepad.all[i]);

            Vector3 offset = new (0, 0, (i == 0 ? 1f : -1f) * offsetAmount); // offset the player position
            player.transform.SetParent(transform, false);
            var rb = player.GetComponent<Rigidbody>();
            rb.position = transform.TransformPoint(offset);
            rb.rotation = Quaternion.Euler(0, -90f, 0);
            player.gameObject.name = "Player " + (i + 1); // rename the player object
            player.uiInputModule = FindAnyObjectByType<InputSystemUIInputModule>();
        }

        if(Gamepad.all.Count == 0)
        {
            player = PlayerInput.Instantiate(
                         playerPrefab,
                         playerIndex: 0,
                         controlScheme: "Keyboard&Mouse",
                         pairWithDevice: Keyboard.current);
            Vector3 offset = new(0, 0, offsetAmount); // offset the player position
            player.transform.SetParent(transform, false);
            var rb = player.GetComponent<Rigidbody>();
            rb.position = transform.TransformPoint(offset);
            rb.rotation = Quaternion.Euler(0, -90f, 0);
            player.gameObject.name = "Player 1"; // rename the player object
            player.uiInputModule = FindAnyObjectByType<InputSystemUIInputModule>();
        }
    }
}
