using System.Collections;
using UnityEngine;

public enum EnemyType
{
    Ghost,
    Mummy,
    Worm,
    Goo,
    Trash,
}

[System.Serializable]
public class EnemySettings
{
    public int hp = 1;
    public float speed = .7f;
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
    int hp = 1;
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
    protected Transform visuals;
    Curve deathBounceCurve;
    EnemySettings settings;

    protected virtual void Awake()
    {
        visuals = transform.GetChild(0);
        rb = GetComponent<Rigidbody>();
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

    public void Hit() => StartCoroutine(HitRoutine());

    IEnumerator HitRoutine()
    {
        hp--;
        if(dashingRoutine != null)
        {
            StopCoroutine(dashingRoutine);
            dashingRoutine = null;
        }
        FMODAudioManager.Instance.TriggerLandingOnEnemySfx(enemyType, hp);
        yield return HitSquashRoutine();
        if (hp <= 0)
        {
            Die();
           yield break;
        }
        speed *= GameSettings.Instance.hurtEnemyMoveMultiplier;
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

        if(resourceDrop != null)
        {
            Carriable resource = Instantiate(resourceDrop, rb.position, rb.rotation);
            resource.SetVelocity(rb.linearVelocity);
            resource.SetAngularVelocity(rb.angularVelocity);

            yield return new Timer(.1f).GetRoutine(t => resource.transform.localScale = Vector3.one * t);
        }

        yield return new WaitForSeconds(1f);
        Destroy(gameObject);
    }

    Coroutine dashingRoutine = null;
    IEnumerator DashRoutine(float distance, float frequency, float repeatDelay)
    {
        while (true)
        {
            float curTime = frequency; // one cycle
            float smoothT = 1, lastSmoothT;

            while (smoothT > .05f)
            {
                curTime -= Time.fixedDeltaTime;
                float t = curTime / frequency;
                lastSmoothT = smoothT;
                smoothT = t*t*t;
                float deltaSmoothT = lastSmoothT - smoothT;
                rb.MovePosition(rb.position + deltaSmoothT * distance * MoveDir);
                visuals.Squash(1f - (smoothT * .6f));

                yield return new WaitForFixedUpdate();
            }
            yield return new WaitForSeconds(repeatDelay);
        }
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
        if (Physics.SphereCast(transform.position, .2f , MoveDir, out RaycastHit hit, .25f, ~LayerMask.GetMask("Player"), QueryTriggerInteraction.Ignore))
        {
            ReflectOffWall(hit.normal);
        }
    }

    void Move()
    {
        switch (enemyType)
        {
            case EnemyType.Ghost:
                rb.MovePosition(rb.position + speed * Time.fixedDeltaTime * MoveDir);

                break;
            case EnemyType.Mummy:
                rb.MovePosition(rb.position + speed * Time.fixedDeltaTime * MoveDir);

                break;
            case EnemyType.Worm:
                rb.MovePosition(rb.position + speed * Time.fixedDeltaTime * MoveDir);

                break;
            case EnemyType.Goo:
                if(dashingRoutine == null)
                {
                    var gooSettings = settings as GooSettings;
                    dashingRoutine = StartCoroutine(DashRoutine(gooSettings.distance, gooSettings.frequency, hp > 1 ? gooSettings.waitDelay : 0));
                }
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
            float heightDiff = collision.transform.position.y - transform.position.y;

            if (heightDiff > 0)
            {
                collision.gameObject.GetComponent<Player>().Jump(1.25f);
                Hit();
            }
            else // player stunned
            {
                collision.gameObject.GetComponent<Player>().Stun(transform.position);
            }
        }
    }
}
