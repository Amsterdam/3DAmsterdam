using System;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
using System.Linq;

namespace Amsterdam3D.Utilities
{
    public class SwapBuildTarget : MonoBehaviour
    {
		public enum BranchType{
			MASTER,
			DEVELOP,
			FEATURE
		}

	    public static void TargetedBuild(BuildTarget buildTarget = BuildTarget.WebGL, BranchType branchType = BranchType.FEATURE)
        {           
            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions()
            {
                scenes = EditorBuildSettings.scenes.Select(scene => scene.path).ToArray(),
                target = buildTarget,
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

        [MenuItem("3D Amsterdam/Build for WebGL")]
        public static void BuildWebGL()
        {
			TargetedBuild(BuildTarget.WebGL);
		}

		[MenuItem("3D Amsterdam/Build for Windows 64 bit")]
		public static void BuildWindows()
		{
			TargetedBuild(BuildTarget.StandaloneWindows64);
		}
    }
}