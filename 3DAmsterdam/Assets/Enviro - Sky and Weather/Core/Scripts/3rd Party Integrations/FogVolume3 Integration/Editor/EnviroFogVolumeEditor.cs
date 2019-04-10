using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(EnviroFogVolumeIntegration))]
public class EnviroFogVolumeEditor : Editor {
#if ENVIRO_FV3_SUPPORT
	private GUIStyle boxStyle; 
	private GUIStyle wrapStyle;
	private GUIStyle headerStyle;

	SerializedObject serializedObj;
	private EnviroFogVolumeIntegration myTarget;

	SerializedProperty clouds,fog,cloudsPosition,fogPosition,moveWithPlayer;
	SerializedProperty coverageMult, absorbtionMult, visibilityMult, renderIntensityMult, fogColorPower;

	void OnEnable()
	{
		myTarget = (EnviroFogVolumeIntegration)target;
		serializedObj = new SerializedObject (myTarget);

		clouds = serializedObj.FindProperty ("clouds");
		fog = serializedObj.FindProperty ("fog");
		moveWithPlayer = serializedObj.FindProperty ("moveWithPlayer");
		cloudsPosition = serializedObj.FindProperty ("cloudsPosition");
		fogPosition = serializedObj.FindProperty ("fogPosition");
		absorbtionMult = serializedObj.FindProperty ("absorbtionMult");
		visibilityMult = serializedObj.FindProperty ("visibilityMult");
		renderIntensityMult = serializedObj.FindProperty ("renderIntensityMult");
		coverageMult = serializedObj.FindProperty ("coverageMult");
		fogColorPower = serializedObj.FindProperty ("fogColorPower");
	}


	public override void OnInspectorGUI ()
	{
		if (boxStyle == null)
		{
			boxStyle = new GUIStyle(GUI.skin.box);
			boxStyle.normal.textColor = GUI.skin.label.normal.textColor;
			boxStyle.fontStyle = FontStyle.Bold;
			boxStyle.alignment = TextAnchor.UpperLeft;
		}

		if (wrapStyle == null)
		{
			wrapStyle = new GUIStyle(GUI.skin.label);
			wrapStyle.fontStyle = FontStyle.Normal;
			wrapStyle.wordWrap = true;
		}

		if (headerStyle == null)
		{
			headerStyle = new GUIStyle(GUI.skin.label);
			headerStyle.fontStyle = FontStyle.Bold;
			headerStyle.wordWrap = true;
		}

#if UNITY_5_6_OR_NEWER
		serializedObj.UpdateIfRequiredOrScript ();
#else
		serializedObj.UpdateIfDirtyOrScript ();
#endif
		EditorGUI.BeginChangeCheck ();
		GUILayout.BeginVertical("Enviro - FogVolume 3 Integration", boxStyle);
		GUILayout.Space(20);
		EditorGUILayout.LabelField("Welcome to the FogVolume 3 Integration for Enviro - Sky and Weather!", wrapStyle);
		GUILayout.EndVertical ();

		GUILayout.BeginVertical("Setup", boxStyle);
		GUILayout.Space(20);
#if GAIA_PRESENT
		if (GUILayout.Button ("Get from GAIA")) {
			myTarget.GetFromGaia ();
		}
#endif
		GUILayout.BeginHorizontal ("",boxStyle);
		if (GUILayout.Button ("Create Clouds")) {
			myTarget.CreateCloudLayer ();
		}
		if (GUILayout.Button ("Create Uniform Fog")) {
			myTarget.CreateUniformFog ();
		}
		//if (GUILayout.Button ("Create Textured Fog")) {
		//	myTarget.CreateTexturedFog ();
		//}
		GUILayout.EndHorizontal ();
		EditorGUILayout.PropertyField (clouds, true, null);
		EditorGUILayout.PropertyField (fog, true, null);
		EditorGUILayout.PropertyField (cloudsPosition, true, null);
		EditorGUILayout.PropertyField (fogPosition, true, null);
		GUILayout.EndVertical ();
		GUILayout.BeginVertical ("Movement Settings",boxStyle);
		GUILayout.Space(20);
		EditorGUILayout.PropertyField (moveWithPlayer, true, null);
		GUILayout.EndVertical ();
		GUILayout.BeginVertical("Controls", boxStyle);
		GUILayout.Space(20);
		EditorGUILayout.PropertyField (coverageMult, true, null);
		EditorGUILayout.PropertyField (absorbtionMult, true, null);
		GUILayout.Space(10);
		EditorGUILayout.LabelField ("Fog Runtime Setting",headerStyle);
		if(myTarget.fogMode == EnviroFogVolumeIntegration.CurrentFogMode.None)
			EditorGUILayout.LabelField ("Current Fog Mode: None");
		else if(myTarget.fogMode == EnviroFogVolumeIntegration.CurrentFogMode.Uniform)
			EditorGUILayout.LabelField ("Current Fog Mode: Uniform");
		else 
			EditorGUILayout.LabelField ("Current Fog Mode: Textured");
		GUILayout.Space(10);
		EditorGUILayout.PropertyField (fogColorPower, true, null);
		if(myTarget.fogMode == EnviroFogVolumeIntegration.CurrentFogMode.Uniform)
		EditorGUILayout.PropertyField (visibilityMult, true, null);
		if(myTarget.fogMode == EnviroFogVolumeIntegration.CurrentFogMode.Textured)
		EditorGUILayout.PropertyField (renderIntensityMult, true, null);
		GUILayout.EndVertical ();

		if (EditorGUI.EndChangeCheck ()) {
			serializedObj.ApplyModifiedProperties ();
		}
	}
#endif
}
