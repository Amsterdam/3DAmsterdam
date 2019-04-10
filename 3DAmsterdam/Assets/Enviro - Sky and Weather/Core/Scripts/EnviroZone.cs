using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[AddComponentMenu("Enviro/Weather Zone")]
public class EnviroZone : MonoBehaviour {

	public enum WeatherUpdateMode
	{
		GameTimeHours,
		RealTimeMinutes
	}

	[Tooltip("Defines the zone name.")]
	public string zoneName;
	[Tooltip("Uncheck to remove OnTriggerExit call when using overlapping zone layout.")]
	public bool ExitToDefault = true;

	public List<EnviroWeatherPrefab> zoneWeather = new List<EnviroWeatherPrefab>();
	public List<EnviroWeatherPrefab> curPossibleZoneWeather;

	[Header("Zone weather settings:")]
	[Tooltip("Add all weather prefabs for this zone here.")]
	public List<EnviroWeatherPreset> zoneWeatherPresets = new List<EnviroWeatherPreset>();
	[Tooltip("Shall weather changes occure based on gametime or realtime?")]
	public WeatherUpdateMode updateMode = WeatherUpdateMode.GameTimeHours;
	[Tooltip("Defines how often (gametime hours or realtime minutes) the system will heck to change the current weather conditions.")]
	public float WeatherUpdateIntervall = 6f;
    [Header("Zone scaling and gizmo:")]
    [Tooltip("Enable this to use a mesh for zone trigger.")]
    public bool useMeshZone = false;
    [Tooltip("Custom Zone Mesh")]
    public Mesh zoneMesh;

    [Tooltip("Defines the zone scale.")]
	public Vector3 zoneScale = new Vector3 (100f, 100f, 100f);
	[Tooltip("Defines the color of the zone's gizmo in editor mode.")]
	public Color zoneGizmoColor = Color.gray;

	[Header("Current active weather:")]
	[Tooltip("The current active weather conditions.")]
	public EnviroWeatherPrefab currentActiveZoneWeatherPrefab;
	public EnviroWeatherPreset currentActiveZoneWeatherPreset;
	[HideInInspector]public EnviroWeatherPrefab lastActiveZoneWeatherPrefab;
	[HideInInspector]public EnviroWeatherPreset lastActiveZoneWeatherPreset;

	private BoxCollider zoneCollider;
    private MeshCollider zoneMeshCollider;
    private double nextUpdate;
	private float nextUpdateRealtime;
	private bool init = false;
	private bool isDefault;


	void Start () 
	{
		if (zoneWeatherPresets.Count > 0)
		{

            if (!useMeshZone)
            {
                zoneCollider = gameObject.AddComponent<BoxCollider>();
                zoneCollider.isTrigger = true;
            }
            else
            {
                zoneMeshCollider = gameObject.AddComponent<MeshCollider>();
                zoneMeshCollider.sharedMesh = zoneMesh;
                zoneMeshCollider.convex = true;
                zoneMeshCollider.isTrigger = true;
            }

			if (!EnviroSkyMgr.instance.IsDefaultZone(gameObject))
				EnviroSkyMgr.instance.RegisterZone (this);
			else 
				isDefault = true;


			UpdateZoneScale ();

			nextUpdate = EnviroSkyMgr.instance.GetCurrentTimeInHours() + WeatherUpdateIntervall;
			nextUpdateRealtime = Time.time + (WeatherUpdateIntervall * 60f); 
		}
		else
		{
			Debug.Log("Please add Weather Prefabs to Zone:" + gameObject.name);
		}
	}

	public void UpdateZoneScale ()
	{
        if (!isDefault && !useMeshZone)
            zoneCollider.size = zoneScale;
        else if (!isDefault && useMeshZone)
            transform.localScale = zoneScale;
        else if (isDefault && !useMeshZone)
            zoneCollider.size = (Vector3.one * (1f / transform.localScale.y)) * 0.25f;
    }

	public void CreateZoneWeatherTypeList ()
	{
		// Add new WeatherPrefabs
		for ( int i = 0; i < zoneWeatherPresets.Count; i++)
		{
			if (zoneWeatherPresets [i] == null) {
				Debug.Log ("Warning! Missing Weather Preset in Zone: " + this.zoneName);
				return;
			}

			bool addThis = true;
			for (int i2 = 0; i2 < EnviroSkyMgr.instance.GetCurrentWeatherPresetList().Count; i2++)
			{
				if (zoneWeatherPresets [i] == EnviroSkyMgr.instance.GetCurrentWeatherPresetList()[i2]) 
				{
					addThis = false;
					zoneWeather.Add (EnviroSkyMgr.instance.GetCurrentWeatherPrefabList()[i2]);
				}
			}

			if (addThis) {
				GameObject wPrefab = new GameObject ();
				EnviroWeatherPrefab wP = wPrefab.AddComponent<EnviroWeatherPrefab> ();
				wP.weatherPreset = zoneWeatherPresets [i];
				wPrefab.name = wP.weatherPreset.Name;

				// Check and create particle systems.
				for (int w = 0; w < wP.weatherPreset.effectSystems.Count; w++)
				{
					if (wP.weatherPreset.effectSystems [w] == null || wP.weatherPreset.effectSystems [w].prefab == null) {
						Debug.Log ("Warning! Missing Particle System Entry: " + wP.weatherPreset.Name);
						Destroy (wPrefab);
						return;
					}
					GameObject eS = (GameObject)Instantiate (wP.weatherPreset.effectSystems [w].prefab, wPrefab.transform);
					eS.transform.localPosition = wP.weatherPreset.effectSystems [w].localPositionOffset;
					eS.transform.localEulerAngles = wP.weatherPreset.effectSystems [w].localRotationOffset;
					ParticleSystem pS = eS.GetComponent<ParticleSystem> ();

					if (pS != null)
						wP.effectSystems.Add (pS);
					else {
						pS = eS.GetComponentInChildren<ParticleSystem> ();
						if (pS != null)
							wP.effectSystems.Add (pS);
						else {
							Debug.Log ("No Particle System found in prefab in weather preset: " + wP.weatherPreset.Name);
							Destroy (wPrefab);
							return;
						}
					}
				}
				wP.effectEmmisionRates.Clear ();
				wPrefab.transform.parent = EnviroSkyMgr.instance.GetVFXHolder().transform;
				wPrefab.transform.localPosition = Vector3.zero;
				wPrefab.transform.localRotation = Quaternion.identity;
				zoneWeather.Add(wP);

                EnviroSkyMgr.instance.GetCurrentWeatherPrefabList().Add (wP);
                EnviroSkyMgr.instance.GetCurrentWeatherPresetList().Add (zoneWeatherPresets [i]);
			}
		}
		
        // Setup Particle Systems Emission Rates
		for (int i = 0; i < zoneWeather.Count; i++)
		{
			for (int i2 = 0; i2 < zoneWeather[i].effectSystems.Count; i2++)
			{
				zoneWeather[i].effectEmmisionRates.Add(EnviroSkyMgr.instance.GetEmissionRate(zoneWeather[i].effectSystems[i2]));
                EnviroSkyMgr.instance.SetEmissionRate(zoneWeather[i].effectSystems[i2],0f);
			}   
		}
			
        //Set initial weather
		if (isDefault && EnviroSkyMgr.instance.GetStartWeatherPreset() != null) 
		{
            EnviroSkyMgr.instance.ChangeWeatherInstant(EnviroSkyMgr.instance.GetStartWeatherPreset());

            for (int i = 0; i < zoneWeather.Count; i++)
            {
                if(zoneWeather[i].weatherPreset == EnviroSkyMgr.instance.GetStartWeatherPreset())
                {
                    currentActiveZoneWeatherPrefab = zoneWeather[i];
                    lastActiveZoneWeatherPrefab = zoneWeather[i];
                }
            }
            currentActiveZoneWeatherPreset = EnviroSkyMgr.instance.GetStartWeatherPreset();
            lastActiveZoneWeatherPreset = EnviroSkyMgr.instance.GetStartWeatherPreset();
		} 
		else 
		{
			currentActiveZoneWeatherPrefab = zoneWeather [0];
			lastActiveZoneWeatherPrefab = zoneWeather [0];
			currentActiveZoneWeatherPreset = zoneWeatherPresets [0];
			lastActiveZoneWeatherPreset = zoneWeatherPresets [0];
		}

		nextUpdate = EnviroSkyMgr.instance.GetCurrentTimeInHours() + WeatherUpdateIntervall;
	}
		
	void BuildNewWeatherList ()
	{
		curPossibleZoneWeather = new List<EnviroWeatherPrefab> ();
		for (int i = 0; i < zoneWeather.Count; i++) 
		{
			switch (EnviroSkyMgr.instance.GetCurrentSeason())
			{
			case EnviroSeasons.Seasons.Spring:
				if (zoneWeather[i].weatherPreset.Spring)
					curPossibleZoneWeather.Add(zoneWeather[i]);
				break;
			case EnviroSeasons.Seasons.Summer:
				if (zoneWeather[i].weatherPreset.Summer)
					curPossibleZoneWeather.Add(zoneWeather[i]);
				break;
			case EnviroSeasons.Seasons.Autumn:
				if (zoneWeather[i].weatherPreset.Autumn)
					curPossibleZoneWeather.Add(zoneWeather[i]);
				break;
			case EnviroSeasons.Seasons.Winter:
				if (zoneWeather[i].weatherPreset.winter)
					curPossibleZoneWeather.Add(zoneWeather[i]);
				break;
			}
		} 
	}

	EnviroWeatherPrefab PossibiltyCheck ()
	{
		List<EnviroWeatherPrefab> over = new List<EnviroWeatherPrefab> ();

		for (int i = 0 ; i < curPossibleZoneWeather.Count;i++)
		{
			int würfel = UnityEngine.Random.Range (0,100);

			if (EnviroSkyMgr.instance.GetCurrentSeason() == EnviroSeasons.Seasons.Spring)
			{
				if (würfel <= curPossibleZoneWeather[i].weatherPreset.possibiltyInSpring)
					over.Add(curPossibleZoneWeather[i]);
			}else
			if (EnviroSkyMgr.instance.GetCurrentSeason() == EnviroSeasons.Seasons.Summer)
			{
					if (würfel <= curPossibleZoneWeather[i].weatherPreset.possibiltyInSummer)
					over.Add(curPossibleZoneWeather[i]);
			}else
			if (EnviroSkyMgr.instance.GetCurrentSeason() == EnviroSeasons.Seasons.Autumn)
			{
						if (würfel <= curPossibleZoneWeather[i].weatherPreset.possibiltyInAutumn)
					over.Add(curPossibleZoneWeather[i]);
			}else
			if (EnviroSkyMgr.instance.GetCurrentSeason() == EnviroSeasons.Seasons.Winter)
			{
							if (würfel <= curPossibleZoneWeather[i].weatherPreset.possibiltyInWinter)
					over.Add(curPossibleZoneWeather[i]);
			}
		} 

		if (over.Count > 0)
		{
            EnviroSkyMgr.instance.NotifyZoneWeatherChanged (over [0].weatherPreset, this);
			return over [0];
		}
		else
			return currentActiveZoneWeatherPrefab;
	}
		
	void WeatherUpdate ()
	{
		nextUpdate = EnviroSkyMgr.instance.GetCurrentTimeInHours() + WeatherUpdateIntervall;
		nextUpdateRealtime = Time.time + (WeatherUpdateIntervall * 60f); 

		BuildNewWeatherList ();

		lastActiveZoneWeatherPrefab = currentActiveZoneWeatherPrefab;
		lastActiveZoneWeatherPreset = currentActiveZoneWeatherPreset;
		currentActiveZoneWeatherPrefab = PossibiltyCheck ();
		currentActiveZoneWeatherPreset = currentActiveZoneWeatherPrefab.weatherPreset;
        EnviroSkyMgr.instance.NotifyZoneWeatherChanged (currentActiveZoneWeatherPreset, this);
	}

    IEnumerator CreateWeatherListLate ()
	{
		yield return 0;
		CreateZoneWeatherTypeList ();
		init = true;
	}

	void LateUpdate () 
	{
        if (EnviroSkyMgr.instance == null)
        {
            Debug.Log("No EnviroSky instance found!");
            return;
        }

        if (EnviroSkyMgr.instance.IsStarted() && !init) 
		{
            if(zoneWeatherPresets.Count < 1)
            {
                Debug.Log("Zone with no Presets! Please assign at least one preset. Deactivated for now!");
                this.enabled = false;
                return;
            }

			if (isDefault) {
				CreateZoneWeatherTypeList ();          
                init = true;
            } else
				StartCoroutine (CreateWeatherListLate ());
		}

		if (updateMode == WeatherUpdateMode.GameTimeHours) {
			if (EnviroSkyMgr.instance.GetCurrentTimeInHours() > nextUpdate && EnviroSkyMgr.instance.IsAutoWeatherUpdateActive() && EnviroSkyMgr.instance.IsStarted())
				WeatherUpdate ();
		} else {
			if (Time.time > nextUpdateRealtime && EnviroSkyMgr.instance.IsAutoWeatherUpdateActive() && EnviroSkyMgr.instance.IsStarted())
				WeatherUpdate ();
		}

        if (EnviroSkyMgr.instance.Player == null)
        {
            // Debug.Log("No Player Assigned in EnviroSky object!");
            return;
        }

        if (isDefault && init && !useMeshZone)                               
			zoneCollider.center = new Vector3(0f,(EnviroSkyMgr.instance.Player.transform.position.y-transform.position.y) / transform.lossyScale.y,0f);
	}


	/// Triggers
	void OnTriggerEnter (Collider col)
	{
		if (EnviroSkyMgr.instance == null)
			return;

        if (EnviroSkyMgr.instance.GetUseWeatherTag()) {
			if (col.gameObject.tag == EnviroSkyMgr.instance.GetEnviroSkyTag()) {
                EnviroSkyMgr.instance.SetCurrentActiveZone(this);
                EnviroSkyMgr.instance.NotifyZoneChanged (this);
			}
		} else {
			if (EnviroSkyMgr.instance.IsEnviroSkyAttached(col.gameObject)) {
                EnviroSkyMgr.instance.SetCurrentActiveZone(this);
                EnviroSkyMgr.instance.NotifyZoneChanged (this);
			}
		}
	}

	void OnTriggerExit (Collider col)
	{
		if (ExitToDefault == false || EnviroSkyMgr.instance == null)
			return;
		
		if (EnviroSkyMgr.instance.GetUseWeatherTag()) {
			if (col.gameObject.tag == EnviroSkyMgr.instance.GetEnviroSkyTag()) {
                EnviroSkyMgr.instance.SetToZone(0);
                EnviroSkyMgr.instance.NotifyZoneChanged (EnviroSkyMgr.instance.GetZoneByID(0));
			}
		} else {
			if (EnviroSkyMgr.instance.IsEnviroSkyAttached(col.gameObject)) {
                EnviroSkyMgr.instance.SetToZone(0);
                EnviroSkyMgr.instance.NotifyZoneChanged (EnviroSkyMgr.instance.GetZoneByID(0));
			}
		}
	}

	void OnDrawGizmos () 
	{
		Gizmos.color = zoneGizmoColor;

        if (useMeshZone && zoneMesh != null)
            Gizmos.DrawMesh(zoneMesh);
        else
            Gizmos.DrawCube(transform.position, new Vector3(zoneScale.x, zoneScale.y, zoneScale.z));
	}
}
