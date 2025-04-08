using UnityEditor;
using UnityEngine;

using UnityEditor.Overlays;
using UnityEditor.Toolbars;

[Overlay(typeof(SceneView), "Isometric Tools", true, defaultDockZone = DockZone.TopToolbar)]
public class IsometricToolsOverlay : ToolbarOverlay
{
    public IsometricToolsOverlay() : base(IsometricAngleButton.ID, SnapButton.ID) { }
}

[EditorToolbarElement(ID, typeof(SceneView))]
public class SnapButton : EditorToolbarButton
{
    public const string ID = "IsometricTools/SnapButton";

    public SnapButton()
    {
        text = "🧲";
        tooltip = "Default snap settings";
        //icon = EditorGUIUtility.IconContent("d_UnityEditor.SceneHierarchyWindow").image as Texture2D; // Change icon
        clicked += SetSnapSettings;
    }

    [MenuItem("Game/Isometric/Default snap settings")]
    static void SetSnapSettings()
    {
        EditorSnapSettings.gridSize
            = EditorSnapSettings.move
            = new Vector3(.1f, 1f, .1f);

        EditorSnapSettings.gridSnapEnabled = true;
        EditorSnapSettings.snapEnabled = true;
        EditorSnapSettings.rotate = 45f;
        EditorSnapSettings.scale = .1f;
    }
}

[EditorToolbarElement(ID, typeof(SceneView))]
public class IsometricAngleButton: EditorToolbarButton
{
    public const string ID = "IsometricTools/IsometricAngleButton";

    public IsometricAngleButton()
    {
        text = "🧭";
        tooltip = "Ctrl+Shift+I";
        //icon = EditorGUIUtility.IconContent("d_UnityEditor.SceneHierarchyWindow").image as Texture2D; // Change icon
        clicked += SetCameraOrientation;
    }

    [MenuItem("Game/Isometric/Set to Isometric Angle %#i")]
    static void SetCameraOrientation()
    {
        var sceneView = SceneView.lastActiveSceneView;
        if (sceneView != null)
        {
            sceneView.LookAt(sceneView.pivot, Quaternion.Euler(45, 45, 0), sceneView.size, true);
        }
    }
}