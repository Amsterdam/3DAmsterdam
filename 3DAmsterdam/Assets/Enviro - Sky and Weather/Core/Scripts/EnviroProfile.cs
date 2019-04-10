using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif


[Serializable]
public class EnviroQualitySettings
{
	[Range(0,1)][Tooltip("Modifies the amount of particles used in weather effects.")]
	public float GlobalParticleEmissionRates = 1f;
	[Tooltip("How often Enviro Growth Instances should be updated. Lower value = smoother growth and more frequent updates but more perfomance hungry!")]
	public float UpdateInterval = 0.5f; //Attention: lower value = smoother growth and more frequent updates but more perfomance hungry!
}
	
[Serializable]
public class EnviroSatelliteVariables
{
	[Tooltip("Name of this satellite")]
	public string name;
	[Tooltip("Prefab with model that get instantiated.")]
	public GameObject prefab = null;
	[Tooltip("This value will influence the satellite orbitpositions.")]
	public float orbit_X;
	[Tooltip("This value will influence the satellite orbitpositions.")]
	public float orbit_Y;
	[Tooltip("The speed of the satellites orbit.")]
	public float speed;
}

[Serializable]
public class EnviroSeasonSettings
{
    [Header("Spring")]
    [Tooltip("Start Day of Year for Spring")]
    [Range(0,366)]
    public int SpringStart = 60;
    [Tooltip("End Day of Year for Spring")]
    [Range(0, 366)]
    public int SpringEnd = 92;
    [Tooltip("Base Temperature in Spring")]
    public AnimationCurve springBaseTemperature = new AnimationCurve();

    [Header("Summer")]
    [Tooltip("Start Day of Year for Summer")]
    [Range(0, 366)]
    public int SummerStart = 93;
    [Tooltip("End Day of Year for Summer")]
    [Range(0, 366)]
    public int SummerEnd = 185;
    [Tooltip("Base Temperature in Summer")]
    public AnimationCurve summerBaseTemperature = new AnimationCurve();

    [Header("Autumn")]
    [Tooltip("Start Day of Year for Autumn")]
    [Range(0, 366)]
    public int AutumnStart = 186;
    [Tooltip("End Day of Year for Autumn")]
    [Range(0, 366)]
    public int AutumnEnd = 276;
    [Tooltip("Base Temperature in Autumn")]
    public AnimationCurve autumnBaseTemperature = new AnimationCurve();

    [Header("Winter")]
    [Tooltip("Start Day of Year for Winter")]
    [Range(0, 366)]
    public int WinterStart = 277;
    [Tooltip("End Day of Year for Winter")]
    [Range(0, 366)]
    public int WinterEnd = 59;
    [Tooltip("Base Temperature in Winter")]
    public AnimationCurve winterBaseTemperature = new AnimationCurve();
}

[Serializable]
public class EnviroWeatherSettings 
{
	[Header("Zones Setup:")]
	[Tooltip("Tag for zone triggers. Create and assign a tag to this gameObject")]
	public bool useTag;

	[Header("Weather Transition Settings:")]
	[Tooltip("Defines the speed of wetness will raise when it is raining.")]
	public float wetnessAccumulationSpeed = 0.05f;
	[Tooltip("Defines the speed of wetness will dry when it is not raining.")]
	public float wetnessDryingSpeed = 0.05f;
	[Tooltip("Defines the speed of snow will raise when it is snowing.")]
	public float snowAccumulationSpeed = 0.05f;
	[Tooltip("Defines the speed of snow will meld when it is not snowing.")]
	public float snowMeltingSpeed = 0.05f;

    [Tooltip("Defines the speed of clouds will change when weather conditions changed.")]
	public float cloudTransitionSpeed = 1f;
	[Tooltip("Defines the speed of fog will change when weather conditions changed.")]
	public float fogTransitionSpeed = 1f;
	[Tooltip("Defines the speed of particle effects will change when weather conditions changed.")]
	public float effectTransitionSpeed = 1f;
	[Tooltip("Defines the speed of sfx will fade in and out when weather conditions changed.")]
	public float audioTransitionSpeed = 0.1f;

    [Header("Lightning Effect:")]
    public GameObject lightningEffect;
    [Range(500f, 10000f)]
    public float lightningRange = 500f;
    [Range(500f, 5000f)]
    public float lightningHeight = 750f;

    [Header("Temperature:")]
    [Tooltip("Defines the speed of temperature changes.")]
    public float temperatureChangingSpeed = 10f;
}

[Serializable]
public class EnviroSkySettings 
{
	public enum SunAndMoonCalc
	{
		Simple,
		Realistic
	}

	public enum MoonPhases
	{
		Custom,
		Realistic
	}

	public enum SkyboxModi
	{
		Default,
        Simple,
		CustomSkybox,
		CustomColor
	}

    public enum SkyboxModiLW
    {
        Simple,
        CustomSkybox,
        CustomColor
    }

    [Header("Sky Mode:")]
	[Tooltip("Select if you want to use enviro skybox your custom material.")]
	public SkyboxModi skyboxMode;
    [Tooltip("Select if you want to use enviro skybox your custom material.")]
    public SkyboxModiLW skyboxModeLW;
    [Tooltip("If SkyboxMode == CustomSkybox : Assign your skybox material here!")]
	public Material customSkyboxMaterial;
	[Tooltip("If SkyboxMode == CustomColor : Select your sky color here!")]
	public Color customSkyboxColor;
    [Tooltip("Enable to render black skybox at ground level.")]
    public bool blackGroundMode = false;

	[Header("Scattering")]
	[Tooltip("Light Wavelength used for atmospheric scattering. Keep it near defaults for earthlike atmospheres, or change for alien or fantasy atmospheres for example.")]
	public Vector3 waveLength = new Vector3(540f,496f,437f);
	[Tooltip("Influence atmospheric scattering.")]
	public float rayleigh = 5.15f;
	[Tooltip("Sky turbidity. Particle in air. Influence atmospheric scattering.")]
	public float turbidity = 1f;
	[Tooltip("Influence scattering near sun.")]
	public float mie = 5f;
	[Tooltip("Influence scattering near sun.")]
	public float g = 0.8f;
	[Tooltip("Intensity gradient for atmospheric scattering. Influence atmospheric scattering based on current sun altitude.")]
	public AnimationCurve scatteringCurve = new AnimationCurve();
	[Tooltip("Color gradient for atmospheric scattering. Influence atmospheric scattering based on current sun altitude.")]
	public Gradient scatteringColor;

	[Header("Sun")]
	public SunAndMoonCalc sunAndMoonPosition = SunAndMoonCalc.Realistic;
	[Tooltip("Intensity of Sun Influence Scale and Dropoff of sundisk.")]
	public float sunIntensity = 100f;
	[Tooltip("Scale of rendered sundisk.")]
	public float sunDiskScale = 20f;
	[Tooltip("Intenisty of rendered sundisk.")]
	public float sunDiskIntensity = 3f;
	[Tooltip("Color gradient for sundisk. Influence sundisk color based on current sun altitude")]
	public Gradient sunDiskColor;

    [Tooltip("Top color of simple skybox.")]
    public Gradient simpleSkyColor;
    [Tooltip("Horizon color of simple skybox.")]
    public Gradient simpleHorizonColor;
    [Tooltip("Sun color of simple skybox.")]
    public Gradient simpleSunColor;
    [Tooltip("Size of sun in simple skybox mode.")]
    public AnimationCurve simpleSunDiskSize = new AnimationCurve();

    [Header("Moon")]
    [Tooltip("Whether to render the moon.")]
    public bool renderMoon = true;
    [Tooltip("The Moon phase mode. Custom = for customizable phase.")]
    public MoonPhases moonPhaseMode = MoonPhases.Realistic;
	[Tooltip("The Moon texture.")]
	public Texture moonTexture;
    [Tooltip("The Moon's Glow texture.")]
    public Texture glowTexture;
    [Tooltip("The color of the moon")]
	public Color moonColor;
	[Range(0f,5f)][Tooltip("Brightness of the moon.")]
	public float moonBrightness = 1f;
	[Range(0f,20f)][Tooltip("Size of the moon.")]
	public float moonSize = 10f;
    [Range(0f, 20f)][Tooltip("Size of the moon glowing effect.")]
    public float glowSize = 10f;
    [Tooltip("Glow around moon.")]
	public AnimationCurve moonGlow = new AnimationCurve();
	[Tooltip("Glow color around moon.")]
	public Color moonGlowColor;
	[Tooltip("Start moon phase when using custom phase mode.(-1f - 1f)")]
	[Range(-1f,1f)]
	public float startMoonPhase = 0.0f;

	[Header("Sky Color Corrections")]
	[Tooltip("Higher values = brighter sky.")]
	public AnimationCurve skyLuminence = new AnimationCurve();
	[Tooltip("Higher values = stronger colors applied BEFORE clouds rendered!")]
	public AnimationCurve skyColorPower = new AnimationCurve();

	[Header("Tonemapping - LDR")]
	[Tooltip("Tonemapping when using LDR")]
	public float skyExposure = 1.5f;

	[Header("Stars")]
	[Tooltip("A cubemap for night sky.")]
	public Cubemap starsCubeMap;
	[Tooltip("Intensity of stars based on time of day.")]
	public AnimationCurve starsIntensity = new AnimationCurve();
    [Tooltip("Stars Twinkling Speed")]
    [Range(0f, 10f)]
    public float starsTwinklingRate = 1f;

    [Header("Galaxy")]
    [Tooltip("A cubemap for night galaxy.")]
    public Cubemap galaxyCubeMap;
    [Tooltip("Intensity of galaxy based on time of day.")]
    public AnimationCurve galaxyIntensity = new AnimationCurve();

    [Header("Sky Dithering")]
    public bool dithering = true;
}

[Serializable]
public class EnviroSatellitesSettings 
{
	[Tooltip("List of satellites.")]
	public List<EnviroSatellite> additionalSatellites = new List<EnviroSatellite>();
}

[Serializable] 
public class EnviroLightSettings // All Lightning Variables
{
	[Header("Direct")]
	[Tooltip("Color gradient for sun and moon light based on sun position in sky.")]
	public Gradient LightColor;
	[Tooltip("Direct light sun intensity based on sun position in sky")]
	public AnimationCurve directLightSunIntensity = new AnimationCurve();
	[Tooltip("Direct light moon intensity based on moon position in sky")]
	public AnimationCurve directLightMoonIntensity = new AnimationCurve();
	[Tooltip("Realtime shadow strength of the directional light.")]
	public AnimationCurve shadowIntensity = new AnimationCurve();
    [Tooltip("Direct lighting y-offset.")]
    [Range(0f,5000f)]
    public float directLightAngleOffset = 0f;
    [Header("Ambient")]
	[Tooltip("Ambient Rendering Mode.")]
	public UnityEngine.Rendering.AmbientMode ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
	[Tooltip("Ambientlight intensity based on sun position in sky.")]
	public AnimationCurve ambientIntensity = new AnimationCurve();
	[Tooltip("Ambientlight sky color based on sun position in sky.")]
	public Gradient ambientSkyColor;
	[Tooltip("Ambientlight Equator color based on sun position in sky.")]
	public Gradient ambientEquatorColor;
	[Tooltip("Ambientlight Ground color based on sun position in sky.")]
	public Gradient ambientGroundColor;

	[Header("Global Reflections")]
    [Tooltip("Enable/disable enviro reflection probe..")]
    public bool globalReflections = true;

    [Tooltip("Enable/disable enviro reflection probe updates based on gametime changes..")]
    public bool globalReflectionsUpdateOnGameTime = true;
    [Tooltip("Enable/disable enviro reflection probe updates based on transform position changes..")]
    public bool globalReflectionsUpdateOnPosition = true;
    [Tooltip("Reflection probe intensity.")]
    [Range(0f, 2f)]
    public float globalReflectionsIntensity = 0.5f;
    [Tooltip("Reflection probe update rate.")]
    public float globalReflectionsUpdate = 0.025f;
    [Tooltip("Reflection probe intensity.")]
    [Range(0.1f,10f)]
    public float globalReflectionsScale = 1f;
}

[Serializable]
public class EnviroDistanceBlurSettings
{
    public bool antiFlicker = true;
    public bool highQuality = true;
    public float radius = 7f;
}


[Serializable]
public class EnviroFogSettings 
{
	[Header("Mode")]
	[Tooltip("Unity's fog mode.")]
	public FogMode Fogmode = FogMode.Exponential;
    [Tooltip("Simple fog = just plain color without scattering.")]
    public bool useSimpleFog = false;
    [Header("Distance Fog")]
	[Tooltip("Use distance fog?")]
	public bool distanceFog = true;
	[Tooltip("Use radial distance fog?")]
	public bool useRadialDistance = true;
	[Tooltip("The distance where fog starts.")]
	public float startDistance = 0.0f;
	[Range(0f,10f)][Tooltip("The intensity of distance fog.")]
	public float distanceFogIntensity = 4.0f;
	[Range(0f,1f)][Tooltip("The maximum density of fog.")]
	public float maximumFogDensity = 0.9f;
	[Header("Height Fog")]
	[Tooltip("Use heightbased fog?")]
	public bool heightFog = true;
	[Tooltip("The height of heightbased fog.")]
	public float height = 90.0f;
	[Range(0f,1f)][Tooltip("The intensity of heightbased fog.")]
	public float heightFogIntensity = 1f;
	[HideInInspector]
	public float heightDensity = 0.15f;
    [Header("Height Fog Noise")]
    [Range(0f, 1f)]
    [Tooltip("The noise intensity of height based fog.")]
    public float noiseIntensity = 1f;
    [Tooltip("The noise intensity offset of height based fog.")]
    [Range(0f, 1f)]
    public float noiseIntensityOffset = 0.3f;
    [Range(0f, 0.1f)]
    [Tooltip("The noise scaling of height based fog.")]
    public float noiseScale = 0.001f;
    [Tooltip("The speed and direction of height based fog.")]
    public Vector2 noiseVelocity = new Vector2(3f, 1.5f);
   // [Header("Fog Scattering")]
	[Tooltip("Influence scattering near sun.")]
	public float mie = 5f;
	[Tooltip("Influence scattering near sun.")]
	public float g = 5f;

    [Header("Fog Dithering")]
    public bool dithering = true;

    [Tooltip("Color gradient for Top Fog")]
    public Gradient simpleFogColor;

    [HideInInspector]
	public float skyFogIntensity = 1f;
}

[Serializable]
public class EnviroVolumeLightingSettings
{
#if ENVIRO_HD
    [Tooltip("Downsampling of volume light rendering.")]
    public EnviroSkyRendering.VolumtericResolution Resolution = EnviroSkyRendering.VolumtericResolution.Quarter;
#endif
    [Tooltip("Activate or deactivate directional volume light rendering.")]
    public bool dirVolumeLighting = true;
    [Header("Quality")]
    [Range(1, 64)]
    public int SampleCount = 8;
    [Header("Light Settings")]
    public AnimationCurve ScatteringCoef = new AnimationCurve();
    [Range(0.0f, 0.1f)]
    public float ExtinctionCoef = 0.05f;
    [Range(0.0f, 0.999f)]
    public float Anistropy = 0.1f;
    public float MaxRayLength = 10.0f;
    [Header("3D Noise")]
    [Tooltip("Use 3D noise for directional lighting. Attention: Expensive operation for directional lights with high sample count!")]
    public bool directLightNoise = false;
    [Range(0f, 1f)]
    [Tooltip("The noise intensity volume lighting.")]
    public float noiseIntensity = 1f;
    [Tooltip("The noise intensity offset of volume lighting.")]
    [Range(0f, 1f)]
    public float noiseIntensityOffset = 0.3f;
    [Range(0f, 0.1f)]
    [Tooltip("The noise scaling of volume lighting.")]
    public float noiseScale = 0.001f;
    [Tooltip("The speed and direction of volume lighting.")]
    public Vector2 noiseVelocity = new Vector2(3f, 1.5f);
}

[Serializable]
public class EnviroLightShaftsSettings 
{
	[Header("Quality Settings")]
	[Tooltip("Lightshafts resolution quality setting.")]
	public EnviroPostProcessing.SunShaftsResolution resolution = EnviroPostProcessing.SunShaftsResolution.Normal;
	[Tooltip("Lightshafts blur mode.")]
	public EnviroPostProcessing.ShaftsScreenBlendMode screenBlendMode = EnviroPostProcessing.ShaftsScreenBlendMode.Screen;
	[Tooltip("Use cameras depth to hide lightshafts?")]
	public bool useDepthTexture = true;

	[Header("Intensity Settings")]
	[Tooltip("Color gradient for lightshafts based on sun position.")]
	public Gradient lightShaftsColorSun;
	[Tooltip("Color gradient for lightshafts based on moon position.")]
	public Gradient lightShaftsColorMoon;
	[Tooltip("Treshhold gradient for lightshafts based on sun position. This will influence lightshafts intensity!")]
	public Gradient thresholdColorSun;
	[Tooltip("Treshhold gradient for lightshafts based on moon position. This will influence lightshafts intensity!")]
	public Gradient thresholdColorMoon;
	[Tooltip("Radius of blurring applied.")]
	public float blurRadius = 6f;
	[Tooltip("Global Lightshafts intensity.")]
	public float intensity = 0.6f;
	[Tooltip("Lightshafts maximum radius.")]
	public float maxRadius = 10f;
}

[Serializable]
public class EnviroAudioSettings 
{
	[Tooltip("A list of all possible thunder audio effects.")]
	public List<AudioClip> ThunderSFX = new List<AudioClip> ();
}

[Serializable]
public class EnviroCloudSettings
{
	public enum CloudDetailQuality
	{
		Low,
		High
	}

    public enum CloudQuality
    {
        Lowest,
        Low,
        Medium,
        High,
        Ultra,
        VR_Low,
        VR_Medium,
        VR_High,
        Custom
    }

    public enum FlatCloudResolution
    {
        R512,
        R1024,
        R2048,
        R4096,
    }

    // Reprojection
    public enum ReprojectionPixelSize
    {
        Off,
        Low,
        Medium,
        High
    }

    public ReprojectionPixelSize reprojectionPixelSize;

    [Tooltip("Choose a clouds rendering quality or setup your own when choosing custom.")]
    public CloudQuality cloudsQuality;
    [Range(10000f,486000f)][Header("Clouds Scale Settings")]
    [Tooltip("Clouds world scale. This settings will influece rendering of clouds at horizon.")]
    public float cloudsWorldScale = 113081f;
    [Tooltip("Clouds start height.")]
    public float bottomCloudHeight = 3000f;
    [Tooltip("Clouds end height.")]
    public float topCloudHeight = 7000f;

	[Header("Clouds Wind Animation")]
	public bool useWindZoneDirection;
	[Range(-1f,1f)][Tooltip("Time scale / wind animation speed of clouds.")]
	public float cloudsTimeScale = 1f;
	[Range(0f,1f)][Tooltip("Global clouds wind speed modificator.")]
	public float cloudsWindStrengthModificator = 0.001f;
	[Range(-1f,1f)][Tooltip("Global clouds wind direction X axes.")]
	public float cloudsWindDirectionX = 1f;
	[Range(-1f,1f)][Tooltip("Global clouds wind direction Y axes.")]
	public float cloudsWindDirectionY = 1f;

	[Header("Cloud Rendering")]
	[Range(32,256)]
    [Tooltip("Number of raymarching samples.")]
    public int raymarchSteps = 150;
    [Tooltip("Increase performance by using less steps when clouds are hidden by objects.")]
    [Range(0.1f, 1f)]
    public float stepsInDepthModificator = 0.75f;
    [Range(1,16)]
    [Tooltip("Downsampling of clouds rendering. 1 = full res, 2 = half Res, ...")]
    public int cloudsRenderResolution = 4;

	[Header("Clouds Lighting")]
	public float hgPhase = 0.5f;
    public float primaryAttenuation = 3f;
    public float secondaryAttenuation = 3f;
    [Tooltip("Global Color for volume clouds based sun positon.")]
	public Gradient volumeCloudsColor;
	[Tooltip("Global Color for clouds based moon positon.")]
	public Gradient volumeCloudsMoonColor;
	[Tooltip("Direct Light intensity for clouds based on time of day.")]
	public AnimationCurve directLightIntensity = new AnimationCurve();
	[Tooltip("Direct Light intensity for clouds based on time of day.")]
	public AnimationCurve ambientLightIntensity = new AnimationCurve();

    [Header("Tonemapping")]
	[Tooltip("Use color tonemapping?")]
	public bool tonemapping = false;
	[Tooltip("Tonemapping exposure")]
	public float cloudsExposure = 1f;
    [Tooltip("LOD Distance for using lower res 3d texture for far away clouds. ")]
    [Range(0f,1f)]
    public float lodDistance = 0.5f;
    [Header("Weather Map")]
    [Tooltip("Tiling of the generated weather map.")]
    public int weatherMapTiling = 5;
    [Tooltip("Option to add own weather map. Red Channel = Coverage, Blue = Clouds Height")]
    public Texture2D customWeatherMap;
    [Tooltip("Weathermap sampling offset.")]
    public Vector2 locationOffset;
    [Range(0f,1f)]
    [Tooltip("Weathermap animation speed.")]
    public float weatherAnimSpeedScale = 0.33f;

	[Header("Clouds Modelling")]
	[Tooltip("The UV scale of base noise. High Values = Low performance!")]
	[Range(2f,100f)]
	public float baseNoiseUV = 20f;
	[Tooltip("The UV scale of detail noise. High Values = Low performance!")]
	[Range(2f,100f)]
	public float detailNoiseUV = 50f;
	[Tooltip("Resolution of Detail Noise Texture.")]
	public CloudDetailQuality detailQuality;

	[Header("Global Clouds Control")]
	[Range(0f,2f)]
	public float globalCloudCoverage = 1f;

	[Tooltip("Texture for cirrus clouds.")]
	public Texture cirrusCloudsTexture;
	[Tooltip("Global Color for flat clouds based sun positon.")]
	public Gradient cirrusCloudsColor;
	[Range(5f,15f)][Tooltip("Flat Clouds Altitude")]
	public float cirrusCloudsAltitude = 10f;

	[Tooltip("Texture for flat procedural clouds.")]
	public Texture flatCloudsNoiseTexture;
    [Tooltip("Resolution of generated flat clouds texture.")]
    public FlatCloudResolution flatCloudsResolution = FlatCloudResolution.R2048;
    [Tooltip("Global Color for flat clouds based sun positon.")]
	public Gradient flatCloudsColor;
    [Tooltip("Scale/Tiling of flat clouds.")]
    public float flatCloudsScale = 2f;
    [Range(1, 12)]
    [Tooltip("Flat Clouds texture generation iterations.")]
    public int flatCloudsNoiseOctaves = 6;
    [Range(30f,100f)][Tooltip("Flat Clouds Altitude")]
	public float flatCloudsAltitude = 70f;
    [Range(0.01f, 1f)]
    [Tooltip("Flat Clouds morphing animation speed.")]
    public float flatCloudsMorphingSpeed = 0.2f;

    [Tooltip("Clouds Shadowcast Intensity. 0 = disabled")]
    [Range(0f, 1f)]
    public float shadowIntensity = 0.0f;
    [Tooltip("Size of the shadow cookie.")]
    [Range(1000, 10000)]
    public int shadowCookieSize = 10000;

    public EnviroParticleClouds ParticleCloudsLayer1 = new EnviroParticleClouds();
    public EnviroParticleClouds ParticleCloudsLayer2 = new EnviroParticleClouds();
}


[Serializable]
public class EnviroParticleClouds
{
    [Tooltip("Particle clouds relative height.")][Range(0f,0.2f)]
    public float height = 0.05f;
    [Tooltip("Global Color for flat clouds based sun positon.")]
    public Gradient particleCloudsColor;
}


    [System.Serializable]
public class EnviroProfile : ScriptableObject 
{
    
	public string version;
	public EnviroLightSettings lightSettings = new EnviroLightSettings();
    public EnviroVolumeLightingSettings volumeLightSettings = new EnviroVolumeLightingSettings();
    public EnviroDistanceBlurSettings distanceBlurSettings = new EnviroDistanceBlurSettings();
    public EnviroSkySettings skySettings = new EnviroSkySettings();
	public EnviroCloudSettings cloudsSettings = new EnviroCloudSettings();
	public EnviroWeatherSettings weatherSettings = new EnviroWeatherSettings();
	public EnviroFogSettings fogSettings = new EnviroFogSettings();
	public EnviroLightShaftsSettings lightshaftsSettings = new EnviroLightShaftsSettings();
	public EnviroSeasonSettings seasonsSettings = new EnviroSeasonSettings();
	public EnviroAudioSettings audioSettings = new EnviroAudioSettings();
	public EnviroSatellitesSettings satelliteSettings = new EnviroSatellitesSettings();
	public EnviroQualitySettings qualitySettings = new EnviroQualitySettings();

	// Inspector categories
	public enum settingsMode
	{
		Lighting,
		Sky,
		Weather,
		Clouds,
		Fog,
        VolumeLighting,
		Lightshafts,
        DistanceBlur,
		Season,
		Satellites,
		Audio,
		Quality
	}

    public enum settingsModeLW
    {
        Lighting,
        Sky,
        Weather,
        Clouds,
        Fog,
        Lightshafts,
        Season,
        Satellites,
        Audio,
        Quality
    }

    [HideInInspector]public settingsMode viewMode;
    [HideInInspector]public settingsModeLW viewModeLW;
    [HideInInspector]public bool showPlayerSetup = true;
	[HideInInspector]public bool showRenderingSetup = false;
	[HideInInspector]public bool showComponentsSetup = false;
	[HideInInspector]public bool showTimeUI = false;
	[HideInInspector]public bool showWeatherUI = false;
	[HideInInspector]public bool showAudioUI = false;
	[HideInInspector]public bool showEffectsUI = false;
	[HideInInspector]public bool modified;
}

public static class EnviroProfileCreation {
	#if UNITY_EDITOR
	[MenuItem("Assets/Create/Enviro/Profile")]
	public static EnviroProfile CreateNewEnviroProfile()
	{
		EnviroProfile profile = ScriptableObject.CreateInstance<EnviroProfile>();

		profile.version = "2.1.0";
		// Setup new profile with default settings
		SetupDefaults (profile);

		// Create and save the new profile with unique name
		string path = AssetDatabase.GetAssetPath (Selection.activeObject);
		if (path == "") 
		{
			path = "Assets/Enviro - Sky and Weather/Core/Profiles";
		} 
		string assetPathAndName = AssetDatabase.GenerateUniqueAssetPath (path + "/New " + "Enviro Profile" + ".asset");
		AssetDatabase.CreateAsset (profile, assetPathAndName);
		AssetDatabase.SaveAssets ();
		AssetDatabase.Refresh();
		//EditorUtility.FocusProjectWindow ();
		//Selection.activeObject = profile;
		return profile;
	}
	#endif

	public static void SetupDefaults (EnviroProfile profile)
	{
		List<Color> gradientColors = new List<Color> ();
		List<float> gradientTimes = new List<float> ();

		//Light Color
		gradientColors.Add (GetColor ("#4C5570"));gradientTimes.Add (0f);
		gradientColors.Add (GetColor ("#4C5570"));gradientTimes.Add (0.46f);
		gradientColors.Add (GetColor ("#C98842"));gradientTimes.Add (0.51f);
		gradientColors.Add (GetColor ("#EAC8A4"));gradientTimes.Add (0.56f);
		gradientColors.Add (GetColor ("#EADCCE"));gradientTimes.Add (1f);
		profile.lightSettings.LightColor = CreateGradient (gradientColors, gradientTimes);
		gradientColors = new List<Color> ();gradientTimes = new List<float> ();
		//Light Sun Intensity
		profile.lightSettings.directLightSunIntensity.AddKey (CreateKey (0f, 0f));
		profile.lightSettings.directLightSunIntensity.AddKey (CreateKey (0f, 0.42f));
		profile.lightSettings.directLightSunIntensity.AddKey (CreateKey (0.75f, 0.5f, 5f, 5f));
		profile.lightSettings.directLightSunIntensity.AddKey (CreateKey (1.5f, 1f));
		//Light Moon Intensity
		profile.lightSettings.directLightMoonIntensity.AddKey (CreateKey (0.01f, 0f));
		profile.lightSettings.directLightMoonIntensity.AddKey (CreateKey (0.01f, 0.42f));
		profile.lightSettings.directLightMoonIntensity.AddKey (CreateKey (0.6f, 0.5f, 2f, 2f));
		profile.lightSettings.directLightMoonIntensity.AddKey (CreateKey (1.0f, 1f));
        //Shadow Intensity
        profile.lightSettings.shadowIntensity.AddKey(CreateKey(1.0f, 0f));
        profile.lightSettings.shadowIntensity.AddKey(CreateKey(1.0f, 1f));

        //Ambient Intensity
        profile.lightSettings.ambientIntensity.AddKey(CreateKey(0.75f,0f));
		profile.lightSettings.ambientIntensity.AddKey(CreateKey(0.75f,1f));
		//Ambient SkyColor
		gradientColors.Add (GetColor ("#4C5570"));gradientTimes.Add (0f);
		gradientColors.Add (GetColor ("#4C5570"));gradientTimes.Add (0.46f);
		gradientColors.Add (GetColor ("#C98842"));gradientTimes.Add (0.51f);
		gradientColors.Add (GetColor ("#99B2C3"));gradientTimes.Add (0.57f);
		gradientColors.Add (GetColor ("#99B2C3"));gradientTimes.Add (1f);
		profile.lightSettings.ambientSkyColor = CreateGradient (gradientColors, gradientTimes);
		gradientColors = new List<Color> ();gradientTimes = new List<float> ();
		//Ambient EquatorColor
		profile.lightSettings.ambientEquatorColor = CreateGradient(GetColor("#2E3344"),0f,GetColor("#414852"),1f);
		//Ambient GroundColor
		profile.lightSettings.ambientGroundColor = CreateGradient(GetColor("#272B39"),0f,GetColor("#3E3631"),1f);
		//ScatteringIntensity
		profile.skySettings.scatteringCurve.AddKey(CreateKey(-25f,0f));
		profile.skySettings.scatteringCurve.AddKey(CreateKey (-10f, 0.5f,55f,55f));
		profile.skySettings.scatteringCurve.AddKey(CreateKey (6.5f, 0.52f,35f,35f));
		profile.skySettings.scatteringCurve.AddKey(CreateKey(11f,1f));

        profile.skySettings.simpleSunDiskSize = new AnimationCurve();
        profile.skySettings.simpleSunDiskSize.AddKey(CreateKey(0.015f, 0.0f));
        profile.skySettings.simpleSunDiskSize.AddKey(CreateKey(0.035f, 0.37f,-0.1f, -0.1f));
        profile.skySettings.simpleSunDiskSize.AddKey(CreateKey(0.015f, 1f));

        // Scattering Color Tint
        gradientColors.Add (GetColor ("#8492C8"));gradientTimes.Add (0f);
		gradientColors.Add (GetColor ("#8492C8"));gradientTimes.Add (0.45f);
		gradientColors.Add (GetColor ("#FFB69C"));gradientTimes.Add (0.527f);
		gradientColors.Add (GetColor ("#D2D2D2"));gradientTimes.Add (0.75f);
		gradientColors.Add (GetColor ("#D2D2D2"));gradientTimes.Add (1f);
		profile.skySettings.scatteringColor = CreateGradient (gradientColors, gradientTimes);
		gradientColors = new List<Color> ();gradientTimes = new List<float> ();
		//Sun Disk Color
		gradientColors.Add (GetColor ("#0A0300"));gradientTimes.Add (0f);
		gradientColors.Add (GetColor ("#FF6211"));gradientTimes.Add (0.45f);
		gradientColors.Add (GetColor ("#FF6917"));gradientTimes.Add (0.55f);
		gradientColors.Add (GetColor ("#FFE2CB"));gradientTimes.Add (0.75f);
		gradientColors.Add (GetColor ("#FFFFFF"));gradientTimes.Add (1f);
		profile.skySettings.sunDiskColor = CreateGradient (gradientColors, gradientTimes);
		gradientColors = new List<Color> ();gradientTimes = new List<float> ();
		//Moon Glow
		profile.skySettings.moonGlow.AddKey(CreateKey (1f, 0f));
		profile.skySettings.moonGlow.AddKey(CreateKey (0f, 0.65f));
		profile.skySettings.moonGlow.AddKey(CreateKey (0f, 1f));
		//Sky Luminance
		profile.skySettings.skyLuminence.AddKey(CreateKey(0f,0f));
		profile.skySettings.skyLuminence.AddKey(CreateKey(0.15f,0.5f));
		profile.skySettings.skyLuminence.AddKey(CreateKey(0.105f,0.62f));
		profile.skySettings.skyLuminence.AddKey(CreateKey(0.1f,1f));
		//Sky Color Power
		profile.skySettings.skyColorPower.AddKey(CreateKey(1.5f,0f));
		profile.skySettings.skyColorPower.AddKey(CreateKey(1.25f,1f));
		//Stars Intensity
		profile.skySettings.starsIntensity.AddKey(CreateKey(0.3f,0f));
		profile.skySettings.starsIntensity.AddKey(CreateKey(0.015f,0.5f));
		profile.skySettings.starsIntensity.AddKey(CreateKey(0.0f,0.6f));
		profile.skySettings.starsIntensity.AddKey(CreateKey(0.0f,1f));
		// Get Texture
		profile.skySettings.moonTexture = GetAssetTexture ("tex_enviro_moon_standard");
		profile.skySettings.starsCubeMap = GetAssetCubemap ("cube_enviro_stars");
        //Galaxy
        profile.skySettings.galaxyIntensity.AddKey(CreateKey(1f, 0f));
        profile.skySettings.galaxyIntensity.AddKey(CreateKey(0.015f, 0.5f));
        profile.skySettings.galaxyIntensity.AddKey(CreateKey(0.0f, 0.6f));
        profile.skySettings.galaxyIntensity.AddKey(CreateKey(0.0f, 1f));
        profile.skySettings.galaxyCubeMap = GetAssetCubemap("cube_enviro_galaxy");

        //Simple Sky Color
        gradientColors.Add(GetColor("#0F1013")); gradientTimes.Add(0f);
        gradientColors.Add(GetColor("#272A35")); gradientTimes.Add(0.465f);
        gradientColors.Add(GetColor("#277BA5")); gradientTimes.Add(0.50f);
        gradientColors.Add(GetColor("#7CA0BA")); gradientTimes.Add(0.56f);
        gradientColors.Add(GetColor("#5687B8")); gradientTimes.Add(1f);

        List<float> alpha = new List<float>();
        List<float> alphaTimes = new List<float>();

        alpha.Add(0f); alphaTimes.Add(0f);
        alpha.Add(0.5f); alphaTimes.Add(0.47f);
        alpha.Add(1f); alphaTimes.Add(1f);
        profile.skySettings.simpleSkyColor = CreateGradient(gradientColors, gradientTimes, alpha, alphaTimes);
        gradientColors = new List<Color>(); gradientTimes = new List<float>();

        //Simple Horizon Color
        gradientColors.Add(GetColor("#0F1013")); gradientTimes.Add(0f);
        gradientColors.Add(GetColor("#272A35")); gradientTimes.Add(0.46f);
        gradientColors.Add(GetColor("#DD8F3B")); gradientTimes.Add(0.506f);
        gradientColors.Add(GetColor("#DADADA")); gradientTimes.Add(0.603f);
        gradientColors.Add(GetColor("#FFFFFF")); gradientTimes.Add(1f);
        profile.skySettings.simpleHorizonColor = CreateGradient(gradientColors, gradientTimes);
        gradientColors = new List<Color>(); gradientTimes = new List<float>();

        //Simple Sun Color
        gradientColors.Add(GetColor("#000000")); gradientTimes.Add(0f);
        gradientColors.Add(GetColor("#FF6340")); gradientTimes.Add(0.46f);
        gradientColors.Add(GetColor("#FF7308")); gradientTimes.Add(0.506f);
        gradientColors.Add(GetColor("#E3C29E")); gradientTimes.Add(0.56f);
        gradientColors.Add(GetColor("#FFE9D4")); gradientTimes.Add(1f);
        profile.skySettings.simpleSunColor = CreateGradient(gradientColors, gradientTimes);
        gradientColors = new List<Color>(); gradientTimes = new List<float>();

        //Simple Fog
        gradientColors.Add(GetColor("#101217")); gradientTimes.Add(0f);
        gradientColors.Add(GetColor("#4C5570")); gradientTimes.Add(0.46f);
        gradientColors.Add(GetColor("#D8A269")); gradientTimes.Add(0.50f);
        gradientColors.Add(GetColor("#C7D7E3")); gradientTimes.Add(0.57f);
        gradientColors.Add(GetColor("#DBE7F0")); gradientTimes.Add(1f);
        profile.fogSettings.simpleFogColor = CreateGradient(gradientColors, gradientTimes);
        gradientColors = new List<Color>(); gradientTimes = new List<float>();

    //Clouds:
        Texture clouds1 = GetAssetTexture("tex_enviro_noise");
		Texture clouds2 = GetAssetTexture("tex_enviro_cirrus");
		if (clouds1 == null || clouds2 == null)
			Debug.Log ("Cannot find cloud textures");
		profile.cloudsSettings.flatCloudsNoiseTexture = clouds1;
		profile.cloudsSettings.cirrusCloudsTexture = clouds2;

		//Clouds Moon Highlight
		profile.cloudsSettings.volumeCloudsMoonColor = CreateGradient(GetColor("#232228"),0f,GetColor("#B6BCDC"),1f);
		
        //Clouds Sky Color
		gradientColors.Add (GetColor ("#17171A"));gradientTimes.Add (0f);
		gradientColors.Add (GetColor ("#17171A"));gradientTimes.Add (0.455f);
		gradientColors.Add (GetColor ("#3D3D3B"));gradientTimes.Add (0.48f);
		gradientColors.Add (GetColor ("#EEB279"));gradientTimes.Add (0.53f);
		gradientColors.Add (GetColor ("#EEF0FF"));gradientTimes.Add (0.6f);
		gradientColors.Add (GetColor ("#ECEEFF"));gradientTimes.Add (1f);
		profile.cloudsSettings.cirrusCloudsColor = CreateGradient (gradientColors, gradientTimes);
		gradientColors = new List<Color> ();gradientTimes = new List<float> ();

		gradientColors.Add (GetColor ("#17171A"));gradientTimes.Add (0f);
		gradientColors.Add (GetColor ("#17171A"));gradientTimes.Add (0.455f);
		gradientColors.Add (GetColor ("#3D3D3B"));gradientTimes.Add (0.48f);
		gradientColors.Add (GetColor ("#EEB279"));gradientTimes.Add (0.53f);
		gradientColors.Add (GetColor ("#EEF0FF"));gradientTimes.Add (0.6f);
		gradientColors.Add (GetColor ("#ECEEFF"));gradientTimes.Add (1f);
		profile.cloudsSettings.flatCloudsColor = CreateGradient (gradientColors, gradientTimes);
		gradientColors = new List<Color> ();gradientTimes = new List<float> ();

		//Clouds Sun Color
		gradientColors.Add (GetColor ("#17171A"));gradientTimes.Add (0f);
		gradientColors.Add (GetColor ("#17171A"));gradientTimes.Add (0.455f);
		gradientColors.Add (GetColor ("#3D3D3B"));gradientTimes.Add (0.48f);
		gradientColors.Add (GetColor ("#EEB279"));gradientTimes.Add (0.53f);
		gradientColors.Add (GetColor ("#CECECE"));gradientTimes.Add (0.58f);
		gradientColors.Add (GetColor ("#CECECE"));gradientTimes.Add (1f);
		profile.cloudsSettings.volumeCloudsColor = CreateGradient (gradientColors, gradientTimes);
		gradientColors = new List<Color> ();gradientTimes = new List<float> ();
		
        //clouds light intensity
		profile.cloudsSettings.directLightIntensity = new AnimationCurve();
		profile.cloudsSettings.directLightIntensity.AddKey(CreateKey(0.02f, 0f));
		profile.cloudsSettings.directLightIntensity.AddKey(CreateKey(0.15f, 0.495f));
        profile.cloudsSettings.directLightIntensity.AddKey(CreateKey(0.15f, 1f));

        //clouds light intensity
        profile.cloudsSettings.ambientLightIntensity = new AnimationCurve();
		profile.cloudsSettings.ambientLightIntensity.AddKey(CreateKey(0.017f, 0f));
		profile.cloudsSettings.ambientLightIntensity.AddKey(CreateKey(0.0f, 0.46f));
		profile.cloudsSettings.ambientLightIntensity.AddKey(CreateKey(0.35f, 0.617f));
        profile.cloudsSettings.ambientLightIntensity.AddKey(CreateKey(0.32f, 1f));

        //Particle Clouds Color
        gradientColors.Add(GetColor("#24272D")); gradientTimes.Add(0f);
        gradientColors.Add(GetColor("#2E3038")); gradientTimes.Add(0.46f);
        gradientColors.Add(GetColor("#544E46")); gradientTimes.Add(0.51f);
        gradientColors.Add(GetColor("#B7BABC")); gradientTimes.Add(0.56f);
        gradientColors.Add(GetColor("#D2D2D2")); gradientTimes.Add(1f);
        profile.cloudsSettings.ParticleCloudsLayer1.particleCloudsColor = CreateGradient(gradientColors, gradientTimes);
        profile.cloudsSettings.ParticleCloudsLayer2.particleCloudsColor = CreateGradient(gradientColors, gradientTimes);
        gradientColors = new List<Color>(); gradientTimes = new List<float>();


        //LightShafts
        gradientColors.Add (GetColor ("#FF703C"));gradientTimes.Add (0f);
		gradientColors.Add (GetColor ("#FF5D00"));gradientTimes.Add (0.47f);
		gradientColors.Add (GetColor ("#FFF4DF"));gradientTimes.Add (0.65f);
		gradientColors.Add (GetColor ("#FFFFFF"));gradientTimes.Add (1f);
		profile.lightshaftsSettings.lightShaftsColorSun = CreateGradient (gradientColors, gradientTimes);
		gradientColors = new List<Color> ();gradientTimes = new List<float> ();
		profile.lightshaftsSettings.lightShaftsColorMoon = CreateGradient(GetColor("#94A8E5"),0f,GetColor("#94A8E5"),1f);
		gradientColors.Add (GetColor ("#1D1D1D"));gradientTimes.Add (0f);
		gradientColors.Add (GetColor ("#1D1D1D"));gradientTimes.Add (0.43f);
		gradientColors.Add (GetColor ("#A6A6A6"));gradientTimes.Add (0.54f);
		gradientColors.Add (GetColor ("#D0D0D0"));gradientTimes.Add (0.65f);
		gradientColors.Add (GetColor ("#C3C3C3"));gradientTimes.Add (1f);
		profile.lightshaftsSettings.thresholdColorSun = CreateGradient (gradientColors, gradientTimes);
		gradientColors = new List<Color> ();gradientTimes = new List<float> ();
		profile.lightshaftsSettings.thresholdColorMoon =  CreateGradient(GetColor("#0B0B0B"),0f,GetColor("#000000"),1f);


        // Weather
        profile.weatherSettings.lightningEffect = GetAssetPrefab("Enviro_Lightning_Strike");

        //Audio
        for (int i = 0; i < 8; i++) {
			profile.audioSettings.ThunderSFX.Add (GetAudioClip ("SFX_Thunder_" + (i+1)));
		}

        //Lightning
        profile.weatherSettings.lightningEffect = GetAssetPrefab("Enviro_Lightning_Strike");

        //Volume Lighting  
        profile.volumeLightSettings.ScatteringCoef = new AnimationCurve();
        profile.volumeLightSettings.ScatteringCoef.AddKey(CreateKey(0.5f, 0f));
        profile.volumeLightSettings.ScatteringCoef.AddKey(CreateKey(0.2f, 0.5f));
        profile.volumeLightSettings.ScatteringCoef.AddKey(CreateKey(0.15f, 1f));
    }

	public static bool UpdateProfile(EnviroProfile profile,string fromV, string toV)
	{
		if (profile == null)
			return false;

        List<Color> gradientColors = new List<Color>();
        List<float> gradientTimes = new List<float>();

        if ((fromV == "2.0.0" || fromV == "2.0.1" || fromV == "2.0.2") && toV == "2.1.0")
        {
            //Galaxy
            profile.skySettings.galaxyIntensity = new AnimationCurve();
            profile.skySettings.galaxyIntensity.AddKey(CreateKey(0.4f, 0f));
            profile.skySettings.galaxyIntensity.AddKey(CreateKey(0.015f, 0.5f));
            profile.skySettings.galaxyIntensity.AddKey(CreateKey(0.0f, 0.6f));
            profile.skySettings.galaxyIntensity.AddKey(CreateKey(0.0f, 1f));
            profile.skySettings.galaxyCubeMap = GetAssetCubemap("cube_enviro_galaxy");

            profile.skySettings.moonTexture = GetAssetTexture("tex_enviro_moon_standard");
            //Lightning
            profile.weatherSettings.lightningEffect = GetAssetPrefab("Enviro_Lightning_Strike");

            //Simple Sky Color
            gradientColors.Add(GetColor("#0F1013")); gradientTimes.Add(0f);
            gradientColors.Add(GetColor("#272A35")); gradientTimes.Add(0.465f);
            gradientColors.Add(GetColor("#277BA5")); gradientTimes.Add(0.50f);
            gradientColors.Add(GetColor("#7CA0BA")); gradientTimes.Add(0.56f);
            gradientColors.Add(GetColor("#5687B8")); gradientTimes.Add(1f);

            List<float> alpha = new List<float>();
            List<float> alphaTimes = new List<float>();

            alpha.Add(0f); alphaTimes.Add(0f);
            alpha.Add(0.5f); alphaTimes.Add(0.47f);
            alpha.Add(1f); alphaTimes.Add(1f);
            profile.skySettings.simpleSkyColor = CreateGradient(gradientColors, gradientTimes, alpha, alphaTimes);
            gradientColors = new List<Color>(); gradientTimes = new List<float>();

            //Simple Horizon Color
            gradientColors.Add(GetColor("#0F1013")); gradientTimes.Add(0f);
            gradientColors.Add(GetColor("#272A35")); gradientTimes.Add(0.46f);
            gradientColors.Add(GetColor("#DD8F3B")); gradientTimes.Add(0.506f);
            gradientColors.Add(GetColor("#DADADA")); gradientTimes.Add(0.603f);
            gradientColors.Add(GetColor("#FFFFFF")); gradientTimes.Add(1f);
            profile.skySettings.simpleHorizonColor = CreateGradient(gradientColors, gradientTimes);
            gradientColors = new List<Color>(); gradientTimes = new List<float>();

            //Simple Sun Color
            gradientColors.Add(GetColor("#000000")); gradientTimes.Add(0f);
            gradientColors.Add(GetColor("#FF6340")); gradientTimes.Add(0.46f);
            gradientColors.Add(GetColor("#FF7308")); gradientTimes.Add(0.506f);
            gradientColors.Add(GetColor("#E3C29E")); gradientTimes.Add(0.56f);
            gradientColors.Add(GetColor("#FFE9D4")); gradientTimes.Add(1f);
            profile.skySettings.simpleSunColor = CreateGradient(gradientColors, gradientTimes);
            gradientColors = new List<Color>(); gradientTimes = new List<float>();

            //Simple Fog
            gradientColors.Add(GetColor("#101217")); gradientTimes.Add(0f);
            gradientColors.Add(GetColor("#4C5570")); gradientTimes.Add(0.46f);
            gradientColors.Add(GetColor("#D8A269")); gradientTimes.Add(0.50f);
            gradientColors.Add(GetColor("#C7D7E3")); gradientTimes.Add(0.57f);
            gradientColors.Add(GetColor("#DBE7F0")); gradientTimes.Add(1f);
            profile.fogSettings.simpleFogColor = CreateGradient(gradientColors, gradientTimes);
            gradientColors = new List<Color>(); gradientTimes = new List<float>();

            //SunDisk Scale
            profile.skySettings.simpleSunDiskSize = new AnimationCurve();
            profile.skySettings.simpleSunDiskSize.AddKey(CreateKey(0.015f, 0.0f));
            profile.skySettings.simpleSunDiskSize.AddKey(CreateKey(0.035f, 0.37f, -0.1f, -0.1f));
            profile.skySettings.simpleSunDiskSize.AddKey(CreateKey(0.015f, 1f));

            //Particle Clouds Color
            gradientColors.Add(GetColor("#24272D")); gradientTimes.Add(0f);
            gradientColors.Add(GetColor("#2E3038")); gradientTimes.Add(0.46f);
            gradientColors.Add(GetColor("#544E46")); gradientTimes.Add(0.51f);
            gradientColors.Add(GetColor("#B7BABC")); gradientTimes.Add(0.56f);
            gradientColors.Add(GetColor("#D2D2D2")); gradientTimes.Add(1f);
            profile.cloudsSettings.ParticleCloudsLayer1.particleCloudsColor = CreateGradient(gradientColors, gradientTimes);
            profile.cloudsSettings.ParticleCloudsLayer2.particleCloudsColor = CreateGradient(gradientColors, gradientTimes);
            gradientColors = new List<Color>(); gradientTimes = new List<float>();

            profile.volumeLightSettings.ScatteringCoef = new AnimationCurve();
            profile.volumeLightSettings.ScatteringCoef.AddKey(CreateKey(0.5f, 0f));
            profile.volumeLightSettings.ScatteringCoef.AddKey(CreateKey(0.2f, 0.5f));
            profile.volumeLightSettings.ScatteringCoef.AddKey(CreateKey(0.15f, 1f));

            profile.version = toV;
            return true;
        }

        if (fromV == "2.0.3" && toV == "2.1.0")
        {
            //Lightning
            profile.weatherSettings.lightningEffect = GetAssetPrefab("Enviro_Lightning_Strike");

            //Simple Sky Color
            gradientColors.Add(GetColor("#0F1013")); gradientTimes.Add(0f);
            gradientColors.Add(GetColor("#272A35")); gradientTimes.Add(0.465f);
            gradientColors.Add(GetColor("#277BA5")); gradientTimes.Add(0.50f);
            gradientColors.Add(GetColor("#7CA0BA")); gradientTimes.Add(0.56f);
            gradientColors.Add(GetColor("#5687B8")); gradientTimes.Add(1f);

            profile.skySettings.moonTexture = GetAssetTexture("tex_enviro_moon_standard");

            List<float> alpha = new List<float>();
            List<float> alphaTimes = new List<float>();

            alpha.Add(0f); alphaTimes.Add(0f);
            alpha.Add(0.5f); alphaTimes.Add(0.47f);
            alpha.Add(1f); alphaTimes.Add(1f);
            profile.skySettings.simpleSkyColor = CreateGradient(gradientColors, gradientTimes, alpha, alphaTimes);
            gradientColors = new List<Color>(); gradientTimes = new List<float>();

            //Simple Horizon Color
            gradientColors.Add(GetColor("#0F1013")); gradientTimes.Add(0f);
            gradientColors.Add(GetColor("#272A35")); gradientTimes.Add(0.46f);
            gradientColors.Add(GetColor("#DD8F3B")); gradientTimes.Add(0.506f);
            gradientColors.Add(GetColor("#DADADA")); gradientTimes.Add(0.603f);
            gradientColors.Add(GetColor("#FFFFFF")); gradientTimes.Add(1f);
            profile.skySettings.simpleHorizonColor = CreateGradient(gradientColors, gradientTimes);
            gradientColors = new List<Color>(); gradientTimes = new List<float>();

            //Simple Sun Color
            gradientColors.Add(GetColor("#000000")); gradientTimes.Add(0f);
            gradientColors.Add(GetColor("#FF6340")); gradientTimes.Add(0.46f);
            gradientColors.Add(GetColor("#FF7308")); gradientTimes.Add(0.506f);
            gradientColors.Add(GetColor("#E3C29E")); gradientTimes.Add(0.56f);
            gradientColors.Add(GetColor("#FFE9D4")); gradientTimes.Add(1f);
            profile.skySettings.simpleSunColor = CreateGradient(gradientColors, gradientTimes);
            gradientColors = new List<Color>(); gradientTimes = new List<float>();

            //Simple Fog
            gradientColors.Add(GetColor("#101217")); gradientTimes.Add(0f);
            gradientColors.Add(GetColor("#4C5570")); gradientTimes.Add(0.46f);
            gradientColors.Add(GetColor("#D8A269")); gradientTimes.Add(0.50f);
            gradientColors.Add(GetColor("#C7D7E3")); gradientTimes.Add(0.57f);
            gradientColors.Add(GetColor("#DBE7F0")); gradientTimes.Add(1f);
            profile.fogSettings.simpleFogColor = CreateGradient(gradientColors, gradientTimes);
            gradientColors = new List<Color>(); gradientTimes = new List<float>();

            profile.skySettings.simpleSunDiskSize = new AnimationCurve();
            profile.skySettings.simpleSunDiskSize.AddKey(CreateKey(0.015f, 0.0f));
            profile.skySettings.simpleSunDiskSize.AddKey(CreateKey(0.035f, 0.37f, -0.1f, -0.1f));
            profile.skySettings.simpleSunDiskSize.AddKey(CreateKey(0.015f, 1f));

            //Particle Clouds Color
            gradientColors.Add(GetColor("#24272D")); gradientTimes.Add(0f);
            gradientColors.Add(GetColor("#2E3038")); gradientTimes.Add(0.46f);
            gradientColors.Add(GetColor("#544E46")); gradientTimes.Add(0.51f);
            gradientColors.Add(GetColor("#B7BABC")); gradientTimes.Add(0.56f);
            gradientColors.Add(GetColor("#D2D2D2")); gradientTimes.Add(1f);
            profile.cloudsSettings.ParticleCloudsLayer1.particleCloudsColor = CreateGradient(gradientColors, gradientTimes);
            profile.cloudsSettings.ParticleCloudsLayer2.particleCloudsColor = CreateGradient(gradientColors, gradientTimes);
            gradientColors = new List<Color>(); gradientTimes = new List<float>();

            profile.volumeLightSettings.ScatteringCoef = new AnimationCurve();
            profile.volumeLightSettings.ScatteringCoef.AddKey(CreateKey(0.5f, 0f));
            profile.volumeLightSettings.ScatteringCoef.AddKey(CreateKey(0.2f, 0.5f));
            profile.volumeLightSettings.ScatteringCoef.AddKey(CreateKey(0.15f, 1f));

            profile.version = toV;
            return true;
        }

        if (fromV == "2.0.4" || fromV == "2.0.5" && toV == "2.1.0")
        {

            profile.skySettings.moonTexture = GetAssetTexture("tex_enviro_moon_standard");

            //Simple Sky Color
            gradientColors.Add(GetColor("#0F1013")); gradientTimes.Add(0f);
            gradientColors.Add(GetColor("#272A35")); gradientTimes.Add(0.465f);
            gradientColors.Add(GetColor("#277BA5")); gradientTimes.Add(0.50f);
            gradientColors.Add(GetColor("#7CA0BA")); gradientTimes.Add(0.56f);
            gradientColors.Add(GetColor("#5687B8")); gradientTimes.Add(1f);

            List<float> alpha = new List<float>();
            List<float> alphaTimes = new List<float>();

            alpha.Add(0f); alphaTimes.Add(0f);
            alpha.Add(0.5f); alphaTimes.Add(0.47f);
            alpha.Add(1f); alphaTimes.Add(1f);
            profile.skySettings.simpleSkyColor = CreateGradient(gradientColors, gradientTimes, alpha, alphaTimes);
            gradientColors = new List<Color>(); gradientTimes = new List<float>();

            //Simple Fog
            gradientColors.Add(GetColor("#101217")); gradientTimes.Add(0f);
            gradientColors.Add(GetColor("#4C5570")); gradientTimes.Add(0.46f);
            gradientColors.Add(GetColor("#D8A269")); gradientTimes.Add(0.50f);
            gradientColors.Add(GetColor("#C7D7E3")); gradientTimes.Add(0.57f);
            gradientColors.Add(GetColor("#DBE7F0")); gradientTimes.Add(1f);
            profile.fogSettings.simpleFogColor = CreateGradient(gradientColors, gradientTimes);
            gradientColors = new List<Color>(); gradientTimes = new List<float>();

            //Simple Horizon Color
            gradientColors.Add(GetColor("#0F1013")); gradientTimes.Add(0f);
            gradientColors.Add(GetColor("#272A35")); gradientTimes.Add(0.46f);
            gradientColors.Add(GetColor("#DD8F3B")); gradientTimes.Add(0.506f);
            gradientColors.Add(GetColor("#DADADA")); gradientTimes.Add(0.603f);
            gradientColors.Add(GetColor("#FFFFFF")); gradientTimes.Add(1f);
            profile.skySettings.simpleHorizonColor = CreateGradient(gradientColors, gradientTimes);
            gradientColors = new List<Color>(); gradientTimes = new List<float>();

            //Simple Sun Color
            gradientColors.Add(GetColor("#000000")); gradientTimes.Add(0f);
            gradientColors.Add(GetColor("#FF6340")); gradientTimes.Add(0.46f);
            gradientColors.Add(GetColor("#FF7308")); gradientTimes.Add(0.506f);
            gradientColors.Add(GetColor("#E3C29E")); gradientTimes.Add(0.56f);
            gradientColors.Add(GetColor("#FFE9D4")); gradientTimes.Add(1f);
            profile.skySettings.simpleSunColor = CreateGradient(gradientColors, gradientTimes);
            gradientColors = new List<Color>(); gradientTimes = new List<float>();

            //SunDisk Scale
            profile.skySettings.simpleSunDiskSize = new AnimationCurve();
            profile.skySettings.simpleSunDiskSize.AddKey(CreateKey(0.015f, 0.0f));
            profile.skySettings.simpleSunDiskSize.AddKey(CreateKey(0.035f, 0.37f, -0.1f, -0.1f));
            profile.skySettings.simpleSunDiskSize.AddKey(CreateKey(0.015f, 1f));

            //Particle Clouds Color
            gradientColors.Add(GetColor("#24272D")); gradientTimes.Add(0f);
            gradientColors.Add(GetColor("#2E3038")); gradientTimes.Add(0.46f);
            gradientColors.Add(GetColor("#544E46")); gradientTimes.Add(0.51f);
            gradientColors.Add(GetColor("#B7BABC")); gradientTimes.Add(0.56f);
            gradientColors.Add(GetColor("#D2D2D2")); gradientTimes.Add(1f);
            profile.cloudsSettings.ParticleCloudsLayer1.particleCloudsColor = CreateGradient(gradientColors, gradientTimes);
            profile.cloudsSettings.ParticleCloudsLayer2.particleCloudsColor = CreateGradient(gradientColors, gradientTimes);
            gradientColors = new List<Color>(); gradientTimes = new List<float>();

            profile.volumeLightSettings.ScatteringCoef = new AnimationCurve();
            profile.volumeLightSettings.ScatteringCoef.AddKey(CreateKey(0.5f, 0f));
            profile.volumeLightSettings.ScatteringCoef.AddKey(CreateKey(0.2f, 0.5f));
            profile.volumeLightSettings.ScatteringCoef.AddKey(CreateKey(0.15f, 1f));

            profile.version = toV;
            return true;
        }

        return false;
	}

	public static GameObject GetAssetPrefab(string name)
	{
		#if UNITY_EDITOR
		string[] assets = AssetDatabase.FindAssets(name, null);
		for (int idx = 0; idx < assets.Length; idx++)
		{
			string path = AssetDatabase.GUIDToAssetPath(assets[idx]);
			if (path.Contains(".prefab"))
			{
				return AssetDatabase.LoadAssetAtPath<GameObject>(path);
			}
		}
		#endif
		return null;
	}

	public static AudioClip GetAudioClip(string name)
	{
		#if UNITY_EDITOR
		string[] assets = AssetDatabase.FindAssets(name, null);
		for (int idx = 0; idx < assets.Length; idx++)
		{
			string path = AssetDatabase.GUIDToAssetPath(assets[idx]);
			if (path.Contains(".wav"))
			{
				return AssetDatabase.LoadAssetAtPath<AudioClip>(path);
			}
		}
		#endif
		return null;
	}

	public static Cubemap GetAssetCubemap(string name)
	{
		#if UNITY_EDITOR
		string[] assets = AssetDatabase.FindAssets(name, null);
		for (int idx = 0; idx < assets.Length; idx++)
		{
			string path = AssetDatabase.GUIDToAssetPath(assets[idx]);
			if (path.Contains(".png"))
			{
				return AssetDatabase.LoadAssetAtPath<Cubemap>(path);
			}
            else if (path.Contains(".jpg"))
            {
                return AssetDatabase.LoadAssetAtPath<Cubemap>(path);
            }
		}
		#endif
		return null;
	}

	public static Texture GetAssetTexture(string name)
	{
		#if UNITY_EDITOR
		string[] assets = AssetDatabase.FindAssets(name, null);
		for (int idx = 0; idx < assets.Length; idx++)
		{
			string path = AssetDatabase.GUIDToAssetPath(assets[idx]);
			if (path.Length > 0)
			{
				return AssetDatabase.LoadAssetAtPath<Texture>(path);
			}
		}
		#endif
		return null;
	}

	public static Gradient CreateGradient(Color clr1, float time1, Color clr2, float time2)
	{
		Gradient nG = new Gradient ();
		GradientColorKey[] gClr = new GradientColorKey[2];
		GradientAlphaKey[] gAlpha = new GradientAlphaKey[2];

		gClr [0].color = clr1;
		gClr [0].time = time1;
		gClr [1].color = clr2;
		gClr [1].time = time2;

		gAlpha [0].alpha = 1f;
		gAlpha [0].time = 0f;
		gAlpha [1].alpha = 1f;
		gAlpha [1].time = 1f;

		nG.SetKeys (gClr, gAlpha);

		return nG;
	}

	public static Gradient CreateGradient(List<Color> clrs,List<float>times)
	{
		Gradient nG = new Gradient ();

		GradientColorKey[] gClr = new GradientColorKey[clrs.Count];
		GradientAlphaKey[] gAlpha = new GradientAlphaKey[2];

		for (int i = 0; i < clrs.Count; i++) {
			gClr [i].color = clrs [i];
			gClr [i].time = times[i];
		}

		gAlpha [0].alpha = 1f;
		gAlpha [0].time = 0f;
		gAlpha [1].alpha = 1f;
		gAlpha [1].time = 1f;

		nG.SetKeys (gClr, gAlpha);
		return nG;
	}

    public static Gradient CreateGradient(List<Color> clrs, List<float> times, List<float> alpha, List<float> timesAlpha)
    {
        Gradient nG = new Gradient();

        GradientColorKey[] gClr = new GradientColorKey[clrs.Count];
        GradientAlphaKey[] gAlpha = new GradientAlphaKey[alpha.Count];

        for (int i = 0; i < clrs.Count; i++)
        {
            gClr[i].color = clrs[i];
            gClr[i].time = times[i];
        }

        for (int i = 0; i < alpha.Count; i++)
        {
            gAlpha[i].alpha = alpha[i];
            gAlpha[i].time = timesAlpha[i];
        }

        nG.SetKeys(gClr, gAlpha);
        return nG;
    }


    public static Color GetColor (string hex)
	{
		Color clr = new Color ();	
		ColorUtility.TryParseHtmlString (hex, out clr);
		return clr;
	}

	public static Keyframe CreateKey (float value, float time)
	{
		Keyframe k = new Keyframe();
		k.value = value;
		k.time = time;
		return k;
	}

	public static Keyframe CreateKey (float value, float time, float inTangent, float outTangent)
	{
		Keyframe k = new Keyframe();
		k.value = value;
		k.time = time;
		k.inTangent = inTangent;
		k.outTangent = outTangent;
		return k;
	}
}
