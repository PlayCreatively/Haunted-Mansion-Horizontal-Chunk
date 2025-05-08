using System.Collections;
using UnityEngine;

#nullable enable
public class GooMachine : MonoBehaviour, IInteractable
{
    [SerializeField] Carriable soapPrefab;

    Renderer rend;
    Transform visual, spawnPoint;
    bool hasGoo = false;

    void Awake()
    {
        visual = transform.Find("Visual");
        spawnPoint = visual.Find("SpawnPoint");
        rend = visual.GetComponent<Renderer>();
    }

    IEnumerator MixRoutine()
    {
        const float offset = Mathf.PI / 4f;
        yield return new Timer(6f).GetRoutinePro((a, t) =>
        {
            const float speed = 2f * Mathf.PI;
            t *= speed;
            visual.Squash(1f + Mathf.Sin(t - offset) * .2f);
        });

        yield return new Timer(2f).GetRoutinePro((a, t) =>
        {
            const float speed = 6f * Mathf.PI;
            t *= speed;
            visual.Squash(1f + Mathf.Sin(t - offset) * .35f);
            float y = 1f - Mathf.Cos(t);
            visual.localPosition = new Vector3(0f, y * .1f, 0f);
        });

        const float jumpHeight = .6f;

        yield return new Timer(.2f).GetRoutinePro((a, t) =>
        {
            a = -2f * a * a * a + 3f * a * a;

            visual.SetLocalPositionAndRotation(new Vector3(0f, a * jumpHeight, 0f), Quaternion.Euler(a * -45f, 0f, 0f));
            visual.Squash(1f + (a * .35f));
        });

        SpawnSoap();

        yield return new Timer(.2f).GetRoutinePro((a, t) =>
        {
            a = (2f * a * a * a) - (3f * a * a) + 1f;

            visual.SetLocalPositionAndRotation(new Vector3(0f, a * jumpHeight, 0f), Quaternion.Euler(a * -45f, 0f, 0f));
            visual.Squash(1f + (a * .35f));

        });

        hasGoo = false;
    }

    public bool Highlight(bool value, InteractiveHand interactiveHand)
    {
        value &= !hasGoo && IsGoo(interactiveHand.ItemInHand);
        rend.material.color = value ? Color.yellow : Color.white;

        return value;
    }

    public bool Interact(Carriable carriable)
    {
        bool successful = !hasGoo && IsGoo(carriable);

        if (successful)
        {
            hasGoo = true;
            carriable.Destroy(transform.position, .1f);
            StartCoroutine(MixRoutine());
        }

        return successful;
    }

    bool IsGoo(Carriable? carriable) => carriable != null && carriable.type == CarriableType.Goo;

    void SpawnSoap()
    {
        soapPrefab = soapPrefab.Spawn(spawnPoint.position, Quaternion.identity, .1f);
        soapPrefab.SetVelocity(visual.forward * 5f);
    }
}
#nullable disable