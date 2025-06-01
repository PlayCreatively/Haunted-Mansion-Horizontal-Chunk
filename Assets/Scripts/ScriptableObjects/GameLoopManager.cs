using UnityEngine;

namespace GameManagers
{
    [DefaultExecutionOrder(ExecutionOrder.Singleton)]
    class GameLoopManager: MonoBehaviour
    {
        [HideInInspector]
        public Canvas canvas;
        ShiftData ShiftData;

        void Awake()
        {
            Debug.Assert(_instance == null, "There can only be one GameLoopManager in the scene!");

            _instance = this;

            ShiftData = ShiftData.Instance;

            canvas = FindFirstObjectByType<Canvas>();
        }

        void Start()
        {
            ShiftData.StartNewShift();
        }

        void Update()
        {
            ShiftData.UpdateShiftTime(Time.deltaTime);
        }

        static GameLoopManager _instance;
        public static GameLoopManager Instance
        {
            get
            {
                Debug.Assert(_instance != null, "GameLoopManager instance is null. Ensure it is present in the scene.");
                return _instance;
            }
        }
    }
}