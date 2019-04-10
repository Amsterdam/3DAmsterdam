using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("Enviro/Interior Zone")]
public class EnviroInterior : MonoBehaviour {
	public enum ZoneTriggerType
    {
        Entry_Exit,
        Zone
    }

    public ZoneTriggerType zoneTriggerType;
    //Feature Controls
    public bool directLighting;
	public bool ambientLighting;
	public bool weatherAudio;
	public bool ambientAudio;
	public bool fog;
    public bool fogColor;
    public bool skybox;
    public bool weatherEffects;

	//Lighting
	public Color directLightingMod = Color.black;
	public Color ambientLightingMod = Color.black;
	public Color ambientEQLightingMod = Color.black;
	public Color ambientGRLightingMod = Color.black;
	private Color curDirectLightingMod;
	private Color curAmbientLightingMod;
	private Color curAmbientEQLightingMod;
	private Color curAmbientGRLightingMod;

    public float directLightFadeSpeed = 2f;
    public float ambientLightFadeSpeed = 2f;

    public Color skyboxColorMod = Color.black;
    private Color curskyboxColorMod;
    public float skyboxFadeSpeed = 2f;

    private bool fadeInDirectLight = false;
	private bool fadeOutDirectLight = false;
	private bool fadeInAmbientLight = false;
	private bool fadeOutAmbientLight = false;

    private bool fadeInSkybox = false;
    private bool fadeOutSkybox = false;

    //Volume
    public float ambientVolume = 0f;
	public float weatherVolume = 0f;
    public AudioClip zoneAudioClip;
    public float zoneAudioVolume = 1f;
    public float zoneAudioFadingSpeed = 1f;
    //Fog
    public Color fogColorMod = Color.black;
    private Color curFogColorMod;
    public float fogFadeSpeed = 2f;
    public float minFogMod = 0f;
    private bool fadeInFog = false;
    private bool fadeOutFog = false;
    private bool fadeInFogColor = false;
    private bool fadeOutFogColor = false;
    //Weather
    public float weatherFadeSpeed = 2f;
    private bool fadeInWeather = false;
    private bool fadeOutWeather = false;

    public List<EnviroTrigger> triggers = new List<EnviroTrigger>();

	private Color fadeOutColor = new Color (0,0,0,0);

	void Start () 
	{
		
	}
	

	public void CreateNewTrigger ()
	{
		GameObject t = new GameObject ();
		t.name = "Trigger " + triggers.Count.ToString ();
		t.transform.SetParent (transform,false);
		t.AddComponent<BoxCollider> ().isTrigger = true;
		EnviroTrigger trig = t.AddComponent<EnviroTrigger> ();
		trig.myZone = this;
		trig.name = t.name;
		triggers.Add(trig);

		#if UNITY_EDITOR
		UnityEditor.Selection.activeObject = t;
		#endif
	}

	public void RemoveTrigger (EnviroTrigger id)
	{
			DestroyImmediate (id.gameObject);
			triggers.Remove (id);
	}

	public void Enter ()
	{
		EnviroSkyMgr.instance.interiorMode = true;
        EnviroSkyMgr.instance.lastInteriorZone = this;

		if (directLighting) {
			fadeOutDirectLight = false;
			fadeInDirectLight = true;
		}

		if (ambientLighting) {
			fadeOutAmbientLight = false;
			fadeInAmbientLight = true;
		}

        if (skybox)
        {
            fadeOutSkybox = false;
            fadeInSkybox = true;
        }

        if(ambientAudio)
			EnviroSkyMgr.instance.ambientAudioVolumeModifier = ambientVolume;
		if(weatherAudio)
            EnviroSkyMgr.instance.weatherAudioVolumeModifier = weatherVolume;

        if (zoneAudioClip != null)
        {
            EnviroSkyMgr.instance.InteriorZoneSettings.currentInteriorZoneAudioFadingSpeed = zoneAudioFadingSpeed;
            EnviroSkyMgr.instance.AudioSettings.AudioSourceZone.FadeIn(zoneAudioClip);
            EnviroSkyMgr.instance.InteriorZoneSettings.currentInteriorZoneAudioVolume = zoneAudioVolume;
        }

        if (fog)
        {
            fadeOutFog = false;
            fadeInFog = true;
        }

        if (fogColor)
        {
            fadeOutFogColor = false;
            fadeInFogColor = true;
        }

        if (weatherEffects)
        {
            fadeOutWeather = false;
            fadeInWeather = true;
        }

    }


	public void Exit ()
	{
		EnviroSkyMgr.instance.interiorMode = false;

		if (directLighting) {
			fadeInDirectLight = false;
			fadeOutDirectLight = true;
		}
		if (ambientLighting) {
			fadeOutAmbientLight = true;
			fadeInAmbientLight = false;
		}

        if (skybox)
        {
            fadeOutSkybox = true;
            fadeInSkybox = false;
        }

        if(ambientAudio)
			EnviroSkyMgr.instance.ambientAudioVolumeModifier = 0f;
		if(weatherAudio)
			EnviroSkyMgr.instance.weatherAudioVolumeModifier = 0f;

        if (zoneAudioClip != null)
        {
            EnviroSkyMgr.instance.InteriorZoneSettings.currentInteriorZoneAudioFadingSpeed = zoneAudioFadingSpeed;
            EnviroSkyMgr.instance.AudioSettings.AudioSourceZone.FadeOut();
        }

        if (fog)
        {
            fadeOutFog = true;
            fadeInFog = false;
        }

        if (fogColor)
        {
            fadeOutFogColor = true;
            fadeInFogColor = false;
        }

        if (weatherEffects)
        {
            fadeOutWeather = true;
            fadeInWeather = false;
        }
    }

    public void StopAllFading()
    {
        if (directLighting)
        {
            fadeInDirectLight = false;
            fadeOutDirectLight = false;
        }
        if (ambientLighting)
        {
            fadeOutAmbientLight = false;
            fadeInAmbientLight = false;
        }

        if (zoneAudioClip != null)
        {
            EnviroSkyMgr.instance.AudioSettings.AudioSourceZone.FadeOut();
        }

        if (skybox)
        {
            fadeOutSkybox = false;
            fadeInSkybox = false;
        }

        if (fog)
        {
            fadeOutFog = false;
            fadeInFog = false;
        }

        if (fogColor)
        {
            fadeOutFogColor = false;
            fadeInFogColor = false;
        }

        if (weatherEffects)
        {
            fadeOutWeather = false;
            fadeInWeather = false;
        }

    }


    void Update () 
	{
        if (EnviroSkyMgr.instance == null || EnviroSkyMgr.instance.IsAvailable() == false)
            return;

        if (directLighting) 
		{
			if (fadeInDirectLight) 
			{
				curDirectLightingMod = Color.Lerp (curDirectLightingMod, directLightingMod, directLightFadeSpeed * Time.deltaTime);
                EnviroSkyMgr.instance.InteriorZoneSettings.currentInteriorDirectLightMod = curDirectLightingMod;
				if (curDirectLightingMod == directLightingMod)
					fadeInDirectLight = false;
			} 
			else if (fadeOutDirectLight) 
			{
				curDirectLightingMod = Color.Lerp (curDirectLightingMod, fadeOutColor, directLightFadeSpeed * Time.deltaTime);
                EnviroSkyMgr.instance.InteriorZoneSettings.currentInteriorDirectLightMod = curDirectLightingMod;
				if (curDirectLightingMod == fadeOutColor)
					fadeOutDirectLight = false;
			}
		}

		if (ambientLighting) 
		{
			if (fadeInAmbientLight) 
			{
				curAmbientLightingMod = Color.Lerp (curAmbientLightingMod, ambientLightingMod,ambientLightFadeSpeed * Time.deltaTime);
                EnviroSkyMgr.instance.InteriorZoneSettings.currentInteriorAmbientLightMod = curAmbientLightingMod;

				if (EnviroSkyMgr.instance.LightSettings.ambientMode == UnityEngine.Rendering.AmbientMode.Trilight) {
					curAmbientEQLightingMod = Color.Lerp (curAmbientEQLightingMod, ambientEQLightingMod, ambientLightFadeSpeed * Time.deltaTime);
                    EnviroSkyMgr.instance.InteriorZoneSettings.currentInteriorAmbientEQLightMod = curAmbientEQLightingMod;

					curAmbientGRLightingMod = Color.Lerp (curAmbientGRLightingMod, ambientGRLightingMod, ambientLightFadeSpeed * Time.deltaTime);
                    EnviroSkyMgr.instance.InteriorZoneSettings.currentInteriorAmbientGRLightMod = curAmbientGRLightingMod;
				}

				if (curAmbientLightingMod == ambientLightingMod)
					fadeInAmbientLight = false;
			} 
			else if (fadeOutAmbientLight) 
			{
				curAmbientLightingMod = Color.Lerp (curAmbientLightingMod, fadeOutColor, 2f * Time.deltaTime);
                EnviroSkyMgr.instance.InteriorZoneSettings.currentInteriorAmbientLightMod = curAmbientLightingMod;

				if (EnviroSkyMgr.instance.LightSettings.ambientMode == UnityEngine.Rendering.AmbientMode.Trilight) {
					curAmbientEQLightingMod = Color.Lerp (curAmbientEQLightingMod, fadeOutColor, 2f * Time.deltaTime);
                    EnviroSkyMgr.instance.InteriorZoneSettings.currentInteriorAmbientEQLightMod = curAmbientEQLightingMod;

					curAmbientGRLightingMod = Color.Lerp (curAmbientGRLightingMod, fadeOutColor, 2f * Time.deltaTime);
                    EnviroSkyMgr.instance.InteriorZoneSettings.currentInteriorAmbientGRLightMod = curAmbientGRLightingMod;
				}

				if (curAmbientLightingMod == fadeOutColor)
					fadeOutAmbientLight = false;
			}
        }

        if (skybox)
        {
            if (fadeInSkybox)
            {
                curskyboxColorMod = Color.Lerp(curskyboxColorMod, skyboxColorMod, skyboxFadeSpeed * Time.deltaTime);
                EnviroSkyMgr.instance.InteriorZoneSettings.currentInteriorSkyboxMod = curskyboxColorMod;
                if (curskyboxColorMod == skyboxColorMod)
                    fadeInSkybox = false;
            }
            else if (fadeOutSkybox)
            {
                curskyboxColorMod = Color.Lerp(curskyboxColorMod, fadeOutColor, skyboxFadeSpeed * Time.deltaTime);
                EnviroSkyMgr.instance.InteriorZoneSettings.currentInteriorSkyboxMod = curskyboxColorMod;
                if (curskyboxColorMod == fadeOutColor)
                    fadeOutSkybox = false;
            }
        }

        if (fog)
            {
                if (fadeInFog)
                {
                    EnviroSkyMgr.instance.InteriorZoneSettings.currentInteriorFogMod = Mathf.Lerp(EnviroSkyMgr.instance.InteriorZoneSettings.currentInteriorFogMod, minFogMod, fogFadeSpeed * Time.deltaTime);
                    if (EnviroSkyMgr.instance.InteriorZoneSettings.currentInteriorFogMod <= minFogMod + 0.001)
                       fadeInFog = false;
                }
                else if (fadeOutFog)
                {
                EnviroSkyMgr.instance.InteriorZoneSettings.currentInteriorFogMod = Mathf.Lerp(EnviroSkyMgr.instance.InteriorZoneSettings.currentInteriorFogMod, 1, (fogFadeSpeed * 2) * Time.deltaTime);
                   if (EnviroSkyMgr.instance.InteriorZoneSettings.currentInteriorFogMod >= 0.999)
                       fadeOutFog = false;
                }
            }

        if (fogColor)
        {
            if (fadeInFogColor)
            {
                curFogColorMod = Color.Lerp(curFogColorMod, fogColorMod, fogFadeSpeed * Time.deltaTime);
                EnviroSkyMgr.instance.InteriorZoneSettings.currentInteriorFogColorMod = curFogColorMod;
                if (curFogColorMod == fogColorMod)
                    fadeInFogColor = false;
            }
            else if (fadeOutFogColor)
            {
                curFogColorMod = Color.Lerp(curFogColorMod, fadeOutColor, fogFadeSpeed * Time.deltaTime);
                EnviroSkyMgr.instance.InteriorZoneSettings.currentInteriorFogColorMod = curFogColorMod;
                if (curFogColorMod == fadeOutColor)
                    fadeOutFogColor = false;
            }
        }


        if (weatherEffects)
        {
            if (fadeInWeather)
            {
                EnviroSkyMgr.instance.InteriorZoneSettings.currentInteriorWeatherEffectMod = Mathf.Lerp(EnviroSkyMgr.instance.InteriorZoneSettings.currentInteriorWeatherEffectMod, 0, weatherFadeSpeed * Time.deltaTime);
                if (EnviroSkyMgr.instance.InteriorZoneSettings.currentInteriorWeatherEffectMod <= 0.001)
                    fadeInWeather = false;
            }
            else if (fadeOutWeather)
            {
                EnviroSkyMgr.instance.InteriorZoneSettings.currentInteriorWeatherEffectMod = Mathf.Lerp(EnviroSkyMgr.instance.InteriorZoneSettings.currentInteriorWeatherEffectMod, 1, (weatherFadeSpeed * 2) * Time.deltaTime);
                if (EnviroSkyMgr.instance.InteriorZoneSettings.currentInteriorWeatherEffectMod >= 0.999)
                    fadeOutWeather = false;
            }
        }
	}
}
