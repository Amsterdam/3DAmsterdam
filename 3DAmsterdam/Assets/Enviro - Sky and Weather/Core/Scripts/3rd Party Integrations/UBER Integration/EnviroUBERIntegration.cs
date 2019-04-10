using UnityEngine;
using System.Collections;
#if ENVIRO_UBER_SUPPORT
[AddComponentMenu("Enviro/Integration/UBER")]
#endif
public class EnviroUBERIntegration : MonoBehaviour
{
#if ENVIRO_UBER_SUPPORT
	public UBER_GlobalParams UBER_Global_Parameters;

	private float curWetness = 0f;
	private float curSnow = 0f;

    public void Start()
    {
		if (UBER_Global_Parameters != null) 
        {
			UBER_Global_Parameters.Simulate = true;
			UBER_Global_Parameters.UseParticleSystem = false;
		} 
        else 
		{
            UBER_Global_Parameters = FindObjectOfType<UBER_GlobalParams>();

            if (UBER_Global_Parameters != null)
            {
                UBER_Global_Parameters.Simulate = true;
                UBER_Global_Parameters.UseParticleSystem = false;
            }
            else
            {
                Debug.LogError("Please setup dynamic weather for UBER!");
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
			   UBER_Global_Parameters.fallIntensity = curWetness;
            else
               UBER_Global_Parameters.fallIntensity = 0f;
		}
		else
        {
            if(curSnow > 0.05f)
			   UBER_Global_Parameters.fallIntensity = curSnow;
            else
               UBER_Global_Parameters.fallIntensity = 0f;
		}

		UBER_Global_Parameters.temperature = EnviroSkyMgr.instance.Weather.currentTemperature;
	}
#endif
}

