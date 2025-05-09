using System.Collections;
using UnityEngine;

public class GooMonster : Enemy
{
    float curRepeatDelay;

    protected override void Awake()
    {
        base.Awake();
        enemyType = EnemyType.Goo;

        var gooSettings = settings as GooSettings;
        curRepeatDelay = gooSettings.waitDelay;
        StartCoroutine(DashRoutine(gooSettings.distance, gooSettings.frequency));
    }

    public override void Hit()
    {
        base.Hit();

        if(hp != 0)
        {
            DropResource().ScaleObject(.1f, 0f, 1f);
        }


        curRepeatDelay = 0;
    }

    IEnumerator DashRoutine(float distance, float frequency)
    {
        while (hp > 0)
        {
            float curTime = frequency; // one cycle
            float smoothT = 1, lastSmoothT;

            while (smoothT > .05f)
            {
                curTime -= Time.fixedDeltaTime;
                float t = curTime / frequency;
                lastSmoothT = smoothT;
                smoothT = t * t * t;
                float deltaSmoothT = lastSmoothT - smoothT;
                rb.MovePosition(rb.position + deltaSmoothT * distance * MoveDir);
                visuals.Squash(1f - (smoothT * .6f));

                yield return new WaitForFixedUpdate();
            }
            yield return new WaitForSeconds(curRepeatDelay);
        }
    }
}
