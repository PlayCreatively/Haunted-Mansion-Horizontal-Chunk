using System.Collections;
using UnityEngine;

public enum EnemyType
{
    Ghost,
    Mummy,
    Worm,
    Spider,
    Goo,
    Trash,
}

[System.Serializable]
public class EnemySettings
{
    public int hp = 1;
    public float speed = .7f;
}

[SelectionBase]
[RequireComponent(typeof(Rigidbody))]
public class Enemy : MonoBehaviour
{
    int hp = 1;
    public EnemyType enemyType;
    public Carriable resourceDrop;
    protected float speed;
    protected Vector3 moveDir = Vector3.zero;
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
        moveDir = new Vector3(Mathf.Cos(randAngle), 0, Mathf.Sin(randAngle));
    }

    public void Hit() => StartCoroutine(HitRoutine());

    IEnumerator HitRoutine()
    {
        hp--;
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

        Carriable resource = Instantiate(resourceDrop, rb.position, rb.rotation);
        resource.SetVelocity(rb.linearVelocity);
        resource.SetAngularVelocity(rb.angularVelocity);

        yield return new Timer(.1f).GetRoutine(t => resource.transform.localScale = Vector3.one * t);

        //resource.SetVelocity(Vector3.up * 4);

        yield return new WaitForSeconds(1f);
        Destroy(gameObject);
    }

    void ReflectOffWall(Vector3 normal)
    {
        // Reflect the move direction off the wall normal
        moveDir = Vector3.Reflect(moveDir, normal);
        moveDir.y = 0;
        moveDir.Normalize();
        visuals.forward = moveDir;
    }

    void FixedUpdate()
    {
        rb.MovePosition(rb.position + moveDir * speed * Time.fixedDeltaTime);

        if(moveDir.sqrMagnitude < 0.01f)
        {
            SetRandomDirection();
        }
        else
        {
            visuals.forward = moveDir;
        }

        // check for wall
        if (Physics.SphereCast(transform.position, .2f , moveDir, out RaycastHit hit, .25f, ~LayerMask.GetMask("Player"), QueryTriggerInteraction.Ignore))
        {
            ReflectOffWall(hit.normal);
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
