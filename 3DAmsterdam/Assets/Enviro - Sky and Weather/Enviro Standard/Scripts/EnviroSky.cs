////////////////////////////////////////////////////////////////////////////
////////////                    EnviroSky.cs                        ////////
////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using System;
using System.Collections.Generic;
using System.Collections;

[ExecuteInEditMode]
public class EnviroSky : EnviroCore 
{
    ////////////////////////////////
    #region Var
    private static EnviroSky _instance; // Creat a static instance for easy access!

    public static EnviroSky instance
    {
        get
        {
            //If _instance hasn't been set yet, we grab it from the scene!
            //This will only happen the first time this reference is used.
            if (_instance == null)
                _instance = GameObject.FindObjectOfType<EnviroSky>();
            return _instance;
        }
    }

    public string prefabVersion = "2.1.0";

    [Header("Virtual Reality")]
    [Tooltip("Enable this when using singlepass rendering.")]
    public bool singlePassVR = false;
    [Tooltip("Enable this to activate volume lighing")]
    [HideInInspector]public bool useVolumeLighting = true;

    [HideInInspector]public bool useVolumeClouds = true;
    [HideInInspector]public bool useFlatClouds = false;
    [HideInInspector]public bool useParticleClouds = false;
    [HideInInspector]public bool useDistanceBlur = true;
    private bool flatCloudsSkybox = false;

    [Header("Scene View Preview")]
    public bool showVolumeLightingInEditor = true;
    public bool showVolumeCloudsInEditor = true;
    public bool showFlatCloudsInEditor = true;
    public bool showFogInEditor = true;
    public bool showDistanceBlurInEditor = true;

    private EnviroCloudSettings.CloudQuality lastCloudsQuality;

    //Camera Components
    [HideInInspector]public Camera satCamera;
    [HideInInspector]public EnviroVolumeLight directVolumeLight;
    [HideInInspector]public EnviroSkyRendering EnviroSkyRender;
                                                                                                                          
    //VolumeLighting
    [HideInInspector]public float globalVolumeLightIntensity;


    // Render Textures
    [HideInInspector]public RenderTexture cloudsRenderTarget;
    [HideInInspector]public RenderTexture flatCloudsRenderTarget;
    [HideInInspector]public RenderTexture weatherMap;
    [HideInInspector]public RenderTexture moonRenderTarget;
    [HideInInspector]public RenderTexture satRenderTarget;
    [HideInInspector]public RenderTexture cloudShadowMap;

    //Wind
    [HideInInspector]public Vector2 cloudAnim;
    [HideInInspector]public Vector2 cloudAnimNonScaled;
    [HideInInspector]public float windStrenght = 0;

    //Materials
    [HideInInspector]public Material skyMat;
    [HideInInspector]public Material flatCloudsMat;
    private Material weatherMapMat;
    private Material cloudShadowMat;

    //private double lastMoonUpdate;
    private float starsTwinklingRot;
    //Inspector
    [HideInInspector]public bool showSettings = false;

    //PostProcessing
    public float blurDistance = 100;
    public float blurIntensity = 1f;
    public float blurSkyIntensity = 1f;
    #endregion
    ////////////////////////////////
    #region Startup Setup

    void Start()
	{
        //Check for Manager first!
        if(EnviroSkyMgr.instance == null)
        {
            Debug.Log("Please use the EnviroSky Manager!");
            gameObject.SetActive(false);
            return;
        }

		//Time
		SetTime (GameTime.Years, GameTime.Days, GameTime.Hours, GameTime.Minutes, GameTime.Seconds);
		lastHourUpdate = Mathf.RoundToInt(internalHour);
		currentTimeInHours = GetInHours (internalHour, GameTime.Days, GameTime.Years, GameTime.DaysInYear);
		Weather.weatherFullyChanged = false;
		thunder = 0f;
        lastCloudsQuality = cloudsSettings.cloudsQuality;

        //Create material
        if (weatherMapMat == null)
            weatherMapMat = new Material(Shader.Find("Enviro/WeatherMap"));

        // Check for Profile
        if (profileLoaded)
        {
			InvokeRepeating ("UpdateEnviroment", 0, qualitySettings.UpdateInterval);// Vegetation Updates
			CreateEffects ("Enviro Effects");  //Create Weather Effects Holder

            // Instantiate Lightning Effect
            if (weatherSettings.lightningEffect != null)
                lightningEffect = Instantiate(weatherSettings.lightningEffect, EffectsHolder.transform).GetComponent<ParticleSystem>();

            if (PlayerCamera != null && Player != null && AssignInRuntime == false && profile != null) {
				Init ();
			}
        }
        StartCoroutine(SetSkyBoxLateAdditive());
    }

    private IEnumerator SetSkyBoxLateAdditive()
    {
        yield return 0;
        if (skyMat != null && RenderSettings.skybox != skyMat)
            SetupSkybox();
    }

    void OnEnable()
	{
		//Set Weather
		Weather.currentActiveWeatherPreset = Weather.zones[0].currentActiveZoneWeatherPreset;
		Weather.lastActiveWeatherPreset = Weather.currentActiveWeatherPreset;

		if (profile == null) {
			Debug.LogError ("No profile assigned!");
			return;
		}

		// Auto Load profile
		if (profileLoaded == false)
			ApplyProfile (profile);

		PreInit ();

		if (AssignInRuntime) {
			started = false;	//Wait for assignment
		} else if (PlayerCamera != null && Player != null){
			Init ();
		}
	}
	/// <summary>
	/// Re-Initilize the system.
	/// </summary>
	public void ReInit ()
	{
		OnEnable ();
	}
	/// <summary>
	/// Pee-Initilize the system.
	/// </summary>
	private void PreInit ()
	{
		// Check time
		if (GameTime.solarTime < GameTime.dayNightSwitch)
			isNight = true;
		else
			isNight = false;

		//return when in server mode!
		if (serverMode)
			return;

		CheckSatellites ();

		// Setup Fog Mode
		RenderSettings.fogMode = fogSettings.Fogmode;

        // Setup Skybox Material
#if ENVIRO_PRO
        if(EnviroSkyMgr.instance.currentRenderPipeline != EnviroSkyMgr.EnviroRenderPipeline.HDRP)
#endif
            SetupSkybox();

        // Set ambient mode
        RenderSettings.ambientMode = lightSettings.ambientMode;

        // Set Fog
        //RenderSettings.fogDensity = 0f;
        //RenderSettings.fogStartDistance = 0f;
        //RenderSettings.fogEndDistance = 1000f;

        // Setup ReflectionProbe
        Components.GlobalReflectionProbe.size = transform.localScale;
		Components.GlobalReflectionProbe.refreshMode = UnityEngine.Rendering.ReflectionProbeRefreshMode.ViaScripting;

		if (!Components.Sun)
        {
            Debug.LogError("Please set sun object in inspector!");
        }

        if (!Components.satellites)
        {
            Debug.LogError("Please set satellite object in inspector!");
        }

        if (Components.Moon){
			MoonTransform = Components.Moon.transform;
			// Set start moon phase
			customMoonPhase = skySettings.startMoonPhase;
		}
		else
        {
            Debug.LogError("Please set moon object in inspector!");
        }

        if (weatherMap == null)
        {
            weatherMap = new RenderTexture(1024, 1024, 0, RenderTextureFormat.Default);
            weatherMap.wrapMode = TextureWrapMode.Repeat;
        }

        if (Components.DirectLight) 
		{
            if (Components.DirectLight.name == "Direct Lght")
            {
                DestroyImmediate(Components.DirectLight.gameObject);
                Components.DirectLight = CreateDirectionalLight();
            }

			MainLight = Components.DirectLight.GetComponent<Light>(); 

			if (directVolumeLight == null)
                directVolumeLight = Components.DirectLight.GetComponent<EnviroVolumeLight> ();

			if (directVolumeLight == null)
				directVolumeLight = Components.DirectLight.gameObject.AddComponent<EnviroVolumeLight> ();

            if (EnviroSkyMgr.instance.dontDestroy && Application.isPlaying)
                DontDestroyOnLoad(Components.DirectLight);
        } 
		else 
		{
            GameObject oldLight = GameObject.Find("Enviro Directional Light");

            if(oldLight != null)
               Components.DirectLight = oldLight.transform;
            else
               Components.DirectLight = CreateDirectionalLight();

            MainLight = Components.DirectLight.GetComponent<Light>();

            if (directVolumeLight == null)
                directVolumeLight = Components.DirectLight.GetComponent<EnviroVolumeLight>();

            if (directVolumeLight == null)
                directVolumeLight = Components.DirectLight.gameObject.AddComponent<EnviroVolumeLight>();

            if (EnviroSkyMgr.instance.dontDestroy && Application.isPlaying)
                DontDestroyOnLoad(Components.DirectLight);
        }

        if (cloudShadowMap != null)
            DestroyImmediate(cloudShadowMap);

        cloudShadowMap = new RenderTexture(2048, 2048, 0,RenderTextureFormat.Default);
        cloudShadowMap.wrapMode = TextureWrapMode.Repeat;

        if (cloudShadowMat != null)
            DestroyImmediate(cloudShadowMat);

        cloudShadowMat = new Material(Shader.Find("Enviro/ShadowCookie"));

         if (cloudsSettings.shadowIntensity > 0)
         {
             Graphics.Blit(weatherMap, cloudShadowMap, cloudShadowMat);

             MainLight.cookie = cloudShadowMap;
             MainLight.cookieSize = 10000;
         }
         else
             MainLight.cookie = null;

        if (Components.particleClouds)
        {
            ParticleSystem[] systems = Components.particleClouds.GetComponentsInChildren<ParticleSystem>();

            if (systems.Length > 0)
                particleClouds.layer1System = systems[0];
            if (systems.Length > 1)
                particleClouds.layer2System = systems[1];

            if (particleClouds.layer1System != null)
                particleClouds.layer1Material = particleClouds.layer1System.GetComponent<ParticleSystemRenderer>().sharedMaterial;

            if (particleClouds.layer2System != null)
                particleClouds.layer2Material = particleClouds.layer2System.GetComponent<ParticleSystemRenderer>().sharedMaterial;
        }
        else
        {
            Debug.LogError("Please set particleCLouds object in inspector!");
        }

    }
    /// <summary>
    /// Creation and assignment of skybox
    /// </summary>
    public void SetupSkybox()
    {
        if (skySettings.skyboxMode == EnviroSkySettings.SkyboxModi.Simple)
        {
            if (skyMat != null)
                DestroyImmediate(skyMat);

            skyMat = new Material(Shader.Find("Enviro/SkyboxSimple"));

            if (skySettings.starsCubeMap != null)
                skyMat.SetTexture("_Stars", skySettings.starsCubeMap);
            if (skySettings.galaxyCubeMap != null)
                skyMat.SetTexture("_Galaxy", skySettings.galaxyCubeMap);

            RenderSettings.skybox = skyMat;
        }
        else if (skySettings.skyboxMode == EnviroSkySettings.SkyboxModi.Default)
        {
            if (skyMat != null)
                DestroyImmediate(skyMat);

            if(!useFlatClouds)
                skyMat = new Material(Shader.Find("Enviro/Skybox"));
            else
                skyMat = new Material(Shader.Find("Enviro/SkyboxFlatClouds"));

            if (skySettings.starsCubeMap != null)
                skyMat.SetTexture("_Stars", skySettings.starsCubeMap);
            if (skySettings.galaxyCubeMap != null)
                skyMat.SetTexture("_Galaxy", skySettings.galaxyCubeMap);

            Cubemap starsTwinklingNoise = Resources.Load("cube_enviro_starsNoise") as Cubemap;

            if(starsTwinklingNoise != null)
                skyMat.SetTexture("_StarsTwinklingNoise", starsTwinklingNoise);

            Texture2D dither = Resources.Load("tex_enviro_dither") as Texture2D;

            if (dither != null)
                skyMat.SetTexture("_DitheringTex", dither);

            RenderSettings.skybox = skyMat;
        }
        else if (skySettings.skyboxMode == EnviroSkySettings.SkyboxModi.CustomSkybox)
        {
            if (skySettings.customSkyboxMaterial != null)
                RenderSettings.skybox = skySettings.customSkyboxMaterial;
        }

        //Update environment texture in next frame!
        if (lightSettings.ambientMode == UnityEngine.Rendering.AmbientMode.Skybox)
            StartCoroutine(UpdateAmbientLightWithDelay());
    }
    /// <summary>
    /// Update the environment texture for skybox ambient mode with one frame delay. Somehow not working in same frame as we create the skybox material.
    /// </summary>
    private IEnumerator UpdateAmbientLightWithDelay()
    {
        yield return 0;
        DynamicGI.UpdateEnvironment();

    }
    /// <summary>
    /// Final Initilization and startup.
    /// </summary>
    private void Init ()
	{
		if (profile == null)
			return;

		if (serverMode) {
			started = true;
			return;
		}

		InitImageEffects ();

		// Setup Camera
		if (PlayerCamera != null) 
		{

			if (setCameraClearFlags)
				PlayerCamera.clearFlags = CameraClearFlags.Skybox;
	
			// Workaround for deferred forve HDR...
			if (PlayerCamera.actualRenderingPath == RenderingPath.DeferredShading)
				SetCameraHDR (PlayerCamera, true);
			else
				SetCameraHDR (PlayerCamera, HDR);

			Components.GlobalReflectionProbe.farClipPlane = PlayerCamera.farClipPlane;
        }

        //Destroy old Cameras not needed for 2.0
        DestroyImmediate(GameObject.Find ("Enviro Cameras"));

		if(satelliteSettings.additionalSatellites.Count > 0)
            CreateSatCamera();

        started = true;
    }
    /// <summary>
	/// Creation and setup of post processing components.
	/// </summary>
	private void InitImageEffects ()
	{
#if ENVIRO_PRO
        if(EnviroSkyMgr.instance.currentRenderPipeline != EnviroSkyMgr.EnviroRenderPipeline.Legacy)
        {
            EnviroSkyRender = PlayerCamera.gameObject.GetComponent<EnviroSkyRendering>();

            if (EnviroSkyRender != null)
                DestroyImmediate(EnviroSkyRender);

            EnviroPostProcessing = PlayerCamera.gameObject.GetComponent<EnviroPostProcessing>();

            if (EnviroPostProcessing != null)
                DestroyImmediate(EnviroPostProcessing);


            return;
        }
#endif
        EnviroSkyRender = PlayerCamera.gameObject.GetComponent<EnviroSkyRendering>();

        if (EnviroSkyRender == null)
            EnviroSkyRender = PlayerCamera.gameObject.AddComponent<EnviroSkyRendering>();

        #if UNITY_EDITOR
        string[] assets = UnityEditor.AssetDatabase.FindAssets("enviro_spot_cookie", null);
        for (int idx = 0; idx < assets.Length; idx++)
        {
            string path = UnityEditor.AssetDatabase.GUIDToAssetPath(assets[idx]);
            if (path.Length > 0)
            {
                EnviroSkyRender.DefaultSpotCookie = UnityEditor.AssetDatabase.LoadAssetAtPath<Texture>(path);
            }
        }
        #endif

        EnviroPostProcessing = PlayerCamera.gameObject.GetComponent<EnviroPostProcessing>();

        if (EnviroPostProcessing == null)
            EnviroPostProcessing = PlayerCamera.gameObject.AddComponent<EnviroPostProcessing>();

    }	
	/// <summary>
	/// Re-create the camera and render texture for satellite rendering
	/// </summary>
	public void CreateSatCamera ()
	{
		Camera[] cams = GameObject.FindObjectsOfType<Camera> ();
		for (int i = 0; i < cams.Length; i++) 
		{
			cams[i].cullingMask &= ~(1 << satelliteRenderingLayer);
		}

		DestroyImmediate(GameObject.Find ("Enviro Sat Camera"));

		GameObject camObj = new GameObject ();	

		camObj.name = "Enviro Sat Camera";
		camObj.transform.position = PlayerCamera.transform.position;
		camObj.transform.rotation = PlayerCamera.transform.rotation;
        camObj.hideFlags = HideFlags.DontSave;
        satCamera = camObj.AddComponent<Camera> ();
        satCamera.farClipPlane = PlayerCamera.farClipPlane;
        satCamera.nearClipPlane = PlayerCamera.nearClipPlane;
        satCamera.aspect = PlayerCamera.aspect;
		SetCameraHDR (satCamera, HDR);
        satCamera.useOcclusionCulling = false;
        satCamera.renderingPath = RenderingPath.Forward;
        satCamera.fieldOfView = PlayerCamera.fieldOfView;
        satCamera.clearFlags = CameraClearFlags.SolidColor;
        satCamera.backgroundColor = new Color(0f,0f,0f,0f);
        satCamera.cullingMask = (1 << satelliteRenderingLayer);
        satCamera.depth = PlayerCamera.depth + 1;
        satCamera.enabled = true;
		PlayerCamera.cullingMask &= ~(1 << satelliteRenderingLayer);

		var format = GetCameraHDR(satCamera) ? RenderTextureFormat.DefaultHDR: RenderTextureFormat.Default;

		satRenderTarget = new RenderTexture (Screen.currentResolution.width, Screen.currentResolution.height,16,format);
        satCamera.targetTexture = satRenderTarget;
        satCamera.enabled = false;
	}

    #endregion
    ////////////////////////////////
    #region Runtime Update
    private void UpdateCameraComponents()
	{
		//Update Fog
		if (EnviroSkyRender != null) 
		{

#if UNITY_2017_3_OR_NEWER
            EnviroSkyRender.volumeLighting = useVolumeLighting;
#else

            if (SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Direct3D9)
                EnviroSkyRender.volumeLighting = false;
            else
                EnviroSkyRender.volumeLighting = useVolumeLighting;
        #endif

            EnviroSkyRender.dirVolumeLighting = volumeLightSettings.dirVolumeLighting;
            EnviroSkyRender.simpleFog = fogSettings.useSimpleFog;
            EnviroSkyRender.distanceFog = fogSettings.distanceFog;
            EnviroSkyRender.heightFog = fogSettings.heightFog;
            EnviroSkyRender.height = fogSettings.height;
            EnviroSkyRender.heightDensity = fogSettings.heightDensity;
            EnviroSkyRender.useRadialDistance = fogSettings.useRadialDistance;
            EnviroSkyRender.startDistance = fogSettings.startDistance;

            if (directVolumeLight != null && !directVolumeLight.isActiveAndEnabled && volumeLightSettings.dirVolumeLighting)
                directVolumeLight.enabled = true;
		}
	}

    private void RenderFlatCloudsMap()
    {
        if (flatCloudsMat == null)
        flatCloudsMat = new Material(Shader.Find("Enviro/FlatCloudMap"));

        flatCloudsRenderTarget = RenderTexture.GetTemporary(512 * ((int)cloudsSettings.flatCloudsResolution + 1), 512 * ((int)cloudsSettings.flatCloudsResolution + 1), 0, RenderTextureFormat.DefaultHDR);
        flatCloudsRenderTarget.wrapMode = TextureWrapMode.Repeat;
        flatCloudsMat.SetVector("_CloudAnimation", cloudAnimNonScaled);
        flatCloudsMat.SetTexture("_NoiseTex", cloudsSettings.flatCloudsNoiseTexture);
        flatCloudsMat.SetFloat("_CloudScale", cloudsSettings.flatCloudsScale);
        flatCloudsMat.SetFloat("_Coverage", cloudsConfig.flatCoverage);
        flatCloudsMat.SetInt("noiseOctaves", cloudsSettings.flatCloudsNoiseOctaves);
        flatCloudsMat.SetFloat("_Softness", cloudsConfig.flatSoftness);
        flatCloudsMat.SetFloat("_Brightness", cloudsConfig.flatBrightness);
        flatCloudsMat.SetFloat("_MorphingSpeed", cloudsSettings.flatCloudsMorphingSpeed);
        Graphics.Blit(null, flatCloudsRenderTarget, flatCloudsMat);
        RenderTexture.ReleaseTemporary(flatCloudsRenderTarget);
    }

    private void RenderWeatherMap()
    {
        if (cloudsSettings.customWeatherMap == null)
        {
            weatherMapMat.SetVector("_WindDir", cloudAnimNonScaled);
            weatherMapMat.SetFloat("_AnimSpeedScale", cloudsSettings.weatherAnimSpeedScale);
            weatherMapMat.SetInt("_Tiling", cloudsSettings.weatherMapTiling);
            weatherMapMat.SetVector("_Location", cloudsSettings.locationOffset);
            double cov = EnviroSky.instance.cloudsConfig.coverage * cloudsSettings.globalCloudCoverage;
            weatherMapMat.SetFloat("_Coverage", (float)System.Math.Round(cov, 4));
            Graphics.Blit(null, weatherMap, weatherMapMat);
        }
    }

    private void RenderCloudMaps ()
    {
        if (Application.isPlaying)
        {
            if (useVolumeClouds)
                RenderWeatherMap();
            if (useFlatClouds)
                RenderFlatCloudsMap();
        }
        else
        {
            if (useVolumeClouds && showVolumeCloudsInEditor)
                RenderWeatherMap();
            if (useFlatClouds && showFlatCloudsInEditor)
                RenderFlatCloudsMap();
        }
    }

    void Update()
	{
		if (profile == null) {
			Debug.Log ("No profile applied! Please create and assign a profile.");
			return;
		}

        if (!started && !serverMode) 
		{
            UpdateTime(GameTime.DaysInYear);
            UpdateSunAndMoonPosition();
            UpdateSceneView();
            CalculateDirectLight();
            UpdateAmbientLight();
            UpdateReflections();
  
            if (AssignInRuntime && PlayerTag != "" && CameraTag != "" && Application.isPlaying) {
               
                // Search for Player by tag
                GameObject plr = GameObject.FindGameObjectWithTag(PlayerTag);
                if (plr != null)
                    Player = plr;
               
                // Search for camera by tag
                for(int i = 0; i < Camera.allCameras.Length; i++)
                {
                    if (Camera.allCameras[i].tag == CameraTag)
                        PlayerCamera = Camera.allCameras[i];
                }

                if (Player != null && PlayerCamera != null) {
					Init ();
					started = true;
				}
				else  {started = false; return;}
			} else {started = false; return;}
		}

		UpdateTime (GameTime.DaysInYear);
		ValidateParameters();

		if (!serverMode) {

            //Check if cloudmode changed
            if (useFlatClouds != flatCloudsSkybox)
                SetupSkybox();

            UpdateSceneView();

            if (!Application.isPlaying && Weather.startWeatherPreset != null)
            {
                UpdateClouds(Weather.startWeatherPreset, false);
                UpdateFog(Weather.startWeatherPreset, false);
                UpdatePostProcessing(Weather.startWeatherPreset, false);
                UpdateWeatherVariables(Weather.startWeatherPreset);
            }

            RenderCloudMaps();
            UpdateCameraComponents ();
			UpdateAmbientLight ();
			UpdateReflections ();
			UpdateWeather ();
            if(Weather.currentActiveWeatherPreset != null && Weather.currentActiveWeatherPreset.cloudsConfig.particleCloudsOverwrite)
                UpdateParticleClouds(true);
            else
                UpdateParticleClouds(useParticleClouds);
            UpdateCloudShadows();
            UpdateSkyRenderingComponent ();
            UpdateSunAndMoonPosition();
            CalculateDirectLight();
            SetMaterialsVariables();
            CalculateSatPositions(LST);

			if (!isNight && GameTime.solarTime < GameTime.dayNightSwitch) {
				isNight = true;
				if (Audio.AudioSourceAmbient != null)
					TryPlayAmbientSFX ();
                EnviroSkyMgr.instance.NotifyIsNight ();
			} else if (isNight && GameTime.solarTime >= GameTime.dayNightSwitch) {
				isNight = false;
				if (Audio.AudioSourceAmbient != null)
					TryPlayAmbientSFX ();
                EnviroSkyMgr.instance.NotifyIsDay ();
			}

            //Change Clouds Quality Settings
            if (lastCloudsQuality != cloudsSettings.cloudsQuality && useVolumeClouds)
                ChangeCloudsQuality(cloudsSettings.cloudsQuality);
        } 
		else 
		{
            UpdateWeather();

            if (!isNight && GameTime.solarTime < GameTime.dayNightSwitch)
            {
                isNight = true;
                EnviroSkyMgr.instance.NotifyIsNight();
            }
            else if (isNight && GameTime.solarTime >= GameTime.dayNightSwitch)
            {
                isNight = false;
                EnviroSkyMgr.instance.NotifyIsDay();
            }
		}
	}

	void LateUpdate()
	{
		if (!serverMode && PlayerCamera != null && Player != null) {
			transform.position = Player.transform.position;
			transform.localScale = new Vector3 (PlayerCamera.farClipPlane, PlayerCamera.farClipPlane, PlayerCamera.farClipPlane);

            if (EffectsHolder != null)
                EffectsHolder.transform.position = Player.transform.position;
        }
	}

    private void UpdateCloudShadows ()
    {
        if (cloudsSettings.shadowIntensity == 0 || !useVolumeClouds)
        {
            if (MainLight.cookie != null)
                MainLight.cookie = null;
        }
        else if (cloudsSettings.shadowIntensity > 0)
        {
            cloudShadowMap.DiscardContents(true,true);

            cloudShadowMat.SetFloat("_shadowIntensity", cloudsSettings.shadowIntensity);

            if (useVolumeClouds)
            {
                cloudShadowMat.SetTexture("_MainTex", weatherMap);
                Graphics.Blit(weatherMap, cloudShadowMap, cloudShadowMat);
            }

            if (Application.isPlaying)
                MainLight.cookie = cloudShadowMap;
            else
                MainLight.cookie = null;

            MainLight.cookieSize = cloudsSettings.shadowCookieSize;
        }
    }
	
	private void SetMaterialsVariables()
	{

        if (skyMat != null)
        {
            if (skySettings.skyboxMode == EnviroSkySettings.SkyboxModi.Simple)
            {
                //Simple
                skyMat.SetColor("_SkyColor", skySettings.simpleSkyColor.Evaluate(GameTime.solarTime));
                skyMat.SetColor("_HorizonColor", skySettings.simpleHorizonColor.Evaluate(GameTime.solarTime));
                skyMat.SetColor("_SunColor", skySettings.simpleSunColor.Evaluate(GameTime.solarTime));
                skyMat.SetFloat("_SunDiskSizeSimple", skySettings.simpleSunDiskSize.Evaluate(GameTime.solarTime));
            }
            else
            {
                skyMat.SetVector("_SunDir", -Components.Sun.transform.forward);
                skyMat.SetVector("_MoonDir", Components.Moon.transform.forward);
                skyMat.SetColor("_MoonColor", skySettings.moonColor);

                skyMat.SetColor("_scatteringColor", skySettings.scatteringColor.Evaluate(GameTime.solarTime));
                skyMat.SetColor("_sunDiskColor", skySettings.sunDiskColor.Evaluate(GameTime.solarTime));
                skyMat.SetColor("_weatherSkyMod", Color.Lerp(currentWeatherSkyMod, interiorZoneSettings.currentInteriorSkyboxMod, interiorZoneSettings.currentInteriorSkyboxMod.a));
                skyMat.SetColor("_weatherFogMod", Color.Lerp(currentWeatherFogMod, interiorZoneSettings.currentInteriorFogColorMod, interiorZoneSettings.currentInteriorFogColorMod.a));
                skyMat.SetVector("_Bm", BetaMie(skySettings.turbidity, skySettings.waveLength) * (skySettings.mie * Fog.scatteringStrenght));
                skyMat.SetVector("_Br", BetaRay(skySettings.waveLength) * skySettings.rayleigh);
                skyMat.SetVector("_mieG", GetMieG(skySettings.g));
                skyMat.SetFloat("_SunIntensity", skySettings.sunIntensity);
                skyMat.SetFloat("_SunDiskSize", skySettings.sunDiskScale);
                skyMat.SetFloat("_SunDiskIntensity", skySettings.sunDiskIntensity);
                skyMat.SetFloat("_SunDiskSize", skySettings.sunDiskScale);
                skyMat.SetFloat("_Exposure", skySettings.skyExposure);
                skyMat.SetFloat("_SkyLuminance", skySettings.skyLuminence.Evaluate(GameTime.solarTime));
                skyMat.SetFloat("_scatteringPower", skySettings.scatteringCurve.Evaluate(GameTime.solarTime));
                skyMat.SetFloat("_SkyColorPower", skySettings.skyColorPower.Evaluate(GameTime.solarTime));
                skyMat.SetFloat("_StarsIntensity", skySettings.starsIntensity.Evaluate(GameTime.solarTime));
                skyMat.SetFloat("_GalaxyIntensity", skySettings.galaxyIntensity.Evaluate(GameTime.solarTime));
      
                if (skySettings.dithering)
                    skyMat.SetInt("_UseDithering", 1);
                else
                    skyMat.SetInt("_UseDithering", 0);

                if (skySettings.moonPhaseMode == EnviroSkySettings.MoonPhases.Realistic)
                {
                    Vector3 targetDirMoon = Components.Moon.transform.position - transform.position;
                    Vector3 targetDirSun = Components.Sun.transform.position - transform.position;
                    float angle = Vector3.Angle(targetDirMoon, targetDirSun);
                    customMoonPhase = Remap(angle, 180f, 0f, 0f, 2f);
                }


                skyMat.SetColor("_moonGlowColor", skySettings.moonGlowColor);

                skyMat.SetVector("_moonParams", new Vector4(skySettings.moonSize, skySettings.glowSize, skySettings.moonGlow.Evaluate(GameTime.solarTime), customMoonPhase));

                //Moon
                if (skySettings.renderMoon)
                {
                    skyMat.SetTexture("_MoonTex", skySettings.moonTexture);
                    skyMat.SetTexture("_GlowTex", skySettings.glowTexture);
                }
                else
                {
                    skyMat.SetTexture("_MoonTex", null);
                    skyMat.SetTexture("_GlowTex", null);
                }

                if (skySettings.blackGroundMode)
                    skyMat.SetInt("_blackGround", 1);
                else
                    skyMat.SetInt("_blackGround", 0);

                float hdr = HDR ? 1f : 0f;
                skyMat.SetFloat("_hdr", hdr);

                if (skySettings.starsTwinklingRate > 0.0f)
                {
                	starsTwinklingRot += skySettings.starsTwinklingRate * Time.deltaTime;
                	Quaternion rot = Quaternion.Euler (starsTwinklingRot, starsTwinklingRot, starsTwinklingRot);
                	Matrix4x4 NoiseRot = Matrix4x4.TRS (Vector3.zero, rot, new Vector3 (1, 1, 1));
                    skyMat.SetMatrix ("_StarsTwinklingMatrix", NoiseRot);
                }

            }

            //Clouds
            skyMat.SetVector("_CloudAnimation", cloudAnim);

            //cirrus
            if (cloudsSettings.cirrusCloudsTexture != null)
                skyMat.SetTexture("_CloudMap", cloudsSettings.cirrusCloudsTexture);

            skyMat.SetColor("_CloudColor", cloudsSettings.cirrusCloudsColor.Evaluate(GameTime.solarTime));
            skyMat.SetFloat("_CloudAltitude", cloudsSettings.cirrusCloudsAltitude);
            skyMat.SetFloat("_CloudAlpha", cloudsConfig.cirrusAlpha);
            skyMat.SetFloat("_CloudCoverage", cloudsConfig.cirrusCoverage);
            skyMat.SetFloat("_CloudColorPower", cloudsConfig.cirrusColorPow);

            //flat procedural
            if (flatCloudsRenderTarget != null)
            {
                skyMat.SetTexture("_Cloud1Map", flatCloudsRenderTarget);
                skyMat.SetColor("_Cloud1Color", cloudsSettings.flatCloudsColor.Evaluate(GameTime.solarTime));
                skyMat.SetFloat("_Cloud1Altitude", cloudsSettings.flatCloudsAltitude);
                skyMat.SetFloat("_Cloud1Alpha", cloudsConfig.flatAlpha);
                skyMat.SetFloat("_Cloud1ColorPower", cloudsConfig.flatColorPow);
            }
        }

        Shader.SetGlobalColor("_EnviroLighting", lightSettings.LightColor.Evaluate(GameTime.solarTime));
        Shader.SetGlobalVector("_SunDirection", -Components.Sun.transform.forward);

        Shader.SetGlobalVector("_SunPosition", Components.Sun.transform.localPosition + (-Components.Sun.transform.forward * 10000f));
        Shader.SetGlobalVector("_MoonPosition", Components.Moon.transform.localPosition);

        Shader.SetGlobalVector ("_SunDir", -Components.Sun.transform.forward);
		Shader.SetGlobalVector ("_MoonDir", -Components.Moon.transform.forward);
		Shader.SetGlobalColor("_scatteringColor", skySettings.scatteringColor.Evaluate(GameTime.solarTime));
		Shader.SetGlobalColor("_sunDiskColor", skySettings.sunDiskColor.Evaluate(GameTime.solarTime));
		Shader.SetGlobalColor("_weatherSkyMod", Color.Lerp(currentWeatherSkyMod, interiorZoneSettings.currentInteriorSkyboxMod, interiorZoneSettings.currentInteriorSkyboxMod.a));
        Shader.SetGlobalColor("_weatherFogMod", Color.Lerp(currentWeatherFogMod, interiorZoneSettings.currentInteriorFogColorMod, interiorZoneSettings.currentInteriorFogColorMod.a));

		Shader.SetGlobalFloat ("_gameTime", Mathf.Clamp(1f-GameTime.solarTime,0.5f,1f));
		Shader.SetGlobalFloat ("_SkyFogHeight", Fog.skyFogHeight);
		Shader.SetGlobalFloat ("_scatteringStrenght", Fog.scatteringStrenght);
		Shader.SetGlobalFloat ("_skyFogIntensity", fogSettings.skyFogIntensity);
		Shader.SetGlobalFloat ("_SunBlocking", Fog.sunBlocking);

		Shader.SetGlobalVector ("_EnviroParams", new Vector4(Mathf.Clamp(1f-GameTime.solarTime,0.5f,1f),fogSettings.distanceFog ? 1f:0f,fogSettings.heightFog ? 1f:0f,HDR ? 1f:0f));

		Shader.SetGlobalVector ("_Bm", BetaMie (skySettings.turbidity, skySettings.waveLength) * (skySettings.mie * (Fog.scatteringStrenght * GameTime.solarTime)));
		Shader.SetGlobalVector ("_BmScene", BetaMie (skySettings.turbidity,skySettings.waveLength) * (fogSettings.mie * (Fog.scatteringStrenght * GameTime.solarTime)));
		Shader.SetGlobalVector ("_Br", BetaRay(skySettings.waveLength) * skySettings.rayleigh);
		Shader.SetGlobalVector ("_mieG", GetMieG (skySettings.g));
		Shader.SetGlobalVector ("_mieGScene", GetMieGScene (skySettings.g));
		Shader.SetGlobalFloat ("_SunIntensity",  skySettings.sunIntensity);

		Shader.SetGlobalFloat ("_SunDiskSize",  skySettings.sunDiskScale);
		Shader.SetGlobalFloat ("_SunDiskIntensity",  skySettings.sunDiskIntensity);
		Shader.SetGlobalFloat ("_SunDiskSize", skySettings.sunDiskScale);

		Shader.SetGlobalFloat ("_Exposure", skySettings.skyExposure);
		Shader.SetGlobalFloat ("_SkyLuminance", skySettings.skyLuminence.Evaluate(GameTime.solarTime));
		Shader.SetGlobalFloat ("_scatteringPower", skySettings.scatteringCurve.Evaluate(GameTime.solarTime));
		Shader.SetGlobalFloat ("_SkyColorPower", skySettings.skyColorPower.Evaluate(GameTime.solarTime));

		Shader.SetGlobalFloat ("_heightFogIntensity", fogSettings.heightFogIntensity);
		Shader.SetGlobalFloat ("_distanceFogIntensity", fogSettings.distanceFogIntensity);

        if (Application.isPlaying || showFogInEditor)
            Shader.SetGlobalFloat("_maximumFogDensity", 1 - fogSettings.maximumFogDensity);
        else if (!showFogInEditor)
        {
            Shader.SetGlobalFloat("_maximumFogDensity", 1f);
        }
        Shader.SetGlobalFloat ("_lightning", thunder);

        if (fogSettings.useSimpleFog)
            Shader.EnableKeyword("ENVIRO_SIMPLE_FOG");
        else
            Shader.DisableKeyword("ENVIRO_SIMPLE_FOG");

        

		if (Weather.currentActiveWeatherPreset != null)
			windStrenght = Weather.currentActiveWeatherPreset.WindStrenght;

		if (cloudsSettings.useWindZoneDirection) {
			cloudsSettings.cloudsWindDirectionX = -Components.windZone.transform.forward.x;
			cloudsSettings.cloudsWindDirectionY = -Components.windZone.transform.forward.z;
		}

		cloudAnim += new Vector2(((cloudsSettings.cloudsTimeScale * (windStrenght * cloudsSettings.cloudsWindDirectionX)) * cloudsSettings.cloudsWindStrengthModificator) * Time.deltaTime,((cloudsSettings.cloudsTimeScale * (windStrenght * cloudsSettings.cloudsWindDirectionY)) * cloudsSettings.cloudsWindStrengthModificator) * Time.deltaTime);
        cloudAnimNonScaled += new Vector2(((cloudsSettings.cloudsTimeScale * (windStrenght * cloudsSettings.cloudsWindDirectionX)) * cloudsSettings.cloudsWindStrengthModificator) * Time.deltaTime * 0.1f, ((cloudsSettings.cloudsTimeScale * (windStrenght * cloudsSettings.cloudsWindDirectionY)) * cloudsSettings.cloudsWindStrengthModificator) * Time.deltaTime * 0.1f);

        if (cloudAnim.x > 1f)
			cloudAnim.x = -1f;
		else if (cloudAnim.x < -1f)
			cloudAnim.x = 1f;

		if (cloudAnim.y > 1f)
			cloudAnim.y = -1f;
		else if (cloudAnim.y < -1f)
			cloudAnim.y = 1f;
	}

    private void UpdateSkyRenderingComponent ()
	{
		if (EnviroSkyRender == null)
			return;

#if ENVIRO_HD
        EnviroSkyRender.Resolution = volumeLightSettings.Resolution;

        if (EnviroSkyRender.fogMat != null) {
            EnviroSkyRender.fogMat.SetTexture ("_Clouds", cloudsRenderTarget);
			float hdr = HDR ? 1f : 0f;
            EnviroSkyRender.fogMat.SetFloat ("_hdr", hdr);
		}
#endif
	}

	// Make the parameters stay in reasonable range
	private void ValidateParameters()
	{
		// Keep GameTime Parameters right!
		internalHour = Mathf.Repeat(internalHour, 24f);
		GameTime.Longitude = Mathf.Clamp(GameTime.Longitude, -180, 180);
		GameTime.Latitude = Mathf.Clamp(GameTime.Latitude, -90, 90);
#if UNITY_EDITOR
		if (GameTime.DayLengthInMinutes <= 0f || GameTime.NightLengthInMinutes<= 0f)
		{
		if (GameTime.DayLengthInMinutes < 0f)
		GameTime.DayLengthInMinutes = 0f;

		if (GameTime.NightLengthInMinutes < 0f)
		GameTime.NightLengthInMinutes = 0f;
		internalHour = 12f;
		customMoonPhase = 0f;
		}

		if(GameTime.Days < 0)
		GameTime.Days = 0;

		if(GameTime.Years < 0)
		GameTime.Years = 0;

		// Moon
		//customMoonPhase = Mathf.Clamp(customMoonPhase, -2f, 2f);
#endif
	}

	private void UpdateClouds (EnviroWeatherPreset i, bool withTransition)
	{
		if (i == null)
			return;

		float speed = 500f * Time.deltaTime;

		if (withTransition)
			speed = weatherSettings.cloudTransitionSpeed * Time.deltaTime;

        cloudsConfig.cirrusAlpha = Mathf.Lerp(cloudsConfig.cirrusAlpha, i.cloudsConfig.cirrusAlpha, speed);
        cloudsConfig.cirrusCoverage = Mathf.Lerp(cloudsConfig.cirrusCoverage, i.cloudsConfig.cirrusCoverage, speed);
        cloudsConfig.cirrusColorPow = Mathf.Lerp(cloudsConfig.cirrusColorPow, i.cloudsConfig.cirrusColorPow, speed);
        //Needed for FV 3 Integration

        cloudsConfig.coverage = Mathf.Lerp(cloudsConfig.coverage, i.cloudsConfig.coverage, speed);
        cloudsConfig.ambientTopColorBrightness = Mathf.Lerp(cloudsConfig.ambientTopColorBrightness, i.cloudsConfig.ambientTopColorBrightness, speed);

        if (useVolumeClouds)
        {        
            cloudsConfig.ambientbottomColorBrightness = Mathf.Lerp(cloudsConfig.ambientbottomColorBrightness, i.cloudsConfig.ambientbottomColorBrightness, speed);            
            cloudsConfig.coverageHeight = Mathf.Lerp(cloudsConfig.coverageHeight, i.cloudsConfig.coverageHeight, speed);
            cloudsConfig.raymarchingScale = Mathf.Lerp(cloudsConfig.raymarchingScale, i.cloudsConfig.raymarchingScale, speed);
            cloudsConfig.skyBlending = Mathf.Lerp(cloudsConfig.skyBlending, i.cloudsConfig.skyBlending, speed);

            cloudsConfig.density = Mathf.Lerp(cloudsConfig.density, i.cloudsConfig.density, speed);
            cloudsConfig.alphaCoef = Mathf.Lerp(cloudsConfig.alphaCoef, i.cloudsConfig.alphaCoef, speed);
            cloudsConfig.scatteringCoef = Mathf.Lerp(cloudsConfig.scatteringCoef, i.cloudsConfig.scatteringCoef, speed);
            cloudsConfig.cloudType = Mathf.Lerp(cloudsConfig.cloudType, i.cloudsConfig.cloudType, speed);
        }

        if (useFlatClouds)
        {
            cloudsConfig.flatAlpha = Mathf.Lerp(cloudsConfig.flatAlpha, i.cloudsConfig.flatAlpha, speed);
            cloudsConfig.flatCoverage = Mathf.Lerp(cloudsConfig.flatCoverage, i.cloudsConfig.flatCoverage, speed);
            cloudsConfig.flatColorPow = Mathf.Lerp(cloudsConfig.flatColorPow, i.cloudsConfig.flatColorPow, speed);

            cloudsConfig.flatSoftness = Mathf.Lerp(cloudsConfig.flatSoftness, i.cloudsConfig.flatSoftness, speed);
            cloudsConfig.flatBrightness = Mathf.Lerp(cloudsConfig.flatBrightness, i.cloudsConfig.flatBrightness, speed);
        }
      

        cloudsConfig.particleLayer1Alpha = Mathf.Lerp(cloudsConfig.particleLayer1Alpha, i.cloudsConfig.particleLayer1Alpha, speed * 0.5f);
        cloudsConfig.particleLayer1Brightness = Mathf.Lerp(cloudsConfig.particleLayer1Brightness, i.cloudsConfig.particleLayer1Brightness, speed * 0.5f);
        //cloudsConfig.particleLayer1ColorPow = Mathf.Lerp(cloudsConfig.particleLayer1ColorPow, i.cloudsConfig.particleLayer1ColorPow, speed);

        cloudsConfig.particleLayer2Alpha = Mathf.Lerp(cloudsConfig.particleLayer2Alpha, i.cloudsConfig.particleLayer2Alpha, speed * 0.5f);
        cloudsConfig.particleLayer2Brightness = Mathf.Lerp(cloudsConfig.particleLayer2Brightness, i.cloudsConfig.particleLayer2Brightness, speed * 0.5f);
        //cloudsConfig.particleLayer2ColorPow = Mathf.Lerp(cloudsConfig.particleLayer2ColorPow, i.cloudsConfig.particleLayer2ColorPow, speed);


        globalVolumeLightIntensity = Mathf.Lerp(globalVolumeLightIntensity, i.volumeLightIntensity, speed);
        shadowIntensityMod = Mathf.Lerp(shadowIntensityMod, i.shadowIntensityMod, speed);
        currentWeatherSkyMod = Color.Lerp (currentWeatherSkyMod, i.weatherSkyMod.Evaluate(GameTime.solarTime), speed);
		currentWeatherFogMod = Color.Lerp (currentWeatherFogMod, i.weatherFogMod.Evaluate(GameTime.solarTime), speed * 10);
		currentWeatherLightMod = Color.Lerp (currentWeatherLightMod, i.weatherLightMod.Evaluate(GameTime.solarTime), speed);
	}

    private void UpdateFog (EnviroWeatherPreset i, bool withTransition)
	{
        // Set the Fog color to light color to match Day-Night cycle and weather
        Color fogClr = Color.Lerp(fogSettings.simpleFogColor.Evaluate(GameTime.solarTime), customFogColor, customFogIntensity);
        RenderSettings.fogColor = Color.Lerp(fogClr, currentWeatherFogMod, currentWeatherFogMod.a);

        if (i != null) {

			float speed = 500f * Time.deltaTime;

			if (withTransition)
				speed = weatherSettings.fogTransitionSpeed * Time.deltaTime;

			if (fogSettings.Fogmode == FogMode.Linear) {
				RenderSettings.fogEndDistance = Mathf.Lerp (RenderSettings.fogEndDistance, i.fogDistance, speed);
				RenderSettings.fogStartDistance = Mathf.Lerp (RenderSettings.fogStartDistance, i.fogStartDistance, speed);
			} else {
				if(updateFogDensity)
					RenderSettings.fogDensity = Mathf.Lerp (RenderSettings.fogDensity, i.fogDensity, speed) * interiorZoneSettings.currentInteriorFogMod;
			}

			fogSettings.heightDensity = Mathf.Lerp (fogSettings.heightDensity, i.heightFogDensity, speed);
			Fog.skyFogHeight = Mathf.Lerp (Fog.skyFogHeight, i.SkyFogHeight, speed);
			Fog.skyFogStrength = Mathf.Lerp (Fog.skyFogStrength, i.SkyFogIntensity, speed);
			fogSettings.skyFogIntensity = Mathf.Lerp (fogSettings.skyFogIntensity, i.SkyFogIntensity, speed);
			Fog.scatteringStrenght = Mathf.Lerp (Fog.scatteringStrenght, i.FogScatteringIntensity, speed);
			Fog.sunBlocking = Mathf.Lerp (Fog.sunBlocking, i.fogSunBlocking, speed);
		}
	}

    private void UpdatePostProcessing(EnviroWeatherPreset i, bool withTransition)
    {

        if (i != null)
        {

            float speed = 500f * Time.deltaTime;

            if (withTransition)
                speed = 10 * Time.deltaTime;

               blurDistance = Mathf.Lerp(blurDistance, i.blurDistance, speed);
               blurIntensity = Mathf.Lerp(blurIntensity, i.blurIntensity, speed);
               blurSkyIntensity = Mathf.Lerp(blurSkyIntensity, i.blurSkyIntensity, speed);
        }
    }

    private void UpdateEffectSystems (EnviroWeatherPrefab id, bool withTransition)
	{
		if (id != null) {

			float speed = 500f * Time.deltaTime;

			if (withTransition)
				speed = weatherSettings.effectTransitionSpeed * Time.deltaTime;

			for (int i = 0; i < id.effectSystems.Count; i++) {

                if (id.effectSystems[i].isStopped)
                    id.effectSystems[i].Play();

                // Set EmissionRate
                float val = Mathf.Lerp (EnviroSkyMgr.instance.GetEmissionRate (id.effectSystems [i]), id.effectEmmisionRates [i] * qualitySettings.GlobalParticleEmissionRates, speed ) * interiorZoneSettings.currentInteriorWeatherEffectMod;
                EnviroSkyMgr.instance.SetEmissionRate (id.effectSystems [i], val);
			}

			for (int i = 0; i < Weather.WeatherPrefabs.Count; i++) {
				if (Weather.WeatherPrefabs [i].gameObject != id.gameObject) {
					for (int i2 = 0; i2 < Weather.WeatherPrefabs [i].effectSystems.Count; i2++) {
						float val2 = Mathf.Lerp (EnviroSkyMgr.instance.GetEmissionRate (Weather.WeatherPrefabs [i].effectSystems [i2]), 0f, speed);

						if (val2 < 1f)
							val2 = 0f;

                        EnviroSkyMgr.instance.SetEmissionRate (Weather.WeatherPrefabs [i].effectSystems [i2], val2);

                        if (val2 == 0f && !Weather.WeatherPrefabs[i].effectSystems[i2].isStopped)
                        {
                            Weather.WeatherPrefabs[i].effectSystems[i2].Stop();
                        }
                    }
				}
			}

            UpdateWeatherVariables(id.weatherPreset);
        }
	}

    private void UpdateWeather()
    {
        //Current active weather not matching current zones weather
        if (Weather.currentActiveWeatherPreset != Weather.currentActiveZone.currentActiveZoneWeatherPreset)
        {
            Weather.lastActiveWeatherPreset = Weather.currentActiveWeatherPreset;
            Weather.lastActiveWeatherPrefab = Weather.currentActiveWeatherPrefab;
            Weather.currentActiveWeatherPreset = Weather.currentActiveZone.currentActiveZoneWeatherPreset;
            Weather.currentActiveWeatherPrefab = Weather.currentActiveZone.currentActiveZoneWeatherPrefab;
            if (Weather.currentActiveWeatherPreset != null)
            {
                EnviroSkyMgr.instance.NotifyWeatherChanged(Weather.currentActiveWeatherPreset);
                Weather.weatherFullyChanged = false;
                if (!serverMode)
                {
                    TryPlayAmbientSFX();
                    UpdateAudioSource(Weather.currentActiveWeatherPreset);

                    if (Weather.currentActiveWeatherPreset.isLightningStorm)
                        StartCoroutine(PlayThunderRandom());
                    else
                    {
                        StopCoroutine(PlayThunderRandom());
                        Components.LightningGenerator.StopLightning();
                    }
                }
            }
        }

        if (Weather.currentActiveWeatherPrefab != null && !serverMode)
        {
            UpdateClouds(Weather.currentActiveWeatherPreset, true);
            UpdateFog(Weather.currentActiveWeatherPreset, true);
            UpdatePostProcessing(Weather.currentActiveWeatherPreset, true);
            UpdateEffectSystems(Weather.currentActiveWeatherPrefab, true);
            if (!Weather.weatherFullyChanged)
                CalcWeatherTransitionState();
        }
        else if (Weather.currentActiveWeatherPrefab != null)
        {
            UpdateWeatherVariables(Weather.currentActiveWeatherPrefab.weatherPreset);
        }
    }

    #endregion
    ////////////////////////////////
    #region API
    /// <summary>
    /// Changes clouds, fog and particle effects to current weather settings instantly.
    /// </summary>
    public void InstantWeatherChange (EnviroWeatherPreset preset, EnviroWeatherPrefab prefab)
    {
        UpdateClouds(preset, false);
        UpdateFog(preset, false);
        UpdatePostProcessing(preset, false);
        UpdateEffectSystems(prefab, false);
    }

    /// <summary>
    /// Change volume clouds quality mode and apply settings.
    /// </summary>
    public void ChangeCloudsQuality(EnviroCloudSettings.CloudQuality q)
    {
        if (q == EnviroCloudSettings.CloudQuality.Custom)
            return;

        switch (q)
        { 

         case EnviroCloudSettings.CloudQuality.Lowest:
            cloudsSettings.bottomCloudHeight = 3000f;
            cloudsSettings.topCloudHeight = 5000f;
            cloudsSettings.cloudsWorldScale = 20000f;
            cloudsSettings.raymarchSteps = 75;
            cloudsSettings.stepsInDepthModificator = 0.7f;
            cloudsSettings.cloudsRenderResolution = 2;
            cloudsSettings.reprojectionPixelSize = EnviroCloudSettings.ReprojectionPixelSize.Medium;
            cloudsSettings.baseNoiseUV = 26f;
            cloudsSettings.detailNoiseUV = 1f;
            cloudsSettings.detailQuality = EnviroCloudSettings.CloudDetailQuality.Low;
        break;

        case EnviroCloudSettings.CloudQuality.Low:
                cloudsSettings.bottomCloudHeight = 3000f;
                cloudsSettings.topCloudHeight = 5000f;
                cloudsSettings.cloudsWorldScale = 20000f;
                cloudsSettings.raymarchSteps = 90;
                cloudsSettings.stepsInDepthModificator = 0.7f;
                cloudsSettings.cloudsRenderResolution = 2;
                cloudsSettings.reprojectionPixelSize = EnviroCloudSettings.ReprojectionPixelSize.Low;
                cloudsSettings.baseNoiseUV = 30f;
                cloudsSettings.detailNoiseUV = 1f;
                cloudsSettings.detailQuality = EnviroCloudSettings.CloudDetailQuality.Low;
                break;

            case EnviroCloudSettings.CloudQuality.Medium:
                cloudsSettings.bottomCloudHeight = 3000f;
                cloudsSettings.topCloudHeight = 5500f;
                cloudsSettings.cloudsWorldScale = 20000f;
                cloudsSettings.raymarchSteps = 128;
                cloudsSettings.stepsInDepthModificator = 0.7f;
                cloudsSettings.cloudsRenderResolution = 1;
                cloudsSettings.reprojectionPixelSize = EnviroCloudSettings.ReprojectionPixelSize.Medium;
                cloudsSettings.baseNoiseUV = 30f;
                cloudsSettings.detailNoiseUV = 50f;
                cloudsSettings.detailQuality = EnviroCloudSettings.CloudDetailQuality.Low;
                break;

            case EnviroCloudSettings.CloudQuality.High:
                cloudsSettings.bottomCloudHeight = 3000f;
                cloudsSettings.topCloudHeight = 6000f;
                cloudsSettings.cloudsWorldScale = 20000f;
                cloudsSettings.raymarchSteps = 150;
                cloudsSettings.stepsInDepthModificator = 0.6f;
                cloudsSettings.cloudsRenderResolution = 1;
                cloudsSettings.reprojectionPixelSize = EnviroCloudSettings.ReprojectionPixelSize.Medium;
                cloudsSettings.baseNoiseUV = 30f;
                cloudsSettings.detailNoiseUV = 50f;
                cloudsSettings.detailQuality = EnviroCloudSettings.CloudDetailQuality.Low;
                break;

            case EnviroCloudSettings.CloudQuality.Ultra:
                cloudsSettings.bottomCloudHeight = 3000f;
                cloudsSettings.topCloudHeight = 6000f;
                cloudsSettings.cloudsWorldScale = 20000f;
                cloudsSettings.raymarchSteps = 200;
                cloudsSettings.stepsInDepthModificator = 0.5f;
                cloudsSettings.cloudsRenderResolution = 1;
                cloudsSettings.reprojectionPixelSize = EnviroCloudSettings.ReprojectionPixelSize.Low;
                cloudsSettings.baseNoiseUV = 30f;
                cloudsSettings.detailNoiseUV = 70f;
                cloudsSettings.detailQuality = EnviroCloudSettings.CloudDetailQuality.Low;
                break;

            case EnviroCloudSettings.CloudQuality.VR_Low:
                cloudsSettings.bottomCloudHeight = 3000f;
                cloudsSettings.topCloudHeight = 4200f;
                cloudsSettings.cloudsWorldScale = 20000f;
                cloudsSettings.raymarchSteps = 60;
                cloudsSettings.cloudsRenderResolution = 2;
                cloudsSettings.reprojectionPixelSize = EnviroCloudSettings.ReprojectionPixelSize.Low;
                cloudsSettings.baseNoiseUV = 20f;
                cloudsSettings.detailNoiseUV = 1f;
                cloudsSettings.detailQuality = EnviroCloudSettings.CloudDetailQuality.Low;
                break;

            case EnviroCloudSettings.CloudQuality.VR_Medium:
                cloudsSettings.bottomCloudHeight = 3000f;
                cloudsSettings.topCloudHeight = 4500f;
                cloudsSettings.cloudsWorldScale = 20000f;
                cloudsSettings.raymarchSteps = 75;
                cloudsSettings.cloudsRenderResolution = 1;
                cloudsSettings.reprojectionPixelSize = EnviroCloudSettings.ReprojectionPixelSize.Medium;
                cloudsSettings.baseNoiseUV = 22f;
                cloudsSettings.detailNoiseUV = 1f;
                cloudsSettings.detailQuality = EnviroCloudSettings.CloudDetailQuality.Low;
                break;

            case EnviroCloudSettings.CloudQuality.VR_High:
                cloudsSettings.bottomCloudHeight = 3000f;
                cloudsSettings.topCloudHeight = 4500f;
                cloudsSettings.cloudsWorldScale = 20000f;
                cloudsSettings.raymarchSteps = 80;
                cloudsSettings.cloudsRenderResolution = 1;
                cloudsSettings.reprojectionPixelSize = EnviroCloudSettings.ReprojectionPixelSize.Medium;
                cloudsSettings.baseNoiseUV = 23f;
                cloudsSettings.detailNoiseUV = 1f;
                cloudsSettings.detailQuality = EnviroCloudSettings.CloudDetailQuality.Low;
                break;
        }

        lastCloudsQuality = q;
        cloudsSettings.cloudsQuality = q;
    }

	/// <summary>
	/// Assign your Player and Camera and Initilize.////
	/// </summary>
	public void AssignAndStart (GameObject player, Camera Camera)
	{
		this.Player = player;
		PlayerCamera = Camera;
		Init ();
		started = true;
	}

	/// <summary>
	/// Assign your Player and Camera and Initilize.////
	/// </summary>
	public void StartAsServer ()
	{
		Player = gameObject;
		serverMode = true;
		Init ();
	}

	/// <summary>
	/// Changes focus on other Player or Camera on runtime.////
	/// </summary>
	/// <param name="Player">Player.</param>
	/// <param name="Camera">Camera.</param>
	public void ChangeFocus (GameObject player, Camera Camera)
	{
		this.Player = player;
		RemoveEnviroCameraComponents (PlayerCamera);
		PlayerCamera = Camera;
        InitImageEffects();
	}
	/// <summary>
	/// Destroy all enviro related camera components on this camera.
	/// </summary> 
	private void RemoveEnviroCameraComponents (Camera cam)
	{

		EnviroSkyRendering renderComponent;
        EnviroPostProcessing postProcessingComponent;

        renderComponent = cam.GetComponent<EnviroSkyRendering> (); 
		if(renderComponent != null)
			Destroy (renderComponent);

        postProcessingComponent = cam.GetComponent<EnviroPostProcessing>();
        if (postProcessingComponent != null)
            Destroy(postProcessingComponent);
    }

    public void Play(EnviroTime.TimeProgressMode progressMode = EnviroTime.TimeProgressMode.Simulated)
    {
        SetupSkybox();

        if (!Components.DirectLight.gameObject.activeSelf)
            Components.DirectLight.gameObject.SetActive(true);

        GameTime.ProgressTime = progressMode;
        if (EffectsHolder != null)
            EffectsHolder.SetActive(true);
        if (EnviroSkyRender != null)
            EnviroSkyRender.enabled = true;
        if (EnviroPostProcessing != null)
            EnviroPostProcessing.enabled = true;
        started = true;
    }

    public void Stop(bool disableLight = false, bool stopTime = true)
    {
        if (disableLight)
            Components.DirectLight.gameObject.SetActive(false);
        if (stopTime)
            GameTime.ProgressTime = EnviroTime.TimeProgressMode.None;

        if (EffectsHolder != null)
            EffectsHolder.SetActive(false);

        if (EnviroSkyRender != null)
            EnviroSkyRender.enabled = false;
        if (EnviroPostProcessing != null)
            EnviroPostProcessing.enabled = false;
        started = false;
    }

    public void Deactivate(bool disableLight = false)
    {
        if (disableLight)
            Components.DirectLight.gameObject.SetActive(false);

        if (EffectsHolder != null)
            EffectsHolder.SetActive(false);

        if (EnviroSkyRender != null)
            EnviroSkyRender.enabled = false;
        if (EnviroPostProcessing != null)
            EnviroPostProcessing.enabled = false;

    }

    public void Activate()
    {
        Components.DirectLight.gameObject.SetActive(true);

        if (EffectsHolder != null)
            EffectsHolder.SetActive(true);

        if (EnviroSkyRender != null)
            EnviroSkyRender.enabled = true;

        if (EnviroPostProcessing != null)
            EnviroPostProcessing.enabled = true;

        TryPlayAmbientSFX();
        //Audio.currentAmbientSource.audiosrc.Play();
        if (Weather.currentAudioSource != null)
            Weather.currentAudioSource.audiosrc.Play();
    }
    #endregion
    ////////////////////////////////
}