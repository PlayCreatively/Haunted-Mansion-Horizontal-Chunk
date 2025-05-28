using GameManagers;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[DefaultExecutionOrder(-100)]
public class RoomManager : MonoBehaviour
{
    static RoomManager _instance;
    public static RoomManager Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject obj = new ("RoomManager");
                _instance = obj.AddComponent<RoomManager>();
            }
            return _instance;
        }
    }

    void OnDestroy()
    {
        _instance = null;
    }

    public readonly List<Room> rooms = new(8);
    public readonly Queue<Room> bookedRooms = new(4);
    [HideInInspector]
    public int CleanedRoomsCount = 0;
    public Action<Room> OnRoomStateChange;
    public Action<int> OnBookingCompleted;

    public Room[] GetAllUnlockedRooms()
    {
        int currentShift = ShiftData.Instance.CurrentShift;
        return rooms.Where(r => r.unlockShift <= currentShift).ToArray();
    }

    public int GetRoomCountForState(RoomState state)
    {
        return rooms.Where(r => r.state == state).Count();
    }
    public Room GetClosestToPlayers()
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
}