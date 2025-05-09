using System;
using UnityEngine;
using UnityEngine.UI;


[Serializable]
public struct ResourceData
{
    public CarriableType type;
    public Sprite[] icons;
    public Carriable prefab;
}

[CreateAssetMenu(fileName = "ResourceIconData", menuName = "ScriptableObjects/ResourceIconData", order = 1)]
public class ResourceInfo : ScriptableObject
{
    static ResourceInfo _instance;
    public static ResourceInfo Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = Resources.Load<ResourceInfo>("ResourceInfo");
            }
            return _instance;
        }
    }
    [SerializeField] ResourceData[] resources = new ResourceData[4];

    public ResourceData Get(CarriableType type)
    {
        foreach (ResourceData resource in resources)
            if (resource.type == type)
                return resource;

        throw new ArgumentException($"Resource of type {type} not found.");
    }

#if UNITY_EDITOR
    [UnityEditor.MenuItem("Game/" + nameof(ResourceInfo))]
    public static void CreateAndShow()
    {
        if (!Instance)
        {
            _instance = CreateInstance<ResourceInfo>();
            UnityEditor.AssetDatabase.CreateAsset(Instance, "Assets/Resources/ResourceInfo.asset");
            UnityEditor.AssetDatabase.SaveAssets();
            UnityEditor.AssetDatabase.Refresh();
        }
        // open properties window
        UnityEditor.EditorUtility.OpenPropertyEditor(_instance);
    }
#endif
}