using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

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
    ParticleSystem[] walkParticles;


    Vector3 moveInput;
    bool grounded;
    float dashValue = 0.5f;
    float jumpSquash = 0.5f;
    bool stunned;
    float holdCharge = 0f;
    bool canMove = true;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        hand = GetComponentInChildren<InteractiveHand>();
        playerInput = GetComponent<PlayerInput>();
        visuals = transform.Find("Visuals");
        col = GetComponent<Collider>();
        Assert.IsNotNull(visuals, $"child named Visuals missing in {name}");
        walkParticles = visuals.GetChild(0).GetComponentsInChildren<ParticleSystem>();
        dashParticles = visuals.GetChild(1).GetComponentsInChildren<ParticleSystem>();
    }

    void Start()
    {
        if(GameSettings.Instance.UsingControllers)
        {
            Debug.Log($"{Gamepad.all.Count} controllers found");

            for (int i = 0; i < Gamepad.all.Count; i++)
                if (i == playerIndex)
                {
                    Debug.Log($"Player {playerIndex} using {Gamepad.all[i].name}");
                    playerInput.SwitchCurrentControlScheme("Gamepad", Gamepad.all[playerIndex]);
                    break;
                }
        }
        else
        {
            playerInput.SwitchCurrentControlScheme(playerIndex == 0 ? "Keyboard&Mouse" : "P2Keyboard", Keyboard.current);
        }

        playerInput.actions["Move"].performed += ctx => moveInput = Quaternion.AngleAxis(45, Vector3.up) * ctx.ReadValue<Vector2>().XZ();
        playerInput.actions["Move"].canceled += _ => moveInput = Vector2.zero;

        playerInput.actions["Interact"].performed += _ => hand.Interact();
        playerInput.actions["Drop"].performed += _ => hand.DropFromHand();
        playerInput.actions["Throw"].canceled += _ => StartCoroutine(Throw());
        playerInput.actions["Dash"].performed += _ => DashInput();
        //playerInput.actions["Next"].performed += _ => hand.IncrementSelection(1);
        //playerInput.actions["Previous"].performed += _ => hand.IncrementSelection(-1);
        playerInput.actions["ArcSelect"].performed += ctx => UpdateSelected(ctx.ReadValue<Vector2>());
        playerInput.actions["ArcSelect"].started += _ => hand.DisplayBackpack = true;
        playerInput.actions["ArcSelect"].canceled += _ => hand.DisplayBackpack = false;
        playerInput.actions["Pause"].performed += _ => Game.RestartGame();
        playerInput.actions["Settings"].performed += _ => GameSettings.Instance.RoomCleaning = !GameSettings.Instance.RoomCleaning;
    }

    public int UpdateSelected(Vector2 dir)
    {
        const float selectionEndDegree = -90f;
        const float selectionStartDegree = 90;
        const int slotCount = 5;
        const float arcWidth = 180f;
        const float selectionWidth = arcWidth / slotCount;

        Vector2 selectionStartAngle = new(Mathf.Cos(selectionStartDegree), Mathf.Sin(selectionStartDegree));
        float angle = -Vector2.SignedAngle(selectionStartAngle, dir);
        angle = Mathf.Clamp(angle, 0, arcWidth);
        int index = (int)((angle) / selectionWidth);

        if (index != slotCount - 1)
            hand.UpdateSelection(index);
        else return index; // TODO: drop bag

        return index;
    }

    void ChargeThrow()
    {
        if(hand.ItemInHand == null) return;

        float chargeTime = GameSettings.Instance.playerThrowChargeTime;

        if (holdCharge < 1f)
        {
            holdCharge += Time.deltaTime / chargeTime;
            holdCharge = Mathf.Min(holdCharge, chargeTime);
        }

        if (holdCharge > .25f)
            visuals.Squash(1f - (holdCharge * .25f));
    }

    IEnumerator Throw()
    {
        if(holdCharge < .35f)
        {
            hand.Throw(0);
            yield break;
        }

        if(grounded)
            Jump(holdCharge);

        yield return new WaitForSeconds(0.14f * holdCharge);
        hand.Throw(GameSettings.Instance.playerThrowForce * holdCharge);
        rb.AddForce(2 * holdCharge * -visuals.forward, ForceMode.VelocityChange);
        holdCharge = 0;
        canMove = false;
        yield return new WaitUntil(() => grounded);
        canMove = true;
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
            FMODAudioManager.Instance.TriggerOnDashStartsSfx();
        }
    }

    public void Stun(Vector3 origin)
    {
        if(stunned) return;

        var dir = transform.position - origin;
        dir.y = 0;
        dir.Normalize();

        dir *= GameSettings.Instance.playerStunForce;
        var item = hand.DropFromHand();
        if(item != null)
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
        visuals.Find("Body").GetComponent<MeshRenderer>().material.SetColor("_Color", value ? Color.red : Color.cyan);
        stunned = value;
        Physics.IgnoreLayerCollision(LayerMask.NameToLayer("Player"), LayerMask.NameToLayer("Enemy"), value);
    }

    public void Jump(float force = 1f)
    {
        var velocity = rb.linearVelocity;
        velocity.y = GameSettings.Instance.playerJumpForce * force;
        rb.linearVelocity = velocity;
        grounded = false;

        jumpSquash = .5f;
    }

    void Update()
    {
        if (jumpSquash > 0)
        {
            jumpSquash = MathF.Max(jumpSquash - Time.deltaTime, 0);
            //visuals.Squash(1f + jumpSquash);
        }

        if(playerInput.actions["Throw"].IsPressed())
            ChargeThrow();

    }

    void FixedUpdate()
    {
        if(stunned || !canMove)
            return;

        visuals.Squash(rb.linearVelocity.y / 15f + 1);

        if (grounded && playerInput.actions.FindAction("Jump").IsPressed())
            Jump();

        ProcessDash();

        Vector3 deltaMove = moveInput * (GameSettings.Instance.playerSpeed - holdCharge * .9f);
        deltaMove.y = rb.linearVelocity.y;
        rb.linearVelocity = deltaMove;
        bool isMoving = moveInput.magnitude > 0.02f;
        if (isMoving)
        {
            if(!walkParticles[0].isPlaying && grounded)
                foreach (var particle in walkParticles)
                {
                    particle.Emit(1);
                    particle.Play();
                }
            visuals.LookAt(transform.position + moveInput, Vector3.up);
        }
        if (walkParticles[0].isPlaying && (!isMoving || !grounded))
            foreach (var particle in walkParticles)
            {
                particle.Stop();
            }

        grounded = false;

        // check if out of bounds
        if (transform.position.y < -10)
        {
            rb.linearVelocity = Vector3.zero;
            transform.localPosition = Vector3.zero;
        }
    }

    void OnCollisionStay(Collision collision)
    {
        foreach (var contact in collision.contacts)
        {
            grounded |= contact.normal.y > .8f && Physics.Raycast(transform.position + Vector3.up * .2f, Vector3.down, 1);
        }
    }
}
