using System;
using System.Collections;
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
    Renderer rend;
    InteractiveHand hand;
    Transform visuals;
    Animator animator;
    ParticleSystem[] dashParticles;
    ParticleSystem[] walkParticles;

    Vector3 moveInput;
    bool grounded;
    float dashValue = 0.5f;
    float jumpSquash = 0.5f;
    bool stunned;
    const float boostRunDuration = 1f;
    float boostRunEnergy = 0;
    Gamepad gamepad;
    bool zoomInput = false;
    float airTime = 0;
    public bool ZoomInput => zoomInput;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        hand = GetComponentInChildren<InteractiveHand>();
        playerInput = GetComponent<PlayerInput>();
        animator = GetComponentInChildren<Animator>();
        visuals = transform.Find("Visuals");
        rend = visuals.Find("Body").GetComponentInChildren<Renderer>();
        col = GetComponent<Collider>();
        Assert.IsNotNull(visuals, $"child named Visuals missing in {name}");
        walkParticles = visuals.GetChild(0).GetComponentsInChildren<ParticleSystem>();
        dashParticles = visuals.GetChild(1).GetComponentsInChildren<ParticleSystem>();
    }

    void Start()
    {
        if (GameSettings.Instance.UsingControllers)
        {
            Debug.Log($"{Gamepad.all.Count} controllers found");

            for (int i = 0; i < Gamepad.all.Count; i++)
                if (i == playerIndex)
                {
                    Debug.Log($"Player {playerIndex} using {Gamepad.all[i].name}");
                    // vibrate controller
                    gamepad = Gamepad.all[playerIndex];
                    playerInput.SwitchCurrentControlScheme("Gamepad", gamepad);
                    break;
                }
        }
        else
        {
            playerInput.SwitchCurrentControlScheme(playerIndex == 0 ? "Keyboard&Mouse" : "P2Keyboard", Keyboard.current);
        }

        playerInput.actions["Move"].performed += MoveInput;
        playerInput.actions["Move"].canceled += MoveInput;
        playerInput.actions["Move"].canceled += _ => boostRunEnergy = 0; ;

        playerInput.actions["Interact"].performed += _ => hand.Interact();
        playerInput.actions["Drop"].performed += _ => hand.DropFromHand();
        playerInput.actions["Throw"].performed += _ => Throw();
        //playerInput.actions["Next"].performed += _ => hand.IncrementSelection(1);
        //playerInput.actions["Previous"].performed += _ => hand.IncrementSelection(-1);
        playerInput.actions["ArcSelect"].canceled += _ => hand.DisplayInventoryUI(0);
        playerInput.actions["ArcSelect"].performed += ctx => UpdateSelected(ctx.ReadValue<Vector2>());
        playerInput.actions["Pause"].performed += _ => Game.RestartGame();
        playerInput.actions["Settings"].performed += _ => GameSettings.Instance.RoomCleaning = !GameSettings.Instance.RoomCleaning;
        playerInput.actions["ZoomOut"].started += _ => zoomInput = true;
        playerInput.actions["ZoomOut"].canceled += _ => zoomInput = false;
    }


    void MoveInput(InputAction.CallbackContext ctx)
    {
        moveInput = Quaternion.AngleAxis(45, Vector3.up) * ctx.ReadValue<Vector2>().XZ();
        animator.SetFloat("Speed", moveInput.magnitude);
    }

    public int UpdateSelected(Vector2 dir)
    {
        if (dir.sqrMagnitude < .1f)
        {
            hand.DisplayInventoryUI(0);
            return -1;
        }
        hand.DisplayInventoryUI(dir.magnitude);

        const float selectionEndDegree = -90f;
        const float selectionStartDegree = 90;
        const int slotCount = 5;
        const float arcWidth = 180f;
        const float selectionWidth = arcWidth / slotCount;

        Vector2 selectionStartAngle = new(Mathf.Cos(selectionStartDegree), Mathf.Sin(selectionStartDegree));
        float angle = -Vector2.SignedAngle(selectionStartAngle, dir) - selectionWidth * .5f;
        angle = Mathf.Clamp(angle, 0, arcWidth);
        int index = (int)((angle) / selectionWidth);

        if (index != slotCount - 1)
            hand.UpdateSelection(index);
        else return index; // TODO: drop bag

        return index;
    }

    void Throw()
    {
        float throwForce = GameSettings.Instance.playerThrowForce;
        const float minThrowForce = .45f;

        float chargeMul = rb.linearVelocity.y > .2f || boostRunEnergy > 0 ? 1f : minThrowForce;
        hand.Throw(throwForce * chargeMul);
    }

    void ProcessDash()
    {
        if (dashValue <= 0)
            return;

        //const float squashAmount = 0.3f;
        //visuals.Squash(1f - squashAmount * dashValue);

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
            FMODAudioManager.Instance.TriggerOnDashStartsSfx();
        }
    }

    public void Vibrate(float value, float duration = .5f, bool low = true, bool high = true)
    {
        if (gamepad == null)
            return;

        float lowValue = low ? value : 0;
        float highValue = high ? value : 0;

        StartCoroutine(new Timer(duration).GetRoutine(a => gamepad.SetMotorSpeeds(lowValue * (1f - (a * a)), highValue * (1f - (a * a)))));
    }

    public void Stun(Vector3 origin)
    {
        if (stunned) return;

        //gamepad.ResumeHaptics();
        Vibrate(1f);

        var dir = transform.position - origin;
        dir.y = 0;
        dir.Normalize();

        dir *= GameSettings.Instance.playerStunForce;
        var item = hand.DropFromHand();
        if (item != null)
        {
            item.SetVelocity(-dir + Vector3.up * 5);
        }
        dir.y = 5;

        rb.linearVelocity = dir;

        FMODAudioManager.Instance.TriggerStunnedSfx();
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
        SetStunned(false);
        enabled = true;
    }

    void SetStunned(bool value)
    {
        rend.material.SetColor("_Color", value ? Color.red : Color.cyan);
        stunned = value;
        animator.SetBool("IsStunned", value);
        Physics.IgnoreLayerCollision(LayerMask.NameToLayer("Player"), LayerMask.NameToLayer("Enemy"), value);
    }

    public void Jump(float force = 1f)
    {
        if (!grounded) // means the player landed on an enemy
            boostRunEnergy = boostRunDuration;

        var velocity = rb.linearVelocity;
        velocity.y = GameSettings.Instance.playerJumpForce * force;
        rb.linearVelocity = velocity;
        grounded = false;

        jumpSquash = .5f;

        animator.SetTrigger("JumpInput");
    }

    public float LastGroundedHeight => lastGroundedHeight;
    float lastGroundedHeight;
    void UpdateLastFloorHeight()
    {
        if (grounded)
            lastGroundedHeight = (int)(rb.position.y+.05f);
    }

    void Update()
    {
        if (jumpSquash > 0)
        {
            jumpSquash = MathF.Max(jumpSquash - Time.deltaTime, 0);
            visuals.Squash(1f + jumpSquash);
        }
        UpdateLastFloorHeight();

    }

    void FixedUpdate()
    {
        if (stunned)
        {
            boostRunEnergy = 0;
            return;
        }

        visuals.Squash(rb.linearVelocity.y / 15f + 1);

        if (!grounded)
        {
            airTime += Time.fixedDeltaTime;
        }

        if (grounded)
        {
            if (airTime > .7f)
            {
                boostRunEnergy = boostRunDuration;
            }
            airTime = 0;
        }

        var jumpAction = playerInput.actions.FindAction("Jump");
        if (jumpAction.IsPressed())
        {
            if (grounded)
            {
                DashInput();
                Jump();
            }
        }

        bool isRunBoosting = grounded && boostRunEnergy > 0;
        if (isRunBoosting)
        {
            if(boostRunEnergy == boostRunDuration)
            {
                FMODAudioManager.Instance.TriggerJumpingOffTheBalconySfx();
                Vibrate(.3f, .1f, true, true);
                foreach (var particle in dashParticles)
                {
                    particle.Play();
                }
            }

            const float time = .8f;
            if(boostRunEnergy > time)
            {
                float squash = boostRunEnergy - time;
                squash /= (1f - time);
                squash *= squash;
                visuals.Squash(1f - squash);
            }

            boostRunEnergy -= Time.deltaTime;
        }

        float runBoost = isRunBoosting ? 2f : 1f;

        foreach (var particle in dashParticles)
        {
            var main = particle.main;
            main.startColor = isRunBoosting ? Color.white * .5f : Color.white;
        }
        foreach (var particle in walkParticles)
        {
            var main = particle.main;
            main.startColor = isRunBoosting ? Color.white * .5f : Color.white;
        }

        ProcessDash();

        // slow player down when charging
        Vector3 deltaMove = GameSettings.Instance.playerSpeed * runBoost * moveInput;
        deltaMove.y = rb.linearVelocity.y;
        rb.linearVelocity = deltaMove;
        bool isMoving = moveInput.magnitude > 0.02f;
        if (isMoving)
        {
            if (!walkParticles[0].isPlaying && grounded)
                foreach (var particle in walkParticles)
                {
                    particle.Emit(1);
                    particle.Play();
                }
            visuals.LookAt(transform.position + moveInput, Vector3.up);
        }

        if (walkParticles[0].isPlaying && (!isMoving || !grounded))
            foreach (var particle in walkParticles)
                particle.Stop();

        animator.SetBool("IsGrounded", grounded);

        grounded = false;

        // check if out of bounds
        if (transform.position.y < -10)
        {
            rb.linearVelocity = Vector3.zero;
            transform.localPosition = Vector3.zero;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Collider playerCol = collision.collider;
            Vector3 playerFeetPos = playerCol.bounds.center - Vector3.up * playerCol.bounds.extents.y;
            Vector3 enemyCenterPos = col.bounds.center;

            // touching: 0 == enemy center, 1 == enemy head, 0 > below enemy center
            float touchHeightPercent = (playerFeetPos.y - enemyCenterPos.y) / col.bounds.extents.y;

            var player = collision.gameObject.GetComponent<Player>();
            if (touchHeightPercent > 0f)
            {
                player.Jump(1.25f);
                player.Vibrate(1f, .25f, false, true);
            }
        }
    }

    void OnCollisionStay(Collision collision)
    {
        foreach (var contact in collision.contacts)
        {
            grounded |= contact.normal.y > .8f && Physics.Raycast(transform.position + Vector3.up * .2f, Vector3.down, 1);
        }
        if (grounded)
        {
            //Debug.Log(collision.impulse.y);
        }
    }
}
