using GameManagers;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using Random = UnityEngine.Random;

public enum RoomState
{
    Occupied,
    Booked,
    //Locked
}

public interface IRoom
{
    event Action<RoomState> OnStateChange;
    event Action<Room.Requirements> OnRequirementsChange;
    Transform Transform { get; }
    Vector2 UIOffset { get; }
    bool IsDirty { get; }
}
public class Room : MonoBehaviour, IRoom
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
    public RoomState state = RoomState.Occupied;
    public Requirements requirements;
    Requirements initialRequirements;
    public int unlockShift = 0;
    public bool IsLocked => unlockShift > ShiftData.Instance.CurrentShift;

    RoomUI roomUI;
    public Vector2 UIOffset;

    public event Action<RoomState> OnStateChange;
    public event Action<Requirements> OnRequirementsChange;
    Animator shineAnimator;
    LockableRoom lockableRoom;

    RoomManager RoomManager;

    void OnEnable()
    {
        RoomManager.rooms.Add(this);
    }

    void OnDisable()
    {
        RoomManager.rooms.Remove(this);
    }

    void Awake()
    {
        RoomManager = RoomManager.Instance;

        var roomUIObj = Resources.Load<GameObject>("RoomUI");
        shineAnimator = GetComponent<Animator>();
        lockableRoom = GetComponent<LockableRoom>();
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
        if (ShiftData.Instance.CurrentShift < unlockShift)
        {
            LockRoom();
            enabled = false;
            return;
        }
    }

    void LockRoom()
    {
        //throw new NotImplementedException();
    }

    void BookRoomIfNone()
    {
        if (RoomManager.GetRoomCountForState(RoomState.Booked) == 0) // rough balancing
        {
            RoomManager.rooms[Random.Range(0, RoomManager.rooms.Count)].Book();
        }
    }

    void Update()
    {
        if (state == RoomState.Booked)
        {
            BookedTime -= Time.deltaTime;
            if (BookedTime <= 0) CheckIn();
        }

    }

    public bool IsClean => !isDirty;
    public bool IsDirty => isDirty;
    public bool IsBooked => state == RoomState.Booked;
    public bool IsOccupied => state == RoomState.Occupied;

    public Transform Transform => transform;

    Vector2 IRoom.UIOffset => UIOffset;

    [ContextMenu("Clean")]
    public void Clean()
    {
        RoomManager.CleanedRoomsCount++;
        UrgencyState = 0;
        FMODAudioManager.Instance.TriggerRoomCleanedSfx();
        Debug.Log($"{gameObject.name} cleaned", gameObject);
        shineAnimator.Play("GlassAnimation", 0, 0f);

        isDirty = false;

        CheckIn();

        ShiftData.Instance.BookingShiftSequence[BookingID].done = true;
        ShiftData.Instance.RemoveBooking(BookingID);
        RoomManager.Instance.OnBookingCompleted?.Invoke(BookingID);
        _bookingID = -1; // reset booking ID after cleaning
    }

    int _bookingID = -1;
    public int BookingID => _bookingID;
    public void UpdateRoomOrderColors(int order) => roomUI.UpdateRoomOrderColors(order);

    public void Book() => Book(GetBookingTime(), Requirements.CreateRandom(), -1);
    public void Book(float time, Requirements requirements, int bookingID)
    {
        BookedTime = time;
        _bookingID = bookingID;

        state = RoomState.Booked;
        isDirty = true;
        this.requirements = requirements;
        initialRequirements = requirements;
        OnRequirementsChange?.Invoke(requirements);

        Debug.Log($"{gameObject.name} booked", gameObject);

        FMODAudioManager.Instance.TriggerRoomBookedSfx();
        lockableRoom.SetFogCeilingActive(false);
        OnStateChange?.Invoke(state);
        RoomManager.OnRoomStateChange?.Invoke(this);
    }

    public void CheckIn()
    {
        //Debug.Log($"{gameObject.name} checked in", gameObject);

        if (isDirty)
        {
            //Clean(); // REMOVE FOR PLAYTEST
            //return;
            roomUI.UpdateBookingTimeUI(0);
            Game.GameOver(this);
            enabled = false;
            return;
        }
        FMODAudioManager.Instance.TriggerRoomCheckInSfx();
        state = RoomState.Occupied;
        lockableRoom.SetFogCeilingActive(true);

        OnStateChange?.Invoke(state);
        RoomManager.OnRoomStateChange?.Invoke(this);
    }

    public void ResourceEnter(CarriableType type, bool enter)
    {
        if (state != RoomState.Occupied)
        {
            requirements[type] += enter ? -1 : 1;
            OnRequirementsChange?.Invoke(requirements);

            //Debug.Log($"{gameObject.name} {type} {(enter ? "added" : "removed")}. Remaining: {requirements[type]}", gameObject);

            if (requirements.IsFulfilled())
            {
                ShiftData.Instance.AddCleanedRoom(this, _bookedTime, requirements, type);

                //Debug.Log($"{gameObject.name} requirements fulfilled. Cleaned!", gameObject);
                Clean();
            }
            else if (enter)
            {
                ShiftData.Instance.AddResourceDelivered(this, type);

                FMODAudioManager.Instance.TriggerResourcePlacedInRoom();
            }
        }
    }

    public bool IsRequired(CarriableType type)
    {
        Debug.Log($"IsRequired: {type} {isDirty && requirements.IsRequired(type)}", gameObject);
        return isDirty && requirements.IsRequired(type);
    }

    public float GetBookingTime()
    {
        int count = RoomManager.GetRoomCountForState(RoomState.Booked);
        const int maxFinishedRooms = 20;

        float alpha = (float)RoomManager.CleanedRoomsCount / maxFinishedRooms;
        float roomTimeBuffer = Lerp(GameSettings.Instance.PostBookedTime, GameSettings.Instance.PostBookedTime * .75f, alpha);
        return (MathF.Log(count, 3) + 1) * roomTimeBuffer;
    }

    float Lerp(float a, float b, float t) => a + (b - a) * t;

    public struct Requirements : IEnumerable<(CarriableType type, int count)>
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

            Requirements requirements = new(0, 0, 0, 0);

            int resourceCount = Random.Range(minAmount, maxAmount + 1);

            for (int i = 0; i < resourceCount; i++)
            {
                int resourceType = Random.Range(0, resourceTypeCount);
                requirements[resourceType]++;
            }

            return requirements;
        }
        public static Requirements CreateRandom(int count)
        {
            Requirements requirements = new(0, 0, 0, 0);

            for (int i = 0; i < count; i++)
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

        public readonly IEnumerator<(CarriableType type, int count)> GetEnumerator()
        {
            for (int i = 0; i < resourceRequirement.Length; i++)
                if (resourceRequirement[i] > 0)
                    yield return ((CarriableType)i, resourceRequirement[i]);
        }

        readonly IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
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