using System.Collections;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

namespace GameManagers
{
    [DefaultExecutionOrder(ExecutionOrder.Singleton)]
    class GameLoopManager: MonoBehaviour
    {
        [HideInInspector]
        public Canvas canvas;
        [HideInInspector]
        public TextMeshProUGUI shiftScoreUI;
        
        ShiftData shiftData;

        void Awake()
        {
            Debug.Assert(_instance == null, "There can only be one GameLoopManager in the scene!");

            _instance = this;

            shiftData = ShiftData.Instance;

            canvas = GameObject.Find("Canvas").GetComponent<Canvas>();
            shiftScoreUI = canvas.transform.Find("ShiftScore").GetComponent<TextMeshProUGUI>();
        }

        void Start()
        {
            shiftData.StartNewShift();
        }

        void Update()
        {
            shiftData.UpdateShiftTime(Time.deltaTime);
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