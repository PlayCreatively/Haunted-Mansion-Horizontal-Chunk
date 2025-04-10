using System;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody)), SelectionBase]
public class Player : MonoBehaviour
{
    PlayerInput playerInput;
    Rigidbody rb;
    InteractiveHand hand;

    Vector2 moveInput;
    bool grounded;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        hand = GetComponentInChildren<InteractiveHand>();
        playerInput = GetComponent<PlayerInput>();
    }

    void Start()
    {
        playerInput.SwitchCurrentControlScheme(
            playerInput.defaultControlScheme,
            Keyboard.current, Gamepad.current
        );

        playerInput.actions["Move"].performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        playerInput.actions["Move"].canceled += _ => moveInput = Vector2.zero;

        playerInput.actions["Interact"].performed += _ => hand.Interact();
        playerInput.actions["Drop"].performed += Drop;
    }

    private void Drop(InputAction.CallbackContext obj)
    {
        if (hand.IsEmpty())
            return;

        hand.RemoveItem(hand.Item);
    }

    void Jump()
    {
        var velocity = rb.linearVelocity;
        velocity.y = GameSettings.Instance.playerJumpForce;
        rb.linearVelocity = velocity;
        grounded = false;
    }

    void FixedUpdate()
    {
        if (grounded && playerInput.actions.FindAction("Jump").IsPressed())
            Jump();

        Vector3 deltaMove = Quaternion.AngleAxis(45, Vector3.up) * new Vector3(moveInput.x, 0, moveInput.y) * GameSettings.Instance.playerSpeed;
        deltaMove.y = rb.linearVelocity.y;
        rb.linearVelocity = deltaMove;
        //rb.rotation = Quaternion.LookRotation(new Vector3(input.x, 0, input.y));
    }

    void OnCollisionStay(Collision collision)
    {
        grounded = false;
        foreach (var contact in collision.contacts)
        {
            grounded |= contact.normal.y > 0 && Physics.Raycast(transform.position, Vector3.down, 1);
        }
    }
}
