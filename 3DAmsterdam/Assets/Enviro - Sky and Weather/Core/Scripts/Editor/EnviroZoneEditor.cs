using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;


[CustomEditor(typeof(EnviroZone))]
public class EnviroEnviroZoneEditor : Editor {

	GUIStyle boxStyle;
	GUIStyle boxStyle2;
	GUIStyle wrapStyle;
	GUIStyle clearStyle;

	EnviroZone myTarget;

	bool showGizmo = true;
	bool showGeneral = true;
	bool showWeather = true;

	ReorderableList weatherList;

	void OnEnable()
	{
		myTarget = (EnviroZone)target;

		weatherList = new ReorderableList(serializedObject,serializedObject.FindProperty("zoneWeatherPresets"),true, true, true, true);
		weatherList.drawHeaderCallback = (Rect rect) =>
		{
			EditorGUI.LabelField(rect, "Weather Presets:");
		};

		weatherList.drawElementCallback =  
			(Rect rect, int index, bool isActive, bool isFocused) => {
			var element = weatherList.serializedProperty.GetArrayElementAtIndex(index);
			rect.y += 2;
			EditorGUI.PropertyField(new Rect(rect.x, rect.y, Screen.width * 0.8f, EditorGUIUtility.singleLineHeight),element,GUIContent.none);
		};

		weatherList.onAddCallback = (ReorderableList l) =>
		{
			var index = l.serializedProperty.arraySize;
			l.serializedProperty.arraySize++;
			l.index = index;
			//var element = l.serializedProperty.GetArrayElementAtIndex(index);
		};
	}

	public override void OnInspectorGUI ()
	{
		
		myTarget = (EnviroZone)target;

		//Set up the box style
		if (boxStyle == null)
		{
			boxStyle = new GUIStyle(GUI.skin.box);
			boxStyle.normal.textColor = GUI.skin.label.normal.textColor;
			boxStyle.fontStyle = FontStyle.Bold;
			boxStyle.alignment = TextAnchor.UpperLeft;
		}

		if (boxStyle2 == null)
		{
			boxStyle2 = new GUIStyle(GUI.skin.label);
			boxStyle2.normal.textColor = GUI.skin.label.normal.textColor;
			boxStyle2.fontStyle = FontStyle.Bold;
			boxStyle2.alignment = TextAnchor.UpperLeft;
		}
			
		//Setup the wrap style
		if (wrapStyle == null)
		{
			wrapStyle = new GUIStyle(GUI.skin.label);
			wrapStyle.fontStyle = FontStyle.Bold;
			wrapStyle.wordWrap = true;
		}

		if (clearStyle == null) {
			clearStyle = new GUIStyle(GUI.skin.label);
			clearStyle.normal.textColor = GUI.skin.label.normal.textColor;
			clearStyle.fontStyle = FontStyle.Bold;
			clearStyle.alignment = TextAnchor.UpperRight;
		}



		// Begin
		GUILayout.BeginVertical("", boxStyle);
		GUILayout.Space(10);
		myTarget.zoneName = EditorGUILayout.TextField ("Zone Name", myTarget.zoneName);
        GUILayout.Space(10);

		// General Setup
		GUILayout.BeginVertical("", boxStyle);
		showGeneral = EditorGUILayout.BeginToggleGroup ("General Configs", showGeneral);
		if (showGeneral) {
            myTarget.useMeshZone = EditorGUILayout.Toggle("Use Custom Mesh", myTarget.useMeshZone);
            if(myTarget.useMeshZone)
            myTarget.zoneMesh = (Mesh)EditorGUILayout.ObjectField("Mesh", myTarget.zoneMesh,typeof(Mesh),false);
			myTarget.zoneScale = EditorGUILayout.Vector3Field ("Zone Scale", myTarget.zoneScale);
            GUILayout.Space(10);
            myTarget.ExitToDefault = EditorGUILayout.Toggle("Exit to Default Zone", myTarget.ExitToDefault);
        }
		EditorGUILayout.EndToggleGroup ();
		EditorGUILayout.EndVertical ();

		// Weather Setup
		GUILayout.BeginVertical("", boxStyle);
		showWeather = EditorGUILayout.BeginToggleGroup ("Weather Configs", showWeather);
		if (showWeather) {

			//GUILayout.Space(15);
			myTarget.updateMode = (EnviroZone.WeatherUpdateMode)EditorGUILayout.EnumPopup ("Weather Update Mode", myTarget.updateMode);
			myTarget.WeatherUpdateIntervall = EditorGUILayout.FloatField ("Weather Update Interval", myTarget.WeatherUpdateIntervall);
			GUILayout.Space(10);
			weatherList.DoLayoutList();
			serializedObject.ApplyModifiedProperties();
		}
		EditorGUILayout.EndToggleGroup ();
		EditorGUILayout.EndVertical ();


		// Gizmo Setup
		GUILayout.BeginVertical("", boxStyle);
		showGizmo = EditorGUILayout.BeginToggleGroup ("Gizmo", showGizmo);
		if (showGizmo) {
			//GUILayout.Space(15);
			myTarget.zoneGizmoColor = EditorGUILayout.ColorField ("Gizmo Color", myTarget.zoneGizmoColor);
			//EditorGUILayout.EndVertical ();
		}
		EditorGUILayout.EndToggleGroup ();
		EditorGUILayout.EndVertical ();


		// END
		EditorGUILayout.EndVertical ();
		EditorUtility.SetDirty (target);
	}
}
