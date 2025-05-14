using System.Collections;
using UnityEngine;

public static class ExtensionMethods
{
    public static Vector2 XZ(this Vector3 v)
    {
        return new(v.x, v.z);
    }

    public static Vector3 XZ(this Vector3 v, float y)
    {
        return new(v.x, y, v.z);
    }

    public static Vector3 XZ(this Vector2 v, float y = 0)
    {
        return new(v.x, y, v.y);
    }

    public static Rect CropToBounds(this Rect rect, Rect bounds)
    {
        // Compute the overlap on each axis.
        float xMin = Mathf.Max(rect.xMin, bounds.xMin);
        float xMax = Mathf.Min(rect.xMax, bounds.xMax);
        float yMin = Mathf.Max(rect.yMin, bounds.yMin);
        float yMax = Mathf.Min(rect.yMax, bounds.yMax);

        // If there’s no overlap, return an “empty” rect.
        if (xMax <= xMin || yMax <= yMin)
            return Rect.zero;   // or use new Rect() if you prefer (same thing).

        // Otherwise return the intersection.
        return Rect.MinMaxRect(xMin, yMin, xMax, yMax);
    }

    public static Rect Restrict(this Rect rect, Rect bounds)
    {
        // 1. Normalise so width & height are guaranteed positive
        if (rect.width < 0) { rect.x += rect.width; rect.width = -rect.width; }
        if (rect.height < 0) { rect.y += rect.height; rect.height = -rect.height; }

        // 2. If the rect is larger than the bounds, trim it first
        rect.width = Mathf.Min(rect.width, bounds.width);
        rect.height = Mathf.Min(rect.height, bounds.height);

        // 3. Slide it so it fits
        float minX = Mathf.Clamp(rect.xMin, bounds.xMin, bounds.xMax - rect.width);
        float minY = Mathf.Clamp(rect.yMin, bounds.yMin, bounds.yMax - rect.height);

        return new Rect(minX, minY, rect.width, rect.height);
    }

    public static Rect ExpandToRatio(this Rect rect, float aspectRatio)
    {
        Vector2 size = rect.width / rect.height < aspectRatio ?
            new Vector2(rect.height * aspectRatio, rect.height) :
            new Vector2(rect.width, rect.width / aspectRatio);
            return new Rect(rect.center - size * .5f, size);
    }

    public static void Squash(this Transform trans, float yNormal)
    {
        float xNormal = 2f - yNormal;

        trans.localScale = new(xNormal, yNormal, xNormal);
    }

    public static bool IsInMask(this CarriableTypeMask mask, CarriableType type)
    {
        CarriableTypeMask typeAsMask = (CarriableTypeMask)(1 << (int)type);
        return (typeAsMask & mask) > 0;
    }

    public static T Spawn<T>(this T prefab, Vector3 position, Quaternion rotation, float time = .2f, Transform parent = null) where T : MonoBehaviour
    {
        T instance = Object.Instantiate(prefab, position, rotation, parent);

        instance.StartCoroutine(instance.transform.ScaleUpObject(time));

        return instance;
    }

    public static void Destroy<T>(this T carriable, Vector3 destination, float time = .2f) where T : Carriable
    {
        carriable.EnablePhysics(false);

        Vector3 startPos = carriable.transform.position;

        IEnumerator Destroy()
        {
            carriable.Destroy();
            yield break;
        }

        IEnumerator moveAndScale = WaitForAll(carriable.transform.ScaleDownObject(time), new Timer(time).GetRoutine(a => carriable.transform.position = Vector3.Lerp(startPos, destination, a)));
        IEnumerator thenDestroy = InOrder(moveAndScale, Destroy());

        carriable.StartCoroutine(thenDestroy);
    }

    static IEnumerator WaitForAll(params IEnumerator[] routines)
    {
        bool running = true;

        while (running)
        {
            foreach (var routine in routines)
                running &= routine.MoveNext();
            yield return null;
        }
    }

    static IEnumerator InOrder(params IEnumerator[] routines)
    {
        foreach (var routine in routines)
            while (routine.MoveNext())
                yield return null;
    }

    public static IEnumerator Then(this IEnumerator first, IEnumerator then)
    {
        while (first.MoveNext())
            yield return null;
        while (then.MoveNext())
            yield return null;
    }

    public static IEnumerator Then(this IEnumerator first, System.Action then)
    {
        while (first.MoveNext())
            yield return null;
        then.Invoke();
    }

    public static IEnumerator ScaleObject(this Transform trans, float duration, bool scaleUp)
    {
        return scaleUp ? trans.ScaleUpObject(duration) : trans.ScaleDownObject(duration);
    }

    public static IEnumerator ScaleObject(this Transform trans, float duration, float scale)
    {
        Vector3 startScale = trans.localScale;
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            t = Mathf.Clamp01(t);
            float smoothT = -2f * t * t * t + 3f * t * t;
            trans.localScale = Vector3.Lerp(startScale, Vector3.one * scale, smoothT);
            yield return null;
        }
        trans.localScale = Vector3.one * scale;
    }

    public static Coroutine ScaleObject<T>(this T mono, float duration, float scale) where T : MonoBehaviour
    {
        return mono.StartCoroutine(mono.transform.ScaleObject(duration, scale));
    }

    public static Coroutine ScaleObject<T>(this T mono, float duration, float from, float to) where T : MonoBehaviour
    {
        mono.transform.localScale = Vector3.one * from;
        return mono.StartCoroutine(mono.transform.ScaleObject(duration, to));
    }


    public static IEnumerator ScaleUpObject(this Transform trans, float duration, bool changeActive = false)
    {
        if (changeActive)
            trans.gameObject.SetActive(true);

        Vector3 defaultScale = trans.localScale;
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            t = Mathf.Clamp01(t);
            trans.localScale = defaultScale * t;
            yield return null;
        }
        trans.localScale = defaultScale;
    }

    public static IEnumerator ScaleDownObject(this Transform trans, float duration, bool changeActive = false)
    {
        Vector3 defaultScale = trans.localScale;
        float t = 1f;
        while (t > 0f)
        {
            t -= Time.deltaTime / duration;
            t = Mathf.Clamp01(t);
            trans.localScale = defaultScale * t;
            yield return null;
        }
        trans.localScale = Vector3.zero;
        yield return null;
        trans.localScale = defaultScale;

        if (changeActive)
            trans.gameObject.SetActive(false);
    }

    public static void SetLayerRecursively(this GameObject root, int layer)
    {
        root.layer = layer;

        foreach (Transform child in root.transform)
            child.gameObject.SetLayerRecursively(layer);
    }

    public static Vector3 Divide(this Vector3 a, Vector3 b)
    {
        return new Vector3(a.x / b.x, a.y / b.y, a.z / b.z);
    }
}
