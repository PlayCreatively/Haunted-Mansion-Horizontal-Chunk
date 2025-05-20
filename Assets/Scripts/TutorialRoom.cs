using System;
using UnityEngine;
using UnityEngine.Assertions;
using Random = UnityEngine.Random;

public class TutorialRoom : MonoBehaviour, IRoom
{
    float stayTime;
    float _bookedTime;
    float BookedTime
    {
        get => _bookedTime;
        set
        {
            _bookedTime = value;
            //if(UrgencyState < 2 && _bookedTime < 15f)
            //    UrgencyState = 2;
            //else if(UrgencyState < 1 && _bookedTime < 30f)
            //    UrgencyState = 1;
            
            roomUI.UpdateBookingTimeUI(_bookedTime);
        }
    }
    int _urgencyState = 0;
    int UrgencyState
    {
        get => _urgencyState;
        set
        {
            if (_urgencyState == value) return;
            _urgencyState = value;
            //roomUI.OnUrgencyUpdated(value);
            FMODAudioManager.Instance.UpdateRunningOutOfTimeSfx(value);
        }
    }

    bool isDirty = false;
    public RoomState state;
    public Room.Requirements requirements;

    RoomUI roomUI;
    public Vector2 UIOffset;

    public event Action<RoomState> OnStateChange;
    public event Action<Room.Requirements> OnRequirementsChange;
    Animator shineAnimator;

    void Awake()
    {
        var roomUIObj = Resources.Load<GameObject>("RoomUI");
        shineAnimator = GetComponent<Animator>();
        Assert.IsNotNull(roomUIObj, "RoomUI prefab not found in Resources folder");

        GameObject canvas = GameObject.Find("Canvas");
        var uiObj = Instantiate(roomUIObj, Vector3.zero, Quaternion.identity, canvas.transform.Find("RoomUI"));
        roomUI = uiObj.GetComponentInChildren<RoomUI>(true);
        roomUI.transform.parent.name = gameObject.name + " UI";
        roomUI.transform.parent.localPosition = Vector3.zero;

        roomUI.AssignRoom(this);

        roomUI.bookingTimeUI.gameObject.SetActive(false);

        roomUI.transform.parent.gameObject.SetActive(false);

        if (GameSettings.Instance.RoomCleaning == false)
            enabled = false;
    }

    void Update()
    {
        //switch (state)
        //{
        //    case RoomState.Booked:
        //        BookedTime -= Time.deltaTime;
        //        if (BookedTime <= 0) CheckIn(); break;
        //    case RoomState.Occupied:
        //        stayTime -= Time.deltaTime;
        //        if (stayTime <= 0) Book(); break;
        //}
    }

    public float StayTime => stayTime;

    public bool IsClean => !isDirty;
    public bool IsDirty => isDirty;
    public bool IsBooked => state == RoomState.Booked;
    public bool IsOccupied => state == RoomState.Occupied;

    public Transform Transform => transform;
    Vector2 IRoom.UIOffset => UIOffset;

    [ContextMenu("Clean")]
    public void Clean()
    {
        UrgencyState = 0;
        FMODAudioManager.Instance.TriggerRoomCleanedSfx();
        Debug.Log($"{gameObject.name} cleaned", gameObject);
        shineAnimator.Play("GlassAnimation", 0, 0f);

        isDirty = false;
        CheckIn();
    }

    public void Book(float time, Room.Requirements requirements)
    {
        BookedTime = time;

        state = RoomState.Booked;
        isDirty = true;
        this.requirements = requirements;
        OnRequirementsChange?.Invoke(requirements);

        Debug.Log($"{gameObject.name} booked", gameObject);

        FMODAudioManager.Instance.TriggerRoomBookedSfx();
        OnStateChange?.Invoke(state);
    }

    public void CheckIn()
    {
        UrgencyState = 0;

        Debug.Log($"{gameObject.name} checked in", gameObject);

        if(isDirty)
        {
            //Clean(); // REMOVE FOR PLAYTEST
            //return;
            roomUI.UpdateBookingTimeUI(0);
            enabled = false;
            return;
        }

        state = RoomState.Occupied;
        stayTime = GameSettings.Instance.GetRandomStayTime;

        OnStateChange?.Invoke(state);
    }

    public void ResourceEnter(CarriableType type, bool enter)
    {
        if (state != RoomState.Occupied)
        {
            requirements[type] += enter ? -1 : 1;
            OnRequirementsChange?.Invoke(requirements);
            Debug.Log($"{gameObject.name} {type} {(enter ? "added" : "removed")}. Remaining: {requirements[type]}", gameObject);

            if (requirements.IsFulfilled())
            {
                Debug.Log($"{gameObject.name} requirements fulfilled. Cleaned!", gameObject);
                Clean();
            }
            else if(enter)
                FMODAudioManager.Instance.TriggerResourcePlacedInRoom();
        }
    }

    public bool IsRequired(CarriableType type)
    {
        Debug.Log($"IsRequired: {type} {isDirty && requirements.IsRequired(type)}", gameObject);
        return isDirty && requirements.IsRequired(type);
    }

    public void ForceRequire(Room.Requirements requirements)
    {
        Book(0, requirements);
    }

    public struct Requirements
    {
        public int[] resourceRequirement;
        public readonly int Count => resourceRequirement.Length;

        static readonly int resourceTypeCount = 4;

        //bool cleaning;

        public Requirements(int tpAmount, int tAmount, int bAmount, int sAmount)
        {
            resourceRequirement = new int[resourceTypeCount];
            this[CarriableType.ToiletPaper] = tpAmount;
            this[CarriableType.Towel] = tAmount;
            this[CarriableType.BedSheet] = bAmount;
            this[CarriableType.Soap] = sAmount;
        }

        public static Requirements CreateRandom()
        {
            (int minAmount, int maxAmount, int minTypes, int maxTypes) = GameSettings.Instance.requirementSettings;

            Requirements requirements = new(0,0,0,0);

            int resourceCount = Random.Range(minAmount, maxAmount + 1);

            for (int i = 0; i < resourceCount; i++)
            {
                int resourceType = Random.Range(0, resourceTypeCount);
                requirements[resourceType]++;
            }

            return requirements;
        }

        public override readonly string ToString()
        {
            string result = "";
            for (int i = 0; i < resourceRequirement.Length; i++)
            {
                if (resourceRequirement[i] > 0)
                    result += $"{(CarriableType)i}: {resourceRequirement[i]} ";
            }
            return result;
        }

        public readonly bool IsFulfilled()
        {
            for (int i = 0; i < resourceRequirement.Length; i++)
                if (resourceRequirement[i] > 0)
                    return false;
            return true;
        }

        public readonly bool IsRequired(CarriableType type) => (int)type < 4 && resourceRequirement[(int)type] > 0;

        public readonly int this[CarriableType type]
        {
            get => resourceRequirement[(int)type];
            set => resourceRequirement[(int)type] = value;
        }

        public readonly int this[int i]
        {
            get => resourceRequirement[i];
            set => resourceRequirement[i] = value;
        }
    }
}