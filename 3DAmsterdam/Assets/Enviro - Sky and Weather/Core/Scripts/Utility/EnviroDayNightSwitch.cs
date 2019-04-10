using UnityEngine;
using System.Collections;

public class EnviroDayNightSwitch : MonoBehaviour {

	private Light[] lightsArray;

	void Start () {

		lightsArray = GetComponentsInChildren<Light> ();

        EnviroSkyMgr.instance.OnDayTime += () =>
		{
			Deactivate () ;
		};

		EnviroSkyMgr.instance.OnNightTime += () =>
		{
			Activate () ;
		};

		if (EnviroSkyMgr.instance.IsNight())
			Activate ();
		else
			Deactivate ();
	}
	

	void Activate () 
	{
		for (int i = 0; i < lightsArray.Length; i++) {
			lightsArray [i].enabled = true;
		}

	}

	void Deactivate () 
	{
		for (int i = 0; i < lightsArray.Length; i++) {
			lightsArray [i].enabled = false;
		}
	}

}
