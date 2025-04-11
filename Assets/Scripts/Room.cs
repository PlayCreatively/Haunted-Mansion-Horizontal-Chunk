using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using Random = UnityEngine.Random;

public enum RoomState
{
    Dirty,
    Booked,
    Occupied,
    Clean,
    //Locked
}


public class Room : MonoBehaviour
{
    float stayTime;
    float bookingTime;

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
        OnRequirementsChange += roomUI.UpdateUI;

        CheckOut();
    }

    void Update()
    {
        switch (state)
        {
            case RoomState.Clean:
                break;
            case RoomState.Dirty:
                break;
            case RoomState.Booked:
                bookingTime -= Time.deltaTime;
                if (bookingTime <= 0)
                {
                    Debug.Log($"{gameObject.name} booking time expired. You lose!", gameObject);
                }
                break;
            case RoomState.Occupied:
                stayTime -= Time.deltaTime;
                if (stayTime <= 0)
                {
                    state = RoomState.Clean;
                    Clean();
                }
                break;
        }
    }

    public RoomState State
    {
        get => state;
        set
        {
            state = value;
            switch (state)
            {
                case RoomState.Clean:
                    Clean();
                    break;
                case RoomState.Dirty:
                    break;
                case RoomState.Occupied:
                    CheckIn();
                    break;
            }
        }
    }
    public float StayTime => stayTime;
    public float BookingTime => bookingTime;
    public static List<Room> Rooms => rooms;

    public bool IsOccupied => state == RoomState.Occupied;
    public bool IsClean => state == RoomState.Clean;
    public bool IsDirty => state == RoomState.Dirty;
    public bool IsBooked => state == RoomState.Booked;

    public void Clean()
    {
        state = RoomState.Clean;
        OnStateChange?.Invoke(state);
        OnRoomStateChange?.Invoke(this);
    }

    public void Book()
    {
        state = RoomState.Dirty;
        bookingTime = Random.Range(GameSettings.Instance.minBookingTime, GameSettings.Instance.maxBookingTime);

        OnStateChange?.Invoke(state);
        OnRoomStateChange?.Invoke(this);
    }

    public void CheckIn()
    {
        state = RoomState.Occupied;
        stayTime = Random.Range(GameSettings.Instance.minStayTime, GameSettings.Instance.maxStayTime);

        OnStateChange?.Invoke(state);
        OnRoomStateChange?.Invoke(this);
    }

    public void CheckOut()
    {
        state = RoomState.Dirty;
        requirements = Requirements.CreateRandom();
        OnRequirementsChange?.Invoke(requirements);

        OnStateChange?.Invoke(state);
        OnRoomStateChange?.Invoke(this);
    }

    public void ResourceEnter(ResourceType type, bool enter)
    {
        if (state == RoomState.Dirty)
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