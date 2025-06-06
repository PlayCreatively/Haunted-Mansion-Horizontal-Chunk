using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

[DefaultExecutionOrder(+10)]
public class SkinSelector : MonoBehaviour
{
    static SkinsSO[] _skins;
    static SkinsSO[] Skins
    {
        get
        {
            if (_skins == null || _skins.Length == 0)
                _skins = Resources.LoadAll<SkinsSO>("Skins");
            return _skins;
        }
    }
    int currentSkinIndex;

    SkinnedMeshRenderer rend;

    void Awake()
    {
        var body = transform.Find("Visuals/Body");
        rend = body.GetComponentInChildren<SkinnedMeshRenderer>();
        currentSkinIndex = PlayerPrefs.GetInt(gameObject.name + " CurrentSkinIndex", 0);
        SetSkin(currentSkinIndex);
    }

    void SetSkin(int skinIndex)
    {
        currentSkinIndex = (skinIndex + Skins.Length) % Skins.Length;
        PlayerPrefs.SetInt(gameObject.name + " CurrentSkinIndex", currentSkinIndex);
        PlayerPrefs.Save();
        Debug.Log($"{gameObject.name} Skin index: {Skins[currentSkinIndex].name}");
        rend.material = Skins[currentSkinIndex].material;
        rend.sharedMesh = Skins[currentSkinIndex].mesh;
    }

    public static void ReassignDefaultSkin(Player player)
        => SetSkin(player, PlayerPrefs.GetInt(player.gameObject.name + " CurrentSkinIndex", 0));

    public static void SetSkin(Player player, int skinIndex)
    {
        var rend = player.MeshRenderer;
        skinIndex = (skinIndex + Skins.Length) % Skins.Length;
        PlayerPrefs.SetInt(player.gameObject.name + " CurrentSkinIndex", skinIndex);
        PlayerPrefs.Save();
        Debug.Log($"{player.gameObject.name} Skin index: {Skins[skinIndex].name}");
        rend.material = Skins[skinIndex].material;
        rend.sharedMesh = Skins[skinIndex].mesh;
    }

    public static int GetSkinIndex(int playerIndex) 
        => PlayerPrefs.GetInt("Player " + playerIndex + " CurrentSkinIndex", 9);

    public static void SetSkin(SkinnedMeshRenderer rend, int skinIndex)
    {
        skinIndex = (skinIndex + Skins.Length) % Skins.Length;
        rend.material = Skins[skinIndex].material;
        rend.sharedMesh = Skins[skinIndex].mesh;
    }

    void OnEnable()
    {
        GetComponent<PlayerInput>().onActionTriggered += HandleInput;
    }

    void OnDisable()
    {
        GetComponent<PlayerInput>().onActionTriggered -= HandleInput;
    }

    private void HandleInput(InputAction.CallbackContext context)
    {
        if (context.action.name == "Next" && context.performed)
        {
            OnNext();
        }
        else if (context.action.name == "Previous" && context.performed)
        {
            OnPrevious();
        }
    }

    public void OnNext() => SetSkin(currentSkinIndex + 1);

    public void OnPrevious() => SetSkin(currentSkinIndex - 1);
}
