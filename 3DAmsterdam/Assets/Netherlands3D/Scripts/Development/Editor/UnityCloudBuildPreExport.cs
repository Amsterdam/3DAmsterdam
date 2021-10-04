using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class UnityCloudBuildPreExport
{
    public static void Execute()
    {
        Debug.Log("Clearing existing unity defines");
        PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildPipeline.GetBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget), new string[] { });
        string customDefines = PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildPipeline.GetBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget));
        if (!string.IsNullOrWhiteSpace(customDefines))
        {
            Debug.LogError($"Unity player custom defines detected [{string.Join(";", customDefines)}]");
            throw new Exception($"Unity player custom defines detected [{string.Join(";", customDefines)}]");
        }
        else
            Debug.Log("Removed Unity player custom defines");
    }

}
