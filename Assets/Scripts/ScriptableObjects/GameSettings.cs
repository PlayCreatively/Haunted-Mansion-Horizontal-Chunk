using UnityEngine;

[CreateAssetMenu(fileName = "NewGameSettings", menuName = "Scriptable Objects/GameSettings")]
public class GameSettings : ScriptableObject
{
    public float playerSpeed = 5f;
    public float playerJumpForce = 5f;

    public static GameSettings Instance { get; private set; }

    public void OnEnable()
    {
        Instance = this;
    }

}
