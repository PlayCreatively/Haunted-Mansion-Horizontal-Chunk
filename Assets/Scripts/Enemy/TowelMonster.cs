using UnityEngine;

public class TowelMonster : Enemy
{
    Animator anim;
    const float animSpeedMul = 1.5f;

    protected override void Awake()
    {
        base.Awake();
        enemyType = EnemyType.TowelMonster;
        anim = GetComponentInChildren<Animator>();
        anim.speed = speed * animSpeedMul;
    }

    protected override void Move()
    {
        rb.MovePosition(rb.position + speed * Time.fixedDeltaTime * MoveDir);

        anim.speed = speed * animSpeedMul;
    }
}
