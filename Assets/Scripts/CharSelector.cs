using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[DefaultExecutionOrder(1)]
public class CharSelector : Selectable
{
    public char Char => charIndex == 0 ? '.' : (char)('A' + charIndex - 1);
    public int CharIndex
    {
        get => charIndex;
        set
        {
            charIndex = value;
            OnTextChanged?.Invoke(Char.ToString());
            textChar.text = Char.ToString();
        }
    }

    public event Action<string> OnTextChanged;

    [SerializeField, NonReorderable]
    Graphic[] secondaryGraphics;

    int charIndex;
    TextMeshProUGUI textChar;

    protected override void Awake()
    {
        base.Awake();
        textChar = GetComponentInChildren<TextMeshProUGUI>();
        targetGraphic = textChar;
        textChar.text = Char.ToString();
    }

    public override void OnSelect(BaseEventData eventData)
    {
        base.OnSelect(eventData);
        foreach (Graphic graphic in secondaryGraphics)
        {
            graphic.color = colors.selectedColor;
        }
    }

    public override void OnDeselect(BaseEventData eventData)
    {
        base.OnDeselect(eventData);
        foreach (Graphic graphic in secondaryGraphics)
        {
            graphic.color = colors.normalColor;
        }
    }

    public override void OnMove(AxisEventData eventData)
    {
        if(eventData.moveVector.y != 0)
            Scroll((int)eventData.moveVector.y);

        base.OnMove(eventData);
    }

    void Scroll(int direction)
    {
        CharIndex = (charIndex + direction + 27) % 27;
    }
}
