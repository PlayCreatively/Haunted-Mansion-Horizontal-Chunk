using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace GameManagers
{ 
    public static class Game
    {
        public static Canvas Canvas => GameLoopManager.Instance.canvas;

        static Image _transitionImage;
        public static Image TransitionImage
        {
            get
            {
                if (_transitionImage == null)
                {
                    var canvasObj = GameObject.Find("TransitionCanvas");
                    if (_transitionImage != null)
                        _transitionImage = canvasObj.GetComponent<Canvas>().transform.Find("TransitionImage").GetComponent<Image>();
                    else
                    {
                        GameObject obj = new GameObject("TransitionCanvas");
                        GameObject.DontDestroyOnLoad(obj);
                        var transitionCanvas = obj.AddComponent<Canvas>();
                        transitionCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
                        transitionCanvas.sortingOrder = 1000; // Ensure it's on top
                        obj.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;

                        _transitionImage = new GameObject("TransitionImage").AddComponent<Image>();
                        _transitionImage.preserveAspect = true;
                        _transitionImage.enabled = false;
                        _transitionImage.transform.SetParent(transitionCanvas.transform, false);
                        // fill image
                        _transitionImage.rectTransform.anchorMin = new Vector2(0, .5f);
                        _transitionImage.rectTransform.anchorMax = new Vector2(1, .5f);
                        _transitionImage.rectTransform.rect.Set(0, 0, 0, 1080 * 2);
                        _transitionImage.rectTransform.offsetMin = Vector2.zero;
                        _transitionImage.rectTransform.offsetMax = Vector2.zero;
                        _transitionImage.rectTransform.pivot = new Vector2(0.5f, 0.5f);
                        _transitionImage.rectTransform.anchoredPosition = Vector2.zero;
                    }
                }
                return _transitionImage;
            }
        }

        static void SceneTransition(Action loadScene, float time, Sprite sprite)
        {
            var image = TransitionImage;
            TransitionImage.StartCoroutine(SceneTransitionRoutine(loadScene, time, sprite, image));
        }
        static IEnumerator SceneTransitionRoutine(Action loadScene, float time, Sprite sprite, Image transitionImage)
        {
            Time.timeScale = 0;
            transitionImage.rectTransform.anchorMin = Vector2.zero;
            transitionImage.rectTransform.anchorMax = Vector2.one;
            transitionImage.sprite = sprite;
            float halfTime = time * 0.5f;
            Camera cam = Camera.main;
            
            yield return new Timer(halfTime, true).GetRoutine(a =>
            {
                _transitionImage.rectTransform.anchorMin = new Vector2(0, .5f);
                _transitionImage.rectTransform.anchorMax = new Vector2(1, .5f);
                _transitionImage.rectTransform.sizeDelta = new Vector2(0, 1080 * 2);
                _transitionImage.rectTransform.anchoredPosition = new Vector2(0, 1080 * (1f - a));
                transitionImage.enabled = true;
            });

            loadScene.Invoke();

            Time.timeScale = 1;

            yield return new Timer(halfTime, true).GetRoutine(a =>
            {
                _transitionImage.rectTransform.anchorMin = new Vector2(0, .5f);
                _transitionImage.rectTransform.anchorMax = new Vector2(1, .5f);
                _transitionImage.rectTransform.sizeDelta = new Vector2(0, 1080 * 2);
                _transitionImage.rectTransform.anchoredPosition = new Vector2(0, -1080 * a);
            });

            transitionImage.enabled = false;
        }

        public static void GameOver(Room failedRoom)
        {
            FMODAudioManager.Instance.TriggerGameOver();
            GameLoopManager.Instance.StartCoroutine(GameOverRoutine(failedRoom));
        }

        public static void ToMainMenu()
        {
            Time.timeScale = 1f;

            SceneTransition(() =>
            {
                var hubCorner = GameObject.FindAnyObjectByType<HubCornerSingleton>();
                // destroy all not destroy on load
                if(hubCorner != null)
                    GameObject.Destroy(hubCorner.gameObject);
                SceneManager.LoadScene(0);
                FMODAudioManager.Instance.StartMenuLeaderboardTheme();
            }, 1f, GameSettings.Instance.transitionSprite);
        }

        public static void ToGameOverScene()
        {
            Time.timeScale = 1f;

            SceneTransition(() =>
            {
                var hubCorner = GameObject.FindAnyObjectByType<HubCornerSingleton>();
                // destroy all not destroy on load
                if(hubCorner != null)
                    GameObject.Destroy(hubCorner.gameObject);
                SceneManager.LoadScene("GameOver");
            }, 1f, GameSettings.Instance.transitionSprite);
        }

        public static async void ToNightShift(float delay)
        {
            while(delay > 0f)
            {
                delay -= Time.unscaledDeltaTime;
                await System.Threading.Tasks.Task.Yield();
            }

            Time.timeScale = 1f;

            SceneTransition(() =>
            {
                // destroy all not destroy on load
                GameObject.Destroy(GameObject.FindAnyObjectByType<HubCornerSingleton>().gameObject);
                SceneManager.LoadScene("NightShiftMode");
            }, 1f, GameSettings.Instance.transitionSprite);

        }

        static IEnumerator GameOverRoutine(Room failedRoom)
        {
            float timeT = 1f;
            while (timeT > 0)
            {
                timeT -= Time.unscaledDeltaTime * .75f;
                Time.timeScale = timeT * timeT;
                yield return null;
            }
            Time.timeScale = 0f;

            yield return PanCamera(failedRoom.GetCenter, 5f);
            yield return new WaitForSecondsRealtime(6f);
            
            // clean
            ShiftData.Instance.ResetData();

            ToGameOverScene();
        }

        static IEnumerator PanCamera(Vector3 target, float time)
        {
            var cam = Camera.main;
            var dynamicCam = cam.GetComponent<DynamicCamera>();
            dynamicCam.enabled = false;
            var startPos = cam.transform.position;
            var endPos = target - cam.transform.forward * 30f;
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
        public bool RoomCleaning = true;
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
        public EnemySettings Mummy, Spider, Worm, TrashMonster;
        public GooSettings GreenGoo;
        [Header("Level Settings")]
        public GameObject[] wallPrefabs = new GameObject[3];
        [Header("Room Settings")]
        [Tooltip("Takes into consideration how many rooms are already booked")]
        public int PostBookedTime = 45;
    
        //public float minPostBookedTime = 60;
        //public float maxPostBookedTime = 60 * 4;
        public float minStayTime = 20;
        public float maxStayTime = 60;
        public float GetRandomStayTime => UnityEngine.Random.Range(minStayTime, maxStayTime);
        public float minPreBookedTime = 5;
        public float maxPreBookedTime = 20;
        public float GetRandomPreBookedTime => UnityEngine.Random.Range(minPreBookedTime, maxPreBookedTime);
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
        [Header("Machine Settings")]
        public float laundryMachineTime = 10f;
        public float soapMachineTime = 10f;


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
        public ResourceInfo soap;
        public ResourceInfo dirtyTowel;
        public ResourceInfo dirtyBedSheet;
        public Sprite transitionSprite;
        [Space(20), Header("Enemies")]
        public Ghost ghost;
        public Mummy mummy;
        public GooMonster gooMonster;
        public TowelMonster towelMonster;


        public ResourceInfo GetResourceInfo(CarriableType type)
        {
            return type switch
            {
                CarriableType.ToiletPaper => toiletPaper,
                CarriableType.Towel => towel,
                CarriableType.BedSheet => bedSheet,
                CarriableType.Soap => soap,
                CarriableType.DirtyTowel => dirtyTowel,
                CarriableType.DirtyBedSheet => dirtyBedSheet,
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
                EnemyType.TowelMonster => Worm,
                EnemyType.Trash => TrashMonster,
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
            };
        }

        public T GetEnemyPrefab<T>() where T : Enemy
        {
            return typeof(T) == typeof(Ghost) ? ghost as T :
                   typeof(T) == typeof(Mummy) ? mummy as T :
                   typeof(T) == typeof(GooMonster) ? gooMonster as T :
                   typeof(T) == typeof(TowelMonster) ? towelMonster as T :
                   throw new ArgumentOutOfRangeException(nameof(T), typeof(T), null);
        }

        public static void ToMainMenu() => Game.ToMainMenu();

#if UNITY_EDITOR
        [UnityEditor.MenuItem("Game/Settings/" + nameof(GameSettings))]
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
}