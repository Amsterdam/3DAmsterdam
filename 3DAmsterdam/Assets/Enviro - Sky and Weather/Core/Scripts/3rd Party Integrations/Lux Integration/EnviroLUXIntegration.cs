using UnityEngine;
using System.Collections;

#if ENVIRO_LUX_SUPPORT
[AddComponentMenu("Enviro/Integration/LUX")]
#endif
public class EnviroLUXIntegration : MonoBehaviour
{
#if ENVIRO_LUX_SUPPORT
	public LuxDynamicWeather LuxDynamicWeatherScript;

	private float curWetness = 0f;
	private float curSnow = 0f;

    public void Start()
    {
		if (LuxDynamicWeatherScript != null) 
		{
			LuxDynamicWeatherScript.ScriptControlledWeather = true;
		} 
		else 
		{

            LuxDynamicWeatherScript = FindObjectOfType<LuxDynamicWeather>();

            if (LuxDynamicWeatherScript != null)
            {
                LuxDynamicWeatherScript.ScriptControlledWeather = true;
            }
            else
            {
                Debug.LogError("Please setup dynamic weather for LUX!");
                this.enabled = false;
            }
		}
    }
	
	void GetParameters ()
	{
		curWetness = EnviroSkyMgr.instance.GetWetnessIntensity();
		curSnow = EnviroSkyMgr.instance.GetSnowIntensity();
	}


    public void Update()
    {
        if (EnviroSkyMgr.instance == null)
            return;

        GetParameters ();

		if (curWetness >= curSnow) 
		{
            if(curWetness > 0.05f)
			    LuxDynamicWeatherScript.Rainfall = curWetness;
            else
                LuxDynamicWeatherScript.Rainfall = 0f;
		} 
		else 
		{            
             if(curSnow > 0.05f)
			    LuxDynamicWeatherScript.Rainfall = curSnow;
             else
                LuxDynamicWeatherScript.Rainfall = 0f;
		}

        LuxDynamicWeatherScript.Temperature = EnviroSkyMgr.instance.Weather.currentTemperature;

    }
#endif
}

