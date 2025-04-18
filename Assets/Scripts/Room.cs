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
    RoomState state;
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
        //Clean();

        roomUI = Resources.Load<RoomUI>("RoomUI");
        Assert.IsNotNull(roomUI, "RoomUI prefab not found in Resources folder");
        GameObject canvas = GameObject.Find("Canvas");
        roomUI = Instantiate(roomUI, Vector3.zero, Quaternion.identity, canvas.transform);
        roomUI.name = gameObject.name + " UI";
        roomUI.room = this;

        CheckOut();
    }

    void Update()
    {
        switch (state)
        {
            case RoomState.NonBooked:
                nonBookedTime -= Time.deltaTime;
                //Debug.Log($"{gameObject.name} dirty. Waiting for cleaning.\n {nonBookedTime} time left", gameObject);
                if(nonBookedTime <= 0)
                {
                    Book();
                    Debug.Log($"{gameObject.name} dirty. Someone booked it. Clean it!", gameObject);
                }
                break;
            case RoomState.Booked:
                BookedTime -= Time.deltaTime;
                if (BookedTime <= 0)
                {
                    CheckIn();
                    if(isDirty)
                        Debug.Log($"{gameObject.name} booking time expired. You lose!", gameObject);
                }
                break;
            case RoomState.Occupied:
                stayTime -= Time.deltaTime;
                if (stayTime <= 0)
                {
                    CheckOut();
                }
                break;
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

        OnStateChange?.Invoke(state);
        OnRoomStateChange?.Invoke(this);
    }

    public void CheckIn()
    {
        Debug.Log($"{gameObject.name} checked in", gameObject);
        state = RoomState.Occupied;
        stayTime = GameSettings.Instance.GetRandomStayTime;

        OnStateChange?.Invoke(state);
        OnRoomStateChange?.Invoke(this);
    }

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
    }

    public void ResourceEnter(ResourceType type, bool enter)
    {
        if (state == RoomState.NonBooked)
        {
            requirements[type] += enter ? -1 : 1;
            OnRequirementsChange?.Invoke(requirements);
            Debug.Log($"{gameObject.name} {type} {(enter ? "added" : "removed")}. Remaining: {requirements[type]}", gameObject);
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

        static readonly int resourceTypeCount = Enum.GetValues(typeof(ResourceType)).Length;

        //bool cleaning;

        public Requirements(int tpAmount, int tAmount, int bAmount)
        {
            resourceRequirement = new int[resourceTypeCount];
            this[ResourceType.ToiletPaper] = tpAmount;
            this[ResourceType.Towel] = tAmount;
            this[ResourceType.BedSheet] = bAmount;
        }

        public static Requirements CreateRandom()
        {
            (int minAmount, int maxAmount, int minTypes, int maxTypes) = GameSettings.Instance.requirementSettings;

            Requirements requirements = new(0,0,0);

            for (int i = 0; i < 2; i++)
            {
                int resourceType = Random.Range(0, resourceTypeCount - 1);
                requirements[resourceType] += Random.Range(minAmount, maxAmount);
            }

            return requirements;
        }

        public readonly bool IsFulfilled()
        {
            for (int i = 0; i < resourceRequirement.Length; i++)
                if (resourceRequirement[i] > 0)
                    return false;
            return true;
        }

        public readonly int this[ResourceType type]
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