using System;
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

    // player settings
    [Header("Player Settings")]
    public float playerSpeed = 5f;
    public float playerJumpForce = 5f;
    public float playerThrowForce = 1f;
    public float playerThrowAngle = 45f;
    public float playerDashSpeed = 5;
    public float playerDashDuration = 0.5f;
    [Header("Level Settings")]
    public GameObject[] wallPrefabs = new GameObject[3];
    [Header("Room Settings")]
    public float minBookingTime = 60;
    public float maxBookingTime = 60 * 4;
    public float minStayTime = 20;
    public float maxStayTime = 60;
    [Header("Room Requirements Settings")]
    public RequirementSettings requirementSettings = new()
    {
        minAmount = 1,
        maxAmount = 5,
        minTypes = 1,
        maxTypes = 3
    };

    [Serializable]
    public struct RequirementSettings
    {
        public int minAmount;
        public int maxAmount;
        public int minTypes;
        public int maxTypes;

        internal readonly void Deconstruct(out int minAmount, out int maxAmount, out int minTypes, out int maxTypes)
        {
            minAmount = this.minAmount;
            maxAmount = this.maxAmount;
            minTypes = this.minTypes;
            maxTypes = this.maxTypes;
        }
    }

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
