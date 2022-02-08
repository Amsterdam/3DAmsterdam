using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

public class VersionHelper : EditorWindow
{
    string versionNum;
    string lastVersion;
    bool changed;
    [MenuItem("T3D/Set version and build")]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof(VersionHelper));
    }

    void OnGUI()
    {
        if (!changed)
        {
            versionNum = PlayerSettings.bundleVersion;
            lastVersion = versionNum;
        }
        EditorGUIUtility.labelWidth = 80;
        versionNum = EditorGUILayout.TextField("Version", versionNum);
        if (lastVersion != versionNum)
        {
            changed = true;
        }
        if (changed)
        {
            if (GUILayout.Button("Set version and make a build"))
            {
                PlayerSettings.bundleVersion = versionNum;
                changed = false;

                MakeBuild(versionNum);

            }
        }
    }

    public static void MakeBuild(string versionname)
    {

        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions()
        {
            scenes = EditorBuildSettings.scenes.Select(scene => scene.path).ToArray(),
            target = BuildTarget.WebGL,
            locationPathName = @$"..\..\Builds\T3D {versionname}"
        };

        Debug.Log("Building to: " + buildPlayerOptions.locationPathName);

        BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
        BuildSummary buildSummary = report.summary;
        if (buildSummary.result == BuildResult.Succeeded)
        {
            Debug.Log("Build " + buildSummary.outputPath + " succeeded: " + buildSummary.totalSize + " bytes");            
        }

        if (buildSummary.result == BuildResult.Failed)
        {
            Debug.Log("Build failed");
        }
    }

}
