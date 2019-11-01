/////////////////////////////////////////////////////////////////////////////////////////////////////////
//////  EnviroRTP - Switches RTP Presets and grass color according current seasons //////
/////////////////////////////////////////////////////////////////////////////////////////////////////////
///

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
#if ENVIRO_RTP_SUPPORT
[AddComponentMenu("Enviro/Integration/RTP")]
#endif
public class EnviroRTPIntegration : MonoBehaviour {
    #if ENVIRO_RTP_SUPPORT
	public Terrain terrain;
	public ReliefTerrain rtp;

	public bool ChangeGrass = true;
	public bool ChangeGrassDensity = true;
	public bool ChangePresets = true;
	public bool ChangePresetsComplete = true;
	public bool UpdateWeatherOnTerrain = true;

	public string RTPSpringPresetName;
	public string RTPSummerPresetName;
	public string RTPAutumnPresetName;
	public string RTPWinterPresetName;
	
	public Color SpringGrassColor;
	public Color SummerGrassColor;
	public Color AutumnGrassColor;
	public Color WinterGrassColor;
	
	public float SpringGrassDensity = 0.7f;
	public float SummerGrassDensity = 1f;
	public float AutumnGrassDensity = 1f;
	public float WinterGrassDensity = 0f;

	private ReliefTerrainPresetHolder currentPreset;
	private ReliefTerrainPresetHolder springPreset;
	private ReliefTerrainPresetHolder summerPreset;
	private ReliefTerrainPresetHolder autumnPreset;
	private ReliefTerrainPresetHolder winterPreset;

	// Use this for initialization
	void Start () 
	{
		if (terrain == null)
			terrain = GetComponent<Terrain> ();

 

        if (ChangePresets) 
		{
			springPreset =  rtp.GetPresetByName(RTPSpringPresetName);
			summerPreset =  rtp.GetPresetByName(RTPSummerPresetName);
			autumnPreset =  rtp.GetPresetByName(RTPAutumnPresetName);
			winterPreset =  rtp.GetPresetByName(RTPWinterPresetName);

			switch (EnviroSky.instance.Seasons.currentSeasons)
			{
			case EnviroSeasons.Seasons.Spring:
				currentPreset = rtp.GetPresetByName(RTPSpringPresetName);
				break;
				
			case EnviroSeasons.Seasons.Summer:
				currentPreset = rtp.GetPresetByName(RTPSpringPresetName);
				break;
				
			case EnviroSeasons.Seasons.Autumn:
				currentPreset = rtp.GetPresetByName(RTPSpringPresetName);
				break;
				
			case EnviroSeasons.Seasons.Winter:
				currentPreset = rtp.GetPresetByName(RTPSpringPresetName);
				break;
			}

		}

		UpdateSeason ();

		EnviroSky.instance.OnSeasonChanged += (EnviroSeasons.Seasons season) =>
		{
			UpdateSeason ();
		};
			
	}
	
	
	// Check for correct Setup
	void OnEnable ()
	{
		if (ChangePresets)
		{
			if(RTPSpringPresetName == null)
			{
				Debug.LogError("Please assign a spring preset in Inspector!");
				this.enabled = false;
			}
			if(RTPSummerPresetName == null)
			{
				Debug.LogError("Please assign a summer preset in Inspector!");
				this.enabled = false;
			}
			if(RTPAutumnPresetName == null)
			{
				Debug.LogError("Please assign a autumn preset in Inspector!");
				this.enabled = false;
			}
			if(RTPWinterPresetName == null)
			{
				Debug.LogError("Please assign a winter preset in Inspector!");
				this.enabled = false;
			}

		}
	}
	
	void ChangeGrassColor (Color ChangeToColor)
	{
		terrain.terrainData.wavingGrassTint = ChangeToColor;
	}
	

	
	void UpdateSeason ()
	{
		switch (EnviroSky.instance.Seasons.currentSeasons)
		{
		case EnviroSeasons.Seasons.Spring:
	
			if(ChangeGrass)
				ChangeGrassColor(SpringGrassColor);
			if(ChangeGrassDensity)
				terrain.detailObjectDensity = SpringGrassDensity;

			if(ChangePresetsComplete)
			rtp.RestorePreset(springPreset);
			break;
			
		case EnviroSeasons.Seasons.Summer:
	
			if(ChangeGrass)
				ChangeGrassColor(SummerGrassColor);
			if(ChangeGrassDensity)
				terrain.detailObjectDensity = SummerGrassDensity;
		
			if(ChangePresetsComplete)
			rtp.RestorePreset(summerPreset);
			break;
			
		case EnviroSeasons.Seasons.Autumn:

			if(ChangeGrass)
				ChangeGrassColor(AutumnGrassColor);
			if(ChangeGrassDensity)
				terrain.detailObjectDensity = AutumnGrassDensity;

			if(ChangePresetsComplete)
			rtp.RestorePreset(autumnPreset);
			break;
			
		case EnviroSeasons.Seasons.Winter:
		
			if(ChangeGrass)
				ChangeGrassColor(WinterGrassColor);	
			if(ChangeGrassDensity)
				terrain.detailObjectDensity = WinterGrassDensity;

			if(ChangePresetsComplete)
			rtp.RestorePreset(winterPreset);
			break;
		}
		
	}


	void UpdateWeather ()
	{
		rtp.globalSettingsHolder._snow_strength = EnviroSky.instance.Weather.curSnowStrength;

		if(EnviroSky.instance.Weather.wetness > 0f)
		{
			rtp.globalSettingsHolder.TERRAIN_GlobalWetness = EnviroSky.instance.Weather.curWetness;
		}
		else
		{
			rtp.globalSettingsHolder.TERRAIN_GlobalWetness = 0f;
		}

		rtp.globalSettingsHolder.Refresh(terrain.materialTemplate, rtp);
	}

	void UpdateRTP ()
	{
		switch (EnviroSky.instance.Seasons.currentSeasons)
		{
		case EnviroSeasons.Seasons.Spring:
			rtp.InterpolatePresets(currentPreset.PresetID, springPreset.PresetID, 0.1f);
			currentPreset = springPreset;
			break;
			
		case EnviroSeasons.Seasons.Summer:
			rtp.InterpolatePresets(currentPreset.PresetID, summerPreset.PresetID, 0.1f);
				currentPreset = summerPreset;
			break;
			
		case EnviroSeasons.Seasons.Autumn:
			rtp.InterpolatePresets(currentPreset.PresetID, autumnPreset.PresetID, 0.1f);
			currentPreset = autumnPreset;
			break;
			
		case EnviroSeasons.Seasons.Winter:
			rtp.InterpolatePresets(currentPreset.PresetID, winterPreset.PresetID, 0.1f);
			currentPreset = winterPreset;
			break;
		}
		rtp.globalSettingsHolder.Refresh();
	}

	IEnumerator ChangeCurrentPreset (ReliefTerrainPresetHolder me)
	{
		yield return new WaitForSeconds (1f);
		currentPreset = me;

	}
	// Update is called once per frame
	void Update () 
	{

        if (EnviroSky.instance == null)
            return;

		if (ChangePresets)
			UpdateRTP ();
		if (UpdateWeatherOnTerrain)
			UpdateWeather ();
	}
#endif
}
