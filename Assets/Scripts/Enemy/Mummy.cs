using GameManagers;

public class Mummy : Enemy
{
    protected override void Awake()
    {
        base.Awake();
        speed = GameSettings.Instance.Mummy.speed;
    }
}