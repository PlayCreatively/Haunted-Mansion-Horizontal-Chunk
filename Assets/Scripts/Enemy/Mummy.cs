using GameManagers;
using UnityEngine;

public class Mummy : Enemy
{
    public AnimationCurve moveCurve;
    Animator anim;
    const float animSpeedMul = 1f;
    public float speedMultiplier = 1f;


    protected override void Awake()
    {
        base.Awake();
        speed = GameSettings.Instance.Mummy.speed;
        enemyType = EnemyType.Mummy;
        anim = GetComponentInChildren<Animator>();
        anim.speed = speed * animSpeedMul;
    }

    protected override void Move()
    {
        var animInfo = anim.GetCurrentAnimatorStateInfo(0);
        float animLength = 3.208f;
        float aTime = animInfo.normalizedTime % 1f;
        float positionInAnimation = aTime * animLength;
        Debug.Log($"position in animation {positionInAnimation}: {moveCurve.Evaluate(positionInAnimation) > 0}");
        float deltaMove = moveCurve.Evaluate(positionInAnimation) * Time.fixedDeltaTime;
        Debug.Log(deltaMove);
        rb.MovePosition(rb.position + speed * speedMultiplier * deltaMove * MoveDir);

        anim.speed = speed * animSpeedMul;
    }
}