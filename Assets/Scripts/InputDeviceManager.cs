using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputDeviceManager : MonoBehaviour
{

    private void Awake()
    {
        InputSystem.onDeviceChange += OnDeviceChange;


    }

    private void OnDeviceChange(InputDevice device, InputDeviceChange change)
    {
        if (change == InputDeviceChange.Added)
        {
            Debug.Log($"Device added: {device}");
        }
        else if (change == InputDeviceChange.Removed)
        {
            Debug.Log($"Device removed: {device}");
        }
        else if (change == InputDeviceChange.Disconnected)
        {
            Debug.Log($"Device disconnected: {device}");
        }
        else if (change == InputDeviceChange.Reconnected)
        {
            Debug.Log($"Device reconnected: {device}");
        }
    }
}
