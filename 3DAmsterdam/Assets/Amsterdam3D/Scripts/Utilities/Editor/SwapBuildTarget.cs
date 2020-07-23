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
        public static void DataTarget(){}

        [MenuItem("3D Amsterdam/Set data target/Production")]
        public static void SwitchBranchMaster()
        {
            PlayerSettings.bundleVersion = "production"; //The place to assign release versioning
            PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.WebGL,"PRODUCTION");
            Debug.Log("Set scripting define symbols to PRODUCTION");
        }
        [MenuItem("3D Amsterdam/Set data target/Development")]
        public static void SwitchBranchDevelop()
        {
            PlayerSettings.bundleVersion = "develop";
            PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.WebGL, "DEVELOPMENT");
            Debug.Log("Set scripting define symbols to DEVELOPMENT");
        }
        [MenuItem("3D Amsterdam/Set data target/Specific feature")]
        public static void SwitchBranchFeature()
        {
            var gitHeadFile = Application.dataPath + "/../../.git/HEAD";
            var headLine = File.ReadAllText(gitHeadFile);
            Debug.Log("Reading git HEAD file:" + headLine);

            if (!headLine.Contains("feature/")){
                Debug.Log("Your branch does not seem to be a feature/ branch");
            }
            var positionLastSlash = headLine.LastIndexOf("/") + 1;
            var featureName = headLine.Substring(positionLastSlash, headLine.Length - positionLastSlash);

            PlayerSettings.bundleVersion = "feature/" + featureName;
            Debug.Log("Version set to feature name: " + Application.version);

            PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.WebGL, "DEVELOPMENT_FEATURE");
            Debug.Log("Set scripting define symbols to DEVELOPMENT_FEATURE");
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
            var verionSuffix = Application.version;

            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions()
            {
                scenes = EditorBuildSettings.scenes.Select(scene => scene.path).ToArray(),
                target = buildTarget,
                locationPathName = "../../" + ((buildTarget==BuildTarget.WebGL) ? "BuildWebGL_" : "BuildDesktop_") + verionSuffix,
                options = BuildOptions.AutoRunPlayer
            };            

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
}