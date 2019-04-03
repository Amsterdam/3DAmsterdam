using UnityEngine;
using System.Collections;

public class EnviroAudioSource : MonoBehaviour {

	public enum AudioSourceFunction
	{
		Weather1,
		Weather2,
		Ambient,
		Ambient2,
		Thunder,
        ZoneAmbient
	}

	public AudioSourceFunction myFunction;
    public AudioSource audiosrc;
	public bool isFadingIn = false;
	public bool isFadingOut = false;


	float currentAmbientVolume;
	float currentWeatherVolume;
    float currentZoneVolume;

    void Start ()
	{

        if (EnviroSkyMgr.instance == null)
        {
            Debug.Log("EnviroSky Manager not found. Deactivate enviro AudioSource");
            this.enabled = false;
            return;
        }

		if (audiosrc == null)
		audiosrc = GetComponent<AudioSource> ();
		
		if (myFunction == AudioSourceFunction.Weather1 || myFunction == AudioSourceFunction.Weather2) 
		{
			audiosrc.loop = true;
			audiosrc.volume = 0f;
		}

		currentAmbientVolume = EnviroSkyMgr.instance.ambientAudioVolume;
		currentWeatherVolume = EnviroSkyMgr.instance.weatherAudioVolume;
    }
		
	public void FadeOut () 
	{
		isFadingOut = true;
		isFadingIn = false;
	}
		
	public void FadeIn (AudioClip clip) 
	{
		isFadingIn = true;
		isFadingOut = false;
		audiosrc.clip = clip;
		audiosrc.Play ();
	}

    public void PlayOneShot(AudioClip clip)
    {
        audiosrc.loop = false;
        audiosrc.clip = clip;
        audiosrc.Play();
    }

    void Update ()
	{
		if (!EnviroSkyMgr.instance.IsStarted() || EnviroSkyMgr.instance == null)
			return;

		currentAmbientVolume = Mathf.Lerp(currentAmbientVolume, EnviroSkyMgr.instance.ambientAudioVolume + EnviroSkyMgr.instance.ambientAudioVolumeModifier, 10f * Time.deltaTime);
		currentWeatherVolume = Mathf.Lerp(currentWeatherVolume, EnviroSkyMgr.instance.weatherAudioVolume + EnviroSkyMgr.instance.weatherAudioVolumeModifier, 10 * Time.deltaTime);

        if (myFunction == AudioSourceFunction.Weather1 || myFunction == AudioSourceFunction.Weather2 || myFunction == AudioSourceFunction.Thunder){
			if (isFadingIn && audiosrc.volume < currentWeatherVolume) {
				audiosrc.volume += EnviroSkyMgr.instance.audioTransitionSpeed * Time.deltaTime;
			} else if (isFadingIn && audiosrc.volume >= currentWeatherVolume - 0.01f) {
				isFadingIn = false;
			}

			if (isFadingOut && audiosrc.volume > 0f) {
				audiosrc.volume -= EnviroSkyMgr.instance.audioTransitionSpeed * Time.deltaTime;
			} else if (isFadingOut && audiosrc.volume == 0f) {
				audiosrc.Stop ();
				isFadingOut = false;
			}

			if (audiosrc.isPlaying && !isFadingOut && !isFadingIn) {
				audiosrc.volume = currentWeatherVolume;
			}
		}
		else if (myFunction == AudioSourceFunction.Ambient || myFunction == AudioSourceFunction.Ambient2)
		{
			if (isFadingIn && audiosrc.volume < currentAmbientVolume) {
				audiosrc.volume += EnviroSkyMgr.instance.audioTransitionSpeed * Time.deltaTime;
			} else if (isFadingIn && audiosrc.volume >= currentAmbientVolume - 0.01f) {
				isFadingIn = false;
			}

			if (isFadingOut && audiosrc.volume > 0f) {
				audiosrc.volume -= EnviroSkyMgr.instance.audioTransitionSpeed * Time.deltaTime;
			} else if (isFadingOut && audiosrc.volume == 0f) {
				audiosrc.Stop ();
				isFadingOut = false;
			}

			if (audiosrc.isPlaying && !isFadingOut && !isFadingIn) {
				audiosrc.volume = currentAmbientVolume;
			}
		}

        else if (myFunction == AudioSourceFunction.ZoneAmbient)
        {
            if (isFadingIn && audiosrc.volume < EnviroSkyMgr.instance.interiorZoneAudioVolume)
            {
                audiosrc.volume += EnviroSkyMgr.instance.interiorZoneAudioFadingSpeed * Time.deltaTime;
            }
            else if (isFadingIn && audiosrc.volume >= EnviroSkyMgr.instance.interiorZoneAudioVolume - 0.01f)
            {
                isFadingIn = false;
            }

            if (isFadingOut && audiosrc.volume > 0f)
            {
                audiosrc.volume -= EnviroSkyMgr.instance.interiorZoneAudioFadingSpeed * Time.deltaTime;
            }
            else if (isFadingOut && audiosrc.volume == 0f)
            {
                audiosrc.Stop();
                isFadingOut = false;
            }

            if (audiosrc.isPlaying && !isFadingOut && !isFadingIn)
            {
                audiosrc.volume = EnviroSkyMgr.instance.interiorZoneAudioVolume;
            }
        }
    }
}
