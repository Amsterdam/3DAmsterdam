using System;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
using System.Linq;
using System.IO;

namespace Amsterdam3D.Utilities
{
    public class SwapBuildTarget : MonoBehaviour
    {
        [MenuItem("3D Amsterdam/Set branch type/master")]
        public static void SwitchBranchMaster()
        {
            PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.WebGL,"BRANCH_MASTER");
            PlayerSettings.bundleVersion = "master";
            Debug.Log("Set scripting define symbols to BRANCH_MASTER");
        }
        [MenuItem("3D Amsterdam/Set branch type/develop")]
        public static void SwitchBranchDevelop()
        {
            PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.WebGL, "BRANCH_DEVELOP");
            PlayerSettings.bundleVersion = "develop";
            Debug.Log("Set scripting define symbols to BRANCH_DEVELOP");
        }
        [MenuItem("3D Amsterdam/Set branch type/feature")]
        public static void SwitchBranchFeature()
        {
            PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.WebGL, "BRANCH_FEATURE");

            var gitHeadFile = Application.dataPath + "/../../.git/HEAD";
            var headLine = File.ReadAllText(gitHeadFile);
            var positionLastSlash = headLine.LastIndexOf("/") + 1;
            var featureName = headLine.Substring(positionLastSlash, headLine.Length - positionLastSlash);

            PlayerSettings.bundleVersion = "feature/" + featureName;
            Debug.Log("Version set to feature name: " + Application.version);

            Debug.Log("Set scripting define symbols to BRANCH_FEATURE");
        }
        [MenuItem("3D Amsterdam/Build for WebGL platform")]
        public static void BuildWebGL()
        {
            TargetedBuild(BuildTarget.WebGL);
        }

        //Optional other future platform targets, for example desktop:
        /*[MenuItem("3D Amsterdam/Build for Windows 64 bit")]
        public static void BuildWindows()
        {
            TargetedBuild(BuildTarget.StandaloneWindows64);
        }*/

        public static void TargetedBuild(BuildTarget buildTarget = BuildTarget.WebGL)
        {           
            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions()
            {
                scenes = EditorBuildSettings.scenes.Select(scene => scene.path).ToArray(),
                target = buildTarget,
                locationPathName = "../../" + ((buildTarget==BuildTarget.WebGL) ? "BuildWebGL" : "BuildDesktop"),
                options = BuildOptions.AutoRunPlayer
            };            

            BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
            BuildSummary buildSummary = report.summary;

            if (buildSummary.result == BuildResult.Succeeded)
            {
                Debug.Log("Build succeeded: " + buildSummary.totalSize + " bytes");
            }

            if (buildSummary.result == BuildResult.Failed)
            {
                Debug.Log("Build failed");
            }
        }
    }
}