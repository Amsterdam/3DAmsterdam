using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace EnviroSamples
{
public class DemoUI : MonoBehaviour {


	public UnityEngine.UI.Slider sliderTime;
	UnityEngine.UI.Slider sliderQuality;
	public UnityEngine.UI.Text timeText;
    public UnityEngine.UI.Text weatherText;
    public UnityEngine.UI.Text temperatureText;
    public UnityEngine.UI.Dropdown weatherDropdown;
	//UnityEngine.UI.Dropdown seasonDropdown;

 	//bool seasonmode = true;
	//bool fastdays = false;

	private bool started = false;

	void Start () 
	{

          if(EnviroSkyMgr.instance == null || !EnviroSkyMgr.instance.IsAvailable())
            {
                this.enabled = false;
                return;
            }

        EnviroSkyMgr.instance.OnWeatherChanged += (EnviroWeatherPreset type) =>
		{
			UpdateWeatherSlider ();	
		};

       
        /*EnviroSkyMgr.instance.OnSeasonChanged += (EnviroSeasons.Seasons season) =>
		{
			UpdateSeasonSlider(season);
		};*/
	}
		
	IEnumerator setupDrodown ()
	{
		started = true;
		yield return new WaitForSeconds (0.1f);

		for (int i = 0; i < EnviroSkyMgr.instance.GetCurrentWeatherPresetList().Count; i++) {
			UnityEngine.UI.Dropdown.OptionData o = new UnityEngine.UI.Dropdown.OptionData();
			o.text = EnviroSkyMgr.instance.GetCurrentWeatherPresetList()[i].Name;
			weatherDropdown.options.Add (o);
		}

		yield return new WaitForSeconds (0.1f);
		UpdateWeatherSlider ();
	}

	public void ChangeTimeSlider () 
	{
		if (sliderTime.value < 0f)
			sliderTime.value = 0f;
		EnviroSkyMgr.instance.SetTimeOfDay (sliderTime.value * 24f);
	}

    public void ChangeCloudQuality(int value)
    {
#if ENVIRO_HD
        EnviroSky.instance.cloudsSettings.cloudsQuality = (EnviroCloudSettings.CloudQuality)value;
#endif
     }

    public void ChangeAmbientVolume (float value)
	{
        EnviroSkyMgr.instance.ambientAudioVolume = value;
	}

	public void ChangeWeatherVolume (float value)
	{
        EnviroSkyMgr.instance.weatherAudioVolume = value;
    }

	public void SetWeatherID (int id) 
	{
		EnviroSkyMgr.instance.ChangeWeather (id);
	}

    public void SetVolumeClouds(bool b)
    {
        EnviroSkyMgr.instance.useVolumeClouds = b;
    }

    public void SetVolumeLighting(bool b)
    {
        EnviroSkyMgr.instance.useVolumeLighting = b;
    }

    public void SetFlatClouds(bool b)
    {
        EnviroSkyMgr.instance.useFlatClouds = b;
    }

    public void SetParticleClouds(bool b)
    {
        EnviroSkyMgr.instance.useParticleClouds = b;
    }

    public void SetSunShafts(bool b)
    {
        EnviroSkyMgr.instance.useSunShafts = b;
    }

    public void SetMoonShafts(bool b)
    {
        EnviroSkyMgr.instance.useMoonShafts = b;
    }

    public void SetSeason (int id)
	{
		switch (id) 
		{
		case 0:
			EnviroSkyMgr.instance.ChangeSeason (EnviroSeasons.Seasons.Spring);
		break;
		case 1:
            EnviroSkyMgr.instance.ChangeSeason (EnviroSeasons.Seasons.Summer);
			break;
		case 2:
            EnviroSkyMgr.instance.ChangeSeason (EnviroSeasons.Seasons.Autumn);
			break;
		case 3:
            EnviroSkyMgr.instance.ChangeSeason (EnviroSeasons.Seasons.Winter);
            break;
		}
	}


	public void SetTimeProgress (int id)
	{
		switch (id) 
		{
		case 0:
			EnviroSkyMgr.instance.SetTimeProgress(EnviroTime.TimeProgressMode.None);
			break;
		case 1:
            EnviroSkyMgr.instance.SetTimeProgress(EnviroTime.TimeProgressMode.Simulated);
            break;
		case 2:
            EnviroSkyMgr.instance.SetTimeProgress(EnviroTime.TimeProgressMode.SystemTime);
			break;
		}
	}


		
	void UpdateWeatherSlider ()
	{
		if (EnviroSkyMgr.instance.GetCurrentWeatherPreset() != null) {
			for (int i = 0; i < weatherDropdown.options.Count; i++) {
				if (weatherDropdown.options [i].text == EnviroSkyMgr.instance.GetCurrentWeatherPreset().Name)
					weatherDropdown.value = i;
			}
		}
	}

	/*void UpdateSeasonSlider (EnviroSeasons.Seasons s)
	{
		switch (s) {
		case EnviroSeasons.Seasons.Spring:
			seasonDropdown.value = 0;
			break;
		case EnviroSeasons.Seasons.Summer:
			seasonDropdown.value = 1;
			break;
		case EnviroSeasons.Seasons.Autumn:
			seasonDropdown.value = 2;
			break;
		case EnviroSeasons.Seasons.Winter:
			seasonDropdown.value = 3;
			break;
		}
	}
    */
	void Update ()
	{
		if (!EnviroSkyMgr.instance.IsStarted())
			return;
		else {
			if(!started)
				StartCoroutine(setupDrodown ());
		}

		timeText.text = EnviroSkyMgr.instance.GetTimeString ();
            if(EnviroSkyMgr.instance.GetCurrentWeatherPreset() != null)
        weatherText.text = EnviroSkyMgr.instance.GetCurrentWeatherPreset().Name;

            temperatureText.text = EnviroSkyMgr.instance.GetCurrentTemperatureString();
    }

	void LateUpdate ()
	{
		sliderTime.value = EnviroSkyMgr.instance.GetUniversalTimeOfDay() / 24f;
	}
}
}