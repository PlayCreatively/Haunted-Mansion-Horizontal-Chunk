using UnityEngine;
using UnityEngine.Events;

[DefaultExecutionOrder(2)]
public class NameInput: MonoBehaviour
{

    CharSelector[] charSelectors;

    void Awake()
    {
        charSelectors = GetComponentsInChildren<CharSelector>();

        foreach (CharSelector charSelector in charSelectors)
            charSelector.OnTextChanged += _ => OnValueChanged.Invoke(GetName());

        charSelectors[0].CharIndex = 1;
    }

    public UnityEvent<string> OnValueChanged;

    public string GetName()
    {
        char[] name = new char[charSelectors.Length];

        for (int i = 0; i < name.Length; i++)
            name[i] = charSelectors[i].Char;

        return new string(name);
    }
}
