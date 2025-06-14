using TMPro;
using UnityEngine;

[RequireComponent(typeof(TextMeshProUGUI))]
public class TextTemplate : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI textMeshPro;
    string template;

    private void Awake()
    {
        textMeshPro = GetComponent<TextMeshProUGUI>();
        template = textMeshPro.text;
    }

    internal void SetText(string message)
    {
        textMeshPro.text = string.Format(template, message.Split(','));
    }

    internal void SetText(params object[] args)
    {
        textMeshPro.text = string.Format(template, args);
    }
}
