using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class EnviroSkyMgr : MonoBehaviour {

    #region Enviro Manager
    private static EnviroSkyMgr _instance; // Creat a static instance for easy access!

    public static EnviroSkyMgr instance
    {
        get
        {
            //If _instance hasn't been set yet, we grab it from the scene!
            //This will only happen the first time this reference is used.
            if (_instance == null)
                _instance = GameObject.FindObjectOfType<EnviroSkyMgr>();
            return _instance;
        }
    }

     
    //Inspector
    [Header("General")]
    [Tooltip("Enable to make sure thast enviro objects don't get destroyed on scene load.")]
    public bool dontDestroy = false;
    public bool showSetup = true;
    public bool showInstances = true;
    public bool showThirdParty = true;
    public bool showThirdPartyShaders = true;
    public bool showThirdPartyMisc = true;
    public bool showThirdPartyNetwork = true;
    public bool showUtiliies = true;
    public enum EnviroSkyVersion
    {
        None,
#if ENVIRO_HD && ENVIRO_LW
        LW,
        HD
#elif ENVIRO_HD
        HD
#elif ENVIRO_LW
        LW
#endif
    }

    public enum EnviroRenderPipeline
    {
        Legacy,
#if ENVIRO_PRO
        HDRP,
        LWRP
#endif
    }

    public EnviroRenderPipeline currentRenderPipeline = EnviroRenderPipeline.Legacy;

#if ENVIRO_HD
    public EnviroSkyVersion currentEnviroSkyVersion = EnviroSkyVersion.HD;
#elif ENVIRO_LW
    public EnviroSkyVersion currentEnviroSkyVersion = EnviroSkyVersion.LW;
#else
    public EnviroSkyVersion currentEnviroSkyVersion = EnviroSkyVersion.None;
#endif


#if ENVIRO_HD
    public EnviroSky enviroHDInstance;
#endif

#if ENVIRO_LW
    public EnviroSkyLite enviroLWInstance;
#endif

#if UNITY_EDITOR
    [UnityEditor.MenuItem("Assets/Create/Enviro/Enviro Sky Manager")]
    public static void CreateManagerInstance()
    {
        if (EnviroSkyMgr.instance == null)
        {
            GameObject mgrObj = new GameObject();
            mgrObj.name = "Enviro Sky Manager";
            EnviroSkyMgr mgr =  mgrObj.AddComponent<EnviroSkyMgr>();
            mgrObj.AddComponent<EnviroEvents>();
            UnityEditor.EditorUtility.FocusProjectWindow();
            UnityEditor.Selection.activeObject = mgr;
        }
    }
#endif

    private void Start()
    {
        if (Application.isPlaying && dontDestroy)
            DontDestroyOnLoad(gameObject);
    }


#if ENVIRO_HD
    public void ActivateHDInstance()
    {
        if (enviroHDInstance != null)
        {

#if ENVIRO_LW
            if (enviroLWInstance != null)
            {
                enviroLWInstance.Deactivate();
                enviroLWInstance.gameObject.SetActive(false);
            }
#endif
            enviroHDInstance.gameObject.SetActive(true);
            enviroHDInstance.Activate();

            currentEnviroSkyVersion = EnviroSkyVersion.HD;
        }
    }
#endif

#if ENVIRO_HD
    public void DeactivateHDInstance()
    {
        if (enviroHDInstance != null)
        {
            enviroHDInstance.Deactivate();
            enviroHDInstance.gameObject.SetActive(false);

#if ENVIRO_LW
            if (enviroLWInstance != null && !enviroLWInstance.gameObject.activeSelf)
                currentEnviroSkyVersion = EnviroSkyVersion.None;
            else
                currentEnviroSkyVersion = EnviroSkyVersion.LW;
#else
                currentEnviroSkyVersion = EnviroSkyVersion.None;
#endif
        }
    }
#endif

#if ENVIRO_LW
    public void ActivateLWInstance()
    {
        if(enviroLWInstance != null)
        {
#if ENVIRO_HD
            if (enviroHDInstance != null)
            {
                enviroHDInstance.Deactivate();
                enviroHDInstance.gameObject.SetActive(false);
            }
#endif    
            enviroLWInstance.gameObject.SetActive(true);
            enviroLWInstance.Activate();

            currentEnviroSkyVersion = EnviroSkyVersion.LW;
        }
    }
#endif

#if ENVIRO_LW
    public void DeactivateLWInstance()
    {
        if (enviroLWInstance != null)
        {
            enviroLWInstance.Deactivate();
            enviroLWInstance.gameObject.SetActive(false);
#if ENVIRO_HD
            if(enviroHDInstance != null && !enviroHDInstance.gameObject.activeSelf)
               currentEnviroSkyVersion = EnviroSkyVersion.None;
            else
                currentEnviroSkyVersion = EnviroSkyVersion.HD;
#else
            currentEnviroSkyVersion = EnviroSkyVersion.None;
#endif

        }
    }
#endif


    public void DeleteHDInstance()
    {
#if ENVIRO_HD
        if (enviroHDInstance != null)
        {
            DestroyImmediate(enviroHDInstance.EffectsHolder);
            DestroyImmediate(enviroHDInstance.gameObject);
            if(enviroHDInstance.EnviroSkyRender != null)
            DestroyImmediate(enviroHDInstance.EnviroSkyRender);
            if (enviroHDInstance.EnviroPostProcessing != null)
                DestroyImmediate(enviroHDInstance.EnviroPostProcessing);
            currentEnviroSkyVersion = EnviroSkyVersion.None;
        }
#endif
    }

    public void DeleteLWInstance()
    {
#if ENVIRO_LW
        if (enviroLWInstance != null)
        {
            DestroyImmediate(enviroLWInstance.EffectsHolder);
            DestroyImmediate(enviroLWInstance.gameObject);
            if (enviroLWInstance.EnviroSkyRender != null)
                DestroyImmediate(enviroLWInstance.EnviroSkyRender);
            if (enviroLWInstance.EnviroPostProcessing != null)
                DestroyImmediate(enviroLWInstance.EnviroPostProcessing);
            currentEnviroSkyVersion = EnviroSkyVersion.None;
        }
#endif
    }


    public void SearchForEnviroInstances()
    {
#if ENVIRO_HD
        enviroHDInstance = GetComponentInChildren<EnviroSky>();
#endif
#if ENVIRO_LW
        enviroLWInstance = GetComponentInChildren<EnviroSkyLite>();
#endif
    }

#if ENVIRO_HD
    public void CreateEnviroHDInstance()
    {
        GameObject prefab = GetAssetPrefab("Internal_Enviro_HD");

        if (prefab != null && EnviroSky.instance == null)
        {
            DeactivateAllInstances();
            GameObject inst = (GameObject)Instantiate(prefab, Vector3.zero, Quaternion.identity);
            inst.name = "EnviroSky Standard";
            inst.transform.SetParent(transform);
            enviroHDInstance = inst.GetComponent<EnviroSky>();
            inst.SetActive(false);
            currentEnviroSkyVersion = EnviroSkyVersion.None;
        }
    }
#endif

#if ENVIRO_HD
    public void CreateEnviroHDVRInstance()
    {
        GameObject prefab = GetAssetPrefab("Internal_Enviro_HD_VR");

        if (prefab != null && EnviroSky.instance == null)
        {
            DeactivateAllInstances();
            GameObject inst = (GameObject)Instantiate(prefab, Vector3.zero, Quaternion.identity);
            inst.name = "EnviroSky Standard for VR";
            inst.transform.SetParent(transform);
            enviroHDInstance = inst.GetComponent<EnviroSky>();
            inst.SetActive(false);
            currentEnviroSkyVersion = EnviroSkyVersion.None;
        }
    }
#endif

#if ENVIRO_LW
    public void CreateEnviroLWInstance()
    {
        GameObject prefab = GetAssetPrefab("Internal_Enviro_LW");

        if (prefab != null && EnviroSkyLite.instance == null)
        {
            DeactivateAllInstances();
            GameObject inst = (GameObject)Instantiate(prefab, Vector3.zero, Quaternion.identity);
            inst.name = "EnviroSky Lite";
            inst.transform.SetParent(transform);
            enviroLWInstance = inst.GetComponent<EnviroSkyLite>();
            inst.SetActive(false);
            currentEnviroSkyVersion = EnviroSkyVersion.None;
        }
}
#endif

#if ENVIRO_LW
    public void CreateEnviroLWMobileInstance()
    {
        GameObject prefab = GetAssetPrefab("Internal_Enviro_LW_MOBILE");

        if (prefab != null && EnviroSkyLite.instance == null)
        {
            DeactivateAllInstances();
            GameObject inst = (GameObject)Instantiate(prefab, Vector3.zero, Quaternion.identity);
            inst.name = "EnviroSky Lite for Mobiles";
            inst.transform.SetParent(transform);
            enviroLWInstance = inst.GetComponent<EnviroSkyLite>();
            inst.SetActive(false);
            currentEnviroSkyVersion = EnviroSkyVersion.None;
        }
    }
#endif

    private void DeactivateAllInstances()
    {
#if ENVIRO_HD
        if(enviroHDInstance != null)
        DeactivateHDInstance();
#endif
#if ENVIRO_LW
        if (enviroLWInstance != null)
            DeactivateLWInstance();
#endif
    }

    public GameObject GetAssetPrefab(string name)
    {
#if UNITY_EDITOR
        string[] assets = UnityEditor.AssetDatabase.FindAssets(name, null);
        for (int idx = 0; idx < assets.Length; idx++)
        {
            string path = UnityEditor.AssetDatabase.GUIDToAssetPath(assets[idx]);
            if (path.Contains(".prefab"))
            {
                return UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(path);
            }
        }
#endif
        return null;
    }

#if ENVIRO_PRO
    public void ActivateHDRP()
    {
        currentRenderPipeline = EnviroRenderPipeline.HDRP;
    }
#endif

#if ENVIRO_PRO
    public void ActivateLWRP()
    {
        currentRenderPipeline = EnviroRenderPipeline.LWRP;
    }
#endif

    public void ActivateLegacyRP()
    {
        currentRenderPipeline = EnviroRenderPipeline.Legacy;
    }


    #endregion
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    #region General

    /// <summary>
    /// Enviro Components References
    /// </summary>
    public EnviroComponents Components
    {
        get
        {
#if ENVIRO_HD && ENVIRO_LW
            if (currentEnviroSkyVersion == EnviroSkyVersion.HD)
                return EnviroSky.instance.Components;
            else
                return EnviroSkyLite.instance.Components;
#elif ENVIRO_HD
        return EnviroSky.instance.Components;
#elif ENVIRO_LW
        return EnviroSkyLite.instance.Components;
#else
        return null;
#endif
        }
        set
        {
#if ENVIRO_HD && ENVIRO_LW
            if (currentEnviroSkyVersion == EnviroSkyVersion.HD)
                EnviroSky.instance.Components = value;
            else
                EnviroSkyLite.instance.Components = value;
#elif ENVIRO_HD
       EnviroSky.instance.Components = value;
#elif ENVIRO_LW
       EnviroSkyLite.instance.Components = value;
#endif
        }
    }

    /// <summary>
    /// Enviro Time Settings
    /// </summary>
    public EnviroTime Time
    {
        get
        {
#if ENVIRO_HD && ENVIRO_LW
            if (currentEnviroSkyVersion == EnviroSkyVersion.HD)
                return EnviroSky.instance.GameTime;
            else
                return EnviroSkyLite.instance.GameTime;
#elif ENVIRO_HD
        return EnviroSky.instance.GameTime;
#elif ENVIRO_LW
        return EnviroSkyLite.instance.GameTime;
#else
        return null;
#endif
        }
        set
        {
#if ENVIRO_HD && ENVIRO_LW
            if (currentEnviroSkyVersion == EnviroSkyVersion.HD)
                EnviroSky.instance.GameTime = value;
            else
                EnviroSkyLite.instance.GameTime = value;
#elif ENVIRO_HD
       EnviroSky.instance.GameTime = value;
#elif ENVIRO_LW
       EnviroSkyLite.instance.GameTime = value;
#endif
        }
    }

    /// <summary>
    /// Enviro Seasons Settings
    /// </summary>
    public EnviroSeasons Seasons
    {
        get
        {
#if ENVIRO_HD && ENVIRO_LW
            if (currentEnviroSkyVersion == EnviroSkyVersion.HD)
                return EnviroSky.instance.Seasons;
            else
                return EnviroSkyLite.instance.Seasons;
#elif ENVIRO_HD
        return EnviroSky.instance.Seasons;
#elif ENVIRO_LW
        return EnviroSkyLite.instance.Seasons;
#else
        return null;
#endif
        }
        set
        {
#if ENVIRO_HD && ENVIRO_LW
            if (currentEnviroSkyVersion == EnviroSkyVersion.HD)
                EnviroSky.instance.Seasons = value;
            else
                EnviroSkyLite.instance.Seasons = value;
#elif ENVIRO_HD
       EnviroSky.instance.Seasons = value;
#elif ENVIRO_LW
       EnviroSkyLite.instance.Seasons = value;
#endif
        }
    }

    /// <summary>
    /// Enviro Seasons Settings
    /// </summary>
    public EnviroSeasonSettings SeasonSettings
    {
        get
        {
#if ENVIRO_HD && ENVIRO_LW
            if (currentEnviroSkyVersion == EnviroSkyVersion.HD)
                return EnviroSky.instance.seasonsSettings;
            else
                return EnviroSkyLite.instance.seasonsSettings;
#elif ENVIRO_HD
        return EnviroSky.instance.seasonsSettings;
#elif ENVIRO_LW
        return EnviroSkyLite.instance.seasonsSettings;
#else
        return null;
#endif
        }
        set
        {
#if ENVIRO_HD && ENVIRO_LW
            if (currentEnviroSkyVersion == EnviroSkyVersion.HD)
                EnviroSky.instance.seasonsSettings = value;
            else
                EnviroSkyLite.instance.seasonsSettings = value;
#elif ENVIRO_HD
       EnviroSky.instance.seasonsSettings = value;
#elif ENVIRO_LW
       EnviroSkyLite.instance.seasonsSettings = value;
#endif
        }
    }

    /// <summary>
    /// Enviro Cloud Settings
    /// </summary>
    public EnviroCloudSettings CloudSettings
    {
        get
        {
#if ENVIRO_HD && ENVIRO_LW
            if (currentEnviroSkyVersion == EnviroSkyVersion.HD)
                return EnviroSky.instance.cloudsSettings;
            else
                return EnviroSkyLite.instance.cloudsSettings;
#elif ENVIRO_HD
        return EnviroSky.instance.cloudsSettings;
#elif ENVIRO_LW
        return EnviroSkyLite.instance.cloudsSettings;
#else
        return null;
#endif
        }
        set
        {
#if ENVIRO_HD && ENVIRO_LW
            if (currentEnviroSkyVersion == EnviroSkyVersion.HD)
                EnviroSky.instance.cloudsSettings = value;
            else
                EnviroSkyLite.instance.cloudsSettings = value;
#elif ENVIRO_HD
       EnviroSky.instance.cloudsSettings = value;
#elif ENVIRO_LW
       EnviroSkyLite.instance.cloudsSettings = value;
#endif
        }
    }

    /// <summary>
    /// Enviro Cloud Settings
    /// </summary>
    public EnviroInteriorZoneSettings InteriorZoneSettings
    {
        get
        {
#if ENVIRO_HD && ENVIRO_LW
            if (currentEnviroSkyVersion == EnviroSkyVersion.HD)
                return EnviroSky.instance.interiorZoneSettings;
            else
                return EnviroSkyLite.instance.interiorZoneSettings;
#elif ENVIRO_HD
        return EnviroSky.instance.interiorZoneSettings;
#elif ENVIRO_LW
        return EnviroSkyLite.instance.interiorZoneSettings;
#else
        return null;
#endif
        }
        set
        {
#if ENVIRO_HD && ENVIRO_LW
            if (currentEnviroSkyVersion == EnviroSkyVersion.HD)
                EnviroSky.instance.interiorZoneSettings = value;
            else
                EnviroSkyLite.instance.interiorZoneSettings = value;
#elif ENVIRO_HD
       EnviroSky.instance.interiorZoneSettings = value;
#elif ENVIRO_LW
       EnviroSkyLite.instance.interiorZoneSettings = value;
#endif
        }
    }

    /// <summary>
    /// Enviro Audio Settings
    /// </summary>
    public EnviroAudio AudioSettings
    {
        get
        {
#if ENVIRO_HD && ENVIRO_LW
            if (currentEnviroSkyVersion == EnviroSkyVersion.HD)
                return EnviroSky.instance.Audio;
            else
                return EnviroSkyLite.instance.Audio;
#elif ENVIRO_HD
        return EnviroSky.instance.Audio;
#elif ENVIRO_LW
        return EnviroSkyLite.instance.Audio;
#else
        return null;
#endif
        }
        set
        {
#if ENVIRO_HD && ENVIRO_LW
            if (currentEnviroSkyVersion == EnviroSkyVersion.HD)
                EnviroSky.instance.Audio = value;
            else
                EnviroSkyLite.instance.Audio = value;
#elif ENVIRO_HD
       EnviroSky.instance.Audio = value;
#elif ENVIRO_LW
       EnviroSkyLite.instance.Audio = value;
#endif
        }
    }

    /// <summary>
    /// Enviro Cloud Config
    /// </summary>
    public EnviroWeatherCloudsConfig Clouds
    {
        get
        {
#if ENVIRO_HD && ENVIRO_LW
            if (currentEnviroSkyVersion == EnviroSkyVersion.HD)
                return EnviroSky.instance.cloudsConfig;
            else
                return EnviroSkyLite.instance.cloudsConfig;
#elif ENVIRO_HD
        return EnviroSky.instance.cloudsConfig;
#elif ENVIRO_LW
        return EnviroSkyLite.instance.cloudsConfig;
#else
        return null;
#endif
        }
        set
        {
#if ENVIRO_HD && ENVIRO_LW
            if (currentEnviroSkyVersion == EnviroSkyVersion.HD)
                EnviroSky.instance.cloudsConfig = value;
            else
                EnviroSkyLite.instance.cloudsConfig = value;
#elif ENVIRO_HD
       EnviroSky.instance.cloudsConfig = value;
#elif ENVIRO_LW
       EnviroSkyLite.instance.cloudsConfig = value;
#endif
        }
    }

    /// <summary>
    /// Enviro Cloud Config
    /// </summary>
    public EnviroWeatherSettings WeatherSettings
    {
        get
        {
#if ENVIRO_HD && ENVIRO_LW
            if (currentEnviroSkyVersion == EnviroSkyVersion.HD)
                return EnviroSky.instance.weatherSettings;
            else
                return EnviroSkyLite.instance.weatherSettings;
#elif ENVIRO_HD
        return EnviroSky.instance.weatherSettings;
#elif ENVIRO_LW
        return EnviroSkyLite.instance.weatherSettings;
#else
        return null;
#endif
        }
        set
        {
#if ENVIRO_HD && ENVIRO_LW
            if (currentEnviroSkyVersion == EnviroSkyVersion.HD)
                EnviroSky.instance.weatherSettings = value;
            else
                EnviroSkyLite.instance.weatherSettings = value;
#elif ENVIRO_HD
       EnviroSky.instance.weatherSettings = value;
#elif ENVIRO_LW
       EnviroSkyLite.instance.weatherSettings = value;
#endif
        }
    }

    /// <summary>
    /// Enviro Lighting Settings
    /// </summary>
    public EnviroLightSettings LightSettings
    {
        get
        {
#if ENVIRO_HD && ENVIRO_LW
            if (currentEnviroSkyVersion == EnviroSkyVersion.HD)
                return EnviroSky.instance.lightSettings;
            else
                return EnviroSkyLite.instance.lightSettings;
#elif ENVIRO_HD
        return EnviroSky.instance.lightSettings;
#elif ENVIRO_LW
        return EnviroSkyLite.instance.lightSettings;
#else
        return null;
#endif
        }
        set
        {
#if ENVIRO_HD && ENVIRO_LW
            if (currentEnviroSkyVersion == EnviroSkyVersion.HD)
                EnviroSky.instance.lightSettings = value;
            else
                EnviroSkyLite.instance.lightSettings = value;
#elif ENVIRO_HD
       EnviroSky.instance.lightSettings = value;
#elif ENVIRO_LW
       EnviroSkyLite.instance.lightSettings = value;
#endif
        }
    }

    /// <summary>
    /// Enviro Volume Lighting Settings
    /// </summary>
    public EnviroVolumeLightingSettings VolumeLightSettings
    {
        get
        {
#if ENVIRO_HD && ENVIRO_LW
            if (currentEnviroSkyVersion == EnviroSkyVersion.HD)
                return EnviroSky.instance.volumeLightSettings;
            else
                return EnviroSkyLite.instance.volumeLightSettings;
#elif ENVIRO_HD
        return EnviroSky.instance.volumeLightSettings;
#elif ENVIRO_LW
        return EnviroSkyLite.instance.volumeLightSettings;
#else
        return null;
#endif
        }
        set
        {
#if ENVIRO_HD && ENVIRO_LW
            if (currentEnviroSkyVersion == EnviroSkyVersion.HD)
                EnviroSky.instance.volumeLightSettings = value;
            else
                EnviroSkyLite.instance.volumeLightSettings = value;
#elif ENVIRO_HD
       EnviroSky.instance.volumeLightSettings = value;
#elif ENVIRO_LW
       EnviroSkyLite.instance.volumeLightSettings = value;
#endif
        }
    }

    /// <summary>
    /// Enviro Volume Lighting Settings
    /// </summary>
    public EnviroLightShaftsSettings LightShaftsSettings
    {
        get
        {
#if ENVIRO_HD && ENVIRO_LW
            if (currentEnviroSkyVersion == EnviroSkyVersion.HD)
                return EnviroSky.instance.lightshaftsSettings;
            else
                return EnviroSkyLite.instance.lightshaftsSettings;
#elif ENVIRO_HD
        return EnviroSky.instance.lightshaftsSettings;
#elif ENVIRO_LW
        return EnviroSkyLite.instance.lightshaftsSettings;
#else
        return null;
#endif
        }
        set
        {
#if ENVIRO_HD && ENVIRO_LW
            if (currentEnviroSkyVersion == EnviroSkyVersion.HD)
                EnviroSky.instance.lightshaftsSettings = value;
            else
                EnviroSkyLite.instance.lightshaftsSettings = value;
#elif ENVIRO_HD
       EnviroSky.instance.lightshaftsSettings = value;
#elif ENVIRO_LW
       EnviroSkyLite.instance.lightshaftsSettings = value;
#endif
        }
    }

    /// <summary>
    /// Enviro Lighting Settings
    /// </summary>
    public EnviroSkySettings SkySettings
    {
        get
        {
#if ENVIRO_HD && ENVIRO_LW
            if (currentEnviroSkyVersion == EnviroSkyVersion.HD)
                return EnviroSky.instance.skySettings;
            else
                return EnviroSkyLite.instance.skySettings;
#elif ENVIRO_HD
        return EnviroSky.instance.skySettings;
#elif ENVIRO_LW
        return EnviroSkyLite.instance.skySettings;
#else
        return null;
#endif
        }
        set
        {
#if ENVIRO_HD && ENVIRO_LW
            if (currentEnviroSkyVersion == EnviroSkyVersion.HD)
                EnviroSky.instance.skySettings = value;
            else
                EnviroSkyLite.instance.skySettings = value;
#elif ENVIRO_HD
       EnviroSky.instance.skySettings = value;
#elif ENVIRO_LW
       EnviroSkyLite.instance.skySettings = value;
#endif
        }
    }


    /// <summary>
    /// Enviro Fog Settings
    /// </summary>
    public EnviroFogSettings FogSettings
    {
        get
        {
#if ENVIRO_HD && ENVIRO_LW
            if (currentEnviroSkyVersion == EnviroSkyVersion.HD)
                return EnviroSky.instance.fogSettings;
            else
                return EnviroSkyLite.instance.fogSettings;
#elif ENVIRO_HD
        return EnviroSky.instance.fogSettings;
#elif ENVIRO_LW
        return EnviroSkyLite.instance.fogSettings;
#else
        return null;
#endif
        }
        set
        {
#if ENVIRO_HD && ENVIRO_LW
            if (currentEnviroSkyVersion == EnviroSkyVersion.HD)
                EnviroSky.instance.fogSettings = value;
            else
                EnviroSkyLite.instance.fogSettings = value;
#elif ENVIRO_HD
       EnviroSky.instance.fogSettings = value;
#elif ENVIRO_LW
       EnviroSkyLite.instance.fogSettings = value;
#endif
        }
    }

    /// <summary>
    /// Enviro Player Reference
    /// </summary>
    public GameObject Player
    {
        get
        {
#if ENVIRO_HD && ENVIRO_LW
            if (currentEnviroSkyVersion == EnviroSkyVersion.HD)
                return EnviroSky.instance.Player;
            else
                return EnviroSkyLite.instance.Player;
#elif ENVIRO_HD
        return EnviroSky.instance.Player;
#elif ENVIRO_LW
        return EnviroSkyLite.instance.Player;
#else
        return null;
#endif
        }
        set
        {
#if ENVIRO_HD && ENVIRO_LW
            if (currentEnviroSkyVersion == EnviroSkyVersion.HD)
                EnviroSky.instance.Player = value;
            else
                EnviroSkyLite.instance.Player = value;
#elif ENVIRO_HD
       EnviroSky.instance.Player = value;
#elif ENVIRO_LW
       EnviroSkyLite.instance.Player = value;
#endif
        }
    }


    /// <summary>
    /// Enviro Camera Reference
    /// </summary>
    public Camera Camera
    {
        get
        {
#if ENVIRO_HD && ENVIRO_LW
            if (currentEnviroSkyVersion == EnviroSkyVersion.HD)
                return EnviroSky.instance.PlayerCamera;
            else
                return EnviroSkyLite.instance.PlayerCamera;
#elif ENVIRO_HD
        return EnviroSky.instance.PlayerCamera;
#elif ENVIRO_LW
        return EnviroSkyLite.instance.PlayerCamera;
#else
        return null;
#endif
        }
        set
        {
#if ENVIRO_HD && ENVIRO_LW
            if (currentEnviroSkyVersion == EnviroSkyVersion.HD)
                EnviroSky.instance.PlayerCamera = value;
            else
                EnviroSkyLite.instance.PlayerCamera = value;
#elif ENVIRO_HD
       EnviroSky.instance.PlayerCamera = value;
#elif ENVIRO_LW
       EnviroSkyLite.instance.PlayerCamera = value;
#endif
        }
    }
   
    /// <summary>
    /// Assigns Player and Camera and starts current enviro instance.
    /// </summary>
    /// <param name="Player"></param>
    public void AssignAndStart(GameObject Player, Camera cam)
    {
#if ENVIRO_HD && ENVIRO_LW
        if (currentEnviroSkyVersion == EnviroSkyVersion.HD)
            EnviroSky.instance.AssignAndStart(Player, cam);
        else
            EnviroSkyLite.instance.AssignAndStart(Player, cam);
#elif ENVIRO_HD
       EnviroSky.instance.AssignAndStart(Player, cam);
#elif ENVIRO_LW
       EnviroSkyLite.instance.AssignAndStart(Player, cam);
#endif
    }

    /// <summary>
    /// Changes Player and Camera.
    /// </summary>
    public void ChangeFocus(GameObject Player, Camera cam)
    {
#if ENVIRO_HD && ENVIRO_LW
        if (currentEnviroSkyVersion == EnviroSkyVersion.HD)
            EnviroSky.instance.ChangeFocus(Player, cam);
        else
            EnviroSkyLite.instance.ChangeFocus(Player, cam);
#elif ENVIRO_HD
       EnviroSky.instance.ChangeFocus(Player, cam);
#elif ENVIRO_LW
       EnviroSkyLite.instance.ChangeFocus(Player, cam);
#endif
    }

    /// <summary>
    /// Starts current enviro instance as server.
    /// </summary>
    public void StartAsServer()
    {
#if ENVIRO_HD && ENVIRO_LW
        if (currentEnviroSkyVersion == EnviroSkyVersion.HD)
            EnviroSky.instance.StartAsServer();
        else
            EnviroSkyLite.instance.StartAsServer();
#elif ENVIRO_HD
       EnviroSky.instance.StartAsServer();
#elif ENVIRO_LW
      EnviroSkyLite.instance.StartAsServer();
#endif
    }

    /// <summary>
    /// Re Initilize Enviro Instance
    /// </summary>
    public void ReInit()
    {
#if ENVIRO_HD && ENVIRO_LW
        if (currentEnviroSkyVersion == EnviroSkyVersion.HD)
            EnviroSky.instance.ReInit();
        else
            EnviroSkyLite.instance.ReInit();
#elif ENVIRO_HD
       EnviroSky.instance.ReInit();
#elif ENVIRO_LW
     EnviroSkyLite.instance.ReInit();
#endif
    }

    /// <summary>
    /// Setup Skybox
    /// </summary>
    public void SetupSkybox()
    {
#if ENVIRO_HD && ENVIRO_LW
        if (currentEnviroSkyVersion == EnviroSkyVersion.HD)
            EnviroSky.instance.SetupSkybox();
        else
            EnviroSkyLite.instance.SetupSkybox();
#elif ENVIRO_HD
       EnviroSky.instance.SetupSkybox();
#elif ENVIRO_LW
      EnviroSkyLite.instance.SetupSkybox();
#endif
    }

    #endregion
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    #region checks

    public bool IsNight()
    {
#if ENVIRO_HD && ENVIRO_LW
        if (currentEnviroSkyVersion == EnviroSkyVersion.HD)
            return EnviroSky.instance.isNight;
        else
            return EnviroSkyLite.instance.isNight;
#elif ENVIRO_HD
         return EnviroSky.instance.isNight;
#elif ENVIRO_LW
         return EnviroSkyLite.instance.isNight;
#else
        return false;
#endif
    }

    public bool IsStarted()
    {
#if ENVIRO_HD && ENVIRO_LW
        if (currentEnviroSkyVersion == EnviroSkyVersion.HD)
            return EnviroSky.instance.started;
        else
            return EnviroSkyLite.instance.started;
#elif ENVIRO_HD
        return EnviroSky.instance.started;
#elif ENVIRO_LW
        return EnviroSkyLite.instance.started;
#else
        return false;
#endif
    }

    public bool IsInterior()
    {
#if ENVIRO_HD && ENVIRO_LW
        if (currentEnviroSkyVersion == EnviroSkyVersion.HD)
            return EnviroSky.instance.interiorMode;
        else
            return EnviroSkyLite.instance.interiorMode;
#elif ENVIRO_HD
        return EnviroSky.instance.interiorMode;
#elif ENVIRO_LW
        return EnviroSkyLite.instance.interiorMode;
#else
        return false;
#endif
    }

    public bool IsEnviroSkyAttached(GameObject obj)
    {
#if ENVIRO_HD && ENVIRO_LW
        if (currentEnviroSkyVersion == EnviroSkyVersion.HD)
            return obj.GetComponent<EnviroSky>();
        else
            return obj.GetComponent<EnviroSkyLite>();
#elif ENVIRO_HD
         return obj.GetComponent<EnviroSky>();
#elif ENVIRO_LW
        return obj.GetComponent<EnviroSkyLite>();
#else
        return false;
#endif
    }

    public bool IsDefaultZone(GameObject zone)
    {
#if ENVIRO_HD && ENVIRO_LW
        if (zone.GetComponent<EnviroSky>() || zone.GetComponent<EnviroSkyLite>())
            return true;
        else
            return false;
#elif ENVIRO_HD
        if (zone.GetComponent<EnviroSky>())
            return true;
        else
            return false;
#elif ENVIRO_LW
       if (zone.GetComponent<EnviroSkyLite>())
            return true;
        else
            return false;
#else
        return false;
#endif
    }


    public bool IsAutoWeatherUpdateActive()
    {
#if ENVIRO_HD && ENVIRO_LW
        if (currentEnviroSkyVersion == EnviroSkyVersion.HD)
            return EnviroSky.instance.Weather.updateWeather;
        else
            return EnviroSkyLite.instance.Weather.updateWeather;
#elif ENVIRO_HD
        return EnviroSky.instance.Weather.updateWeather;
#elif ENVIRO_LW
        return EnviroSkyLite.instance.Weather.updateWeather;
#else
        return false;
#endif
    }
    public bool IsAvailable()
    {
#if ENVIRO_HD && ENVIRO_LW
        if (currentEnviroSkyVersion == EnviroSkyVersion.HD)
            if (EnviroSky.instance == null)
            {
                return false;
            }
            else
            {
                return true;
            }
        else
        {
            if (EnviroSkyLite.instance == null)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
#elif ENVIRO_HD
            if (EnviroSky.instance == null)
            {
                return false;
            }
            else
            {
                return true;
            }
#elif ENVIRO_LW
           if (EnviroSkyLite.instance == null)
            {
                return false;
            }
            else
            {
                return true;
            }
#else
        return false;

#endif
    }
    #endregion
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    #region Weather

    /// <summary>
    /// Enviro weather runtime values
    /// </summary>
    public EnviroWeather Weather
    {
        get
        {
#if ENVIRO_HD && ENVIRO_LW
            if (currentEnviroSkyVersion == EnviroSkyVersion.HD)
                return EnviroSky.instance.Weather;
            else
                return EnviroSkyLite.instance.Weather;
#elif ENVIRO_HD
        return EnviroSky.instance.Weather;
#elif ENVIRO_LW
        return EnviroSkyLite.instance.Weather;
#else
        return null;
#endif
        }
        set
        {
#if ENVIRO_HD && ENVIRO_LW
            if (currentEnviroSkyVersion == EnviroSkyVersion.HD)
                EnviroSky.instance.Weather = value;
            else
                EnviroSkyLite.instance.Weather = value;
#elif ENVIRO_HD
       EnviroSky.instance.Weather = value;
#elif ENVIRO_LW
       EnviroSkyLite.instance.Weather = value;
#endif
        }
    }

    public bool GetUseWeatherTag()
    {
#if ENVIRO_HD && ENVIRO_LW
        if (currentEnviroSkyVersion == EnviroSkyVersion.HD)
            return EnviroSky.instance.weatherSettings.useTag;
        else
            return EnviroSkyLite.instance.weatherSettings.useTag;

#elif ENVIRO_HD
      return EnviroSky.instance.weatherSettings.useTag;
#elif ENVIRO_LW
       return EnviroSkyLite.instance.weatherSettings.useTag;
#else
        return false;
#endif
    }

    public string GetEnviroSkyTag()
    {
#if ENVIRO_HD && ENVIRO_LW
        if (currentEnviroSkyVersion == EnviroSkyVersion.HD)
            return EnviroSky.instance.tag;
        else
            return EnviroSkyLite.instance.tag;

#elif ENVIRO_HD
      return EnviroSky.instance.tag;
#elif ENVIRO_LW
        return EnviroSkyLite.instance.tag;
#else
        return "";
#endif
    }

    public float GetSnowIntensity()
    {
#if ENVIRO_HD && ENVIRO_LW
        if (currentEnviroSkyVersion == EnviroSkyVersion.HD)
            return EnviroSky.instance.Weather.curSnowStrength;
        else
            return EnviroSkyLite.instance.Weather.curSnowStrength;

#elif ENVIRO_HD
       return EnviroSky.instance.Weather.curSnowStrength;
#elif ENVIRO_LW
        return EnviroSkyLite.instance.Weather.curSnowStrength;
#else
        return 0f;
#endif
    }

    public float GetWetnessIntensity()
    {
#if ENVIRO_HD && ENVIRO_LW
        if (currentEnviroSkyVersion == EnviroSkyVersion.HD)
            return EnviroSky.instance.Weather.curWetness;
        else
            return EnviroSkyLite.instance.Weather.curWetness;

#elif ENVIRO_HD
       return EnviroSky.instance.Weather.curWetness;
#elif ENVIRO_LW
        return EnviroSkyLite.instance.Weather.curWetness;
#else
        return 0f;
#endif
    }

    /// <summary>
    /// Get current temperature.
    /// </summary>
    public string GetCurrentTemperatureString()
    {
        int tInt = 0;
#if ENVIRO_HD && ENVIRO_LW
        if (currentEnviroSkyVersion == EnviroSkyVersion.HD)
            tInt = (int)EnviroSky.instance.Weather.currentTemperature;
        else
            tInt = (int)EnviroSkyLite.instance.Weather.currentTemperature;

        string t = tInt.ToString() + "°C";
        return t;
#elif ENVIRO_HD
        tInt = (int)EnviroSky.instance.Weather.currentTemperature;
        string t = tInt.ToString() + "°C";
        return t;
#elif ENVIRO_LW
        tInt = (int)EnviroSkyLite.instance.Weather.currentTemperature;
        string t = tInt.ToString() + "°C";
        return t;
#else
        return "";
#endif
    }

    /// <summary>
    /// Set and Get custom fog intensity
    /// </summary>
    public float CustomFogIntensity
    {
        get
        {
#if ENVIRO_HD && ENVIRO_LW
            if (currentEnviroSkyVersion == EnviroSkyVersion.HD)
                return EnviroSky.instance.customFogIntensity;
            else
                return EnviroSkyLite.instance.customFogIntensity;
#elif ENVIRO_HD
        return EnviroSky.instance.customFogIntensity;
#elif ENVIRO_LW
        return EnviroSkyLite.instance.customFogIntensity;
#else
        return 0f;
#endif
        }
        set
        {
#if ENVIRO_HD && ENVIRO_LW
            if (currentEnviroSkyVersion == EnviroSkyVersion.HD)
                EnviroSky.instance.customFogIntensity = value;
            else
                EnviroSkyLite.instance.customFogIntensity = value;
#elif ENVIRO_HD
       EnviroSky.instance.customFogIntensity = value;
#elif ENVIRO_LW
       EnviroSkyLite.instance.customFogIntensity = value;
#endif
        }
    }

    /// <summary>
    /// Set and get custom fog color
    /// </summary>
    public Color CustomFogColor
    {
        get
        {
#if ENVIRO_HD && ENVIRO_LW
            if (currentEnviroSkyVersion == EnviroSkyVersion.HD)
                return EnviroSky.instance.customFogColor;
            else
                return EnviroSkyLite.instance.customFogColor;

#elif ENVIRO_HD
        return EnviroSky.instance.customFogColor;
#elif ENVIRO_LW
        return EnviroSkyLite.instance.customFogColor;
#else
        return new Color(0, 0, 0, 0);
#endif
        }
        set
        {
#if ENVIRO_HD && ENVIRO_LW
            if (currentEnviroSkyVersion == EnviroSkyVersion.HD)
                EnviroSky.instance.customFogColor = value;
            else
                EnviroSkyLite.instance.customFogColor = value;
#elif ENVIRO_HD
       EnviroSky.instance.customFogColor = value;
#elif ENVIRO_LW
       EnviroSkyLite.instance.customFogColor = value;
#endif
        }
    }

    /// <summary>
    /// Activate or deactivate setting of unity Fog Density.
    /// </summary>
    public bool UpdateFogIntensity
    {
        get
        {
#if ENVIRO_HD && ENVIRO_LW
            if (currentEnviroSkyVersion == EnviroSkyVersion.HD)
                return EnviroSky.instance.updateFogDensity;
            else
                return EnviroSkyLite.instance.updateFogDensity;
#elif ENVIRO_HD
        return EnviroSky.instance.updateFogDensity;
#elif ENVIRO_LW
        return EnviroSkyLite.instance.updateFogDensity;
#else
        return false;
#endif
        }
        set
        {
#if ENVIRO_HD && ENVIRO_LW
            if (currentEnviroSkyVersion == EnviroSkyVersion.HD)
                EnviroSky.instance.updateFogDensity = value;
            else
                EnviroSkyLite.instance.updateFogDensity = value;
#elif ENVIRO_HD
       EnviroSky.instance.updateFogDensity = value;
#elif ENVIRO_LW
       EnviroSkyLite.instance.updateFogDensity = value;
#endif
        }
    }

    #endregion
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    #region Zones
    public EnviroZone GetZoneByID(int id)
    {
#if ENVIRO_HD && ENVIRO_LW
        if (currentEnviroSkyVersion == EnviroSkyVersion.HD)
            return EnviroSky.instance.Weather.zones[id];
        else
            return EnviroSkyLite.instance.Weather.zones[id];
#elif ENVIRO_HD
        return EnviroSky.instance.Weather.zones[id];
#elif ENVIRO_LW
        return EnviroSkyLite.instance.Weather.zones[id];
#else
        return null;
#endif
    }

    public void RegisterZone(EnviroZone z)
    {
#if ENVIRO_HD && ENVIRO_LW
        if (currentEnviroSkyVersion == EnviroSkyVersion.HD)
            EnviroSky.instance.RegisterZone(z);
        else
            EnviroSkyLite.instance.RegisterZone(z);
#elif ENVIRO_HD
        EnviroSky.instance.RegisterZone(z);
#elif ENVIRO_LW
        EnviroSkyLite.instance.RegisterZone(z);
#endif
    }

#endregion
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    #region Time
    /// <summary>
    /// Get current time in hours. UTC0 (12.5 = 12:30)
    /// </summary>
    /// <returns>The the current time of day in hours.</returns>
    public float GetUniversalTimeOfDay()
    {
#if ENVIRO_HD && ENVIRO_LW
        if(currentEnviroSkyVersion == EnviroSkyVersion.HD)
            return EnviroSky.instance.internalHour - EnviroSky.instance.GameTime.utcOffset;
        else
            return EnviroSkyLite.instance.internalHour - EnviroSkyLite.instance.GameTime.utcOffset;
#elif ENVIRO_HD
         return EnviroSky.instance.internalHour - EnviroSky.instance.GameTime.utcOffset;
#elif ENVIRO_LW
         return EnviroSkyLite.instance.internalHour - EnviroSkyLite.instance.GameTime.utcOffset;
#else
        return 0f;
#endif
    }

    public double GetCurrentTimeInHours()
    {
#if ENVIRO_HD && ENVIRO_LW
        if (currentEnviroSkyVersion == EnviroSkyVersion.HD)
            return EnviroSky.instance.currentTimeInHours;
        else
            return EnviroSkyLite.instance.currentTimeInHours;
#elif ENVIRO_HD
         return EnviroSky.instance.currentTimeInHours;
#elif ENVIRO_LW
         return EnviroSkyLite.instance.currentTimeInHours;
#else
        return 0f;
#endif
    }


    public EnviroSeasons.Seasons GetCurrentSeason()
    {
#if ENVIRO_HD && ENVIRO_LW
        if (currentEnviroSkyVersion == EnviroSkyVersion.HD)
            return EnviroSky.instance.Seasons.currentSeasons;
        else
            return EnviroSkyLite.instance.Seasons.currentSeasons;
#elif ENVIRO_HD
         return EnviroSky.instance.Seasons.currentSeasons;
#elif ENVIRO_LW
         return EnviroSkyLite.instance.Seasons.currentSeasons;
#else
        return EnviroSeasons.Seasons.Spring;
#endif
    }


    /// <summary>
    /// Sets the year.
    /// </summary>
    public void SetYears(int year)
    {
#if ENVIRO_HD && ENVIRO_LW
        if (currentEnviroSkyVersion == EnviroSkyVersion.HD)
            EnviroSky.instance.GameTime.Years = year;
        else
            EnviroSkyLite.instance.GameTime.Years = year;
#elif ENVIRO_HD
          EnviroSky.instance.GameTime.Years = year;
#elif ENVIRO_LW
         EnviroSkyLite.instance.GameTime.Years = year;
#endif
    }

    /// <summary>
    /// Sets the days.
    /// </summary>
    public void SetDays(int days)
    {
#if ENVIRO_HD && ENVIRO_LW
        if (currentEnviroSkyVersion == EnviroSkyVersion.HD)
            EnviroSky.instance.GameTime.Days = days;
        else
            EnviroSkyLite.instance.GameTime.Days = days;
#elif ENVIRO_HD
          EnviroSky.instance.GameTime.Days = days;
#elif ENVIRO_LW
         EnviroSkyLite.instance.GameTime.Days = days;
#endif
    }


    /// <summary>
    /// Set the exact date. by DateTime
    /// </summary>
    public void SetTime(System.DateTime date)
    {
#if ENVIRO_HD && ENVIRO_LW
        if (currentEnviroSkyVersion == EnviroSkyVersion.HD)
            EnviroSky.instance.SetTime(date);
        else
            EnviroSkyLite.instance.SetTime(date);
#elif ENVIRO_HD
          EnviroSky.instance.SetTime(date);
#elif ENVIRO_LW
         EnviroSkyLite.instance.SetTime(date);
#endif
    }

    /// <summary>
    /// Set the exact date.
    /// </summary>
    public void SetTime(int year, int dayOfYear, int hour, int minute, int seconds)
    {
#if ENVIRO_HD && ENVIRO_LW
        if (currentEnviroSkyVersion == EnviroSkyVersion.HD)
            EnviroSky.instance.SetTime(year, dayOfYear, hour, minute, seconds);
        else
            EnviroSkyLite.instance.SetTime(year, dayOfYear, hour, minute, seconds);
#elif ENVIRO_HD
          EnviroSky.instance.SetTime(year, dayOfYear, hour, minute, seconds);
#elif ENVIRO_LW
         EnviroSkyLite.instance.SetTime(year, dayOfYear, hour, minute, seconds);
#endif
    }

    /// <summary>
    /// Set the time of day in hours. (12.5 = 12:30)
    /// </summary>
    public void SetTimeOfDay(float timeOfDay)
    {
#if ENVIRO_HD && ENVIRO_LW
        if (currentEnviroSkyVersion == EnviroSkyVersion.HD)
            EnviroSky.instance.SetInternalTimeOfDay(timeOfDay);
        else
            EnviroSkyLite.instance.SetInternalTimeOfDay(timeOfDay);
#elif ENVIRO_HD
          EnviroSky.instance.SetInternalTimeOfDay(timeOfDay);
#elif ENVIRO_LW
         EnviroSkyLite.instance.SetInternalTimeOfDay(timeOfDay);
#endif
    }


    /// <summary>
    /// Set the time of day in hours. (12.5 = 12:30)
    /// </summary>
    public void ChangeSeason(EnviroSeasons.Seasons s)
    {
#if ENVIRO_HD && ENVIRO_LW
        if (currentEnviroSkyVersion == EnviroSkyVersion.HD)
            EnviroSky.instance.ChangeSeason(s);
        else
            EnviroSkyLite.instance.ChangeSeason(s);
#elif ENVIRO_HD
          EnviroSky.instance.ChangeSeason(s);
#elif ENVIRO_LW
         EnviroSkyLite.instance.ChangeSeason(s);
#endif
    }

    /// <summary>
    /// Set time progession modes. For example to stop time progression.
    /// </summary>
    /// <returns>The the current time of day in hours.</returns>
    public void SetTimeProgress(EnviroTime.TimeProgressMode tpm)
    {
#if ENVIRO_HD && ENVIRO_LW
        if (currentEnviroSkyVersion == EnviroSkyVersion.HD)
            EnviroSky.instance.GameTime.ProgressTime = tpm;
        else
           EnviroSkyLite.instance.GameTime.ProgressTime = tpm;
#elif ENVIRO_HD
         EnviroSky.instance.GameTime.ProgressTime = tpm;
#elif ENVIRO_LW
         EnviroSkyLite.instance.GameTime.ProgressTime = tpm;
#endif
    }

    /// <summary>
    /// Get current time in a nicely formatted string with seconds!
    /// </summary>
    /// <returns>The time string.</returns>
    public string GetTimeStringWithSeconds()
    {
#if ENVIRO_HD && ENVIRO_LW
        if (currentEnviroSkyVersion == EnviroSkyVersion.HD)
            return string.Format("{0:00}:{1:00}:{2:00}", EnviroSky.instance.GameTime.Hours, EnviroSky.instance.GameTime.Minutes, EnviroSky.instance.GameTime.Seconds);
        else
            return string.Format("{0:00}:{1:00}:{2:00}", EnviroSkyLite.instance.GameTime.Hours, EnviroSkyLite.instance.GameTime.Minutes, EnviroSkyLite.instance.GameTime.Seconds);
#elif ENVIRO_HD
        return string.Format("{0:00}:{1:00}:{2:00}", EnviroSky.instance.GameTime.Hours, EnviroSky.instance.GameTime.Minutes, EnviroSky.instance.GameTime.Seconds);
#elif ENVIRO_LW
        return string.Format("{0:00}:{1:00}:{2:00}", EnviroSkyLite.instance.GameTime.Hours, EnviroSkyLite.instance.GameTime.Minutes, EnviroSkyLite.instance.GameTime.Seconds);
#else
        return "";
#endif
    }

    /// <summary>
    /// Get current time in a nicely formatted string!
    /// </summary>
    /// <returns>The time string.</returns>
    public string GetTimeString()
    {
#if ENVIRO_HD && ENVIRO_LW
        if (currentEnviroSkyVersion == EnviroSkyVersion.HD)
            return string.Format("{0:00}:{1:00}", EnviroSky.instance.GameTime.Hours, EnviroSky.instance.GameTime.Minutes);
        else
            return string.Format("{0:00}:{1:00}", EnviroSkyLite.instance.GameTime.Hours, EnviroSkyLite.instance.GameTime.Minutes);
#elif ENVIRO_HD
           return string.Format("{0:00}:{1:00}", EnviroSky.instance.GameTime.Hours, EnviroSky.instance.GameTime.Minutes);
#elif ENVIRO_LW
        return string.Format("{0:00}:{1:00}", EnviroSkyLite.instance.GameTime.Hours, EnviroSkyLite.instance.GameTime.Minutes);
#else
        return "";
#endif
    }

    /// <summary>
    /// Get current year.
    /// </summary>
    public int GetCurrentYear()
    {
#if ENVIRO_HD && ENVIRO_LW
        if (currentEnviroSkyVersion == EnviroSkyVersion.HD)
            return EnviroSky.instance.GameTime.Years;
        else
            return EnviroSkyLite.instance.GameTime.Years;
#elif ENVIRO_HD
            return EnviroSky.instance.GameTime.Years;
#elif ENVIRO_LW
           return EnviroSkyLite.instance.GameTime.Years;
#else
        return 0;
#endif
    }
    /// <summary>
    /// Get current Day.
    /// </summary>
    public int GetCurrentDay()
    {
#if ENVIRO_HD && ENVIRO_LW
        if (currentEnviroSkyVersion == EnviroSkyVersion.HD)
            return EnviroSky.instance.GameTime.Days;
        else
            return EnviroSkyLite.instance.GameTime.Days;
#elif ENVIRO_HD
            return EnviroSky.instance.GameTime.Days;
#elif ENVIRO_LW
           return EnviroSkyLite.instance.GameTime.Days;
#else
        return 0;
#endif
    }
    /// <summary>
    /// Get current hour.
    /// </summary>
    public int GetCurrentHour()
    {
#if ENVIRO_HD && ENVIRO_LW
        if (currentEnviroSkyVersion == EnviroSkyVersion.HD)
            return EnviroSky.instance.GameTime.Hours;
        else
            return EnviroSkyLite.instance.GameTime.Hours;
#elif ENVIRO_HD
            return EnviroSky.instance.GameTime.Hours;
#elif ENVIRO_LW
           return EnviroSkyLite.instance.GameTime.Hours;
#else
        return 0;
#endif
    }
    /// <summary>
    /// Get current minutes.
    /// </summary>
    public int GetCurrentMinute()
    {
#if ENVIRO_HD && ENVIRO_LW
        if (currentEnviroSkyVersion == EnviroSkyVersion.HD)
            return EnviroSky.instance.GameTime.Minutes;
        else
            return EnviroSkyLite.instance.GameTime.Minutes;
#elif ENVIRO_HD
            return EnviroSky.instance.GameTime.Minutes;
#elif ENVIRO_LW
           return EnviroSkyLite.instance.GameTime.Minutes;
#else
        return 0;
#endif
    }

    /// <summary>
    /// Get current minutes.
    /// </summary>
    public int GetCurrentSecond()
    {
#if ENVIRO_HD && ENVIRO_LW
        if (currentEnviroSkyVersion == EnviroSkyVersion.HD)
            return EnviroSky.instance.GameTime.Seconds;
        else
            return EnviroSkyLite.instance.GameTime.Seconds;
#elif ENVIRO_HD
            return EnviroSky.instance.GameTime.Seconds;
#elif ENVIRO_LW
           return EnviroSkyLite.instance.GameTime.Seconds;
#else
        return 0;
#endif
    }
#endregion
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    #region Weather

    /// <summary>
    /// Set weather directly with list id of Weather.WeatherTemplates. No transtions!
    /// </summary>
    public void ChangeWeatherInstant(int weatherId)
    {
#if ENVIRO_HD && ENVIRO_LW
        if (currentEnviroSkyVersion == EnviroSkyVersion.HD)
            EnviroSky.instance.SetWeatherOverwrite(weatherId);
        else
            EnviroSkyLite.instance.SetWeatherOverwrite(weatherId);
#elif ENVIRO_HD
        EnviroSky.instance.SetWeatherOverwrite(weatherId);
#elif ENVIRO_LW
        EnviroSkyLite.instance.SetWeatherOverwrite(weatherId);
#endif
    }
    /// <summary>
    /// Set weather directly with preset of Weather.WeatherTemplates. No transtions!
    /// </summary>
    public void ChangeWeatherInstant(EnviroWeatherPreset preset)
    {
#if ENVIRO_HD && ENVIRO_LW
        if (currentEnviroSkyVersion == EnviroSkyVersion.HD)
            EnviroSky.instance.SetWeatherOverwrite(preset);
        else
            EnviroSkyLite.instance.SetWeatherOverwrite(preset);
#elif ENVIRO_HD
        EnviroSky.instance.SetWeatherOverwrite(preset);
#elif ENVIRO_LW
        EnviroSkyLite.instance.SetWeatherOverwrite(preset);
#endif
    }

    /// <summary>
    /// Set weather over id with smooth transtion.
    /// </summary>
    public void ChangeWeather(int weatherId)
    {
#if ENVIRO_HD && ENVIRO_LW
        if (currentEnviroSkyVersion == EnviroSkyVersion.HD)
            EnviroSky.instance.ChangeWeather(weatherId);
        else
            EnviroSkyLite.instance.ChangeWeather(weatherId);
#elif ENVIRO_HD
        EnviroSky.instance.ChangeWeather(weatherId);
#elif ENVIRO_LW
        EnviroSkyLite.instance.ChangeWeather(weatherId);
#endif
    }

    /// <summary>
    /// Set weather over Preset with smooth transtion.
    /// </summary>
    public void ChangeWeather(EnviroWeatherPreset preset)
    {
#if ENVIRO_HD && ENVIRO_LW
        if (currentEnviroSkyVersion == EnviroSkyVersion.HD)
            EnviroSky.instance.ChangeWeather(preset);
        else
            EnviroSkyLite.instance.ChangeWeather(preset);
#elif ENVIRO_HD
        EnviroSky.instance.ChangeWeather(preset);
#elif ENVIRO_LW
        EnviroSkyLite.instance.ChangeWeather(preset);
#endif
    }

    /// <summary>
    /// Set weather over Name with smooth transtion.
    /// </summary>
    public void ChangeWeather(string Name)
    {
#if ENVIRO_HD && ENVIRO_LW
        if (currentEnviroSkyVersion == EnviroSkyVersion.HD)
            EnviroSky.instance.ChangeWeather(Name);
        else
            EnviroSkyLite.instance.ChangeWeather(Name);
#elif ENVIRO_HD
        EnviroSky.instance.ChangeWeather(Name);
#elif ENVIRO_LW
        EnviroSkyLite.instance.ChangeWeather(Name);
#endif
    }


    public EnviroZone GetCurrentActiveZone()
    {
#if ENVIRO_HD && ENVIRO_LW
        if (currentEnviroSkyVersion == EnviroSkyVersion.HD)
            return EnviroSky.instance.Weather.currentActiveZone;
        else
            return EnviroSkyLite.instance.Weather.currentActiveZone;
#elif ENVIRO_HD
         return EnviroSky.instance.Weather.currentActiveZone;
#elif ENVIRO_LW
         return EnviroSkyLite.instance.Weather.currentActiveZone;
#else
        return null;
#endif
    }

    public void SetCurrentActiveZone(EnviroZone z)
    {
#if ENVIRO_HD && ENVIRO_LW
        if (currentEnviroSkyVersion == EnviroSkyVersion.HD)
            EnviroSky.instance.Weather.currentActiveZone = z;
        else
            EnviroSkyLite.instance.Weather.currentActiveZone = z;
#elif ENVIRO_HD
         EnviroSky.instance.Weather.currentActiveZone = z;
#elif ENVIRO_LW
         EnviroSkyLite.instance.Weather.currentActiveZone = z;
#endif
    }

    public void InstantWeatherChange(EnviroWeatherPreset preset, EnviroWeatherPrefab prefab)
    {
#if ENVIRO_HD && ENVIRO_LW
        if (currentEnviroSkyVersion == EnviroSkyVersion.HD)
            EnviroSky.instance.InstantWeatherChange(preset, prefab);
        else
            EnviroSkyLite.instance.InstantWeatherChange(preset, prefab);
#elif ENVIRO_HD
         EnviroSky.instance.InstantWeatherChange(preset, prefab);
#elif ENVIRO_LW
         EnviroSkyLite.instance.InstantWeatherChange(preset, prefab);
#endif
    }

    public void SetToZone(int z)
    {
#if ENVIRO_HD && ENVIRO_LW
        if (currentEnviroSkyVersion == EnviroSkyVersion.HD)
            EnviroSky.instance.Weather.currentActiveZone = EnviroSky.instance.Weather.zones[z];
        else
            EnviroSkyLite.instance.Weather.currentActiveZone = EnviroSkyLite.instance.Weather.zones[z];
#elif ENVIRO_HD
         EnviroSky.instance.Weather.currentActiveZone = EnviroSky.instance.Weather.zones[z];
#elif ENVIRO_LW
         EnviroSkyLite.instance.Weather.currentActiveZone = EnviroSkyLite.instance.Weather.zones[z];
#endif
    }

    public EnviroWeatherPreset GetCurrentWeatherPreset ()
    {
#if ENVIRO_HD && ENVIRO_LW
        if (currentEnviroSkyVersion == EnviroSkyVersion.HD)
            return EnviroSky.instance.Weather.currentActiveWeatherPreset;
        else
            return EnviroSkyLite.instance.Weather.currentActiveWeatherPreset;
#elif ENVIRO_HD
         return EnviroSky.instance.Weather.currentActiveWeatherPreset;
#elif ENVIRO_LW
         return EnviroSkyLite.instance.Weather.currentActiveWeatherPreset;
#else
        return null;
#endif
    }

    public EnviroWeatherPreset GetStartWeatherPreset()
    {
#if ENVIRO_HD && ENVIRO_LW
        if (currentEnviroSkyVersion == EnviroSkyVersion.HD)
            return EnviroSky.instance.Weather.startWeatherPreset;
        else
            return EnviroSkyLite.instance.Weather.startWeatherPreset;
#elif ENVIRO_HD
         return EnviroSky.instance.Weather.startWeatherPreset;
#elif ENVIRO_LW
         return EnviroSkyLite.instance.Weather.startWeatherPreset;
#else
        return null;
#endif
    }

    public List<EnviroWeatherPreset> GetCurrentWeatherPresetList()
    {
#if ENVIRO_HD && ENVIRO_LW
        if (currentEnviroSkyVersion == EnviroSkyVersion.HD)
            return EnviroSky.instance.Weather.weatherPresets;
        else
            return EnviroSkyLite.instance.Weather.weatherPresets;
#elif ENVIRO_HD
         return EnviroSky.instance.Weather.weatherPresets;
#elif ENVIRO_LW
         return EnviroSkyLite.instance.Weather.weatherPresets;
#else
        return null;
#endif
    }

    public List<EnviroWeatherPrefab> GetCurrentWeatherPrefabList()
    {
#if ENVIRO_HD && ENVIRO_LW
        if (currentEnviroSkyVersion == EnviroSkyVersion.HD)
            return EnviroSky.instance.Weather.WeatherPrefabs;
        else
            return EnviroSkyLite.instance.Weather.WeatherPrefabs;
#elif ENVIRO_HD
         return EnviroSky.instance.Weather.WeatherPrefabs;
#elif ENVIRO_LW
         return EnviroSkyLite.instance.Weather.WeatherPrefabs;
#else
        return null;
#endif
    }

    public List<EnviroZone> GetZoneList()
    {
#if ENVIRO_HD && ENVIRO_LW
        if (currentEnviroSkyVersion == EnviroSkyVersion.HD)
            return EnviroSky.instance.Weather.zones;
        else
            return EnviroSkyLite.instance.Weather.zones;
#elif ENVIRO_HD
         return EnviroSky.instance.Weather.zones;
#elif ENVIRO_LW
         return EnviroSkyLite.instance.Weather.zones;
#else
        return null;
#endif
    }

    public void ChangeZoneWeather(int zoneId, int weatherId)
    {
#if ENVIRO_HD && ENVIRO_LW
        if (currentEnviroSkyVersion == EnviroSkyVersion.HD)
        {
            EnviroSky.instance.Weather.zones[zoneId].currentActiveZoneWeatherPrefab = EnviroSky.instance.Weather.WeatherPrefabs[weatherId];
            EnviroSky.instance.Weather.zones[zoneId].currentActiveZoneWeatherPreset = EnviroSky.instance.Weather.WeatherPrefabs[weatherId].weatherPreset;
        }
        else
        {
            EnviroSkyLite.instance.Weather.zones[zoneId].currentActiveZoneWeatherPrefab = EnviroSkyLite.instance.Weather.WeatherPrefabs[weatherId];
            EnviroSkyLite.instance.Weather.zones[zoneId].currentActiveZoneWeatherPreset = EnviroSkyLite.instance.Weather.WeatherPrefabs[weatherId].weatherPreset;
        }
#elif ENVIRO_HD
         EnviroSky.instance.Weather.zones[zoneId].currentActiveZoneWeatherPrefab = EnviroSky.instance.Weather.WeatherPrefabs[weatherId];
         EnviroSky.instance.Weather.zones[zoneId].currentActiveZoneWeatherPreset = EnviroSky.instance.Weather.WeatherPrefabs[weatherId].weatherPreset;
#elif ENVIRO_LW
         EnviroSkyLite.instance.Weather.zones[zoneId].currentActiveZoneWeatherPrefab = EnviroSkyLite.instance.Weather.WeatherPrefabs[weatherId];
         EnviroSkyLite.instance.Weather.zones[zoneId].currentActiveZoneWeatherPreset = EnviroSkyLite.instance.Weather.WeatherPrefabs[weatherId].weatherPreset;
#endif
    }
    public void SetAutoWeatherUpdates(bool b)
    {
#if ENVIRO_HD && ENVIRO_LW
        if (currentEnviroSkyVersion == EnviroSkyVersion.HD)
            EnviroSky.instance.Weather.updateWeather = b;
           
        else
            EnviroSkyLite.instance.Weather.updateWeather = b;
#elif ENVIRO_HD
        EnviroSky.instance.Weather.updateWeather = b;
#elif ENVIRO_LW
        EnviroSkyLite.instance.Weather.updateWeather = b;
#endif
    }


#endregion
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    #region Audio
    /// <summary>
    /// Get and Set ambient audio volume
    /// </summary>
    public float ambientAudioVolume
    {
        get
        {
#if ENVIRO_HD && ENVIRO_LW
            if (currentEnviroSkyVersion == EnviroSkyVersion.HD)
                return EnviroSky.instance.Audio.ambientSFXVolume;
            else
                return EnviroSkyLite.instance.Audio.ambientSFXVolume;
#elif ENVIRO_HD
         return EnviroSky.instance.Audio.ambientSFXVolume;
#elif ENVIRO_LW
          return EnviroSkyLite.instance.Audio.ambientSFXVolume;
#else
        return 0f;
#endif
        }

        set
        {
#if ENVIRO_HD && ENVIRO_LW
            if (currentEnviroSkyVersion == EnviroSkyVersion.HD)
                EnviroSky.instance.Audio.ambientSFXVolume = value;
            else
                EnviroSkyLite.instance.Audio.ambientSFXVolume = value;
#elif ENVIRO_HD
         EnviroSky.instance.Audio.ambientSFXVolume = value;
#elif ENVIRO_LW
         EnviroSkyLite.instance.Audio.ambientSFXVolume = value;
#else
        return;
#endif

        }
    }
    /// <summary>
    /// Get and Set weather audio volume
    /// </summary>
    public float weatherAudioVolume
    {
        get
        {
#if ENVIRO_HD && ENVIRO_LW
            if (currentEnviroSkyVersion == EnviroSkyVersion.HD)
                return EnviroSky.instance.Audio.weatherSFXVolume;
            else
                return EnviroSkyLite.instance.Audio.weatherSFXVolume;
#elif ENVIRO_HD
         return EnviroSky.instance.Audio.weatherSFXVolume;
#elif ENVIRO_LW
          return EnviroSkyLite.instance.Audio.weatherSFXVolume;
#else
        return 0f;
#endif
        }

        set
        {
#if ENVIRO_HD && ENVIRO_LW
            if (currentEnviroSkyVersion == EnviroSkyVersion.HD)
                EnviroSky.instance.Audio.weatherSFXVolume = value;
            else
                EnviroSkyLite.instance.Audio.weatherSFXVolume = value;
#elif ENVIRO_HD
         EnviroSky.instance.Audio.weatherSFXVolume = value;
#elif ENVIRO_LW
         EnviroSkyLite.instance.Audio.weatherSFXVolume = value;
#else
        return;
#endif


        }
    }
    /// <summary>
    /// Get and Set ambient audio volume modifier
    /// </summary>
    public float ambientAudioVolumeModifier
    {
        get
        {
#if ENVIRO_HD && ENVIRO_LW
            if (currentEnviroSkyVersion == EnviroSkyVersion.HD)
                return EnviroSky.instance.Audio.ambientSFXVolumeMod;
            else
                return EnviroSkyLite.instance.Audio.ambientSFXVolumeMod;
#elif ENVIRO_HD
         return EnviroSky.instance.Audio.ambientSFXVolumeMod;
#elif ENVIRO_LW
         return EnviroSkyLite.instance.Audio.ambientSFXVolumeMod;
#else
        return 0f;
#endif
        }
        set
        {
#if ENVIRO_HD && ENVIRO_LW
            if (currentEnviroSkyVersion == EnviroSkyVersion.HD)
                EnviroSky.instance.Audio.ambientSFXVolumeMod = value;
            else
                EnviroSkyLite.instance.Audio.ambientSFXVolumeMod = value;
#elif ENVIRO_HD
         EnviroSky.instance.Audio.ambientSFXVolumeMod = value;
#elif ENVIRO_LW
         EnviroSkyLite.instance.Audio.ambientSFXVolumeMod = value;
#endif

        }
    }
    /// <summary>
    /// Get and Set weather audio volume modifier
    /// </summary>
    public float weatherAudioVolumeModifier
    {
        get
        {
#if ENVIRO_HD && ENVIRO_LW
            if (currentEnviroSkyVersion == EnviroSkyVersion.HD)
                return EnviroSky.instance.Audio.weatherSFXVolumeMod;
            else
                return EnviroSkyLite.instance.Audio.weatherSFXVolumeMod;
#elif ENVIRO_HD
         return EnviroSky.instance.Audio.weatherSFXVolumeMod;
#elif ENVIRO_LW
          return EnviroSkyLite.instance.Audio.weatherSFXVolumeMod;
#else
        return 0f;
#endif
        }
        set
        {
#if ENVIRO_HD && ENVIRO_LW
        if (currentEnviroSkyVersion == EnviroSkyVersion.HD)
            EnviroSky.instance.Audio.weatherSFXVolumeMod = value;
        else
            EnviroSkyLite.instance.Audio.weatherSFXVolumeMod = value;
#elif ENVIRO_HD
       EnviroSky.instance.Audio.weatherSFXVolumeMod = value;
#elif ENVIRO_LW
       EnviroSkyLite.instance.Audio.weatherSFXVolumeMod = value;
#endif

        }
    }
    /// <summary>
    /// Get and set audio transition speed
    /// </summary>
    public float audioTransitionSpeed
    {
        get
        {
#if ENVIRO_HD && ENVIRO_LW
            if (currentEnviroSkyVersion == EnviroSkyVersion.HD)
                return EnviroSky.instance.weatherSettings.audioTransitionSpeed;
            else
                return EnviroSkyLite.instance.weatherSettings.audioTransitionSpeed;
#elif ENVIRO_HD
         return EnviroSky.instance.weatherSettings.audioTransitionSpeed;
#elif ENVIRO_LW
        return EnviroSkyLite.instance.weatherSettings.audioTransitionSpeed;
#else
        return 0f;
#endif
        }
        set
        {
#if ENVIRO_HD && ENVIRO_LW
        if (currentEnviroSkyVersion == EnviroSkyVersion.HD)
            EnviroSky.instance.weatherSettings.audioTransitionSpeed = value;
        else
            EnviroSkyLite.instance.weatherSettings.audioTransitionSpeed = value;
#elif ENVIRO_HD
          EnviroSky.instance.weatherSettings.audioTransitionSpeed = value;
#elif ENVIRO_LW
          EnviroSkyLite.instance.weatherSettings.audioTransitionSpeed = value;
#else
        return;
#endif
        }
    }
    /// <summary>
    /// Get and set interior zone audio volume
    /// </summary>
    public float interiorZoneAudioVolume
    {
        get
        {
#if ENVIRO_HD && ENVIRO_LW
            if (currentEnviroSkyVersion == EnviroSkyVersion.HD)
                return EnviroSky.instance.interiorZoneSettings.currentInteriorZoneAudioVolume;
            else
                return EnviroSkyLite.instance.interiorZoneSettings.currentInteriorZoneAudioVolume;
#elif ENVIRO_HD
         return EnviroSky.instance.interiorZoneSettings.currentInteriorZoneAudioVolume;
#elif ENVIRO_LW
         return EnviroSkyLite.instance.interiorZoneSettings.currentInteriorZoneAudioVolume;
#else
        return 0f;
#endif
        }
        set
        {
#if ENVIRO_HD && ENVIRO_LW
            if (currentEnviroSkyVersion == EnviroSkyVersion.HD)
                EnviroSky.instance.interiorZoneSettings.currentInteriorZoneAudioVolume = value;
            else
                EnviroSkyLite.instance.interiorZoneSettings.currentInteriorZoneAudioVolume = value;
#elif ENVIRO_HD
        EnviroSky.instance.interiorZoneSettings.currentInteriorZoneAudioVolume = value;
#elif ENVIRO_LW
        EnviroSkyLite.instance.interiorZoneSettings.currentInteriorZoneAudioVolume = value;
#else
        return;
#endif
        }
    }
    /// <summary>
    /// Get and set interior zone audio fading speed
    /// </summary>
    public float interiorZoneAudioFadingSpeed
    {
        get
        {
#if ENVIRO_HD && ENVIRO_LW
            if (currentEnviroSkyVersion == EnviroSkyVersion.HD)
                return EnviroSky.instance.interiorZoneSettings.currentInteriorZoneAudioFadingSpeed;
            else
                return EnviroSkyLite.instance.interiorZoneSettings.currentInteriorZoneAudioFadingSpeed;
#elif ENVIRO_HD
         return EnviroSky.instance.interiorZoneSettings.currentInteriorZoneAudioFadingSpeed;
#elif ENVIRO_LW
         return EnviroSkyLite.instance.interiorZoneSettings.currentInteriorZoneAudioFadingSpeed;
#else
        return 0f;
#endif
        }

        set
        {
#if ENVIRO_HD && ENVIRO_LW
            if (currentEnviroSkyVersion == EnviroSkyVersion.HD)
                EnviroSky.instance.interiorZoneSettings.currentInteriorZoneAudioFadingSpeed = value;
            else
                EnviroSkyLite.instance.interiorZoneSettings.currentInteriorZoneAudioFadingSpeed = value;
#elif ENVIRO_HD
          EnviroSky.instance.interiorZoneSettings.currentInteriorZoneAudioFadingSpeed = value;
#elif ENVIRO_LW
        EnviroSkyLite.instance.interiorZoneSettings.currentInteriorZoneAudioFadingSpeed = value;
#else
        return;
#endif
        }

    }
#endregion
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    #region Utilites

#endregion
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    #region Helper

    public GameObject GetVFXHolder()
        {
#if ENVIRO_HD && ENVIRO_LW
            if (currentEnviroSkyVersion == EnviroSkyVersion.HD)
                return EnviroSky.instance.Weather.VFXHolder;
            else
                return EnviroSkyLite.instance.Weather.VFXHolder;
#elif ENVIRO_HD
             return EnviroSky.instance.Weather.VFXHolder;
#elif ENVIRO_LW
             return EnviroSkyLite.instance.Weather.VFXHolder;
#else
        return null;
#endif
    }
    /// <summary>
    /// Triggers a ligtning flash when setting higher than zero.
    /// </summary>
    /// <param name="trigger"></param>
    public void SetLightningFlashTrigger(float trigger)
        {
#if ENVIRO_HD && ENVIRO_LW
            if (currentEnviroSkyVersion == EnviroSkyVersion.HD)
                EnviroSky.instance.thunder = trigger;
            else
                EnviroSkyLite.instance.thunder = trigger;
#elif ENVIRO_HD
              EnviroSky.instance.thunder = trigger;
#elif ENVIRO_LW
              EnviroSkyLite.instance.thunder = trigger;
#endif
        }

        public float GetEmissionRate(ParticleSystem system)
        {
            return system.emission.rateOverTime.constantMax;
        }


        public void SetEmissionRate(ParticleSystem sys, float emissionRate)
        {
            var emission = sys.emission;
            var rate = emission.rateOverTime;
            rate.constantMax = emissionRate;
            emission.rateOverTime = rate;
        }

        public void RegisterVegetationInstance(EnviroVegetationInstance v)
        {
#if ENVIRO_HD && ENVIRO_LW
            if (currentEnviroSkyVersion == EnviroSkyVersion.HD)
                EnviroSky.instance.RegisterMe(v);
            else
                EnviroSkyLite.instance.RegisterMe(v);
#elif ENVIRO_HD
                  EnviroSky.instance.RegisterMe(v);
#elif ENVIRO_LW
              EnviroSkyLite.instance.RegisterMe(v);
#endif
        }

        public double GetInHours(float hours, float days, float years)
        {
#if ENVIRO_HD && ENVIRO_LW
            if (currentEnviroSkyVersion == EnviroSkyVersion.HD)
                return EnviroSky.instance.GetInHours(hours, days, years, EnviroSky.instance.GameTime.DaysInYear);
            else
                return EnviroSkyLite.instance.GetInHours(hours, days, years, EnviroSkyLite.instance.GameTime.DaysInYear);
#elif ENVIRO_HD
            return EnviroSky.instance.GetInHours(hours, days, years, EnviroSky.instance.GameTime.DaysInYear);
#elif ENVIRO_LW
         return EnviroSkyLite.instance.GetInHours(hours, days, years, EnviroSkyLite.instance.GameTime.DaysInYear);
#else
        return 0;
            
#endif

    }
    #endregion
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    #region Features

    /// <summary>
    /// Enable/Disable Volumn Clouds
    /// </summary>
    public bool useVolumeClouds
    {
        get
        {
#if ENVIRO_HD
            if (currentEnviroSkyVersion == EnviroSkyVersion.HD)
                return EnviroSky.instance.useVolumeClouds;
            else
                return false;
#else
            return false;
#endif
        }
        set
        {
#if ENVIRO_HD
            if (currentEnviroSkyVersion == EnviroSkyVersion.HD)
                EnviroSky.instance.useVolumeClouds = value;
#endif
        }
    }
    /// <summary>
    /// Enable/Disable Volumn Lighting
    /// </summary>
    public bool useVolumeLighting
    {
        get
        {
#if ENVIRO_HD
            if (currentEnviroSkyVersion == EnviroSkyVersion.HD)
                return EnviroSky.instance.useVolumeLighting;
            else
                return false;
#else
            return false;
#endif
        }
        set
        {
#if ENVIRO_HD
            if (currentEnviroSkyVersion == EnviroSkyVersion.HD)
                EnviroSky.instance.useVolumeLighting = value;
#endif
        }
    }
    /// <summary>
    /// Enable/Disable flat clouds
    /// </summary>
    public bool useFlatClouds
    {
        get
        {
#if ENVIRO_HD
            if (currentEnviroSkyVersion == EnviroSkyVersion.HD)
                return EnviroSky.instance.useFlatClouds;
            else
                return false;
#else
            return false;

#endif
        }

        set
        {
#if ENVIRO_HD
            if (currentEnviroSkyVersion == EnviroSkyVersion.HD)
                EnviroSky.instance.useFlatClouds = value;
#endif
        }
    }
    /// <summary>
    /// Enable/Disable flat clouds
    /// </summary>
    public bool useParticleClouds
    {
        get
        {
#if ENVIRO_HD && ENVIRO_LW
            if (currentEnviroSkyVersion == EnviroSkyVersion.HD)
              return EnviroSky.instance.useParticleClouds;
            else
               return EnviroSkyLite.instance.useParticleClouds;
#elif ENVIRO_HD
          return EnviroSky.instance.useParticleClouds;
#elif ENVIRO_LW
          return EnviroSkyLite.instance.useParticleClouds;   
#else
        return false;      
#endif
        }
        set
        {
#if ENVIRO_HD && ENVIRO_LW
            if (currentEnviroSkyVersion == EnviroSkyVersion.HD)
                EnviroSky.instance.useParticleClouds = value;
            else
                EnviroSkyLite.instance.useParticleClouds = value;

#elif ENVIRO_HD
              EnviroSky.instance.useParticleClouds = value;
#elif ENVIRO_LW
            EnviroSkyLite.instance.useParticleClouds = value;           
#endif

        }
    }
    /// <summary>
    /// Enable/Disable sun light shafts clouds
    /// </summary>
    public bool useSunShafts
    {
        get
        {
#if ENVIRO_HD && ENVIRO_LW
            if (currentEnviroSkyVersion == EnviroSkyVersion.HD)
              return EnviroSky.instance.LightShafts.sunLightShafts;
            else
                return EnviroSkyLite.instance.LightShafts.sunLightShafts;

#elif ENVIRO_HD
        return EnviroSky.instance.LightShafts.sunLightShafts;
#elif ENVIRO_LW
        return EnviroSkyLite.instance.LightShafts.sunLightShafts;
#else
        return false;
#endif
        }
        set
        {
#if ENVIRO_HD && ENVIRO_LW
            if (currentEnviroSkyVersion == EnviroSkyVersion.HD)
                EnviroSky.instance.LightShafts.sunLightShafts = value;
            else
                EnviroSkyLite.instance.LightShafts.sunLightShafts = value;

#elif ENVIRO_HD
         EnviroSky.instance.LightShafts.sunLightShafts = value;
#elif ENVIRO_LW
         EnviroSkyLite.instance.LightShafts.sunLightShafts = value;
#endif
        }
    }
    /// <summary>
    /// Enable/Disable moon light shafts clouds
    /// </summary>
    public bool useMoonShafts
    {
        get
        {
#if ENVIRO_HD && ENVIRO_LW
            if (currentEnviroSkyVersion == EnviroSkyVersion.HD)
                return EnviroSky.instance.LightShafts.moonLightShafts;
            else
                return EnviroSkyLite.instance.LightShafts.moonLightShafts;

#elif ENVIRO_HD
        return EnviroSky.instance.LightShafts.moonLightShafts;
#elif ENVIRO_LW
        return EnviroSkyLite.instance.LightShafts.moonLightShafts;
#else
        return false;
#endif
        }
        set
        {
#if ENVIRO_HD && ENVIRO_LW
            if (currentEnviroSkyVersion == EnviroSkyVersion.HD)
                EnviroSky.instance.LightShafts.moonLightShafts = value;
            else
                EnviroSkyLite.instance.LightShafts.moonLightShafts = value;

#elif ENVIRO_HD
         EnviroSky.instance.LightShafts.moonLightShafts = value;
#elif ENVIRO_LW
         EnviroSkyLite.instance.LightShafts.moonLightShafts = value;
#endif
        }
    }
    /// <summary>
    /// Enable/Disable distance blurring
    /// </summary>
    public bool useDistanceBlur
    {
        get
        {
#if ENVIRO_HD && ENVIRO_LW
            if (currentEnviroSkyVersion == EnviroSkyVersion.HD)
                return EnviroSky.instance.useDistanceBlur;
            else
                return false;

#elif ENVIRO_HD
        return EnviroSky.instance.useDistanceBlur;
#elif ENVIRO_LW
        return false;
#else
        return false;
#endif
        }
        set
        {
#if ENVIRO_HD && ENVIRO_LW
            if (currentEnviroSkyVersion == EnviroSkyVersion.HD)
                EnviroSky.instance.useDistanceBlur = value;
#elif ENVIRO_HD
        EnviroSky.instance.useDistanceBlur = value;
#elif ENVIRO_LW
 
#endif
        }
    }

    #endregion
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    #region Events

    // Events
    public delegate void HourPassed();
        public delegate void DayPassed();
        public delegate void YearPassed();
        public delegate void WeatherChanged(EnviroWeatherPreset weatherType);
        public delegate void ZoneWeatherChanged(EnviroWeatherPreset weatherType, EnviroZone zone);
        public delegate void SeasonChanged(EnviroSeasons.Seasons season);
        public delegate void isNightE();
        public delegate void isDay();
        public delegate void ZoneChanged(EnviroZone zone);
        public event HourPassed OnHourPassed;
        public event DayPassed OnDayPassed;
        public event YearPassed OnYearPassed;
        public event WeatherChanged OnWeatherChanged;
        public event ZoneWeatherChanged OnZoneWeatherChanged;
        public event SeasonChanged OnSeasonChanged;
        public event isNightE OnNightTime;
        public event isDay OnDayTime;
        public event ZoneChanged OnZoneChanged;

        // Events:
        public virtual void NotifyHourPassed()
        {
            if (OnHourPassed != null)
                OnHourPassed();
        }
        public virtual void NotifyDayPassed()
        {
            if (OnDayPassed != null)
                OnDayPassed();
        }
        public virtual void NotifyYearPassed()
        {
            if (OnYearPassed != null)
                OnYearPassed();
        }
        public virtual void NotifyWeatherChanged(EnviroWeatherPreset type)
        {
            if (OnWeatherChanged != null)
                OnWeatherChanged(type);
        }
        public virtual void NotifyZoneWeatherChanged(EnviroWeatherPreset type, EnviroZone zone)
        {
            if (OnZoneWeatherChanged != null)
                OnZoneWeatherChanged(type, zone);
        }
        public virtual void NotifySeasonChanged(EnviroSeasons.Seasons season)
        {
            if (OnSeasonChanged != null)
                OnSeasonChanged(season);
        }
        public virtual void NotifyIsNight()
        {
            if (OnNightTime != null)
                OnNightTime();
        }
        public virtual void NotifyIsDay()
        {
            if (OnDayTime != null)
                OnDayTime();
        }
        public virtual void NotifyZoneChanged(EnviroZone zone)
        {
            if (OnZoneChanged != null)
                OnZoneChanged(zone);
        }

#endregion
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    #region InteriorZones
    
    /// <summary>
    /// Get and Set interior mode used by InteriorZones
    /// </summary>
    public bool interiorMode
    {
        get
        {
#if ENVIRO_HD && ENVIRO_LW
            if (currentEnviroSkyVersion == EnviroSkyVersion.HD)
                return EnviroSky.instance.interiorMode;
            else
                return EnviroSkyLite.instance.interiorMode;
#elif ENVIRO_HD
            return EnviroSky.instance.interiorMode;
#elif ENVIRO_LW
             return EnviroSkyLite.instance.interiorMode;
#else
            return false;
#endif
        }
        set
        {
#if ENVIRO_HD && ENVIRO_LW
            if (currentEnviroSkyVersion == EnviroSkyVersion.HD)
                EnviroSky.instance.interiorMode = value;
            else
                EnviroSkyLite.instance.interiorMode = value;
#elif ENVIRO_HD
            EnviroSky.instance.interiorMode = value;
#elif ENVIRO_LW
            EnviroSkyLite.instance.interiorMode = value;
#else
            return;
#endif
        }
    }
    
    /// <summary>
    /// Get and Set last Interior zone
    /// </summary>
    public EnviroInterior lastInteriorZone
    {
        get
        {
#if ENVIRO_HD && ENVIRO_LW
            if (currentEnviroSkyVersion == EnviroSkyVersion.HD)
                return EnviroSky.instance.lastInteriorZone;
            else
                return EnviroSkyLite.instance.lastInteriorZone;
#elif ENVIRO_HD
             return EnviroSky.instance.lastInteriorZone;
#elif ENVIRO_LW
            return EnviroSkyLite.instance.lastInteriorZone;
#else
         return null;
#endif
        }
        set
        {
#if ENVIRO_HD && ENVIRO_LW
            if (currentEnviroSkyVersion == EnviroSkyVersion.HD)
                EnviroSky.instance.lastInteriorZone = value;
            else
                EnviroSkyLite.instance.lastInteriorZone = value;
#elif ENVIRO_HD
              EnviroSky.instance.lastInteriorZone = value;
#elif ENVIRO_LW
                EnviroSkyLite.instance.lastInteriorZone = value;
#else
         return;
#endif
        }

    }
   
    
    
    
    
#endregion
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
}
