using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody)), SelectionBase]
public class Player : MonoBehaviour
{
    PlayerInput playerInput;
    Rigidbody rb;
    InteractiveHand hand;
    Transform visuals;
    ParticleSystem[] dashParticles;
    ParticleSystem walkParticles;


    Vector3 moveInput;
    bool grounded;
    float dashValue = 0.5f;
    float jumpSquash = 0.5f;

void Awake()
    {
        rb = GetComponent<Rigidbody>();
        hand = GetComponentInChildren<InteractiveHand>();
        playerInput = GetComponent<PlayerInput>();
        visuals = transform.Find("Visuals");
        Assert.IsNotNull(visuals, $"child named Visuals missing in {name}");
        walkParticles = visuals.GetChild(0).GetComponentInChildren<ParticleSystem>();
        dashParticles = visuals.GetChild(1).GetComponentsInChildren<ParticleSystem>();
    }

    void Start()
    {
        List<InputDevice> devices = new(2);
        if(Keyboard.current != null)
            devices.Add(Keyboard.current);
        if (Gamepad.current != null)
        {
            devices.Add(Gamepad.current);
            Debug.Log($"Gamepad found: {Gamepad.current}");
        }

        playerInput.SwitchCurrentControlScheme(
            playerInput.defaultControlScheme,
            devices.ToArray() // Use the devices array
        );

        playerInput.actions["Move"].performed += ctx => moveInput = Quaternion.AngleAxis(45, Vector3.up) * ctx.ReadValue<Vector2>().XZ();
        playerInput.actions["Move"].canceled += _ => moveInput = Vector2.zero;

        playerInput.actions["Interact"].performed += _ => hand.Interact();
        playerInput.actions["Drop"].performed += Drop;
        playerInput.actions["Throw"].performed += _ => hand.Throw();
        playerInput.actions["Dash"].performed += _ => DashInput();
    }

    void ProcessDash()
    {
        if (dashValue <= 0)
            return;

        const float squashAmount = 0.3f;
        visuals.Squash(1f - squashAmount * dashValue);

        float dashDelta = Mathf.Min(Time.deltaTime, dashValue);
        dashValue -= dashDelta;
        rb.AddForce(dashValue * GameSettings.Instance.playerDashSpeed * moveInput, ForceMode.VelocityChange);
    }

    void DashInput()
    {
        if (dashValue <= 0)
        {
            foreach (var particle in dashParticles)
            {
                particle.Play();
            }

            dashValue = GameSettings.Instance.playerDashDuration;
        }
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

        jumpSquash = .5f;
    }

    private void Update()
    {
        if (jumpSquash > 0)
        {
            jumpSquash = MathF.Max(jumpSquash - Time.deltaTime, 0);
            //visuals.Squash(1f + jumpSquash);
        }

    }

    void FixedUpdate()
    {
        visuals.Squash(rb.linearVelocity.y / 15f + 1);


        if (grounded && playerInput.actions.FindAction("Jump").IsPressed())
            Jump();

        ProcessDash();

        Vector3 deltaMove = moveInput * GameSettings.Instance.playerSpeed;
        deltaMove.y = rb.linearVelocity.y;
        rb.linearVelocity = deltaMove;
        bool isMoving = moveInput.magnitude > 0.02f;
        if (isMoving)
        {
            if(!walkParticles.isPlaying && grounded)
                walkParticles.Play();
            visuals.LookAt(transform.position + moveInput, Vector3.up);
        }
        if(walkParticles.isPlaying && (!isMoving || !grounded))
            walkParticles.Stop();


            grounded = false;
    }

    void OnCollisionStay(Collision collision)
    {
        foreach (var contact in collision.contacts)
        {
            grounded |= contact.normal.y > .8f && Physics.Raycast(transform.position + Vector3.up * .2f, Vector3.down, 1);
        }
    }
}
