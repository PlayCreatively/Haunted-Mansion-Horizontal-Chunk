using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class Elevator : MonoBehaviour
{
    const float size = 2f; // Size of the elevator platform
    readonly Bounds platformBounds = new(Vector3.up * (size / 2), new(size, size, size)); // Extents of the elevator platform

    public float speed = 1.0f; // Speed of the sine wave
    public int floorCount = 3;

    public Transform platform;

    float baseline = 0.0f; // Baseline position
    float openWindow = 0.2f; // Distance window for opening the elevator door
    ElevatorDoor[] doors;
    float currentY = 0.0f; // Current Y position of the elevator
    int targetFloor = 0; // Target floor for the elevator
    int moveDir = 1;
    State state;

    readonly Queue<int> floorQueue = new(1); // Queue of floors to visit

    enum State
    {
        Idle,
        Moving
    }

    float GetY(float x) => x - Mathf.Sin(2 * Mathf.PI * x) / (Mathf.PI * 2);

    bool IsPlayerInElevator()
    {
        var colliders = new Collider[1];

        return Physics.OverlapBoxNonAlloc(
            platform.position + platformBounds.center,
            platformBounds.extents,
            colliders,
            Quaternion.identity,
            LayerMask.GetMask("Player")
        ) > 0;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(platform.position + platformBounds.center, platformBounds.size);
    }

    void Awake()
    {
        // Set the baseline to the current position
        baseline = platform.position.y;
        // Find all ElevatorDoor components in the children
        doors = GetComponentsInChildren<ElevatorDoor>();

        UpdateOpenDoors();
    }

    void FixedUpdate()
    {
        if (!GameSettings.Instance.smartElevator)
        {
            SetY(Mathf.PingPong(Time.time * speed, floorCount - 1));
            UpdateOpenDoors();
        }
        else
            switch (state)
            {
                case State.Idle:
                    if (IsPlayerInElevator())
                        state = State.Moving;
                    break;
                case State.Moving:
                    Assert.IsTrue(targetFloor != -1);
                    Assert.IsTrue(moveDir != 0);
                    UpdateFloorTraversal();

                    if ((currentY - (int)currentY) == 0)
                    {
                        ReachedFloor();
                    }
                    break;
            }
    }

    void SetY(float y) => platform.position = new Vector3(
        platform.position.x,
        baseline + GetY(y),
        platform.position.z
    );


    void UpdateFloorTraversal()
    {
        float moveDelta = MathF.Min(Mathf.Abs(currentY - targetFloor), Time.fixedDeltaTime * speed);
        currentY += moveDelta * moveDir;

        SetY(currentY);

        UpdateOpenDoors();
    }

    void UpdateOpenDoors()
    {
        for (int i = 0; i < doors.Length; i++)
        {
            doors[i].Open(IsWithinWindow(i));
        }
    }

    void ReachedFloor()
    {
        //Debug.Log("Reached floor " + targetFloor);
        if (floorQueue.Count > 0)
        {
            Debug.Log("Next floor in queue: " + floorQueue.Peek());
            targetFloor = floorQueue.Dequeue();
            moveDir = Math.Sign(targetFloor - currentY);
        }
        else
        {
            state = State.Idle;
            if(currentY + moveDir < 0 || currentY + moveDir >= floorCount)
            {
                moveDir = -moveDir;
            }

            targetFloor += moveDir;

            if(IsPlayerInElevator())
            {
                //Debug.Log("Player in elevator, moving to floor " + targetFloor);
                state = State.Moving;
            }
        }


        // TODO: ding sound
    }

    bool IsWithinWindow(float x)
    {
        float v = x - (platform.position.y - baseline);
        return Mathf.Abs(v) < openWindow;
    }

    public void CallElevator(int floor)
    {
        if (floor == currentY || (state == State.Moving && floor == targetFloor))
            return;

        //Debug.Log("calling elevator to floor " + floor);

        if (state == State.Idle)
        {
            targetFloor = floor;
            moveDir = (int)Mathf.Sign(floor - currentY);
            state = State.Moving;
        }
        else
        {
            // Queue floor
            floorQueue.Enqueue(floor);
        }
    }
}
