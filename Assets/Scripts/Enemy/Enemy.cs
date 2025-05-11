using Mono.Cecil;
using System.Collections;
using UnityEngine;

public enum EnemyType
{
    Ghost,
    Mummy,
    TowelMonster,
    Goo,
    Trash,
}

[System.Serializable]
public class EnemySettings
{
    public int hp = 1;
    public float speed = .7f;
    public float hurtSpeedMultiplier = 2f;
}

[System.Serializable]
public class GooSettings : EnemySettings
{
    public float frequency = .5f, distance = 1.5f, waitDelay = .8f;
}

[SelectionBase]
[RequireComponent(typeof(Rigidbody))]
public class Enemy : MonoBehaviour
{
    protected int hp = 1;
    public EnemyType enemyType;
    public Carriable resourceDrop;
    protected float speed;
    Vector3 __moveDir;
    protected Vector3 MoveDir
    {
        get => __moveDir;
        set
        {
            __moveDir = value;
            if (value.sqrMagnitude > 0.01f)
            {
                visuals.forward = value;
            }
        }
    }
    // components
    protected Rigidbody rb;
    protected Collider col;
    protected Transform visuals;
    Curve deathBounceCurve;
    protected EnemySettings settings;

    protected virtual void Awake()
    {
        visuals = transform.GetChild(0);
        rb = GetComponent<Rigidbody>();
        if(!TryGetComponent(out col))
            col = GetComponentInChildren<Collider>();

        rb.isKinematic = false;
        deathBounceCurve = Resources.Load<Curve>("EnemyDeathBounce");

        settings = GameSettings.Instance.GetEnemySettings(enemyType);
        speed = settings.speed;
        hp = settings.hp;
    }

    protected virtual void OnEnable()
    {
        SetRandomDirection();
    }

    void SetRandomDirection()
    {
        float randAngle = Random.Range(0, Mathf.PI * 2);
        MoveDir = new Vector3(Mathf.Cos(randAngle), 0, Mathf.Sin(randAngle));
    }

    public virtual void Hit() => StartCoroutine(HitRoutine());

    IEnumerator HitRoutine()
    {
        hp--;
        visuals.GetComponentInChildren<Renderer>().material.SetFloat("_Angry", 1f);
        FMODAudioManager.Instance.TriggerLandingOnEnemySfx(enemyType, hp);
        yield return HitSquashRoutine();
        if (hp <= 0)
        {
            Die();
           yield break;
        }
        speed *= settings.hurtSpeedMultiplier;
    }

    IEnumerator HitSquashRoutine()
    {
        Timer hitSquashTimer = new(.09f);
        while (!hitSquashTimer.Finished)
        {
            float t = deathBounceCurve.Evaluate(hitSquashTimer.Normal);
            visuals.Squash(t);
            yield return null;
        }
        visuals.Squash(1);
    }

    public void Die()
    {
        StopAllCoroutines();
        rb.detectCollisions = false;
        rb.freezeRotation = false;
        rb.useGravity = false;
        enabled = false;

        StartCoroutine(DieRoutine());
    }

    IEnumerator DieRoutine()
    {
        yield return HitSquashRoutine();

        rb.useGravity = true;
        rb.AddForce(Vector3.up * 8, ForceMode.VelocityChange);
        rb.AddTorque(Random.onUnitSphere * 5, ForceMode.VelocityChange);
        yield return new WaitForSeconds(.15f);

        yield return new Timer(.1f).GetRoutine(t => transform.localScale = Vector3.one * (1f - t));

        var resource = DropResource();
        yield return new Timer(.1f).GetRoutine(t => resource.transform.localScale = Vector3.one * t);

        yield return new WaitForSeconds(1f);
        Destroy(gameObject);
    }

    protected Carriable DropResource()
    {
        if (resourceDrop != null)
        {
            Carriable resource = Instantiate(resourceDrop, rb.position, rb.rotation);
            resource.SetVelocity(rb.linearVelocity);
            resource.SetAngularVelocity(rb.angularVelocity);
            return resource;
        }
        return null;
    }

    void ReflectOffWall(Vector3 normal)
    {
        // Reflect the move direction off the wall normal
        Vector3 dir = MoveDir;
        dir = Vector3.Reflect(dir, normal);
        dir.y = 0;
        MoveDir = dir.normalized;
    }

    void FixedUpdate()
    {
        Move();

        if (MoveDir.sqrMagnitude < 0.01f)
        {
            SetRandomDirection();
        }

        // check for wall
        if (Physics.SphereCast(transform.position, .2f , MoveDir, out RaycastHit hit, .4f, ~LayerMask.GetMask("Player", "Enemy"), QueryTriggerInteraction.Ignore))
        {
            ReflectOffWall(hit.normal);
        }
    }

    protected virtual void Move()
    {
        switch (enemyType)
        {
            case EnemyType.Ghost:
                rb.MovePosition(rb.position + speed * Time.fixedDeltaTime * MoveDir);

                break;
            case EnemyType.Mummy:
                rb.MovePosition(rb.position + speed * Time.fixedDeltaTime * MoveDir);

                break;
            case EnemyType.TowelMonster:
                rb.MovePosition(rb.position + speed * Time.fixedDeltaTime * MoveDir);

                break;
            case EnemyType.Goo:
                
                break;
            case EnemyType.Trash:
                rb.MovePosition(rb.position + speed * Time.fixedDeltaTime * MoveDir);

                break;
            default:
                break;
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Collider playerCol = collision.collider;
            Vector3 playerFeetPos = playerCol.bounds.center - Vector3.up * playerCol.bounds.extents.y;
            Debug.DrawRay(playerFeetPos, collision.transform.forward * 2, Color.red, 1f);
            Vector3 enemyCenterPos = col.bounds.center;
            Debug.DrawRay(enemyCenterPos, transform.forward * 2, Color.red, 1f);

            // touching: 0 == enemy center, 1 == enemy head, 0 > below enemy center
            float touchHeightPercent = (playerFeetPos.y - enemyCenterPos.y) / col.bounds.extents.y;

            var player = collision.gameObject.GetComponent<Player>();
            if (touchHeightPercent > 0f)
            {
                player.Jump(1.25f);
                player.Vibrate(1f, .25f, false, true);
                Hit();
            }
            else // player stunned
            {
                player.Stun(transform.position);
                player.Vibrate(1f);
            }
        }
    }
}
