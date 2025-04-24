using UnityEngine;


public class Ghost : Enemy
{
    protected override void Awake()
    {
        base.Awake();
        speed = GameSettings.Instance.Ghost.speed;
    }
}
