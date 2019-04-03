using System.Collections;
using System.Collections.Generic;
using UnityEngine.Audio;
using UnityEngine;

[AddComponentMenu("Enviro/Utility/Audio Mixer Support")]
public class EnviroAudioMixerSupport : MonoBehaviour {

	[Header("Mixer")]
	public AudioMixer audioMixer;

	[Header("Group Names")]
	public string ambientMixerGroup;
	public string weatherMixerGroup;
	public string thunderMixerGroup;

	void Start () 
	{
		if(audioMixer!= null && EnviroSkyMgr.instance != null)
			StartCoroutine (Setup ());
	}
	

	IEnumerator Setup ()
	{
		yield return 0;
		if (EnviroSkyMgr.instance.IsStarted()) {

			if(ambientMixerGroup != "")
			{
                EnviroSkyMgr.instance.AudioSettings.AudioSourceAmbient.audiosrc.outputAudioMixerGroup = audioMixer.FindMatchingGroups(ambientMixerGroup)[0];
                EnviroSkyMgr.instance.AudioSettings.AudioSourceAmbient2.audiosrc.outputAudioMixerGroup = audioMixer.FindMatchingGroups(ambientMixerGroup)[0];
            }

            if (weatherMixerGroup != "")
			{
                EnviroSkyMgr.instance.AudioSettings.AudioSourceWeather.audiosrc.outputAudioMixerGroup = audioMixer.FindMatchingGroups(weatherMixerGroup)[0];
                EnviroSkyMgr.instance.AudioSettings.AudioSourceWeather2.audiosrc.outputAudioMixerGroup = audioMixer.FindMatchingGroups(weatherMixerGroup)[0];
            }

            if (thunderMixerGroup != "")
			{
				
                EnviroSkyMgr.instance.AudioSettings.AudioSourceThunder.audiosrc.outputAudioMixerGroup = audioMixer.FindMatchingGroups(thunderMixerGroup)[0];
            }
        }
            else
            {
			    StartCoroutine (Setup ());
		    }
	}
}
