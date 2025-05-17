using UnityEngine;

public class SkinSelector : MonoBehaviour
{
    SkinsSO[] skins;
    int currentSkinIndex;

    SkinnedMeshRenderer rend;

    void Awake()
    {
        skins = Resources.LoadAll<SkinsSO>("Skins");
        var body = transform.Find("Visuals/Body");
        rend = body.GetComponentInChildren<SkinnedMeshRenderer>();
        currentSkinIndex = PlayerPrefs.GetInt(gameObject.name + " CurrentSkinIndex", 0);
        SetSkin(currentSkinIndex);
    }

    void SetSkin(int skinIndex)
    {
        currentSkinIndex = (skinIndex + skins.Length) % skins.Length;
        PlayerPrefs.SetInt(gameObject.name + " CurrentSkinIndex", currentSkinIndex);
        PlayerPrefs.Save();
        Debug.Log($"{gameObject.name} Skin index: {skins[currentSkinIndex].name}");
        rend.material = skins[currentSkinIndex].material;
        rend.sharedMesh = skins[currentSkinIndex].mesh;
    }

    public void OnNext() => SetSkin(currentSkinIndex + 1);

    public void OnPrevious() => SetSkin(currentSkinIndex - 1);
}
