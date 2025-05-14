using System.Collections;
using UnityEngine;

#nullable enable
public class LaundryMashine : MonoBehaviour, IInteractable
{
    [SerializeField] Material[] dirtyLaundryMat;

    Renderer rend;
    Transform visual, spawnPoint;
    public GameObject Visual => visual.gameObject;
    Renderer dirtyLaundryVisual;
    GameObject gooVisual;
    const CarriableTypeMask allowedCarriables = allowedLaundry | CarriableTypeMask.Goo;
    const CarriableTypeMask allowedLaundry = CarriableTypeMask.DirtyTowel | CarriableTypeMask.DirtyBedsheet;
    const CarriableType none = (CarriableType)(-1);

    CarriableType currentItem = none;
    bool hasGoo = false;

    void Awake()
    {
        visual = transform.Find("Visual");
        dirtyLaundryVisual = visual.Find("DirtyLaundry").GetComponent<Renderer>();
        gooVisual = visual.Find("Goo").gameObject;
        gooVisual.SetActive(false);
        dirtyLaundryVisual.enabled = false;
        spawnPoint = visual.Find("SpawnPoint");
        rend = visual.GetComponent<Renderer>();
    }

    [ContextMenu("Spawn")]
    public void Spawn()
    {
        SpawnLaundry(CarriableType.DirtyTowel);
    }

    IEnumerator LaunderingRoutine()
    {
        const float offset = Mathf.PI / 4f;
        yield return new Timer(6f).GetRoutinePro((a, t) =>
        {
            const float speed = 2f * Mathf.PI;
            t *= speed;
            visual.Squash(1f + Mathf.Sin(t - offset) * .2f);
        });

        StartCoroutine(gooVisual.transform.ScaleDownObject(.2f, true));

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

        SpawnLaundry(currentItem - 4);
        currentItem = none;

        yield return new Timer(.2f).GetRoutinePro((a, t) =>
        {
            a = (2f * a * a * a) - (3f * a * a) + 1f;

            visual.SetLocalPositionAndRotation(new Vector3(0f, a * jumpHeight, 0f), Quaternion.Euler(a * -45f, 0f, 0f));
            visual.Squash(1f + (a * .35f));

        });

        currentItem = none;
        hasGoo = false;
        dirtyLaundryVisual.enabled = false;
    }

    public bool CanHighlight(bool value, InteractiveHand interactiveHand)
    {
        var item = interactiveHand.ItemInHand;
        value &= IsAllowed(item) && (currentItem == none && allowedLaundry.IsInMask(item!.type) || (!hasGoo && item!.type == CarriableType.Goo));
        return value;
    }

    public bool Interact(Carriable carriable)
    {
        bool successful = false;

        if (currentItem == none && IsDirtyLaundry(carriable))
        {
            InsertLaundry(carriable.type);
            carriable.Destroy(transform.position, .1f);
            successful = true;
        }
        else if (!hasGoo && carriable.type == CarriableType.Goo)
        {
            carriable.Destroy(transform.position, .1f);
            StartCoroutine(gooVisual.transform.ScaleUpObject(.1f, true));
            hasGoo = true;
            successful = true;
        }

        if(successful && hasGoo && currentItem != none)
            StartCoroutine(LaunderingRoutine());

        return successful;
    }

    bool IsDirtyLaundry(Carriable? laundry) => laundry != null && allowedLaundry.IsInMask(laundry.type);
    bool IsAllowed(Carriable? laundry) => laundry != null && allowedCarriables.IsInMask(laundry.type);

    public void InsertLaundry(CarriableType type)
    {
        if (currentItem != none)
        {
            Debug.Log("LaundryMashine: Already has an item");
            return;
        }

        if (!allowedLaundry.IsInMask(type)) return;

        currentItem = type;
        dirtyLaundryVisual.material = dirtyLaundryMat[(int)type - 5];
        dirtyLaundryVisual.enabled = true;
        StartCoroutine(dirtyLaundryVisual.transform.ScaleUpObject(.3f));
    }

    void SpawnLaundry(CarriableType type)
    {
        var laundry = ResourceInfo.Instance.Get(type).prefab;
        var spawnedLaundry = laundry.Spawn(spawnPoint.position, Quaternion.identity, .1f);
        spawnedLaundry.SetVelocity(visual.forward * 5f);
    }
}
#nullable disable