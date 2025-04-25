using System.Collections;
using UnityEngine;

[System.Serializable]
public class EnemySettings
{
    public float speed = 1.0f;
}

[SelectionBase]
[RequireComponent(typeof(Rigidbody))]
public class Enemy : MonoBehaviour
{
    public Carriable resourceDrop;
    protected float speed;
    protected Vector3 moveDir = Vector3.zero;
    // components
    protected Rigidbody rb;
    protected Transform visuals;
    Curve deathBounceCurve;

    protected virtual void Awake()
    {
        visuals = transform.GetChild(0);
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = false;
        deathBounceCurve = Resources.Load<Curve>("EnemyDeathBounce");
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

    public void Die()
    {
        FMODAudioManager.Instance.TriggerLandingOnTheMummySfx(0);
        rb.detectCollisions = false;
        rb.freezeRotation = false;
        rb.useGravity = false;
        enabled = false;

        StartCoroutine(DieRoutine());
    }

    IEnumerator DieRoutine()
    {
        Timer deathTimer = new (.1f);

        while (!deathTimer.Finished)
        {
            yield return null;
            float t = deathBounceCurve.Evaluate(deathTimer.Normal);
            visuals.Squash(t);
        }

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
        if (Physics.SphereCast(transform.position, .2f , moveDir, out RaycastHit hit, .25f, LayerMask.GetMask("Wall")))
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
                collision.gameObject.GetComponent<Player>().Jump();
                Die();
            }
            else // player stunned
            {
                collision.gameObject.GetComponent<Player>().Stun(transform.position);
            }
        }
    }
}
