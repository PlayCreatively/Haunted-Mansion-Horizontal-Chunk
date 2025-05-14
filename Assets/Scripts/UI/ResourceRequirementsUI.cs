using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(ArcLayoutGroup))]
public class ResourceRequirementsUI : MonoBehaviour
{
    public Image[] resourceUI;
    public bool showNumbers = true;

    ArcLayoutGroup arcLayoutGroup;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        resourceUI = GetComponentsInChildren<Image>();
        arcLayoutGroup = GetComponent<ArcLayoutGroup>();
    }

    public void UpdateRequirements(Room.Requirements requirements)
    {
        for (int i = 0; i < requirements.Count; i++)
        {
            bool isRequired = requirements[i] > 0;

            resourceUI[i].gameObject.SetActive(isRequired);
            if(isRequired)
            {
                CarriableType type = (CarriableType)i;
                if (type == CarriableType.Soap)
                    resourceUI[i].sprite = ResourceInfo.Instance.Get(type).icons[0];
                else
                {
                    Sprite icon = showNumbers ? ResourceInfo.Instance.Get(type).icons[requirements[i] - 1] : ResourceInfo.Instance.Get(type).icons[0];
                    resourceUI[i].sprite = icon;
                }
            }
        }

        arcLayoutGroup.UpdateArc();
    }
}
