using UnityEngine;


public class Ghost : Enemy
{
    protected override void Awake()
    {
        base.Awake();
        speed = GameSettings.Instance.Ghost.speed;
    }

    protected override void Move()
    {
        if(hp == 1)
            MoveDir = Quaternion.Euler(0, Time.deltaTime * 90f, 0) * MoveDir;

        rb.MovePosition(rb.position + speed * Time.fixedDeltaTime * MoveDir);
    }

    
}
