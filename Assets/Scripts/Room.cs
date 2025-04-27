using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using Random = UnityEngine.Random;

public enum RoomState
{
    NonBooked,
    Booked,
    Occupied,
    //Locked
}


public class Room : MonoBehaviour
{
    float stayTime;
    float _bookedTime;
    float BookedTime
    {
        get => _bookedTime;
        set
        {
            _bookedTime = value;
            roomUI.UpdateBookingTimeUI(_bookedTime);
        }
    }
    float nonBookedTime = 0f;

    bool isDirty = false;
    public RoomState state;
    public Requirements requirements;

    RoomUI roomUI;

    protected readonly static List<Room> rooms = new(4);
    public Action<RoomState> OnStateChange;
    public Action<Requirements> OnRequirementsChange;
    public static Action<Room> OnRoomStateChange;

    void OnEnable() => rooms.Add(this);
    void OnDisable() => rooms.Remove(this);

    void Awake()
    {
        var roomUIObj = Resources.Load<GameObject>("RoomUI");
        Assert.IsNotNull(roomUIObj, "RoomUI prefab not found in Resources folder");

        GameObject canvas = GameObject.Find("Canvas");
        roomUI = Instantiate(roomUIObj, Vector3.zero, Quaternion.identity, canvas.transform.Find("RoomUI")).GetComponentInChildren<RoomUI>();
        roomUI.transform.parent.name = gameObject.name + " UI";
        roomUI.transform.parent.localPosition = Vector3.zero;
        roomUI.room = this;

    }

    void Start()
    {
        CheckIn();
    }

    void Update()
    {
        switch (state)
        {
            case RoomState.NonBooked:
                nonBookedTime -= Time.deltaTime;
                if(nonBookedTime <= 0) Book(); break;
            case RoomState.Booked:
                BookedTime -= Time.deltaTime;
                if (BookedTime <= 0) CheckIn(); break;
            case RoomState.Occupied:
                stayTime -= Time.deltaTime;
                if (stayTime <= 0) CheckOut(); break;
        }
    }

    public float StayTime => stayTime;
    public float NonBookedTime => nonBookedTime;
    public static List<Room> Rooms => rooms;

    public bool IsClean => !isDirty;
    public bool IsDirty => isDirty;
    public bool IsBooked => state == RoomState.Booked;
    public bool IsOccupied => state == RoomState.Occupied;

    public void Clean()
    {
        FMODAudioManager.Instance.TriggerRoomCleanedSfx();
        Debug.Log($"{gameObject.name} cleaned", gameObject);
        isDirty = false;
        OnStateChange?.Invoke(state);
        OnRoomStateChange?.Invoke(this);
    }

    public void Book()
    {
        Debug.Log($"{gameObject.name} booked", gameObject);
        state = RoomState.Booked;
        BookedTime = GameSettings.Instance.GetRandomBookedTime;

        FMODAudioManager.Instance.TriggerRoomBookedSfx();
        OnStateChange?.Invoke(state);
        OnRoomStateChange?.Invoke(this);
    }

    public void CheckIn()
    {
        Debug.Log($"{gameObject.name} checked in", gameObject);

        if(isDirty)
        {
            Game.GameOver();
        }

        state = RoomState.Occupied;
        stayTime = GameSettings.Instance.GetRandomStayTime;

        OnStateChange?.Invoke(state);
        OnRoomStateChange?.Invoke(this);
    }

    [ContextMenu("Check Out")]
    public void CheckOut()
    {
        Debug.Log($"{gameObject.name} checked out", gameObject);
        state = RoomState.NonBooked;
        isDirty = true;
        nonBookedTime = GameSettings.Instance.GetRandomNonBookedTime;
        requirements = Requirements.CreateRandom();
        OnRequirementsChange?.Invoke(requirements);

        OnStateChange?.Invoke(state);
        OnRoomStateChange?.Invoke(this);
        FMODAudioManager.Instance.TriggerRoomCheckOutSfx();
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

    bool TryGetAvailableRoom(out Room availableRoom)
    {
        foreach (var room in rooms)
            if (room.IsClean)
            {
                room.Book();
                availableRoom = room;
                return true;
            }

        availableRoom = null;
        return false;
    }

    public struct Requirements
    {
        public int[] resourceRequirement;
        public readonly int Count => resourceRequirement.Length;

        static readonly int resourceTypeCount = 3;

        //bool cleaning;

        public Requirements(int tpAmount, int tAmount, int bAmount)
        {
            resourceRequirement = new int[resourceTypeCount];
            this[CarriableType.ToiletPaper] = tpAmount;
            this[CarriableType.Towel] = tAmount;
            this[CarriableType.BedSheet] = bAmount;
        }

        public static Requirements CreateRandom()
        {
            (int minAmount, int maxAmount, int minTypes, int maxTypes) = GameSettings.Instance.requirementSettings;

            Requirements requirements = new(0,0,0);

            for (int i = 0; i < 5; i++)
            {
                int resourceType = Random.Range(0, resourceTypeCount);
                requirements[resourceType] += Random.Range(minAmount, maxAmount);
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

public class RoomManager : MonoBehaviour
{
    public static RoomManager Instance { get; private set; }
    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }
}