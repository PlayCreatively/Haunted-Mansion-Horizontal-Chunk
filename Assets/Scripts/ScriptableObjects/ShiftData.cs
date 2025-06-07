using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GameManagers
{
    [DefaultExecutionOrder(ExecutionOrder.Singleton-1)]
    [CreateAssetMenu(fileName = nameof(ShiftData), menuName = "ScriptableObjects/" + nameof(ShiftData), order = 1)]
    public class ShiftData : ScriptableObject
    {
        public const int MaxConsecutiveBookings = 3;

        //// GAME SETTINGS ////
        [Header("Shift Settings")]
        public float shiftDuration = 60 * 4;
        public float maxTimeForResource = 60f, minTimeForResource = 30f;
        public int minBookings = 1, maxBookings = 7;
        //public int minResourcesPerBooking = 1, maxResourcesPerBooking = 3;
        const int addedResourcesPerShift = 2;
        [Range(0, 1)]
        public float maxAlpha = .5f;
        [Header("Score Settings")]
        public int scorePerCleanedRoom = 10;
        public int scorePerResource = 5;

        //// LOCAL VARIABLES ////
        int currentShift = 0;
        float currentTimeIntoShift = 0f;
        int[] bookingRequirementCountPerShift = new int[3];
        Room.Requirements[] currentShiftBookingRequirements;
        int currentShiftMaxConcurrentBookings = 0;

        //// SCORING DATA - SHIFT ////
        int shiftScore;
        int roomsCleanedCount = 0;
        //// SCORING DATA - GLOBAL ////
        public PlayerScoreData playerScoreData = new ();

        void OnEnable()
        {
            ResetData();
        }

        ////PUBLIC API ////
        public (Room room, Room.Requirements requirements, bool done)[] BookingShiftSequence => bookingShiftSequence;
        public int CurrentShift => currentShift;
        public float TimeIntoShift => currentTimeIntoShift;
        public float TimeIntoShiftAlpha => currentTimeIntoShift / shiftDuration;
        public int CurrentBookingCount => GetBookingCount(currentShift);
        public event Action<int> OnShiftEnd;
        public int ShiftScore => shiftScore;
        public int CalculateTimeBonus(float timeLeft) => Mathf.RoundToInt(timeLeft * .1f);
        public int CalculatePerResourceBonus(CarriableType resourceType) => resourceType switch
        {
            CarriableType.ToiletPaper => scorePerResource,
            _ => scorePerResource * 2, // Towel, BedSheet, Soap
        };
        public int CalculateCumulativeRequirementsBonus(Room.Requirements requirements)
        {
            int resourcesValueCount = 0;
            foreach (var (type, count) in requirements)
                if (type == CarriableType.ToiletPaper)
                    resourcesValueCount += count;
                else
                    resourcesValueCount += count * 2; // Towel, BedSheet, Soap

            return Mathf.RoundToInt(Mathf.Pow(resourcesValueCount, 1.4f)) * 5;

            float r = resourcesValueCount;
            const float a = 3.33333f, b = -17.5f, c = 44.16667f, d = -20f;
            return Mathf.RoundToInt(a * r * r * r + b * r * r + c * r + d);
        }

        //// PUBLIC API ////
        public void AddCleanedRoom(Room room, float timeLeft, Room.Requirements requirements, CarriableType lastResource)
        {
            roomsCleanedCount++;

            int deltaScore = 0;
            deltaScore += CalculateTimeBonus(timeLeft);
            deltaScore += CalculateCumulativeRequirementsBonus(requirements);
            deltaScore += CalculatePerResourceBonus(lastResource);

            shiftScore += deltaScore;
            playerScoreData.score += deltaScore;

            ScoreBubbleUI.SpawnScore(deltaScore, room.transform.position + room.UIOffset.XZ());
        }

        public void AddResourceDelivered(Room room, CarriableType resourceType)
        {
            int deltaScore = CalculatePerResourceBonus(resourceType);
            shiftScore += deltaScore;
            playerScoreData.score += deltaScore;

            ScoreBubbleUI.SpawnScore(deltaScore, room.transform.position + room.UIOffset.XZ());
        }

        public float GetBookingTime(int bookingIndex) => (float)(GetRoomTimeAlpha(GetAlpha(currentShift), GetBookingCount(currentShift), bookingIndex) * shiftDuration);
        public float GetBookingTimeAlpha(int bookingIndex) => (float)GetRoomTimeAlpha(GetAlpha(currentShift), GetBookingCount(currentShift), bookingIndex);

        //// INTERNAL API ////

        /// <summary>
        /// A sequence of rooms and their requirements for the current shift.
        /// </summary>
        (Room room, Room.Requirements requirements, bool done)[] bookingShiftSequence;
        internal void StartNewShift()
        {
            shiftScore = 0;
            shiftEnded = false;

            currentTimeIntoShift = 0f;

            AddResourceAndBooking(ref bookingRequirementCountPerShift, CurrentShift);
            currentShiftBookingRequirements = GetRequirementsForShift(bookingRequirementCountPerShift);
            int bookingCount = currentShiftBookingRequirements.Length;
            {
                static float Lerp(float a, float b, float t) => a + (b - a) * t;

                int resourcesCount = 0;
                for (int i = 0; i < bookingRequirementCountPerShift.Length; i++)
                    resourcesCount += bookingRequirementCountPerShift[i] * (i+1);
                Debug.Log(bookingCount + $" bookings with {resourcesCount} resources: 1): {bookingRequirementCountPerShift[0]}, 2): {bookingRequirementCountPerShift[1]}, 3): {bookingRequirementCountPerShift[2]}");

                float curTimeForResource = Lerp(maxTimeForResource, minTimeForResource, (float)currentShift / (maxBookings - minBookings));
                shiftDuration = curTimeForResource * resourcesCount;
            }

            var roomSequence = GetRandomSequenceOfUnlockedRooms(bookingCount);

            bookingShiftSequence = new (Room room, Room.Requirements requirements, bool done)[bookingCount];
            bookingQueue = new List<int>(bookingCount);

            for (int i = 0; i < bookingCount; i++)
            {
                bookingShiftSequence[i] = (roomSequence[i], currentShiftBookingRequirements[i], false);
                bookingQueue.Add(i); // Initialize booking queue with indices
            }

            // book the first rooms
            int initialBookingCount = Math.Min(currentShiftMaxConcurrentBookings, bookingCount);
            for (int i = 0; i < initialBookingCount; i++)
                ActivateBooking(i);

            UpdateRoomOrderColors();
        }

        void ActivateBooking(int i)
        {
            var (room, requirements, _) = bookingShiftSequence[i];
            Debug.Assert(room.IsBooked == false, "Room is already booked: " + room.name);
            room.Book((float)GetBookingTime(i), requirements, i);
        }

        /// <summary>
        /// Returns a sequence of randomly selected unlocked rooms, ensuring that the same room does not appear again from the last min(maxConsecutiveRoomCount, unlockedRoomsCount - 1) bookings.
        /// </summary>
        public Room[] GetRandomSequenceOfUnlockedRooms(int count)
        {
            var allUnlockedRooms = RoomManager.Instance.GetAllUnlockedRooms();
            int nBack = Math.Min(MaxConsecutiveBookings, allUnlockedRooms.Length - 1);
            currentShiftMaxConcurrentBookings = Math.Min(MaxConsecutiveBookings, allUnlockedRooms.Length);

            Room[] rooms = new Room[count];
            Debug.Assert(allUnlockedRooms.Length > 1, "Not enough unlocked rooms to select from.");

            string stringOfRoomIds = "";

            Queue<int> lastRooms = new(nBack);
            while (count > 0)
            {
                // Get a random index, ensuring it is not the same as the last nBack rooms
                HashSet<int> availableRooms = new(allUnlockedRooms.Length);
                for (int i = 0; i < allUnlockedRooms.Length; i++)
                    availableRooms.Add(i);

                foreach (int room in lastRooms)
                    availableRooms.Remove(room);

                Debug.Assert(availableRooms.Count > 0, "No available rooms left to select from.");

                int randomIndex = availableRooms.ElementAt(UnityEngine.Random.Range(0, availableRooms.Count));

                rooms[--count] = allUnlockedRooms[randomIndex];
                stringOfRoomIds += randomIndex + ", ";

                lastRooms.Enqueue(randomIndex);
                if (lastRooms.Count > nBack)
                    lastRooms.Dequeue();
            }

            Debug.Log($"Selected rooms for shift {currentShift}: {stringOfRoomIds.TrimEnd(',', ' ')}");
            Debug.Log($"Selected rooms for shift {currentShift}: {string.Join("\n ", rooms.Select(r => r.name + r.transform.position))}");
            Debug.Log($"All rooms : {string.Join("\n", allUnlockedRooms.Select(r => r.name + r.transform.position))}");
            return rooms;
        }

        bool shiftEnded = false;
        internal void UpdateShiftTime(float deltaTime)
        {
            if(shiftEnded || bookingShiftSequence == null)
                return;

            currentTimeIntoShift += deltaTime;
            if (AreAllBookingsCompleted)
            {
                shiftEnded = true;
                OnShiftEnd?.Invoke(currentShift);
                currentShift++;
                Game.ToNightShift(3);
            }

            int nextBookingIndex() => bookingShiftSequence.FirstIndex(b => !b.done && !b.room.IsBooked);
            int bookedRoomsCount () => bookingShiftSequence.Where(b => !b.done && b.room.IsBooked).Count();
            if (bookedRoomsCount() < MaxConsecutiveBookings)
            {
                int nextBooking = nextBookingIndex();
                if (nextBooking != -1)
                    ActivateBooking(nextBooking);
            }
        }

        List<int> bookingQueue;

        internal void RemoveBooking(int bookingID)
        {
            bookingQueue.Remove(bookingID);

            UpdateRoomOrderColors();
        }

        void UpdateRoomOrderColors()
        {
            for (int i = 0; i < bookingQueue.Count && i < MaxConsecutiveBookings; i++)
                BookingShiftSequence[bookingQueue[i]].room.UpdateRoomOrderColors(i);
        }

        internal bool AreAllBookingsCompleted => bookingQueue.Count() == 0;
        internal bool AreAnyBookingsNotRunning => bookingShiftSequence.Any(b => !b.done && !b.room.IsBooked);

        internal void ResetData() // TODO: Add remaining fields
        {
            currentShift = 0;
            currentTimeIntoShift = 0f;
            bookingShiftSequence = null;
            currentShiftBookingRequirements = null;
            currentShiftMaxConcurrentBookings = 0;
            for (int i = 0; i < bookingRequirementCountPerShift.Length; i++)
                bookingRequirementCountPerShift[i] = 0;
            playerScoreData = new();

            shiftScore = 0;
            roomsCleanedCount = 0;
            playerScoreData.score = 0;
        }

        Room.Requirements[] GetRequirementsForShift(int[] bookingRequirementCountPerShift)
        {
            int[] copy = new int[bookingRequirementCountPerShift.Length];
            Array.Copy(bookingRequirementCountPerShift, copy, bookingRequirementCountPerShift.Length);
            int bookingCount = 0;
            for (int i = 0; i < copy.Length; i++)
                bookingCount += copy[i];

            Room.Requirements[] requirementsList = new Room.Requirements[bookingCount];

            for (int i = 0; i < bookingCount; i++)
            {
                int randomIndex;
                do  randomIndex = UnityEngine.Random.Range(0, copy.Length);
                while (copy[randomIndex] <= 0);

                copy[randomIndex]--;

                requirementsList[i] = Room.Requirements.CreateRandom(randomIndex + 1);
            }

            for (int i = 0; i < copy.Length; i++)
                Debug.Assert(copy[i] == 0, i + " requirements left: " + copy[i]);

            return requirementsList;
        }

        void AddResourceAndBooking(ref int[] bookingRequirementCountPerShift, int shiftIndex)
        {
            for (int i = 0; i < bookingRequirementCountPerShift.Length; i++)
                bookingRequirementCountPerShift[i] = 0;
            
            int shiftI = 0;

            while (shiftI <= shiftIndex)
            {
                if (shiftI < maxBookings - minBookings)
                    bookingRequirementCountPerShift[0] += 1;

                if(bookingRequirementCountPerShift[0] > 0 && (shiftI & 1) == 0)
                {
                    bookingRequirementCountPerShift[0]--;
                    bookingRequirementCountPerShift[1]++;
                }
                else if(bookingRequirementCountPerShift[1] > 0 && (shiftI & 1) == 1)
                {
                    bookingRequirementCountPerShift[1]--;
                    bookingRequirementCountPerShift[2]++;
                }
                Debug.Log($"({bookingRequirementCountPerShift[0]}, {bookingRequirementCountPerShift[1]}, {bookingRequirementCountPerShift[2]}) for shift {shiftIndex + 1}");
                shiftI++;
            }
        }
        
        internal int GetBookingCount(int shiftIndex)
        {
            return Math.Min(minBookings + shiftIndex, maxBookings);
        }

        internal double GetAlpha(int shiftIndex)
        {
            return (double)shiftIndex / (maxBookings - minBookings) * maxAlpha;
        }

        internal double GetRoomTimeAlpha(double a, int bookingCount, int bookingIndex)
        {
            Debug.Assert(bookingIndex >= 0, "n must be greater than or equal to 0");
            Debug.Assert(a >= 0 && a <= 1, "a must be between 0 and 1");
            Debug.Assert(bookingCount > 0, "c must be greater than 0");
            Debug.Assert(bookingIndex < bookingCount, "roomI must be less than roomCount");

            bookingIndex++;

            if (a == 0) return (double)bookingIndex / bookingCount;           // linear
            if (a == 1) return Math.Sqrt((double)bookingIndex / bookingCount); // square-root
            return (-(1 - a) + Math.Sqrt((1 - a) * (1 - a) + 4 * a * bookingIndex / (double)bookingCount))
                   / (2 * a);
        }

        static ShiftData _instance;
        public static ShiftData Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = Resources.Load<ShiftData>(nameof(ShiftData));
                }
                return _instance;
            }
        }

    #if UNITY_EDITOR
        [UnityEditor.MenuItem("Game/Settings/" + nameof(ShiftData))]
        public static void CreateAndShow()
        {
            if (!Instance)
            {
                _instance = CreateInstance<ShiftData>();
                UnityEditor.AssetDatabase.CreateAsset(Instance, $"Assets/Resources/{nameof(ShiftData)}.asset");
                UnityEditor.AssetDatabase.SaveAssets();
                UnityEditor.AssetDatabase.Refresh();
            }
            // open properties window
            UnityEditor.EditorUtility.OpenPropertyEditor(_instance);
        }
#endif
    }

    #if UNITY_EDITOR


    [UnityEditor.CustomEditor(typeof(ShiftData))]
    public class ShiftDataEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            ShiftData shiftData = (ShiftData)target;

            // times between rooms
            double longestTime, shortestTime;
            longestTime = shiftData.shiftDuration / shiftData.minBookings;
            double GetToughestRoomTime(int i) => shiftData.GetRoomTimeAlpha(shiftData.maxAlpha, shiftData.maxBookings, i);
            shortestTime = GetToughestRoomTime(shiftData.maxBookings - 1) - GetToughestRoomTime(shiftData.maxBookings - 2);
            shortestTime *= shiftData.shiftDuration;
            GUILayout.Label($"Time between rooms: \n\tLongest  in shift 0:\t\t{longestTime}s \n\tShortest in shift <={shiftData.maxBookings - shiftData.minBookings}:\t{string.Format("{0:0.00}", shortestTime)}s", new GUIStyle { fontSize = 16, fontStyle = FontStyle.Bold, normal = { textColor = Color.white } });
        }
    }


    #endif
}

public static class LeaderBoardManager
{
    public static List<PlayerScoreData> leaderBoardDatas = new(10);
    public const string LeaderBoardFile = "LeaderBoardData.json";
    public static string LeaderBoardFilePath => Application.persistentDataPath + "/" + LeaderBoardFile;

    public static void AddLeaderBoardData(PlayerScoreData data)
    {
        if(leaderBoardDatas.Contains(data))
        {
            Debug.LogWarning("LeaderBoardData already exists: " + data);
            return;
        }

        leaderBoardDatas.Add(data);
        leaderBoardDatas = leaderBoardDatas.OrderByDescending(d => d.score).ToList();

        SaveLeaderBoardData();
    }

    public static void SaveLeaderBoardData()
    {
        if (leaderBoardDatas.Count == 0)
        {
            Debug.LogWarning("No LeaderBoardData to save.");
            return;
        }
        string json = JsonUtility.ToJson(new LeaderBoardList(leaderBoardDatas.Take(10).ToArray()), true);
        Debug.Log(json);
        System.IO.File.WriteAllText(LeaderBoardFilePath, json);
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
    public static void LoadLeaderBoardData()
    {
        if (!System.IO.File.Exists(LeaderBoardFilePath))
        {
            Debug.LogWarning("LeaderBoardData file not found: " + LeaderBoardFilePath);
            return;
        }
        string json = System.IO.File.ReadAllText(LeaderBoardFilePath);
        var data = JsonUtility.FromJson<LeaderBoardList>(json);
        leaderBoardDatas = new List<PlayerScoreData>(data.leaderBoardDatas);
        leaderBoardDatas = leaderBoardDatas.OrderByDescending(d => d.score).ToList();
        Debug.Log("LeaderBoardData loaded from " + LeaderBoardFilePath);
    }

    public static int GetLeadingUpPlaceInLeaderBoard(int score)
    {
        Debug.Assert(leaderBoardDatas.Count != 0);

        for (int i = leaderBoardDatas.Count - 1; i >= 0; i--)
        {
            if (leaderBoardDatas[i].score > score)
                return i + 1; // +1 because places are 1-indexed
        }

        if (leaderBoardDatas[0].score < score)
            return 1;
        else
            return leaderBoardDatas.Count + 1; // If score is lower than all, return last place
    }

    public static PlayerScoreData GetLeadingUpPlayer(int totalScore)
    {
        return leaderBoardDatas.First(data => data.score < totalScore);
    }
}

[Serializable]
public struct LeaderBoardList
{
    public PlayerScoreData[] leaderBoardDatas;
    public LeaderBoardList(PlayerScoreData[] leaderBoardDatas)
    {
        this.leaderBoardDatas = leaderBoardDatas;
    }
    public LeaderBoardList(ICollection<PlayerScoreData> leaderBoardDatas)
    {
        this.leaderBoardDatas = leaderBoardDatas.ToArray();
    }
    public static implicit operator PlayerScoreData[](LeaderBoardList list) => list.leaderBoardDatas;
}

[Serializable]
public record PlayerScoreData
{
    public string name;
    public int skinID;
    public int score;
    public int shift;
    public int roomsCleaned;
    public int MVP;

    public void SaveToFile(string filePath)
    {
        System.IO.File.WriteAllText(filePath, JsonUtility.ToJson(this, true));
    }

    public static PlayerScoreData LoadFromFile(string filePath)
    {
        if (!System.IO.File.Exists(filePath))
        {
            Debug.LogError($"File not found: {filePath}");
            return default;
        }
        string json = System.IO.File.ReadAllText(filePath);
        return JsonUtility.FromJson<PlayerScoreData>(json);
    }
}