using UnityEngine;


[CreateAssetMenu(fileName = "NewGameSettings", menuName = "Scriptable Objects/GameSettings")]
public class GameSettings : ScriptableObject
{
    static GameSettings _instance;

    public static GameSettings Instance
    {
        get
        {
            if (_instance == null)
                return _instance = Resources.Load<GameSettings>("GameSettings");

            return _instance;
        }
    }

    public float playerSpeed = 5f;
    public float playerJumpForce = 5f;
    public GameObject[] wallPrefabs = new GameObject[3];

#if UNITY_EDITOR
    [UnityEditor.MenuItem("Game/" + nameof(GameSettings))]
    public static void CreateAndShow()
    {
        if (Instance == null)
        {
            _instance = CreateInstance<GameSettings>();
            UnityEditor.AssetDatabase.CreateAsset(Instance, "Assets/Resources/GameSettings.asset");
            UnityEditor.AssetDatabase.SaveAssets();
            UnityEditor.AssetDatabase.Refresh();
        }
        // open properties window
        UnityEditor.EditorUtility.OpenPropertyEditor(Instance);
    }
#endif

}
