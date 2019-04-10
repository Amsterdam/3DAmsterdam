using UnityEngine;
using System.Collections;

#if ENVIRO_PHOTON_SUPPORT
using Photon.Pun;
[RequireComponent(typeof (PhotonView))]
[AddComponentMenu("Enviro/Integration/PhotonSynchronization")]
#endif
#if ENVIRO_PHOTON_SUPPORT
public class EnviroPhotonIntegration : MonoBehaviourPunCallbacks, IPunObservable
{ 
#else
public class EnviroPhotonIntegration : MonoBehaviour
{

#endif
#if ENVIRO_PHOTON_SUPPORT
    public ViewSynchronization synchronizationType = ViewSynchronization.Unreliable;
	public float updateSmoothing = 15f;
	private float networkHours;

	void Start () 
	{
		networkHours = EnviroSkyMgr.instance.GetUniversalTimeOfDay();
		photonView.ObservedComponents[0] = this;
		photonView.Synchronization = synchronizationType;
	}

	public override void OnJoinedRoom()
	{
		if (PhotonNetwork.IsMasterClient) 
		{
            EnviroSkyMgr.instance.Weather.updateWeather = true;
			EnviroSkyMgr.instance.OnZoneWeatherChanged += (EnviroWeatherPreset type, EnviroZone zone) => {
				SendWeatherToClient (type, zone);
			};

			EnviroSkyMgr.instance.OnSeasonChanged += (EnviroSeasons.Seasons season) => {
				SendSeasonToClient (season);
			};

		} 
		else 
		{
            EnviroSkyMgr.instance.Time.ProgressTime = EnviroTime.TimeProgressMode.None;
            EnviroSkyMgr.instance.Weather.updateWeather = false;
            EnviroSkyMgr.instance.Seasons.calcSeasons = false;
			StartCoroutine (GetWeather ());
		}
	}

	IEnumerator GetWeather ()
	{
		yield return 0;
		photonView.RPC("GetWeatherAndSeason", RpcTarget.MasterClient);
	}

	public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
	{
		if (stream.IsWriting) 
		{
			stream.SendNext(EnviroSkyMgr.instance.GetUniversalTimeOfDay());
			stream.SendNext(EnviroSkyMgr.instance.GetCurrentDay());
			stream.SendNext(EnviroSkyMgr.instance.GetCurrentYear());
		} 
		else 
		{
			networkHours = (float) stream.ReceiveNext();
            EnviroSkyMgr.instance.Time.Days = (int) stream.ReceiveNext();
            EnviroSkyMgr.instance.Time.Years = (int) stream.ReceiveNext();
		}
	}


	void SendWeatherToClient (EnviroWeatherPreset type, EnviroZone zone)
	{
        int weatherID = 0;
		int zoneID = 0;

		for(int i = 0; i < EnviroSkyMgr.instance.Weather.weatherPresets.Count; i++)
		{
			if (EnviroSkyMgr.instance.Weather.weatherPresets [i] == type)
				weatherID = i;
		}
		for (int i = 0; i < EnviroSkyMgr.instance.Weather.zones.Count; i++) 
		{
			if (EnviroSkyMgr.instance.Weather.zones [i] == zone)
				zoneID = i;
		}
 
		photonView.RPC("SendWeatherUpdate", RpcTarget.OthersBuffered,weatherID,zoneID);
	}

	void SendSeasonToClient (EnviroSeasons.Seasons s)
	{
		photonView.RPC("SendSeasonUpdate",RpcTarget.OthersBuffered,(int)s);
	}

	[PunRPC]
	void GetWeatherAndSeason ()
	{
		for (int i = 0; i < EnviroSkyMgr.instance.Weather.zones.Count; i++) 
		{
			SendWeatherToClient(EnviroSkyMgr.instance.Weather.zones[i].currentActiveZoneWeatherPreset, EnviroSkyMgr.instance.Weather.zones[i]);
		}

		SendSeasonToClient(EnviroSkyMgr.instance.Seasons.currentSeasons);
	}



	[PunRPC]
	void SendWeatherUpdate (int id, int zone) 
	{
        EnviroSkyMgr.instance.Weather.zones [zone].currentActiveZoneWeatherPreset = EnviroSkyMgr.instance.Weather.zones [zone].zoneWeatherPresets [id];
        EnviroSkyMgr.instance.Weather.zones[zone].currentActiveZoneWeatherPrefab = EnviroSkyMgr.instance.Weather.zones[zone].zoneWeather[id];
    }

	[PunRPC]
	void SendSeasonUpdate (int id) 
	{
		switch (id) 
		{
		case 0:
                EnviroSkyMgr.instance.Seasons.currentSeasons = EnviroSeasons.Seasons.Spring;
		break;

		case 1:
                EnviroSkyMgr.instance.Seasons.currentSeasons = EnviroSeasons.Seasons.Summer;
		break;

		case 2:
                EnviroSkyMgr.instance.Seasons.currentSeasons = EnviroSeasons.Seasons.Autumn;
		break;

		case 3:
                EnviroSkyMgr.instance.Seasons.currentSeasons = EnviroSeasons.Seasons.Winter;
		break;
		}
	}

	void Update ()
	{

        if (EnviroSkyMgr.instance == null || !EnviroSkyMgr.instance.IsAvailable())
            return;

        if (!PhotonNetwork.IsMasterClient) 
		{
			if (networkHours < 0.5f && EnviroSkyMgr.instance.Time.Hours > 23f)
                EnviroSkyMgr.instance.SetTimeOfDay(networkHours);

            EnviroSkyMgr.instance.SetTimeOfDay(Mathf.Lerp (EnviroSkyMgr.instance.GetUniversalTimeOfDay(), (float)networkHours, Time.deltaTime * updateSmoothing));
		}

	}
#endif
}
