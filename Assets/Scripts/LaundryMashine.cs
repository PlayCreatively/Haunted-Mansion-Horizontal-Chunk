using System.Collections;
using UnityEngine;
using static UnityEditor.Progress;

#nullable enable
public class LaundryMashine : MonoBehaviour, IInteractable
{
    [SerializeField] Material[] dirtyLaundryMat;

    Renderer rend;
    Transform visual, spawnPoint;
    Renderer dirtyLaundryVisual;
    const CarriableTypeMask allowedTypes = CarriableTypeMask.DirtyTowel | CarriableTypeMask.DirtyBedsheet;
    const CarriableType none = (CarriableType)(-1);

    public float speed, offset;

    CarriableType currentItem = none;

    void Awake()
    {
        visual = transform.Find("Visual");
        dirtyLaundryVisual = visual.Find("DirtyLaundry").GetComponent<Renderer>();
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

        SpawnLaundry(currentItem - 3);
        currentItem = none;

        yield return new Timer(.2f).GetRoutinePro((a, t) =>
        {
            a = (2f * a * a * a) - (3f * a * a) + 1f;

            visual.SetLocalPositionAndRotation(new Vector3(0f, a * jumpHeight, 0f), Quaternion.Euler(a * -45f, 0f, 0f));
            visual.Squash(1f + (a * .35f));

        });

        currentItem = none;
        dirtyLaundryVisual.enabled = false;
    }

    public void Highlight(bool value, InteractiveHand interactiveHand)
    {
        value &= IsCorrectItem(interactiveHand.ItemInHand);
        rend.material.color = value ? Color.yellow : Color.white;
    }

    public bool Interact(Carriable carriable)
    {
        if (currentItem == none && IsCorrectItem(carriable))
        {
            InsertLaundry(carriable.type);
            carriable.Destroy(transform.position, .1f);
            return true;
        }

        return false;
    }

    bool IsCorrectItem(Carriable? laundry) => laundry != null && allowedTypes.IsInMask(laundry.type);

    public void InsertLaundry(CarriableType type)
    {
        if (currentItem != none)
        {
            Debug.Log("LaundryMashine: Already has an item");
            return;
        }

        if (!allowedTypes.IsInMask(type)) return;

        currentItem = type;
        dirtyLaundryVisual.material = dirtyLaundryMat[(int)type - 4];
        dirtyLaundryVisual.enabled = true;
        StartCoroutine(dirtyLaundryVisual.transform.ScaleUpObject(.3f));
        StartCoroutine(LaunderingRoutine());
    }

    void SpawnLaundry(CarriableType type)
    {
        var laundry = GameSettings.Instance.GetResourceInfo(type).prefab;
        laundry = laundry.Spawn(spawnPoint.position, Quaternion.identity, .1f);
        laundry.SetVelocity(visual.forward * 5f);
    }
}
#nullable disable