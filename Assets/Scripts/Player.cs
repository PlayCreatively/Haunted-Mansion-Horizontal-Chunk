using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody)), SelectionBase]
public class Player : MonoBehaviour
{
    public int playerIndex = 0;

    PlayerInput playerInput;
    Rigidbody rb;
    Collider col;
    InteractiveHand hand;
    Transform visuals;
    ParticleSystem[] dashParticles;
    ParticleSystem walkParticles;


    Vector3 moveInput;
    bool grounded;
    float dashValue = 0.5f;
    float jumpSquash = 0.5f;
    bool stunned;
    float skipForce;

void Awake()
    {
        rb = GetComponent<Rigidbody>();
        hand = GetComponentInChildren<InteractiveHand>();
        playerInput = GetComponent<PlayerInput>();
        visuals = transform.Find("Visuals");
        col = GetComponent<Collider>();
        Assert.IsNotNull(visuals, $"child named Visuals missing in {name}");
        walkParticles = visuals.GetChild(0).GetComponentInChildren<ParticleSystem>();
        dashParticles = visuals.GetChild(1).GetComponentsInChildren<ParticleSystem>();

    }

    void Start()
    {
        List<InputDevice> devices = new(2);
        for (int i = 0; i < Gamepad.all.Count; i++)
        {
            devices.Add(Gamepad.all[playerIndex]);
        }

        if(Keyboard.current != null)
            devices.Add(Keyboard.current);

        playerInput.SwitchCurrentControlScheme(
            playerInput.defaultControlScheme,
            devices.ToArray() // Use the devices array
        );

        playerInput.actions["Move"].performed += ctx => moveInput = Quaternion.AngleAxis(45, Vector3.up) * ctx.ReadValue<Vector2>().XZ();
        playerInput.actions["Move"].canceled += _ => moveInput = Vector2.zero;

        playerInput.actions["Interact"].performed += _ => hand.Interact();
        playerInput.actions["Drop"].performed += _ => hand.DropFromHand();
        playerInput.actions["Throw"].performed += _ => hand.Throw();
        playerInput.actions["Dash"].performed += _ => DashInput();
        playerInput.actions["Next"].performed += _ => hand.IncrementSelection(1);
        playerInput.actions["Previous"].performed += _ => hand.IncrementSelection(-1);
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

    public void Stun(Vector3 origin)
    {
        if(stunned) return;

        var dir = transform.position - origin;
        dir.y = 0;
        dir.Normalize();
        dir *= GameSettings.Instance.playerStunForce;
        dir.y = 5;

        rb.AddForce(dir, ForceMode.VelocityChange);

        StartCoroutine(StunRoutine());
    }

    IEnumerator StunRoutine()
    {
        enabled = false;
        SetStunned(true);
        var physicsMat = col.material;
        physicsMat.bounciness = 1f;
        physicsMat.dynamicFriction = physicsMat.staticFriction = 0f;
        physicsMat.bounceCombine = PhysicsMaterialCombine.Maximum;
        physicsMat.frictionCombine = PhysicsMaterialCombine.Minimum;
        float stunDuration = GameSettings.Instance.playerStunDuration;
        yield return new WaitForSeconds(stunDuration);
        col.material.bounciness = 0f;
        physicsMat.dynamicFriction = physicsMat.staticFriction = .6f;
        SetStunned(false);
        enabled = true;
    }

    void SetStunned(bool value)
    {
        visuals.Find("Body").GetComponent<MeshRenderer>().material.SetColor("_Color", value ? Color.red : Color.cyan);
        stunned = value;
        Physics.IgnoreLayerCollision(LayerMask.NameToLayer("Player"), LayerMask.NameToLayer("Enemy"), value);
    }

    public void Jump()
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
        if(stunned)
            return;

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
