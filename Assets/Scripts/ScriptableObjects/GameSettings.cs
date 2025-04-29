using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public static class Game
{
    static GameManager _gameManager;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    public static void Init()
    {
        _gameManager = GameObject.Find("GameManager").AddComponent<GameManager>();
    }

    public static void GameOver(Room failedRoom)
    {
        FMODAudioManager.Instance.TriggerGameOver();
        _gameManager.StartCoroutine(GameOverRoutine(failedRoom));
    }

    static IEnumerator GameOverRoutine(Room failedRoom)
    {
        float timeT = 1f;
        while (timeT > 0)
        {
            Debug.Log(Time.timeScale);
            timeT -= Time.unscaledDeltaTime * .75f;
            Time.timeScale = timeT * timeT;
            yield return null;
        }
        Time.timeScale = 0f;

        yield return PanCamera(failedRoom.transform.position, 5f);
        yield return new WaitForSecondsRealtime(5f);
        var gameOverScreen = _gameManager.canvas.transform.Find("GameOverOverlay").GetComponent<Image>();
        gameOverScreen.gameObject.SetActive(true);
        gameOverScreen.color = new Color(1, 1, 1, 0);
        float t = 0f;
        while (t < 1f)
        {
            t += Time.unscaledDeltaTime;
            gameOverScreen.color = new Color(1, 1, 1, t);
            yield return null;
        }
        gameOverScreen.color = Color.white;
    }

    static IEnumerator PanCamera(Vector3 target, float time)
    {
        var cam = Camera.main;
        var dynamicCam = cam.GetComponent<DynamicCamera>();
        dynamicCam.enabled = false;
        var startPos = cam.transform.position;
        var endPos = target - cam.transform.forward * 10f;
        float t = 0f;
        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / time;
            cam.transform.position = Smoothing(startPos, endPos, t * t);
            yield return null;
        }
        
        static Vector3 Smoothing(Vector3 from, Vector3 to, float t) => new (
                Mathf.SmoothStep(from.x, to.x, t),
                Mathf.SmoothStep(from.y, to.y, t),
                Mathf.SmoothStep(from.z, to.z, t)
            );
    }
}

class GameManager: MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public Canvas canvas;
    public Player[] players;
    public Room[] rooms;
    //public Enemy[] enemies;
    //public Carriable[] items;

    void GetAllReferences()
    {
        players = FindObjectsByType<Player>(0);
        Debug.Log($"{players.Length} players found");

        rooms = FindObjectsByType<Room>(0);
        Debug.Log($"{rooms.Length} rooms found");

        canvas = FindFirstObjectByType<Canvas>();
    }

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }
    void Start()
    {
        GetAllReferences();
    }
}

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

    public bool UsingControllers;
    // player settings
    [Header("Player Settings")]
    public float playerSpeed = 5f;
    public float playerJumpForce = 5f;
    public float playerThrowForce = 1f;
    public float playerThrowAngle = 45f;
    public float playerThrowChargeTime = 1f;
    public float playerDashSpeed = 5;
    public float playerDashDuration = 0.5f;
    public float playerStunForce = 1;
    public float playerStunDuration = 1;
    [Header("Enemy Settings")]
    public float hurtEnemyMoveMultiplier = 1.5f;
    public EnemySettings Ghost;
    public EnemySettings Mummy, GreenGoo, Spider, Worm, TrashMonster;
    [Header("Level Settings")]
    public GameObject[] wallPrefabs = new GameObject[3];
    [Header("Room Settings")]
    public float minBookedTime = 60;
    public float maxBookedTime = 60 * 4;
    public float GetBookedTime()
    {
        return Room.Rooms.Where(r => r.state == RoomState.Booked).Count() * 5;
    }
    public float minStayTime = 20;
    public float maxStayTime = 60;
    public float GetRandomStayTime => UnityEngine.Random.Range(minStayTime, maxStayTime);
    public float minNonBookedTime = 5;
    public float maxNonBookedTime = 20;
    public float GetRandomNonBookedTime => UnityEngine.Random.Range(minNonBookedTime, maxNonBookedTime);
    [Header("Room Requirements Settings")]
    public RoomRequirementSettings requirementSettings = new()
    {
        minAmount = 1,
        maxAmount = 5,
        minTypes = 1,
        maxTypes = 3
    };
    [Header("Elevator Settings")]
    public bool smartElevator = true;

    [Serializable]
    public struct RoomRequirementSettings
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

    [Serializable]
    public struct ResourceInfo
    {
        public Carriable prefab;
        public GameObject visualPrefab;
        public Mesh mesh;
    }

    [Space(20), Header("Static Settings")]
    public ResourceInfo toiletPaper;
    public ResourceInfo towel;
    public ResourceInfo bedSheet;

    public ResourceInfo GetResourceInfo(CarriableType type)
    {
        return type switch
        {
            CarriableType.ToiletPaper => toiletPaper,
            CarriableType.Towel => towel,
            CarriableType.BedSheet => bedSheet,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
    }
    public ResourceInfo GetResourceInfo(CarriableTypeMask type)
    {
        return type switch
        {
            CarriableTypeMask.ToiletPaper => toiletPaper,
            CarriableTypeMask.Towel => towel,
            CarriableTypeMask.BedSheet => bedSheet,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
    }

    public EnemySettings GetEnemySettings(EnemyType type)
    {
        return type switch
        {
            EnemyType.Ghost => Ghost,
            EnemyType.Mummy => Mummy,
            EnemyType.Goo => GreenGoo,
            EnemyType.Spider => Spider,
            EnemyType.Worm => Worm,
            EnemyType.Trash => TrashMonster,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
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
