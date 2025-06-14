using GameManagers;
using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class SpiderwebBlocker : MonoBehaviour
{
    public int shiftUnlock = 1;
    Renderer rend;

    void Awake()
    {
        rend = GetComponent<Renderer>();
    }

    void OnEnable()
    {
        ShiftData.Instance.OnShiftEnd += OnShiftChanged;
    }

    void OnDisable()
    {
        ShiftData.Instance.OnShiftEnd -= OnShiftChanged;
    }

    void OnShiftChanged(int newShift)
    {
        if (newShift >= shiftUnlock)
            Destroy(gameObject);
    }

    public void Dissolve()
    {
        new Timer(0.5f).GetRoutine(a =>
        {
            rend.material.SetFloat("_DissolveAmount", a);
            rend.enabled = false;
        });
    }
}
