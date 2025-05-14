using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#nullable enable
public class SoapMachine : MonoBehaviour, IInteractable
{
    [SerializeField] Carriable soapPrefab;

    Renderer rend;
    Transform visual, spawnPoint;
    bool hasGoo = false;
    Animator[] animators;

    public GameObject Visual => visual.gameObject;

    void Awake()
    {
        visual = transform.Find("Visual");
        spawnPoint = transform.Find("SpawnPoint");
        rend = visual.GetComponent<Renderer>();
        animators = GetAllAnimators(visual).ToArray();
        foreach (var anim in animators)
        {
            anim.enabled = false;
        }
    }

    IEnumerable<Animator> GetAllAnimators(Transform trans)
    {
        foreach (Transform child in trans)
            if (child.TryGetComponent(out Animator anim))
                yield return anim;
            else
                foreach (var nestedAnim in GetAllAnimators(child))
                    yield return nestedAnim;
    }

    IEnumerator MixRoutine()
    {
        float duration = GameSettings.Instance.soapMachineTime;

        foreach (var anim in animators)
        {
            anim.enabled = true;
            // start from beginning

        }

        yield return new WaitForSeconds(duration);

        SpawnSoap();

        //yield return new Timer(.2f).GetRoutinePro((a, t) =>
        //{
        //    a = (2f * a * a * a) - (3f * a * a) + 1f;

        //    visual.SetLocalPositionAndRotation(new Vector3(0f, a * jumpHeight, 0f), Quaternion.Euler(a * -45f, 0f, 0f));
        //    visual.Squash(1f + (a * .35f));

        //});

        foreach (var anim in animators)
        {
            anim.enabled = false;
        }

        hasGoo = false;

        yield break;
    }

    public bool CanHighlight(bool value, InteractiveHand interactiveHand)
    {
        value &= !hasGoo && IsGoo(interactiveHand.ItemInHand);

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
        var spawnedSoap = soapPrefab.Spawn(spawnPoint.position, spawnPoint.rotation, .1f);
        spawnedSoap.SetVelocity((spawnPoint.right + Vector3.up) * 3f);
    }
}
#nullable disable