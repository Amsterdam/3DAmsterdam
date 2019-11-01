#if AQUAS_PRESENT

using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[AddComponentMenu("Enviro/Integration/AQUAS Integration")]
public class EnviroAquasIntegration : MonoBehaviour {

	[Header("AQUAS Water Plane")]
	public GameObject waterObject;

	[Header("Setup")]
	public bool deactivateAquasReflectionProbe = true;
	public bool deactivateEnviroFogUnderwater = true;

	[Header("Settings")]
	[Range(0f,1f)]
	public float underwaterFogColorInfluence = 0.3f;
	//
	//private GameObject enviroWaterDepth;
	private AQUAS_LensEffects aquasUnderWater;
	private bool isUnderWater;
	//

	private bool defaultDistanceFog;
	private bool defaultHeightFog;

	void Start () 
	{
		if (EnviroSkyMgr.instance == null) 
		{
			Debug.Log ("No EnviroSky Manager in scene! This component will be disabled!");
			this.enabled = false;
			return;
		}

		if(GameObject.Find ("UnderWaterCameraEffects") != null)
			aquasUnderWater = GameObject.Find ("UnderWaterCameraEffects").GetComponent<AQUAS_LensEffects> ();
	
		defaultDistanceFog = EnviroSkyMgr.instance.FogSettings.distanceFog;
		defaultHeightFog = EnviroSkyMgr.instance.FogSettings.heightFog;

		SetupEnviroWithAQUAS ();
	}

	void Update () 
	{
         if (EnviroSkyMgr.instance == null)
            return;

		//Check if we are underwater! Deactivate the workaround plane and enviro fog.
		if (waterObject != null && aquasUnderWater != null) {
			if (aquasUnderWater.underWater && !isUnderWater) {
				if (deactivateEnviroFogUnderwater) {
                    EnviroSkyMgr.instance.FogSettings.distanceFog = false;
                    EnviroSkyMgr.instance.FogSettings.heightFog = false;
                    EnviroSkyMgr.instance.CustomFogIntensity = underwaterFogColorInfluence;
                    
				}
                EnviroSkyMgr.instance.UpdateFogIntensity = false;
                EnviroSkyMgr.instance.ambientAudioVolumeModifier = -1f;
                EnviroSkyMgr.instance.weatherAudioVolumeModifier = -1f;
				isUnderWater = true;
			} else if (!aquasUnderWater.underWater && isUnderWater) {
				if (deactivateEnviroFogUnderwater) {
                    EnviroSkyMgr.instance.UpdateFogIntensity = true;
                    EnviroSkyMgr.instance.FogSettings.distanceFog = defaultDistanceFog;
                    EnviroSkyMgr.instance.FogSettings.heightFog = defaultHeightFog;
					RenderSettings.fogDensity = EnviroSkyMgr.instance.Weather.currentActiveWeatherPreset.fogDensity;
                    EnviroSkyMgr.instance.CustomFogColor = aquasUnderWater.underWaterParameters.fogColor;
                    EnviroSkyMgr.instance.CustomFogIntensity = 0f;
				}
                EnviroSkyMgr.instance.ambientAudioVolumeModifier = 0f;
                EnviroSkyMgr.instance.weatherAudioVolumeModifier = 0f;
                isUnderWater = false;
			}
		}
	}

	public void SetupEnviroWithAQUAS ()
	{
		if (waterObject != null) {

			if (deactivateAquasReflectionProbe)
				DeactivateReflectionProbe (waterObject);

			if (EnviroSkyMgr.instance.FogSettings.distanceFog == false && EnviroSkyMgr.instance.FogSettings.heightFog == false)
				deactivateEnviroFogUnderwater = false;

			if (aquasUnderWater != null)
				aquasUnderWater.setAfloatFog = false;
			
			} else {
				Debug.Log ("AQUAS Object not found! This component will be disabled!");
				this.enabled = false;
				return;
			}
	}


	private void DeactivateReflectionProbe (GameObject aquas)
	{
		GameObject probe = GameObject.Find (aquas.name + "/Reflection Probe");
		if (probe != null)
			probe.GetComponent<ReflectionProbe> ().enabled = false;
		else
		Debug.Log ("Cannot find AQUAS Reflection Probe!");
	}
}
#endif