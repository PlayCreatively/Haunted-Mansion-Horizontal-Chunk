using GameManagers;
using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody)), SelectionBase]
public class Player : MonoBehaviour
{
    public int playerIndex = 0;

    PlayerInput playerInput;
    [HideInInspector]
    public Rigidbody rb;
    [HideInInspector]
    public CapsuleCollider col;
    SphereCollider ballCollider;
    [HideInInspector]
    public SkinnedMeshRenderer rend;
    InteractiveHand hand;
    Transform visuals;
    Animator animator;
    ParticleSystem[] dashParticles;
    ParticleSystem[] walkParticles;
    ParticleSystem[] sparkParticles;
    ParticleSystem[] landingParticles;
    Vector3 moveInput;
    bool grounded;
    private bool wasRunBoosting = false;
    float dashValue = 0f;
    float jumpSquash = 0f;
    bool stunned;
    const float boostRunDuration = 1f;
    float boostRunEnergy = 0;
    Gamepad gamepad;
    bool zoomInput = false;
    float airTime = 0;
    public bool ZoomInput => zoomInput;
    public bool IsStunned => stunned;
    public SkinnedMeshRenderer MeshRenderer => rend;

    void Awake()
    {
        transform.parent = GameObject.Find("Players").transform;
        transform.localPosition = Vector3.up;

        rb = GetComponent<Rigidbody>();
        hand = GetComponentInChildren<InteractiveHand>();
        playerInput = GetComponent<PlayerInput>();
        animator = GetComponentInChildren<Animator>();
        visuals = transform.Find("Visuals");
        rend = visuals.Find("Body").GetComponentInChildren<SkinnedMeshRenderer>();
        col = GetComponent<CapsuleCollider>();
        ballCollider = visuals.GetComponent<SphereCollider>();
        Assert.IsNotNull(visuals, $"child named Visuals missing in {name}");
        walkParticles = visuals.GetChild(0).GetComponentsInChildren<ParticleSystem>();
        dashParticles = visuals.GetChild(1).GetComponentsInChildren<ParticleSystem>();
        landingParticles = visuals.GetChild(4).GetComponentsInChildren<ParticleSystem>();
        sparkParticles = visuals.GetChild(5).GetComponentsInChildren<ParticleSystem>();
        
        Vibrate(0f); // stop vibration on start
    }

    void SetUpController()
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
                    return;
                }

            Destroy(gameObject);
        }
        else
        {
            playerInput.currentActionMap = new InputActionMap("Player");
            playerInput.SwitchCurrentControlScheme(playerIndex == 0 ? "Keyboard&Mouse" : "P2Keyboard", Keyboard.current);
        }
    }

    void OnActionTriggered(InputAction.CallbackContext ctx)
    {
        if (ctx.phase == InputActionPhase.Performed)
            switch (ctx.action.name)
            {
                case "Interact": hand.Interact(); break;
                case "Drop": hand.DropFromHand(); break;
                case "Dash": DashInput(); break;
                //case "Jump": Jump(); break;
                case "Throw": Throw(); break;
                case "Pause": Game.ToMainMenu(); break;
            }

        switch (ctx.action.name)
        {
            case "Move": MoveInput(ctx); break;
            case "ArcSelect":
                if (ctx.phase == InputActionPhase.Canceled)
                    hand.DisplayInventoryUI(0);
                else if (ctx.phase == InputActionPhase.Performed)
                    UpdateSelected(ctx.ReadValue<Vector2>());
                break;
            case "ZoomOut": zoomInput = ctx.phase != InputActionPhase.Canceled; break;
        }
    }

    void OnEnable()
    {
        hand.enabled = true;
        playerInput.ActivateInput();
        playerInput.onActionTriggered += OnActionTriggered;
        gamepad = playerInput.GetDevice<Gamepad>();
    }

    void OnDisable()
    {
        gamepad.SetMotorSpeeds(0f, 0f); // stop vibration on disable
        hand.enabled = false;
        playerInput.DeactivateInput();
        playerInput.onActionTriggered -= OnActionTriggered;
        gamepad = null;
    }



    void MoveInput(InputAction.CallbackContext ctx)
    {
        if (ctx.phase == InputActionPhase.Canceled)
        {
            moveInput = Vector3.zero;
            animator.SetFloat("Speed", 0);
            col.material.dynamicFriction = 1f; // reset friction
            col.material.staticFriction = 1f;
            return;
        }

        col.material.dynamicFriction = 0; // set friction to 0 when moving
        col.material.staticFriction = 0f;


        moveInput = Quaternion.AngleAxis(Camera.main.transform.eulerAngles.y, Vector3.up) * ctx.ReadValue<Vector2>().XZ();
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

        if (dir.sqrMagnitude < .3f)
            return -1;

        const float selectionEndDegree = -90f;
        const float selectionStartDegree = 90;
        const int slotCount = 5;
        const float arcWidth = 180f;
        const float selectionWidth = arcWidth / slotCount;


        Vector2 selectionStartAngle = new(Mathf.Cos(selectionStartDegree), Mathf.Sin(selectionStartDegree));
        float angle = MathF.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        angle = Math.Clamp(angle, -90f, 90f);
        angle = (90f - angle);
        //Debug.Log($"angle: {angle} i: {(int)((angle) / selectionWidth)}");
        int index = (int)((angle) / selectionWidth);
        index = Mathf.Clamp(index, 0, slotCount - 1);
        hand.UpdateSelection(index);
        //else return index; // TODO: drop bag

        return index;
    }

    void Throw()
    {
        float throwForce = GameSettings.Instance.playerThrowForce;
        const float minThrowForce = .45f;

        float chargeMul = !grounded || boostRunEnergy > 0 ? 1f : minThrowForce;
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

        StartCoroutine(new Timer(duration).GetRoutine(a => playerInput.GetDevice<Gamepad>().SetMotorSpeeds(lowValue * (1f - (a * a)), highValue * (1f - (a * a)))));
    }

    public void LandingVibrate() => Vibrate(.3f, .1f, true, true);

    public void Stun(Collider other)
    {
        if (stunned) return;

        //gamepad.ResumeHaptics();
        Vibrate(1f);

        moveInput = Vector3.zero;

        var dir = transform.position - other.transform.position;
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
        StartCoroutine(StunRoutine(other));
    }

    IEnumerator StunRoutine(Collider other)
    {
        enabled = false;
        SetStunned(true);
        visuals.Squash(1f);
        rb.freezeRotation = false;
        col.enabled = false;
        ballCollider.enabled = true;
        float stunDuration = GameSettings.Instance.playerStunDuration;
        yield return new WaitForSeconds(stunDuration);
        col.enabled = true;
        ballCollider.enabled = false;
        rb.rotation = Quaternion.identity;
        rb.freezeRotation = true;

        SetStunned(false);
        enabled = true;

        yield return IgnoreUntilExit(other);
    }

    IEnumerator IgnoreUntilExit(Collider col)
    {
        if(!col.bounds.Intersects(this.col.bounds)) yield break;

        Physics.IgnoreCollision(col, this.col, true);
        yield return new WaitWhile(() => col.bounds.Intersects(this.col.bounds));
        Physics.IgnoreCollision(col, this.col, false);
    }


    void SetStunned(bool value)
    {
        rend.material.SetColor("_Color", value ? Color.red : Color.cyan);
        stunned = value;
        animator.SetBool("IsStunned", value);
        //Physics.IgnoreLayerCollision(LayerMask.NameToLayer("Player"), LayerMask.NameToLayer("Enemy"), value);
    }

    public void Jump(float force = 1f)
    {
        if (!grounded) // means the player landed on an enemy
        {
            foreach (var particle in landingParticles)
                particle.Play();
            boostRunEnergy = boostRunDuration;
        }

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
            lastGroundedHeight = (int)(rb.position.y + .05f);
    }

    void Update()
    {
        if (jumpSquash > 0)
        {
            jumpSquash = MathF.Max(jumpSquash - Time.deltaTime, 0);
            visuals.Squash(1f + jumpSquash);
        }
        if (!playerInput.actions["Move"].enabled)
            foreach (var item in playerInput.actions)
            {
                item.Enable();
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
                foreach (var particle in landingParticles)
                    particle.Play();
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
            if (boostRunEnergy == boostRunDuration)
            {
                FMODAudioManager.Instance.TriggerJumpingOffTheBalconySfx();
                LandingVibrate();
                foreach (var particle in dashParticles)
                {
                    particle.Play();
                }
            }

            const float time = .8f;
            if (boostRunEnergy > time)
            {
                float squash = boostRunEnergy - time;
                squash /= (1f - time);
                squash *= squash;
                visuals.Squash(1f - Mathf.Min(squash, .9f));
            }

            boostRunEnergy -= Time.deltaTime;
        }

// Handle continuous boost trail emission
if (isRunBoosting && !wasRunBoosting)
{
    
    foreach (var trail in sparkParticles)
    {
        trail.Play();
    }
}
else if (!isRunBoosting && wasRunBoosting)
{
    foreach (var trail in sparkParticles)
    {
        trail.Stop();
    }
}
    wasRunBoosting = isRunBoosting;
    
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
                player.LandingVibrate();
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
