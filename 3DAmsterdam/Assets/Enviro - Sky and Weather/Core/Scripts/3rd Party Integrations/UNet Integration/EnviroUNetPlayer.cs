/// <summary>
/// This component can be used to synchronize time and weather in games where server is a player too.
/// </summary>

using UnityEngine;
#if ENVIRO_UNET_SUPPORT
using UnityEngine.Networking;
#endif
using System.Collections;
#if ENVIRO_UNET_SUPPORT
[AddComponentMenu("Enviro/Integration/UNet Player")]
[RequireComponent(typeof (NetworkIdentity))]
public class EnviroUNetPlayer : NetworkBehaviour {
#else
public class EnviroUNetPlayer : MonoBehaviour {
#endif
#if ENVIRO_UNET_SUPPORT
    public bool assignOnStart = true;
    public bool findSceneCamera = true;

    public GameObject Player;
	public Camera PlayerCamera;

	public void Start()
	{
		// Deactivate if it isn't ours!
		if (!isLocalPlayer && !isServer) {
			this.enabled = false;
			return;
		}
  
        if (PlayerCamera == null && findSceneCamera)
            PlayerCamera = Camera.main;

        if (isLocalPlayer) 
		{
			if (assignOnStart && Player != null && PlayerCamera != null)
				EnviroSkyMgr.instance.AssignAndStart (Player, PlayerCamera);

			Cmd_RequestSeason ();
			Cmd_RequestCurrentWeather ();
		}
	}
		
	[Command]
	void Cmd_RequestSeason ()
	{
		RpcRequestSeason((int)EnviroSkyMgr.instance.GetCurrentSeason());
	}

	[ClientRpc]
	void RpcRequestSeason (int season)
	{
        EnviroSkyMgr.instance.ChangeSeason((EnviroSeasons.Seasons)season);
	}

	[Command]
	void Cmd_RequestCurrentWeather ()
	{
		for (int i = 0; i < EnviroSkyMgr.instance.Weather.zones.Count; i++) 
		{
			for (int w = 0; w < EnviroSkyMgr.instance.Weather.WeatherPrefabs.Count; w++)
			{
                if (EnviroSkyMgr.instance.Weather.WeatherPrefabs[w] == EnviroSkyMgr.instance.Weather.zones[i].currentActiveZoneWeatherPrefab)
					RpcRequestCurrentWeather(w,i);
			}
		}
	}

	[ClientRpc]
	void RpcRequestCurrentWeather (int weather, int zone)
	{
        EnviroSkyMgr.instance.ChangeZoneWeather(zone, weather);
    }
#endif
}
