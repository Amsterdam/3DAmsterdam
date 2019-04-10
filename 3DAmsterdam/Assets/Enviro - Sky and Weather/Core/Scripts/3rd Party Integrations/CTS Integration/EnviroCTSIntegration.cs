using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if CTS_PRESENT
using CTS;

[AddComponentMenu("Enviro/Integration/CTS Integration")]
public class EnviroCTSIntegration : MonoBehaviour {

	public CTSWeatherManager ctsWeatherManager;

	public bool updateSnow;
	public bool updateWetness;
	public bool updateSeasons;

	private float daysInYear;

	void Start () 
	{
		if (ctsWeatherManager == null) {
			ctsWeatherManager = GameObject.FindObjectOfType<CTSWeatherManager> ();
		}

		if(ctsWeatherManager == null) {
			Debug.LogWarning("CTS WeatherManager not found! Component -> CTS -> Add Weather Manager");
			return;
		}

		if (EnviroSkyMgr.instance == null) {
			Debug.LogWarning("EnviroSky Manager not found! Please add Enviro Manager to your scene!");
			return;
		}
	daysInYear =  EnviroSkyMgr.instance.Time.DaysInYear;
	}
	

	void Update () 
	{
		if (ctsWeatherManager == null || EnviroSkyMgr.instance == null)
			return;

		if (updateSnow)
			ctsWeatherManager.SnowPower = EnviroSkyMgr.instance.GetSnowIntensity();

		if(updateWetness)
			ctsWeatherManager.RainPower = EnviroSkyMgr.instance.GetWetnessIntensity();

		if (updateSeasons) {
			ctsWeatherManager.Season = Mathf.Lerp (0f, 4f, EnviroSkyMgr.instance.GetCurrentDay() / daysInYear);
		}
	}
}
#endif
