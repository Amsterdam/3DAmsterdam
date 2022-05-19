using Netherlands3D;
using Netherlands3D.Settings;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Rendering;

public class EnviromentSettings : MonoBehaviour
{
    public static EnviromentProfile activeEnviromentProfile;
    public static Light sun;

    [SerializeField]
    private int skyIndexMobile = 1;

    [SerializeField]
    private int skyIndexDesktop = 0;

    [SerializeField]
    private EnviromentProfile[] selectableProfiles;

	public EnviromentProfile[] SelectableProfiles { get => selectableProfiles; private set => selectableProfiles = value; }

    [SerializeField]
    private Material proceduralSkyMaterial;
    [SerializeField]
    private Material texturedSkyMaterial;

    [SerializeField]
    private Light directionalLightSun;

    [SerializeField]
    private float sunUpAmount = 1.0f;
    [SerializeField]
    private float sunDownAmount = -1.0f;

    [SerializeField]
    private Material intensityMaterialTrees;

    private static bool visualsUpdateRequired = false;

    private static bool useSkyboxForReflections = true;

    [SerializeField]
    private MeshRenderer sunGraphic;
    [SerializeField]
    private MeshRenderer sunHaloGraphic;

    public static EnviromentSettings Instance;

    private Coroutine loadingTextureProgress;

	private void Awake()
	{
        Instance = this;

        //Work on copies of our EnviromentSettings profiles
        for (int i = 0; i < selectableProfiles.Length; i++)
		{
            selectableProfiles[i] = Instantiate(selectableProfiles[i]);
        }

        if (directionalLightSun)
        {
            sun = directionalLightSun;
        }
        else
        {
            sun = FindObjectOfType<Light>();
        }
    }

	private void OnDisable()
	{
        //Clear loaded asset texture reference
        texturedSkyMaterial.SetTexture("_Tex", null);
    }

	public void ApplyEnviroment(bool mobile = false)
    {
        //Load up our enviroment based on platform (mobile should be lightweight)
        ApplyEnviromentProfile((mobile) ? skyIndexMobile : skyIndexDesktop);
        UpdateSunBasedVisuals();
    }

    public static void SetSunAngle(Vector3 angles)
    {
        sun.transform.localRotation = Quaternion.Euler(angles);
        visualsUpdateRequired = true;
    }

	private void Update()
	{
        if (visualsUpdateRequired)
            UpdateSunBasedVisuals();
    }

    public void ApplyEnviromentProfile(int profileIndex)
    {
        var profile = selectableProfiles[profileIndex];

        if (loadingTextureProgress != null) StopCoroutine(loadingTextureProgress);
        loadingTextureProgress = StartCoroutine(LoadEnviromentProfile(profile));

        ApplyReflectionSettings();
    }
    private IEnumerator LoadEnviromentProfile(EnviromentProfile enviromentProfile)
    {
        //Always destroy our prevous loaded skybox texture
        if(activeEnviromentProfile && activeEnviromentProfile.loadedTexture != null)
        {
            Destroy(activeEnviromentProfile.loadedTexture);
		}

        //Select our new profile
        activeEnviromentProfile = enviromentProfile;

        if (activeEnviromentProfile.isTexturedSky && activeEnviromentProfile.texturePath != "")
        {
            var skyboxTextureUrl = "";
            var splitTarget = Application.absoluteURL.LastIndexOf('/');
            if (splitTarget != -1)
            {
                var relativeDirectory = Application.absoluteURL.Substring(0, splitTarget);
                skyboxTextureUrl = $"{relativeDirectory}/{activeEnviromentProfile.texturePath}";
            }
#if UNITY_EDITOR
            skyboxTextureUrl = $"file:///{Application.dataPath}/WebGLTemplates/Netherlands3D/{activeEnviromentProfile.texturePath}";
 #endif

            Debug.Log("Loading skybox texture:" + skyboxTextureUrl);
            using (UnityWebRequest uwr = UnityWebRequestTexture.GetTexture(skyboxTextureUrl,true))
            {
                yield return uwr.SendWebRequest();
                if (uwr.result != UnityWebRequest.Result.Success)
                {
                    Debug.Log(uwr.error);
                }
                else
                {
                    // Get downloaded asset bundle
                    activeEnviromentProfile.loadedTexture = (Texture2D)DownloadHandlerTexture.GetContent(uwr);
                    texturedSkyMaterial.SetTexture("_Tex", activeEnviromentProfile.loadedTexture);
                    //texturedSkyMaterial.SetColor();
                    RenderSettings.skybox = texturedSkyMaterial;

                    SetReflections(useSkyboxForReflections);
                }
            }
        }
        else
        {
            SetReflections(useSkyboxForReflections);
            RenderSettings.skybox = proceduralSkyMaterial;
        }

        //Set the proper graphic for the representation of the Sun
        sunGraphic.enabled = activeEnviromentProfile.sunTexture;
        sunGraphic.material.SetTexture("_MainTexture", activeEnviromentProfile.sunTexture);

        sunHaloGraphic.enabled = activeEnviromentProfile.haloTexture;
        sunHaloGraphic.material.SetTexture("_MainTexture", activeEnviromentProfile.haloTexture);

        UpdateSunBasedVisuals();
    }

    public static void SetReflections(bool realtimeReflectionsAreOn = false)
    {
        useSkyboxForReflections = realtimeReflectionsAreOn;

        if (!activeEnviromentProfile) return;
        ApplyReflectionSettings();
    }

    private static void ApplyReflectionSettings()
    {
        if (!useSkyboxForReflections)
        {
            RenderSettings.defaultReflectionMode = DefaultReflectionMode.Custom;
        }
        else
        {
            RenderSettings.defaultReflectionMode = DefaultReflectionMode.Skybox;
        }
    }
    
    public void UpdateSunBasedVisuals()
    {
        //Reduce sun strength when we go down the horizon
        sun.intensity = Mathf.InverseLerp(sunDownAmount, sunUpAmount, Vector3.Dot(sun.transform.forward,Vector3.up));

        //Apply sunlight to tree darkness (who use a very simple unlit shader)
        intensityMaterialTrees.SetFloat("_Light", Mathf.Max(sun.intensity, 0.3f));

        //Change the fog and ambient color based on this intensity
        RenderSettings.fogColor = Color.Lerp(
            activeEnviromentProfile.fogColorNight,
            activeEnviromentProfile.fogColorDay,
            sun.intensity
        );

        //Sky colors
        RenderSettings.ambientSkyColor = Color.Lerp(
            activeEnviromentProfile.skyColorsNight[0],
            activeEnviromentProfile.skyColorsDay[0],
            sun.intensity
        );
        RenderSettings.ambientEquatorColor = Color.Lerp(
            activeEnviromentProfile.skyColorsNight[1],
            activeEnviromentProfile.skyColorsDay[1],
            sun.intensity
        );
        RenderSettings.ambientGroundColor = Color.Lerp(
            activeEnviromentProfile.skyColorsNight[2],
            activeEnviromentProfile.skyColorsDay[2],
            sun.intensity
        );

        if(activeEnviromentProfile.isTexturedSky)
            RenderSettings.skybox.SetFloat("_Exposure", sun.intensity);

        var sunHorizon = Mathf.Clamp(Mathf.InverseLerp(0.6f, 0.7f, sun.intensity),0.0f,1.0f);

        if(activeEnviromentProfile.haloTexture)
            sunHaloGraphic.material.SetColor("_BaseColor", Color.Lerp(Color.black, activeEnviromentProfile.sunHaloTextureTintColor * sunHorizon, sun.intensity));

        visualsUpdateRequired = false;
    }
}
