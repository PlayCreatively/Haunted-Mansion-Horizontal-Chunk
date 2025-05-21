using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TutorialManager : MonoBehaviour
{
    IEnumerator Start() => TutorialRoutine();

    public Transform mummySpawnPlacement, gooSpawnPlacement, towelMonsterSpawnPlacement, ghostSpawnPlacement;
    public Player[] players;
    public Transform[] stages;
    public Transform[] spiderwebs;
    public Collider firstGoHereTrigger, finalGoHereTrigger;


    IEnumerator TutorialRoutine()
    {
        players = FindObjectsByType<Player>(0);

        int step = 0;
        yield return MoveToView(step, true, true);

        yield return GoHereRoutine(firstGoHereTrigger);

        yield return MoveToView(step, false, true);
        yield return MoveToView(++step, true);

        yield return JumpOnMummy();

        yield return MoveToView(step, false);
        yield return MoveToView(++step, true);

        yield return PlaceInRoom();

        yield return MoveToView(step, false);
        yield return MoveToView(++step, true);

        yield return Goo();

        yield return MoveToView(step, false);
        yield return MoveToView(++step, true);

        yield return Laundry();

        yield return MoveToView(step, false);
        yield return MoveToView(++step, true);

        yield return spiderwebs[3].ScaleDownObject(.2f, true);
        yield return GoHereRoutine(finalGoHereTrigger);

        Destroy(players[0].transform.parent.gameObject);

        SceneManager.LoadScene("MainMenu", LoadSceneMode.Single);
    }

    public TutorialRoom secondRoom;
    IEnumerator Laundry()
    {
        yield return spiderwebs[2].ScaleDownObject(.2f, true);

        Enemy.Spawn<TowelMonster>(towelMonsterSpawnPlacement.position);
        Enemy.Spawn<Ghost>(ghostSpawnPlacement.position);

        secondRoom.ForceRequire(new Room.Requirements(0, 1, 1, 0));
        while (secondRoom.IsDirty)
        {
            if (spawnedGoo == null)
                spawnedGoo = Enemy.Spawn<GooMonster>(gooSpawnPlacement.position);

            yield return null;
        }
    }

    GooMonster spawnedGoo = null;
    IEnumerator Goo()
    {
        yield return spiderwebs[1].ScaleDownObject(.2f, true);

        spawnedGoo = Enemy.Spawn<GooMonster>(gooSpawnPlacement.position);

        firstRoom.ForceRequire(new Room.Requirements(0, 0, 0, 1));

        while (firstRoom.IsDirty)
        {
            if(spawnedGoo == null)
                spawnedGoo = Enemy.Spawn<GooMonster>(gooSpawnPlacement.position);

            yield return null;
        }
    }

    IEnumerator GoHereRoutine(Collider trigger)
    {
        bool playersTouchingTrigger = false;
        while (!playersTouchingTrigger)
        {
            playersTouchingTrigger = true;
            for (int i = 0; i < players.Length; i++)
                if(players[i] != null)
                    playersTouchingTrigger &=
                        trigger.bounds.Intersects(players[i].col.bounds);
            yield return null;
        }
    }

    public TextMeshPro oneMoreTime;
    IEnumerator JumpOnMummy()
    {
        var mummy = Enemy.Spawn<Mummy>(mummySpawnPlacement.position);
        oneMoreTime.gameObject.SetActive(false);

        while(mummy.HP > 1)
            yield return null;

        yield return oneMoreTime.transform.ScaleUpObject(.2f, true);

        while (mummy != null)
        {
            yield return null;
        }
    }

    public TutorialRoom firstRoom;
    IEnumerator PlaceInRoom()
    {
        yield return spiderwebs[0].ScaleDownObject(.2f, true);
        firstRoom.enabled = true;
        firstRoom.ForceRequire(new Room.Requirements(1, 0, 0, 0));

        while(firstRoom.IsDirty)
            yield return null;
    }

    public IEnumerator MoveToView(int i, bool enter, bool animate = false)
    {
        if (enter)
            stages[i].gameObject.SetActive(true);

        var stage = stages[i];

        var instructions = stage.GetComponentsInChildren<TextMeshPro>(true);

        if (enter)
            foreach (var instruction in instructions)
                instruction.transform.localScale = Vector3.zero;
        else
            foreach (var instruction in instructions)
                instruction.ScaleObject(.2f, 1f, 0f);

        stage.gameObject.SetActive(true);
        float y = stage.position.y;
        if(animate)
            yield return new Timer(1.5f).GetRoutine(a => {
                a = -2f * a * a * a + 3f * a * a;
                stage.position = new Vector3(stage.position.x, y - (enter ? (1f - a) : a), stage.position.z);
                });
        else
            yield return new WaitForSeconds(.7f);

        if (enter)
            foreach (var instruction in instructions)
                instruction.ScaleObject(.5f, 0f, 1f);
        else
            stages[i].gameObject.SetActive(false);
    }

}
