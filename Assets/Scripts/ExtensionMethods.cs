using System.Collections;
using Unity.VisualScripting;
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
        return mono.StartCoroutine(mono.transform.ScaleObject(.1f, 0.05f));
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

    public static Vector3 Divide(this Vector3 a, Vector3 b)
    {
        return new Vector3(a.x / b.x, a.y / b.y, a.z / b.z);
    }
}
