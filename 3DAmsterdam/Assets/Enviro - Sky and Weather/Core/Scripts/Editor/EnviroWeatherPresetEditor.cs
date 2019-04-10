using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;


[CustomEditor(typeof(EnviroWeatherPreset))]
public class EnviroWeatherPresetEditor : Editor {

	GUIStyle boxStyle;
	GUIStyle boxStyle2;
	GUIStyle wrapStyle;
	GUIStyle clearStyle;

	EnviroWeatherPreset myTarget;

	public bool showAudio = false;
	public bool showFog = false;
	public bool showSeason = false;
	public bool showClouds = false;
	public bool showGeneral = false;
    public bool showPostProcessing = false;

    SerializedObject serializedObj;
	SerializedProperty fogMod;
	SerializedProperty skyMod;
	SerializedProperty lightMod;
	void OnEnable()
	{
		myTarget = (EnviroWeatherPreset)target;

		serializedObj = new SerializedObject (myTarget);
		fogMod = serializedObj.FindProperty ("weatherFogMod");
		skyMod = serializedObj.FindProperty ("weatherSkyMod");
		lightMod = serializedObj.FindProperty ("weatherLightMod");
	}

	public override void OnInspectorGUI ()
	{

		myTarget = (EnviroWeatherPreset)target;
		#if UNITY_5_6_OR_NEWER
		serializedObj.UpdateIfRequiredOrScript ();
		#else
		serializedObj.UpdateIfDirtyOrScript ();
		#endif
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
		myTarget.Name = EditorGUILayout.TextField ("Name", myTarget.Name);
		GUILayout.Space(10);

		// General Setup
		GUILayout.BeginVertical("", boxStyle);
		showGeneral = EditorGUILayout.BeginToggleGroup ("General Configs", showGeneral);
		if (showGeneral) {
			GUILayout.BeginVertical("Sky and Light Color", boxStyle);
			GUILayout.Space(15);
			EditorGUI.BeginChangeCheck();
			EditorGUILayout.PropertyField(skyMod, true, null);
			EditorGUILayout.PropertyField(lightMod, true, null);
            myTarget.volumeLightIntensity = EditorGUILayout.Slider("Volume Light Intensity", myTarget.volumeLightIntensity, 0, 2);
            myTarget.shadowIntensityMod = EditorGUILayout.Slider("Shadow Intensity Mod", myTarget.shadowIntensityMod, -1f, 1f);
            if (EditorGUI.EndChangeCheck())
			{
				serializedObj.ApplyModifiedProperties();
			}
			EditorGUILayout.EndVertical ();
			GUILayout.BeginVertical("Weather Condition", boxStyle);
			GUILayout.Space(15);
			myTarget.WindStrenght = EditorGUILayout.Slider("Wind Intensity",myTarget.WindStrenght,0f,1f);
			myTarget.wetnessLevel = EditorGUILayout.Slider("Maximum Wetness",myTarget.wetnessLevel,0f,1f);
			myTarget.snowLevel = EditorGUILayout.Slider("Maximum Snow",myTarget.snowLevel,0f,1f);
            myTarget.temperatureLevel = EditorGUILayout.Slider("Temperature Modification", myTarget.temperatureLevel, -50f, 50f);
            myTarget.isLightningStorm = EditorGUILayout.Toggle ("Lightning Storm", myTarget.isLightningStorm);
			myTarget.lightningInterval = EditorGUILayout.Slider("Lightning Interval",myTarget.lightningInterval,2f,60f);
			EditorGUILayout.EndVertical ();
			GUILayout.BeginVertical("Particle Effects", boxStyle);
			GUILayout.Space(15);
			if (!Application.isPlaying) {
				if (GUILayout.Button ("Add")) {
					myTarget.effectSystems.Add (new EnviroWeatherEffects ());
				}
			} else
				EditorGUILayout.LabelField ("Can't add effects in runtime!");
			for (int i = 0; i < myTarget.effectSystems.Count; i++) {
				GUILayout.BeginVertical ("Effect " + (i+1), boxStyle);
				GUILayout.Space(15);
				myTarget.effectSystems[i].prefab = (GameObject)EditorGUILayout.ObjectField ("Effect Prefab", myTarget.effectSystems[i].prefab, typeof(GameObject), true);
				myTarget.effectSystems [i].localPositionOffset = EditorGUILayout.Vector3Field ("Position Offset", myTarget.effectSystems [i].localPositionOffset);
				myTarget.effectSystems [i].localRotationOffset = EditorGUILayout.Vector3Field ("Rotation Offset", myTarget.effectSystems [i].localRotationOffset);
				if (GUILayout.Button ("Remove")) {
					myTarget.effectSystems.Remove (myTarget.effectSystems[i]);
				}
				GUILayout.EndVertical ();
			}
			EditorGUILayout.EndVertical ();

		}
		EditorGUILayout.EndToggleGroup ();
		EditorGUILayout.EndVertical ();


		// Season Setup
		GUILayout.BeginVertical("", boxStyle);
		showSeason = EditorGUILayout.BeginToggleGroup ("Season Configs", showSeason);
		if (showSeason) {

			myTarget.Spring = EditorGUILayout.Toggle ("Spring",myTarget.Spring);
			if (myTarget.Spring)
				myTarget.possibiltyInSpring = EditorGUILayout.Slider ("Spring Possibility",myTarget.possibiltyInSpring, 0, 100);
			myTarget.Summer = EditorGUILayout.Toggle ("Summer",myTarget.Summer);
			if (myTarget.Summer)
				myTarget.possibiltyInSummer = EditorGUILayout.Slider ("Summer Possibility",myTarget.possibiltyInSummer, 0, 100);
			myTarget.Autumn = EditorGUILayout.Toggle ("Autumn",myTarget.Autumn);
			if (myTarget.Autumn)
				myTarget.possibiltyInAutumn = EditorGUILayout.Slider ("Autumn Possibility",myTarget.possibiltyInAutumn, 0, 100);
			myTarget.winter = EditorGUILayout.Toggle ("Winter",myTarget.winter);
			if (myTarget.winter)
				myTarget.possibiltyInWinter = EditorGUILayout.Slider ("Winter Possibility",myTarget.possibiltyInWinter, 0, 100);
			//Add Content
		}
		EditorGUILayout.EndToggleGroup ();
		EditorGUILayout.EndVertical ();



		// Clouds Setup
		GUILayout.BeginVertical("", boxStyle);
		showClouds = EditorGUILayout.BeginToggleGroup ("Clouds Configs", showClouds);
		if (showClouds) {
            //Add Cloud Stuff
#if ENVIRO_HD
            if (EnviroSkyMgr.instance == null || EnviroSkyMgr.instance.currentEnviroSkyVersion == EnviroSkyMgr.EnviroSkyVersion.HD)
            {
                GUILayout.BeginVertical("Volume Clouds", boxStyle);
                GUILayout.Space(20);
                myTarget.cloudsConfig.ambientTopColorBrightness = EditorGUILayout.Slider("Ambient Top Color Brightness", myTarget.cloudsConfig.ambientTopColorBrightness, 0f, 3f);
                myTarget.cloudsConfig.ambientbottomColorBrightness = EditorGUILayout.Slider("Ambient Bottom Color Brightness", myTarget.cloudsConfig.ambientbottomColorBrightness,0f,3f);
                GUILayout.Space(10);
                myTarget.cloudsConfig.alphaCoef = EditorGUILayout.Slider("Alpha Factor", myTarget.cloudsConfig.alphaCoef, 1f, 4f);
                myTarget.cloudsConfig.scatteringCoef = EditorGUILayout.Slider("Light Scattering Factor", myTarget.cloudsConfig.scatteringCoef, 0f, 2f);
                myTarget.cloudsConfig.skyBlending = EditorGUILayout.Slider("Sky Blending Factor", myTarget.cloudsConfig.skyBlending, 0.25f, 1f);
                GUILayout.Space(10);
                myTarget.cloudsConfig.coverage = EditorGUILayout.Slider("Coverage", myTarget.cloudsConfig.coverage, -1f, 2f);
                myTarget.cloudsConfig.coverageHeight = EditorGUILayout.Slider("Coverage Height", myTarget.cloudsConfig.coverageHeight, 0f, 2f);
                myTarget.cloudsConfig.raymarchingScale = EditorGUILayout.Slider("Raymarch Step Modifier", myTarget.cloudsConfig.raymarchingScale, 0.25f, 1f);
                myTarget.cloudsConfig.density = EditorGUILayout.Slider("Density", myTarget.cloudsConfig.density, 1f, 2f);
                myTarget.cloudsConfig.cloudType = EditorGUILayout.Slider("Cloud Type", myTarget.cloudsConfig.cloudType, 0f, 1f);
                EditorGUILayout.EndVertical();
                GUILayout.BeginVertical("Flat Clouds", boxStyle);
                GUILayout.Space(20);
                myTarget.cloudsConfig.flatAlpha = EditorGUILayout.Slider("Flat Clouds Alpha", myTarget.cloudsConfig.flatAlpha, 1f, 2f);
                myTarget.cloudsConfig.flatCoverage = EditorGUILayout.Slider("Flat Clouds Coverage", myTarget.cloudsConfig.flatCoverage, 0f, 1f);
                myTarget.cloudsConfig.flatSoftness = EditorGUILayout.Slider("Flat Clouds Softness", myTarget.cloudsConfig.flatSoftness, 0f, 1f);
                myTarget.cloudsConfig.flatBrightness = EditorGUILayout.Slider("Flat Clouds Brighness", myTarget.cloudsConfig.flatBrightness, 0f, 1f);
                myTarget.cloudsConfig.flatColorPow = EditorGUILayout.Slider("Flat Clouds Color Power", myTarget.cloudsConfig.flatColorPow, 0.1f, 5f);
                EditorGUILayout.EndVertical();
            }
#endif

                GUILayout.BeginVertical("Particle Clouds", boxStyle);
                GUILayout.Space(20);
#if ENVIRO_HD
            if (EnviroSkyMgr.instance == null || EnviroSkyMgr.instance.currentEnviroSkyVersion == EnviroSkyMgr.EnviroSkyVersion.HD)
            {
                myTarget.cloudsConfig.particleCloudsOverwrite = EditorGUILayout.Toggle("Enable Particle Clouds Overwrite", myTarget.cloudsConfig.particleCloudsOverwrite);
                GUILayout.Space(10);
            }
#endif
                myTarget.cloudsConfig.particleLayer1Alpha = EditorGUILayout.Slider("Layer 1 Alpha", myTarget.cloudsConfig.particleLayer1Alpha, 0f, 1f);
                myTarget.cloudsConfig.particleLayer1Brightness = EditorGUILayout.Slider("Layer 1 Brightness", myTarget.cloudsConfig.particleLayer1Brightness, 0f, 1f);
                myTarget.cloudsConfig.particleLayer1ColorPow = EditorGUILayout.Slider("Layer 1 Color Power", myTarget.cloudsConfig.particleLayer1ColorPow, 0.1f, 5f);
                GUILayout.Space(20);
                myTarget.cloudsConfig.particleLayer2Alpha = EditorGUILayout.Slider("Layer 2 Alpha", myTarget.cloudsConfig.particleLayer2Alpha, 0f, 1f);
                myTarget.cloudsConfig.particleLayer2Brightness = EditorGUILayout.Slider("Layer 2 Brightness", myTarget.cloudsConfig.particleLayer2Brightness, 0f, 1f);
                myTarget.cloudsConfig.particleLayer2ColorPow = EditorGUILayout.Slider("Layer 2 Color Power", myTarget.cloudsConfig.particleLayer2ColorPow, 0.1f, 5f);
                EditorGUILayout.EndVertical();

            GUILayout.BeginVertical("Cirrus Clouds", boxStyle);
			GUILayout.Space(20);
			myTarget.cloudsConfig.cirrusAlpha = EditorGUILayout.Slider ("Cirrus Clouds Alpha", myTarget.cloudsConfig.cirrusAlpha,0f,1f);
			myTarget.cloudsConfig.cirrusCoverage = EditorGUILayout.Slider ("Cirrus Clouds Coverage", myTarget.cloudsConfig.cirrusCoverage,0f,1f);
			myTarget.cloudsConfig.cirrusColorPow = EditorGUILayout.Slider ("Cirrus Clouds Color Power", myTarget.cloudsConfig.cirrusColorPow,0.1f,5f);
			EditorGUILayout.EndVertical ();


		}
		EditorGUILayout.EndToggleGroup ();
		EditorGUILayout.EndVertical ();

		// Fog Setup
		GUILayout.BeginVertical("", boxStyle);
		showFog = EditorGUILayout.BeginToggleGroup ("Fog Configs", showFog);
		if (showFog) {
			GUILayout.BeginVertical ("Fog Intensity", boxStyle);
			GUILayout.Space(15);
			GUILayout.BeginVertical ("Linear",boxStyle2);
			GUILayout.Space(15);
			myTarget.fogStartDistance = EditorGUILayout.FloatField ("Start Distance", myTarget.fogStartDistance);
			myTarget.fogDistance = EditorGUILayout.FloatField ("End Distance", myTarget.fogDistance);
			EditorGUILayout.EndVertical ();
			GUILayout.BeginVertical ("EXP",boxStyle2);
			GUILayout.Space(15);
			myTarget.fogDensity = EditorGUILayout.FloatField ("Density", myTarget.fogDensity);
			EditorGUILayout.EndVertical ();
			GUILayout.BeginVertical ("Height",boxStyle2);
			GUILayout.Space(15);
			myTarget.heightFogDensity = EditorGUILayout.Slider ("Height Fog Density", myTarget.heightFogDensity,0f,10f);
			EditorGUILayout.EndVertical ();
			EditorGUILayout.EndVertical ();

			GUILayout.BeginVertical ("Advanced", boxStyle);
			GUILayout.Space(15);
			EditorGUI.BeginChangeCheck();
			EditorGUILayout.PropertyField(fogMod, true, null);
			if(EditorGUI.EndChangeCheck())
			{
				serializedObj.ApplyModifiedProperties();
			}
			myTarget.FogScatteringIntensity = EditorGUILayout.Slider ("Scattering Intensity", myTarget.FogScatteringIntensity,1f,10f);
			myTarget.fogSunBlocking = EditorGUILayout.Slider ("Sundisk Intensity", myTarget.fogSunBlocking,0f,1f);
			GUILayout.BeginVertical ("Sky Fog",boxStyle2);
			GUILayout.Space(15);
			myTarget.SkyFogHeight = EditorGUILayout.Slider ("Sky Fog Height", myTarget.SkyFogHeight,0f,5f);
			myTarget.SkyFogIntensity = EditorGUILayout.Slider ("Sky Fog Intensity", myTarget.SkyFogIntensity,0f,1f);
#if ENVIRO_LW
            if (EnviroSkyMgr.instance == null || EnviroSkyMgr.instance.currentEnviroSkyVersion == EnviroSkyMgr.EnviroSkyVersion.LW)
            {
                myTarget.moonIntensity = EditorGUILayout.Slider("Moon Intensity", myTarget.moonIntensity, 0.0f, 1f);
            }
#endif
            EditorGUILayout.EndVertical ();
			EditorGUILayout.EndVertical ();
			//Add Content
		}
		EditorGUILayout.EndToggleGroup ();
		EditorGUILayout.EndVertical ();


#if ENVIRO_HD
        if (EnviroSkyMgr.instance == null || EnviroSkyMgr.instance.currentEnviroSkyVersion == EnviroSkyMgr.EnviroSkyVersion.HD)
        {
            GUILayout.BeginVertical("", boxStyle);
            showPostProcessing = EditorGUILayout.BeginToggleGroup("Distance Blur Configs", showPostProcessing);
            if (showPostProcessing)
            {
                myTarget.blurDistance = EditorGUILayout.Slider("Blur Distance", myTarget.blurDistance, 0f, 5000f);
                myTarget.blurIntensity = EditorGUILayout.Slider("Blur Intensity", myTarget.blurIntensity, 0f, 1f);
                myTarget.blurSkyIntensity = EditorGUILayout.Slider("Sky Blur Intensity", myTarget.blurSkyIntensity, 0f, 5f);
            }
            EditorGUILayout.EndToggleGroup();
            EditorGUILayout.EndVertical();
        }
#endif

        // Audio Setup
        GUILayout.BeginVertical("", boxStyle);
		showAudio = EditorGUILayout.BeginToggleGroup ("Audio Configs", showAudio);
		if (showAudio) {
			myTarget.weatherSFX = (AudioClip)EditorGUILayout.ObjectField ("Weather Soundeffect",myTarget.weatherSFX, typeof(AudioClip), true);
			GUILayout.Space(10);
			myTarget.SpringDayAmbient = (AudioClip)EditorGUILayout.ObjectField ("Spring Day Ambient",myTarget.SpringDayAmbient, typeof(AudioClip), true);
			myTarget.SpringNightAmbient = (AudioClip)EditorGUILayout.ObjectField ("Spring Night Ambient",myTarget.SpringNightAmbient, typeof(AudioClip), true);
			GUILayout.Space(10);
			myTarget.SummerDayAmbient = (AudioClip)EditorGUILayout.ObjectField ("Summer Day Ambient",myTarget.SummerDayAmbient, typeof(AudioClip), true);
			myTarget.SummerNightAmbient = (AudioClip)EditorGUILayout.ObjectField ("Summer Night Ambient",myTarget.SummerNightAmbient, typeof(AudioClip), true);
			GUILayout.Space(10);
			myTarget.AutumnDayAmbient = (AudioClip)EditorGUILayout.ObjectField ("Autumn Day Ambient",myTarget.AutumnDayAmbient, typeof(AudioClip), true);
			myTarget.AutumnNightAmbient = (AudioClip)EditorGUILayout.ObjectField ("Autumn Night Ambient",myTarget.AutumnNightAmbient, typeof(AudioClip), true);
			GUILayout.Space(10);
			myTarget.WinterDayAmbient = (AudioClip)EditorGUILayout.ObjectField ("Winter Day Ambient",myTarget.WinterDayAmbient, typeof(AudioClip), true);
			myTarget.WinterNightAmbient = (AudioClip)EditorGUILayout.ObjectField ("Winter Night Ambient",myTarget.WinterNightAmbient, typeof(AudioClip), true);
			//Add Content
		}
		EditorGUILayout.EndToggleGroup ();
		EditorGUILayout.EndVertical ();


		// END
		EditorGUILayout.EndVertical ();
		EditorUtility.SetDirty (target);
	}
}
