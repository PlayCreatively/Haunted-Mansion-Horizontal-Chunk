
using UnityEditor;
using UnityEditor.Build.Profile;
using UnityEditor.Build.Reporting;
using UnityEngine;

class Builder
{
    const string 
        path = "Builds/webGL/",
        pushCMD = "butler-windows/push.cmd",
        outputPath = "webGL-setup/";

    [MenuItem("Tools/🛠+⏩ | Build and Push", priority = 1)]
    public static void Build()
    {
        // Search for all scenes in the project.
        string[] levels = new string[EditorBuildSettings.scenes.Length];
        for (int i = 0; i < levels.Length; i++)
        {
            levels[i] = EditorBuildSettings.scenes[i].path;
        }

        // Build player.
        BuildPipeline.BuildPlayer(levels, path + "", BuildTarget.WebGL, BuildOptions.Development | BuildOptions.UncompressedAssetBundle);

        // Copy the Build folder to the webGL-setup folder.
        const string toCopy = "Build";
        FileUtil.ReplaceDirectory(path + toCopy, outputPath + toCopy);

        Push();
    }

    [MenuItem("Build/Build with Active Profile")]
    public static void Build2()
    {
        // Retrieve the currently active Build Profile
        BuildProfile activeProfile = BuildProfile.GetActiveBuildProfile();

        if (activeProfile == null)
        {
            Debug.LogError("No active Build Profile is set.");
            return;
        }

        // Set up the build options using the active profile
        BuildPlayerWithProfileOptions options = new BuildPlayerWithProfileOptions
        {
            buildProfile = activeProfile,
            locationPathName = "Builds/webGL",
            options = BuildOptions.None
        };

        // Initiate the build process
        BuildReport report = BuildPipeline.BuildPlayer(options);
        BuildSummary summary = report.summary;

        // Output the result of the build
        if (summary.result == BuildResult.Succeeded)
        {
            Debug.Log("Build succeeded: " + summary.totalSize + " bytes");
        }
        else if (summary.result == BuildResult.Failed)
        {
            Debug.LogError("Build failed");
            return;
        }


        // Copy the Build folder to the webGL-setup folder.
        const string toCopy = "Build";
        FileUtil.ReplaceDirectory(path + toCopy, outputPath + toCopy);

        Push();
    }

    [MenuItem("Tools/⏩ | Push to Itch", priority = -1)]
    public static void Push()
    {
        if (SystemInfo.operatingSystem.Contains("Windows") == false)
            Debug.Log("This operation only works on Windows. You are using " + SystemInfo.operatingSystem);

        // reveal push.cmd in explorer
        EditorUtility.RevealInFinder(Application.dataPath.Replace("Assets", "") + pushCMD);

        //// execute push.cmd
        //EditorUtility.OpenWithDefaultApp(Application.dataPath.Replace("Assets", "") + pushCMD);
    }
}

