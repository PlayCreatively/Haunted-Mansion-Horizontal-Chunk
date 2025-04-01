using System;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class Player : MonoBehaviour
{
    // new input system
    InputSystem_Actions inputActions;

    Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        inputActions = new InputSystem_Actions();
        inputActions.Enable();
        inputActions.Player.Jump.performed += Jump;
    }

    void Jump(InputAction.CallbackContext context)
    {
        Debug.Log("Jump");
        rb.linearVelocity = new Vector3(0, GameSettings.Instance.playerJumpForce, 0);
    }

    void FixedUpdate()
    {
        Vector2 input = inputActions.Player.Move.ReadValue<Vector2>();
        Vector3 deltaMove = Quaternion.AngleAxis(45, Vector3.up) * new Vector3(input.x, 0, input.y) * GameSettings.Instance.playerSpeed;
        rb.linearVelocity = deltaMove;
        //rb.rotation = Quaternion.LookRotation(new Vector3(input.x, 0, input.y));
    }
}
