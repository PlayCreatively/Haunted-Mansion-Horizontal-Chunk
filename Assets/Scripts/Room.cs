using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using Random = UnityEngine.Random;

public enum RoomState
{
    PreBooked,
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
            //if(UrgencyState < 2 && _bookedTime < 15f)
            //    UrgencyState = 2;
            //else if(UrgencyState < 1 && _bookedTime < 30f)
            //    UrgencyState = 1;
            
            roomUI.UpdateBookingTimeUI(_bookedTime);
        }
    }
    float preBookedTime = 0f;
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
    public Requirements requirements;

    RoomUI roomUI;
    public Vector2 UIOffset;

    protected readonly static List<Room> rooms = new(8);
    public readonly static Queue<Room> bookedRooms = new(4);
    public static int CleanedRoomsCount = 0;
    public Action<RoomState> OnStateChange;
    public Action<Requirements> OnRequirementsChange;
    public static Action<Room> OnRoomStateChange;
    Animator shineAnimator;

    void OnEnable() => rooms.Add(this);
    void OnDisable() => rooms.Remove(this);

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
        roomUI.transform.parent.gameObject.SetActive(false);

        if (GameSettings.Instance.RoomCleaning == false)
            enabled = false;
    }

    void Start()
    {
        CheckIn();

        if (GetRoomCountForState(RoomState.PreBooked) == 0) // rough balancing
        {
            GetClosestToPlayers().CheckOut();
        }

    }

    void BookRoomIfNone()
    {
        if (GetRoomCountForState(RoomState.PreBooked) == 0) // rough balancing
        {
            rooms[Random.Range(0, rooms.Count)].CheckOut();
        }
    }

    void Update()
    {
        switch (state)
        {
            case RoomState.PreBooked:
                preBookedTime -= Time.deltaTime;
                if(preBookedTime <= 0) Book(); break;
            case RoomState.Booked:
                BookedTime -= Time.deltaTime;
                if (BookedTime <= 0) CheckIn(); break;
            case RoomState.Occupied:
                stayTime -= Time.deltaTime;
                if (stayTime <= 0) CheckOut(); break;
        }
    }

    public float StayTime => stayTime;
    public float NonBookedTime => preBookedTime;
    public float Bookedtime => _bookedTime;
    public static List<Room> Rooms => rooms;

    public bool IsClean => !isDirty;
    public bool IsDirty => isDirty;
    public bool IsBooked => state == RoomState.Booked;
    public bool IsOccupied => state == RoomState.Occupied;

    [ContextMenu("Clean")]
    public void Clean()
    {
        CleanedRoomsCount++;
        UrgencyState = 0;
        FMODAudioManager.Instance.TriggerRoomCleanedSfx();
        Debug.Log($"{gameObject.name} cleaned", gameObject);
        shineAnimator.Play("GlassAnimation", 0, 0f);

        isDirty = false;
        CheckIn();
        //BookRoomIfNone();
    }

    public void Book()
    {
        bookedRooms.Enqueue(this);
        UpdateArrowColors();
        Debug.Log($"{gameObject.name} booked", gameObject);
        state = RoomState.Booked;
        BookedTime = GetPostBookedTime();
        if (GetRoomCountForState(RoomState.PreBooked) == 0) // THIS NEEDS TO BE REPLACED BEFORE PLAYTEST
        {
            BookedTime += 40f;
        }

        FMODAudioManager.Instance.TriggerRoomBookedSfx();
        OnStateChange?.Invoke(state);
        OnRoomStateChange?.Invoke(this);
    }

    public void CheckIn()
    {
        if (bookedRooms.Contains(this))
            bookedRooms.Dequeue();

        UpdateArrowColors();
        UrgencyState = 0;

        Debug.Log($"{gameObject.name} checked in", gameObject);

        if(isDirty)
        {
            //Clean(); // REMOVE FOR PLAYTEST
            //return;
            roomUI.UpdateBookingTimeUI(0);
            Game.GameOver(this);
            enabled = false;
            return;
        }

        state = RoomState.Occupied;
        stayTime = GameSettings.Instance.GetRandomStayTime;

        OnStateChange?.Invoke(state);
        OnRoomStateChange?.Invoke(this);
    }

    [ContextMenu("Check Out")]
    public void CheckOut()
    {
        if(GetRoomCountForState(RoomState.Booked) >= 4)
        {
            CheckIn(); return;
        }

        Debug.Log($"{gameObject.name} checked out", gameObject);
        state = RoomState.PreBooked;
        isDirty = true;
        preBookedTime = GameSettings.Instance.GetRandomPreBookedTime;
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

    public bool IsRequired(CarriableType type)
    {
        Debug.Log($"IsRequired: {type} {isDirty && requirements.IsRequired(type)}", gameObject);
        return isDirty && requirements.IsRequired(type);
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

    public float GetPostBookedTime()
    {
        int count = GetRoomCountForState(RoomState.Booked);
        const int maxFinishedRooms = 20;

        float alpha = (float)CleanedRoomsCount / maxFinishedRooms;
        float roomTimeBuffer = Lerp(GameSettings.Instance.PostBookedTime, GameSettings.Instance.PostBookedTime * .75f, alpha);
        return (MathF.Log(count, 3) + 1) * roomTimeBuffer;
    }

    float Lerp(float a, float b, float t)
    {
        return a + (b - a) * t;
    }

    void UpdateArrowColors()
    {
        Color[] colors = { new (214f/255, 105f/255, 107f/255), new (214f/255, 153f / 255, 105f/255), new (218f/255, 213f/255, 202f/255) };

        int i = 0;
        foreach (var room in bookedRooms)
        {
            room.roomUI.UpdateRoomOrderColors(colors[i]);
            i = Mathf.Min(i + 1, 2);
        }
    }

    public static int GetRoomCountForState(RoomState state)
    {
        return Room.Rooms.Where(r => r.state == state).Count();
    }
    public static Room GetClosestToPlayers()
    {
        var players = GameObject.FindGameObjectsWithTag("Player");
        Vector3 center = Vector3.zero;
        foreach (var player in players)
            center += player.transform.position;
        center /= players.Length;

        float closestDistance = Mathf.Infinity;
        Room closestRoom = null;
        foreach (var room in rooms)
        {
            float distance = Vector3.Distance(room.transform.position, center);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestRoom = room;
            }
        }

        return closestRoom;
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