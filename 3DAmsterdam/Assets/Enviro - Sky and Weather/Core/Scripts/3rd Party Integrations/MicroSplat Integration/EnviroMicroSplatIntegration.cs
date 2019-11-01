using UnityEngine;
using System.Collections;
#if ENVIRO_MICROSPLAT_SUPPORT
[AddComponentMenu("Enviro/Integration/MicroSplat Integration")]
#endif
public class EnviroMicroSplatIntegration : MonoBehaviour {
#if ENVIRO_MICROSPLAT_SUPPORT
    [Header("Wetness")]
    public bool UpdateWetness = true;
    [Range(0f, 1f)]
    public float maxWetness = 1f;
    [Header("Rain Ripples")]
    public bool UpdateRainRipples = true;
    [Header("Puddle Settings")]
    public bool UpdatePuddles = true;
    [Range(0f,1f)]
    public float maxPuddle = 1f;
    [Header("Stream Settings")]
    public bool UpdateStreams = true;
    [Header("Snow Settings")]
    public bool UpdateSnow = true;

	void Update () 
	{
    	if (EnviroSkyMgr.instance == null)
			return;

		if (UpdateSnow){
            Shader.SetGlobalFloat ("_Global_SnowLevel", EnviroSkyMgr.instance.GetSnowIntensity());
		}

		if (UpdateWetness) {
            Shader.SetGlobalVector ("_Global_WetnessParams", new Vector2(EnviroSkyMgr.instance.GetWetnessIntensity(), maxWetness));
		}
			
		if (UpdatePuddles) {
            Shader.SetGlobalVector ("_Global_PuddleParams", new Vector2(EnviroSkyMgr.instance.GetWetnessIntensity(), maxPuddle));
		}

		if (UpdateRainRipples) {
            Shader.SetGlobalFloat("_Global_RainIntensity", EnviroSkyMgr.instance.GetWetnessIntensity());
        }

        if (UpdateStreams) {
            Shader.SetGlobalFloat("_Global_StreamMax", EnviroSkyMgr.instance.GetWetnessIntensity());
        }

        //Sync all MicroSplat Values
        //MicroSplatTerrain.SyncAll();
    }
#endif
}
