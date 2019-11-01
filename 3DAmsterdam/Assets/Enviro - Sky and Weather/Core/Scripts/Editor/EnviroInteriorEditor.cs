using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

[CustomEditor(typeof(EnviroInterior))]
public class EnviroInteriorEditor : Editor {

	GUIStyle boxStyle;
	GUIStyle wrapStyle;
    GUIStyle headerStyle;
    EnviroInterior myTarget;


	void OnEnable()
	{
		myTarget = (EnviroInterior)target;
	}
	
	public override void OnInspectorGUI ()
	{

		myTarget = (EnviroInterior)target;

		if (boxStyle == null) {
			boxStyle = new GUIStyle (GUI.skin.box);
			boxStyle.normal.textColor = GUI.skin.label.normal.textColor;
			boxStyle.fontStyle = FontStyle.Bold;
			boxStyle.alignment = TextAnchor.UpperLeft;
		}

		if (wrapStyle == null)
		{
			wrapStyle = new GUIStyle(GUI.skin.label);
			wrapStyle.fontStyle = FontStyle.Normal;
			wrapStyle.wordWrap = true;
			wrapStyle.alignment = TextAnchor.UpperLeft;
		}

        if (headerStyle == null)
        {
            headerStyle = new GUIStyle(GUI.skin.label);
            headerStyle.fontStyle = FontStyle.Bold;
            headerStyle.wordWrap = true;
            headerStyle.alignment = TextAnchor.UpperLeft;
        }


        GUILayout.BeginVertical("Enviro - Interior Zone", boxStyle);
		GUILayout.Space(20);
		EditorGUILayout.LabelField("Welcome to the Interior Zone for Enviro - Sky and Weather!", wrapStyle);
		GUILayout.EndVertical ();

		GUILayout.BeginVertical("Setup", boxStyle);
		GUILayout.Space(20);
        myTarget.zoneTriggerType = (EnviroInterior.ZoneTriggerType)EditorGUILayout.EnumPopup("Zone Trigger Type", myTarget.zoneTriggerType);

        if (GUILayout.Button ("Create New Trigger")) {
			myTarget.CreateNewTrigger ();
		} 

		for (int i = 0; i < myTarget.triggers.Count; i++) {

            if (myTarget.triggers[i] != null)
            {
                GUILayout.BeginVertical("", boxStyle);
                GUILayout.Space(10);
                myTarget.triggers[i].Name = EditorGUILayout.TextField("Name", myTarget.triggers[i].Name);
                GUILayout.Space(10);
                if (GUILayout.Button("Select"))
                {
                    Selection.activeObject = myTarget.triggers[i].gameObject;
                }
                if (GUILayout.Button("Remove"))
                {
                    myTarget.RemoveTrigger(myTarget.triggers[i]);
                }
                GUILayout.EndVertical();
            }

		}


		GUILayout.EndVertical ();
		GUILayout.BeginVertical("Lighting", boxStyle);
		GUILayout.Space(20);
		myTarget.directLighting = EditorGUILayout.BeginToggleGroup("Direct Light Modifications", myTarget.directLighting);
		myTarget.directLightingMod = EditorGUILayout.ColorField ("Direct Lighting Mod", myTarget.directLightingMod);
        myTarget.directLightFadeSpeed = EditorGUILayout.Slider("Direct Fading Speed", myTarget.directLightFadeSpeed, 0.01f, 100f);
        EditorGUILayout.EndToggleGroup ();
	
		myTarget.ambientLighting = EditorGUILayout.BeginToggleGroup("Ambient Light Modifications", myTarget.ambientLighting);
		myTarget.ambientLightingMod = EditorGUILayout.ColorField ("Ambient Sky Lighting Mod", myTarget.ambientLightingMod);
        if (EnviroSkyMgr.instance != null && EnviroSkyMgr.instance.IsAvailable())
        {
            if (EnviroSkyMgr.instance.LightSettings.ambientMode == UnityEngine.Rendering.AmbientMode.Trilight)
            {
                myTarget.ambientEQLightingMod = EditorGUILayout.ColorField("Ambient Equator Lighting Mod", myTarget.ambientEQLightingMod);
                myTarget.ambientGRLightingMod = EditorGUILayout.ColorField("Ambient Ground Lighting Mod", myTarget.ambientGRLightingMod);
            }
        }
        else
        {
            myTarget.ambientEQLightingMod = EditorGUILayout.ColorField("Ambient Equator Lighting Mod", myTarget.ambientEQLightingMod);
            myTarget.ambientGRLightingMod = EditorGUILayout.ColorField("Ambient Ground Lighting Mod", myTarget.ambientGRLightingMod);
        }

        myTarget.ambientLightFadeSpeed = EditorGUILayout.Slider("Ambient Fading Speed", myTarget.ambientLightFadeSpeed, 0.01f, 100f);
        EditorGUILayout.EndToggleGroup ();
		GUILayout.EndVertical ();
        GUILayout.BeginVertical("Skybox", boxStyle);
        GUILayout.Space(20);
        myTarget.skybox = EditorGUILayout.BeginToggleGroup("Skybox Modifications", myTarget.skybox);
        myTarget.skyboxColorMod = EditorGUILayout.ColorField("Skybox Color Mod", myTarget.skyboxColorMod);
        myTarget.skyboxFadeSpeed = EditorGUILayout.Slider("Skybox Fading Speed", myTarget.skyboxFadeSpeed, 0.01f, 100f);
        EditorGUILayout.EndToggleGroup();
        GUILayout.EndVertical();
        GUILayout.BeginVertical("Audio", boxStyle);
		GUILayout.Space(20);
		myTarget.ambientAudio = EditorGUILayout.BeginToggleGroup("Ambient Audio Modifications", myTarget.ambientAudio);
		myTarget.ambientVolume = EditorGUILayout.Slider ("Ambient Audio Mod", myTarget.ambientVolume,-1f,0f);
		EditorGUILayout.EndToggleGroup ();
		myTarget.weatherAudio = EditorGUILayout.BeginToggleGroup("Weather Audio Modifications", myTarget.weatherAudio);
		myTarget.weatherVolume = EditorGUILayout.Slider ("Weather Audio Mod", myTarget.weatherVolume,-1f,0f);
		EditorGUILayout.EndToggleGroup ();
        GUILayout.Space(20);
        GUILayout.Label("Zone Audio", headerStyle);
        myTarget.zoneAudioClip = (AudioClip)EditorGUILayout.ObjectField("Zone Ambient Clip", myTarget.zoneAudioClip, typeof(AudioClip), false);
        myTarget.zoneAudioVolume = EditorGUILayout.Slider("Zone Ambient Volume", myTarget.zoneAudioVolume, 0f, 1f);
        myTarget.zoneAudioFadingSpeed = EditorGUILayout.Slider("Zone Ambient Fading Speed", myTarget.zoneAudioFadingSpeed, 0.01f, 100f);
        GUILayout.EndVertical ();
        GUILayout.BeginVertical("Fog", boxStyle);
        GUILayout.Space(20);
        myTarget.fogFadeSpeed = EditorGUILayout.Slider("Fog Fading Speed", myTarget.fogFadeSpeed, 0.01f, 100f);
        myTarget.fog = EditorGUILayout.BeginToggleGroup("Fog Intensity Modifications", myTarget.fog);
        myTarget.minFogMod = EditorGUILayout.Slider("Fog Min Value", myTarget.minFogMod, 0f, 1f);
        EditorGUILayout.EndToggleGroup();
        myTarget.fogColor = EditorGUILayout.BeginToggleGroup("Fog Color Modifications", myTarget.fogColor);
        myTarget.fogColorMod = EditorGUILayout.ColorField("Fog Color Mod", myTarget.fogColorMod);
        EditorGUILayout.EndToggleGroup();
        GUILayout.EndVertical();

        GUILayout.BeginVertical("Weather Effects", boxStyle);
        GUILayout.Space(20);
        myTarget.weatherEffects = EditorGUILayout.BeginToggleGroup("Weather Effects Modifications", myTarget.weatherEffects);
        myTarget.weatherFadeSpeed = EditorGUILayout.Slider("Weather Effects Fading Speed", myTarget.weatherFadeSpeed, 0.01f, 100f);
        EditorGUILayout.EndToggleGroup();
        GUILayout.EndVertical();
    }
}
