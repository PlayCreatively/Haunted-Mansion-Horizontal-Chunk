using GameManagers;
using UnityEngine;

public class Mummy : Enemy
{
    public AnimationCurve moveCurve;
    Animator anim;
    const float animSpeedMul = 2f;
    public float speedMultiplier = 1f;
    public float visionRadius = 4f;


    protected override void Awake()
    {
        enemyType = EnemyType.Mummy;
        base.Awake();
        anim = GetComponentInChildren<Animator>();
        anim.speed = speed * animSpeedMul;
    }

    protected override void Move()
    {
        var animInfo = anim.GetCurrentAnimatorStateInfo(0);
        float animLength = 3.208f;
        float aTime = animInfo.normalizedTime % 1f;
        float positionInAnimation = aTime * animLength;
        float doMove = moveCurve.Evaluate(positionInAnimation);

        if (doMove > 0f)
            foreach (var player in FindObjectsByType<Player>(0))
            {
                Vector3 dirToPlayer = player.transform.position - transform.position;
                if (Mathf.Abs(dirToPlayer.y) > .5f) continue; 

                dirToPlayer.y = 0;
                if (dirToPlayer.sqrMagnitude < visionRadius * visionRadius)
                    if(Physics.Raycast(transform.position, dirToPlayer, out RaycastHit hit, dirToPlayer.magnitude, LayerMask.GetMask("Wall"), QueryTriggerInteraction.Ignore) == false)
                    {
                        MoveDir = dirToPlayer.normalized;
                        break;
                    }
            }

        float deltaMove = doMove * Time.fixedDeltaTime;
        rb.MovePosition(rb.position + speed * speedMultiplier * animSpeedMul * deltaMove * MoveDir);

        anim.speed = speed * animSpeedMul;
    }


    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1, 0, 0, .5f);
        Gizmos.DrawWireSphere(transform.position, visionRadius);
    }
}