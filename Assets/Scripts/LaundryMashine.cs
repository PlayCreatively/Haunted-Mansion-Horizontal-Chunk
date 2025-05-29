using GameManagers;
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
    int gooCount = 0;
    bool isLaundering = false;

    public int DefaultLayer { get; private set; }

    void Awake()
    {
        visual = transform.Find("Visual");
        DefaultLayer = Visual.layer;
        dirtyLaundryVisual = visual.Find("DirtyLaundry").GetComponent<Renderer>();
        gooVisual = visual.Find("Goo").gameObject;
        gooVisual.SetActive(false);
        dirtyLaundryVisual.enabled = false;
        spawnPoint = visual.Find("SpawnPoint");
        rend = visual.GetComponent<Renderer>();
    }

    IEnumerator LaunderingRoutine(CarriableType toSpawn)
    {
        float duration = GameSettings.Instance.laundryMachineTime;
        const float offset = Mathf.PI / 4f;
        yield return new Timer(duration * .6f).GetRoutinePro((a, t) =>
        {
            const float speed = 2f * Mathf.PI;
            t *= speed;
            visual.Squash(1f + Mathf.Sin(t - offset) * .2f);
        });

        if(gooCount != 2)
            StartCoroutine(gooVisual.transform.ScaleDownObject(.4f, true));

        yield return new Timer(duration * .2f).GetRoutinePro((a, t) =>
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

        if (gooCount == 2)
            StartCoroutine(gooVisual.transform.ScaleDownObject(.4f, true));

        SpawnLaundry(toSpawn);

        yield return new Timer(.2f).GetRoutinePro((a, t) =>
        {
            a = (2f * a * a * a) - (3f * a * a) + 1f;

            visual.SetLocalPositionAndRotation(new Vector3(0f, a * jumpHeight, 0f), Quaternion.Euler(a * -45f, 0f, 0f));
            visual.Squash(1f + (a * .35f));

        });

        currentItem = none;
        gooCount = 0;
        dirtyLaundryVisual.enabled = false;
        isLaundering = false;
    }

    public bool CanHighlight(bool value, InteractiveHand interactiveHand)
    {
        var item = interactiveHand.ItemInHand;
        value &= !isLaundering && IsAllowed(item) && (currentItem == none && allowedLaundry.IsInMask(item!.type) || (gooCount < 2 && item!.type == CarriableType.Goo));
        return value;
    }

    public bool Interact(Carriable carriable)
    {
        if(isLaundering) return false;

        bool successful = false;

        if (currentItem == none && IsDirtyLaundry(carriable))
        {
            successful = true;
            InsertLaundry(carriable.type);
            Debug.Log($"LaundryMashine: Inserted {carriable.type}");
            carriable.Destroy(transform.position, .1f);
        }
        else if (gooCount < 2 && carriable.type == CarriableType.Goo)
        {
            successful = true;
            carriable.Destroy(transform.position, .1f);
            if(gooCount == 0)
                StartCoroutine(gooVisual.transform.ScaleUpObject(.1f, true));
            gooCount++;
            Debug.Log($"LaundryMashine: Inserted Goo {gooCount}");
            if (gooCount == 2)
            {
                dirtyLaundryVisual.material = dirtyLaundryMat[2];
                dirtyLaundryVisual.enabled = true;
                isLaundering = true;
                StartCoroutine(LaunderingRoutine(CarriableType.Soap));
                return true;
            }

        }

        if(successful && gooCount == 1 && currentItem != none)
        {
            isLaundering = true;
            StartCoroutine(LaunderingRoutine(currentItem - 4));
        }

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
        FMODAudioManager.Instance.TriggerLaundryDoneSfx();
        var laundry = ResourceInfo.Instance.Get(type).prefab;
        var spawnedLaundry = laundry.Spawn(spawnPoint.position, Quaternion.identity, .1f);
        spawnedLaundry.SetVelocity(visual.forward * 5f);
    }
}
#nullable disable