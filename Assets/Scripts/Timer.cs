using System;
using System.Collections;
using UnityEngine;

public struct Timer
{
    public readonly bool unscaled;
    float startTime, duration;

    public Timer(float duration, bool unscaled = false)
    {
        this.duration = duration;
        this.unscaled = unscaled;
        startTime = unscaled ? Time.unscaledTime : Time.time;
    }

    public void Start(float duration)
    {
        this.duration = duration;
        startTime = GetTime;
    }

    readonly float GetTime => unscaled ? Time.unscaledTime : Time.time;

    public void Restart()
    {
        startTime = GetTime;
    }

    public void Offset(float duration)
    {
        startTime += duration;
    }

    public readonly float SubNormal(float max)
    {
        return Mathf.Clamp01(UnClampedNormal / max);
    }

    public readonly IEnumerator GetRoutine(Action<float> action)
    {
        while (!Finished)
        {
            yield return null;
            action.Invoke(Normal);
        }
    }

    public readonly IEnumerator GetRoutinePro(Action<float, float> action)
    {
        while (!Finished)
        {
            yield return null;
            action.Invoke(Normal, GetTime - startTime);
        }
    }

    public readonly bool Finished => GetTime >= startTime + duration;

    public readonly float Normal => Mathf.Clamp01(UnClampedNormal);
    public readonly float Inverse => 1f - Normal;
    public readonly float UnClampedNormal => (GetTime - startTime) / duration;

    public static implicit operator bool (Timer timer)
    {
        return timer.Finished;
    }

    public static implicit operator float (Timer timer)
    {
        return timer.Normal;
    }
}
