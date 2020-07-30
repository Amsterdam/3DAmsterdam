using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
class BuildSettings
{
	static BuildSettings()
	{
		PlayerSettings.WebGL.threadsSupport = false;
	}
}