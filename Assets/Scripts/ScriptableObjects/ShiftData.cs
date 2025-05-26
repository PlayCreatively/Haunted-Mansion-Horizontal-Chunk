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
        public const int MaxConsecutiveBookings = 4;

        //// GAME SETTINGS ////
        [Header("Shift Settings")]
        public float shiftDuration = 60 * 3;
        public int minBookings = 1, maxBookings = 10;
        //public int minResourcesPerBooking = 1, maxResourcesPerBooking = 3;
        const int addedResourcesPerShift = 2;
        [Range(0, 1)]
        public float maxAlpha = .5f;

        //// LOCAL VARIABLES ////
        int currentShift = 0;
        float currentTimeIntoShift = 0f;
        int[] bookingRequirementCountPerShift = new int[3];
        Room.Requirements[] currentShiftBookingRequirements;
        int nextBookingIndex = 0;
        float nextBookingTime = 0f;
        int currentShiftMaxConcurrentBookings = 0;

        void OnEnable()
        {
            ResetData();
        }

        ////PUBLIC API ////
        public int CurrentShift => currentShift;
        public float TimeIntoShift => currentTimeIntoShift;
        public float TimeIntoShiftAlpha => currentTimeIntoShift / shiftDuration;
        public int CurrentBookingCount => GetBookingCount(currentShift);
        public event Action<int> OnShiftEnd;

        //// PUBLIC API ////
        public float GetBookingTime(int bookingIndex) => (float)(GetRoomTimeAlpha(GetAlpha(currentShift), GetBookingCount(currentShift), bookingIndex) * shiftDuration);
        public float GetBookingTimeAlpha(int bookingIndex) => (float)GetRoomTimeAlpha(GetAlpha(currentShift), GetBookingCount(currentShift), bookingIndex);

        //// INTERNAL API ////

        /// <summary>
        /// A sequence of rooms and their requirements for the current shift.
        /// </summary>
        (Room, Room.Requirements)[] bookingShiftSequence;
        internal void StartNewShift()
        {
            currentTimeIntoShift = 0f;
            nextBookingIndex = 0;

            AddResourceAndBooking(ref bookingRequirementCountPerShift, CurrentShift);
            currentShiftBookingRequirements = GetRequirementsForShift(bookingRequirementCountPerShift);
            int bookingCount = currentShiftBookingRequirements.Length;
            { // debug
                int resourcesCount = 0;
                for (int i = 0; i < bookingRequirementCountPerShift.Length; i++)
                    resourcesCount += bookingRequirementCountPerShift[i] * (i+1);
                Debug.Log(bookingCount + $" bookings with {resourcesCount} resources: 1): {bookingRequirementCountPerShift[0]}, 2): {bookingRequirementCountPerShift[1]}, 3): {bookingRequirementCountPerShift[2]}");
            }
            var roomSequence = GetRandomSequenceOfUnlockedRooms(bookingCount);

            bookingShiftSequence = new (Room, Room.Requirements)[bookingCount];

            for (int i = 0; i < bookingCount; i++)
                bookingShiftSequence[i] = (roomSequence[i], currentShiftBookingRequirements[i]);

            // book the first rooms
            int initialBookingCount = Math.Min(currentShiftMaxConcurrentBookings, bookingCount);
            for (int i = 0; i < initialBookingCount; i++)
                ActivateBooking(i);
            nextBookingIndex = initialBookingCount;
            if(nextBookingIndex < bookingShiftSequence.Length)
                nextBookingTime = GetBookingTime(nextBookingIndex);
        }

        void ActivateBooking(int i)
        {
            var booking = bookingShiftSequence[i];
            booking.Item1.Book((float)GetBookingTime(i), booking.Item2);
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

                lastRooms.Enqueue(randomIndex);
                if (lastRooms.Count > nBack)
                    lastRooms.Dequeue();
            }

            return rooms;
        }

        internal void UpdateShiftTime(float deltaTime)
        {
            currentTimeIntoShift += deltaTime;
            if (currentTimeIntoShift >= shiftDuration || AreAllBookingsCompleted)
            {
                OnShiftEnd?.Invoke(currentShift);
                currentShift++;
                Game.ToMainMenu();
            }

            if (nextBookingIndex < bookingShiftSequence.Length && currentTimeIntoShift >= nextBookingTime)
            {
                ActivateBooking(nextBookingIndex);
                nextBookingIndex++;
                if (nextBookingIndex < bookingShiftSequence.Length)
                {
                    nextBookingTime = GetBookingTime(nextBookingIndex);
                }
                else // No more bookings
                {
                    nextBookingTime = float.MaxValue;
                }
            }
        }

        internal bool TryBookNextRoom()
        {
            bool canBook = nextBookingIndex < bookingShiftSequence.Length;

            if(canBook)
            {
                ActivateBooking(nextBookingIndex);
                nextBookingIndex++;
            }

            return canBook;
        }

        //IEnumerator BookNextRoomCoroutine()
        //{
        //    yield return new WaitForSeconds(TimeUntilCheckIn(nextBookingIndex));
        //    TryBookNextRoom();
        //}

        internal bool AreAllBookingsCompleted => nextBookingIndex >= bookingShiftSequence.Length && RoomManager.Instance.bookedRooms.Count < 1;

        internal void ResetData() // TODO: Add remaining fields
        {
            currentShift = 0;
            currentTimeIntoShift = 0f;
            nextBookingIndex = 0;
            bookingShiftSequence = null;
            currentShiftBookingRequirements = null;
            currentShiftMaxConcurrentBookings = 0;
            for (int i = 0; i < bookingRequirementCountPerShift.Length; i++)
                bookingRequirementCountPerShift[i] = 0;
        }

        //// HELPER FUNCTIONS ////

        // 1x 14% 14%
        // 2x 50% 25%
        // 3x 36% 12%

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
