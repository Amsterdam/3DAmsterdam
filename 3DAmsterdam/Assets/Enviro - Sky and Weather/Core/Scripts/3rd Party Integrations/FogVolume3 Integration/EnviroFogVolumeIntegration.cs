using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if ENVIRO_FV3_SUPPORT
[ExecuteInEditMode]
[AddComponentMenu("Enviro/Integration/Fog Volume 3 Integration")]
#endif
public class EnviroFogVolumeIntegration : MonoBehaviour {
    #if ENVIRO_FV3_SUPPORT
	public enum CurrentFogMode
	{
		None,
		Uniform,
		Textured
	}
		

	public CurrentFogMode fogMode;

	[Header("Fog Volume Objects")]
	public FogVolume clouds;
	public FogVolume fog;

	[Header("Creation Settings")]
	public Vector3 cloudsPosition = new Vector3 (0f,250f,0f);
	public Vector3 fogPosition = new Vector3 (0f,200f,0f);

	public bool moveWithPlayer = true;

	[Header("Clouds Runtime Settings")]
	[Range(0f,10f)]
	public float coverageMult = 3f;
	[Range(0f,2f)]
	public float absorbtionMult = 1.2f;

	[Range(0f,1f)]
	public float fogColorPower = 0.75f;
	[Range(0f,1f)]
	public float visibilityMult = 0.1f;
	[Range(0f,1f)]
	public float renderIntensityMult = 0.1f;

	private Light myLight;
	private float cloudCoverage;

	void Start () {
		
		if (EnviroSkyMgr.instance == null) {
			this.enabled = false;
			Debug.Log ("Deactivated Fog Volume Integration. No EnviroSkyMgr instance in scene!");
			return;
		}
		myLight = EnviroSkyMgr.instance.GetEnviroComponents().DirectLight.GetComponent<Light>();

		if (fog == null)
			fogMode = CurrentFogMode.None;
	}

	//Control the fog and Clouds based on time of day and weather.
	void Update () 
	{
        if (EnviroSkyMgr.instance == null)
            return;

        //Clouds
        if (clouds != null)
		{
			//Set cloud coverage based on first enviro layer.
			cloudCoverage = 0f;
			// Set cloud time and weather based weather colors.
			clouds.InscatteringColor = myLight.color;
			clouds._AmbientColor = EnviroSkyMgr.instance.EnviroCloudSettings.volumeCloudsColor.Evaluate (EnviroSkyMgr.instance.EnviroTime.solarTime) * RenderSettings.fogColor;
		
			cloudCoverage = EnviroSkyMgr.instance.EnviroCloudConfig.coverage;
			clouds.Coverage = Mathf.Clamp(cloudCoverage * coverageMult,0f,5f);
			clouds.Absorption = (1.01f - EnviroSkyMgr.instance.EnviroCloudConfig.topColor.grayscale) * absorbtionMult;
		}

		//Ground Fog
		if (fog != null) 
		{
			fog.FogMainColor = EnviroSkyMgr.instance.EnviroCloudSettings.volumeCloudsColor.Evaluate (EnviroSkyMgr.instance.EnviroTime.solarTime) * (RenderSettings.fogColor * fogColorPower);
			fog._AmbientColor = RenderSettings.ambientLight;
			fog.InscatteringColor = myLight.color;
			fog.VolumeFogInscatteringColor = myLight.color;

			if (fogMode == CurrentFogMode.Uniform)
				fog.Visibility = (1f - RenderSettings.fogDensity) * 500f * visibilityMult;
			else
				fog.Visibility = 1f;
			
			if (fogMode == CurrentFogMode.Textured)
				fog.NoiseIntensity = Mathf.Clamp01(RenderSettings.fogDensity * 500f * renderIntensityMult);

			fog.Absorption = Mathf.Clamp01(1.01f- EnviroSkyMgr.instance.EnviroTime.solarTime);
		}
	}

	void LateUpdate ()
	{
		if (moveWithPlayer) {

			if (clouds != null) {
				clouds.transform.position = new Vector3 (EnviroSkyMgr.instance.GetPlayer().transform.position.x, clouds.transform.position.y, EnviroSkyMgr.instance.GetPlayer().transform.position.z);
			}

			if (fog != null) {
				fog.transform.position = new Vector3 (EnviroSkyMgr.instance.GetPlayer().transform.position.x, fog.transform.position.y, EnviroSkyMgr.instance.GetPlayer().transform.position.z);
			}
		}
	}

	public void GetFromGaia()
	{
		FogVolume gFV = null;
		FogVolume cloudsFV = null;


		GameObject groundFog = GameObject.Find("Fog Volume [Ground Fog]");
		if(groundFog != null)
			gFV = groundFog.GetComponent<FogVolume>();
		
		if (gFV != null)
			fog = gFV;

		GameObject cloudsGo = GameObject.Find("Fog Volume [Clouds]");
		if(cloudsGo != null)
			cloudsFV = cloudsGo.GetComponent<FogVolume>();

		if (cloudsFV != null)
			clouds = cloudsFV;
	}

	public void CreateUniformFog()
	{
		fogMode = CurrentFogMode.Uniform;
		//Pick colour of main light
		Color mainLightColor = Color.white;
		mainLightColor = EnviroSky.instance.Components.DirectLight.GetComponent<Light>().color;
	
		GameObject groundFogGo = GameObject.Find("Fog Volume [Ground Fog] for Enviro");
	
		DestroyImmediate (groundFogGo);

		if (groundFogGo == null)
		{
			groundFogGo = new GameObject();
			groundFogGo.name = "Fog Volume [Ground Fog] for Enviro";
			Renderer rend = groundFogGo.AddComponent<MeshRenderer>();
			if (rend != null) {
				rend.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
				rend.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;
				rend.receiveShadows = false;
				rend.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;
			}
			groundFogGo.AddComponent<FogVolume>();
		}

		//Adjust Fog Settings
		FogVolume fVolume = groundFogGo.GetComponent<FogVolume>();
		if (fVolume != null)
		{
			fVolume.transform.position = fogPosition;
			fVolume.fogVolumeScale = new Vector3(EnviroSkyMgr.instance.GetCamera().farClipPlane, EnviroSkyMgr.instance.GetCamera().farClipPlane * 0.2f, EnviroSkyMgr.instance.GetCamera().farClipPlane);
			fVolume.FogMainColor = Color.grey;
			fVolume.Visibility = 800f;
			fVolume.EnableInscattering = true;
			fVolume.InscatteringColor = Color.Lerp(mainLightColor, Color.black, 0.8f);
			fVolume.VolumeFogInscatteringAnisotropy = 0.59f;
			fVolume.InscatteringIntensity = 0.07f;
			fVolume.InscatteringStartDistance = 5f;
			fVolume.InscatteringTransitionWideness = 300f;
			fVolume.DrawOrder = 3;
			fVolume._PushAlpha = 1.0025f;
			fVolume._ztest = UnityEngine.Rendering.CompareFunction.Always;

			// Assign Fog for runtime Controls
			fog = fVolume;
			CheckCamera ();
		}
	}


	public void CreateTexturedFog()
	{
		fogMode = CurrentFogMode.Textured;

		//Pick colour of main light
		Color mainLightColor = Color.white;
		mainLightColor = EnviroSky.instance.Components.DirectLight.GetComponent<Light>().color;

		GameObject groundFogGo = GameObject.Find("Fog Volume [Ground Fog] for Enviro");

		DestroyImmediate (groundFogGo);

		if (groundFogGo == null)
		{
			groundFogGo = new GameObject();
			groundFogGo.name = "Fog Volume [Ground Fog] for Enviro";
			Renderer rend = groundFogGo.AddComponent<MeshRenderer>();
			if (rend != null) {
				rend.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
				rend.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;
				rend.receiveShadows = false;
				rend.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;
			}
			groundFogGo.AddComponent<FogVolume>();
		}

		//Adjust Fog Settings
		FogVolume fVolume = groundFogGo.GetComponent<FogVolume>();
		if (fVolume != null)
		{
			fVolume.transform.position = fogPosition;
			fVolume.fogVolumeScale = new Vector3(EnviroSkyMgr.instance.GetCamera().farClipPlane, EnviroSkyMgr.instance.GetCamera().farClipPlane * 0.2f, EnviroSkyMgr.instance.GetCamera().farClipPlane);
			fVolume.FogMainColor = Color.grey;
			fVolume.Visibility = 800f;

			fVolume._FogType = FogVolume.FogType.Textured;
			fVolume.EnableGradient = true;
			fVolume.bVolumeFog = false;
			fVolume._BlendMode = FogVolumeRenderer.BlendMode.PremultipliedTransparency;

			fVolume.EnableInscattering = true;
			fVolume.InscatteringColor = Color.Lerp(mainLightColor, Color.black, 0.8f);
			fVolume.VolumeFogInscatteringAnisotropy = 0.59f;
			fVolume.InscatteringIntensity = 0.07f;
			fVolume.InscatteringStartDistance = 5f;
			fVolume.InscatteringTransitionWideness = 300f;
		

			fVolume.DrawOrder = 3;
			fVolume._PushAlpha = 1.0025f;
			fVolume._ztest = UnityEngine.Rendering.CompareFunction.Always;

			// Assign Fog for runtime Controls
			fog = fVolume;
			CheckCamera ();
		}
	}

	/// <summary>
	/// Creates a Clouds Layer for Enviro.
	/// </summary>
	public void CreateCloudLayer ()
	{
		//Pick colour of main light
		Color mainLightColor = Color.white;

		mainLightColor = EnviroSky.instance.Components.DirectLight.GetComponent<Light>().color;

		//Get the main camera
		Camera mainCamera = Camera.main;

		//First make sure its not already in scene - if it isnt then add it
		FogVolume fVolume;
		GameObject goClouds = GameObject.Find("Fog Volume [Clouds] for Enviro");

		if (goClouds == null)
		{
			
			goClouds = new GameObject();
			goClouds.name = "Fog Volume [Clouds] for Enviro";
			Renderer rend = goClouds.AddComponent<MeshRenderer>();
			if (rend != null) {
				rend.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
				rend.receiveShadows = false;
				rend.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;
				rend.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;
			}
			fVolume = goClouds.AddComponent<FogVolume>();
			//Create the horizon
			GameObject goHorizon = GameObject.CreatePrimitive(PrimitiveType.Plane);
			goHorizon.name = "Horizon";
			goHorizon.transform.parent = goClouds.transform;
			goHorizon.transform.localPosition = new Vector3(0f, -79f, 0f);
			goHorizon.GetComponent<MeshRenderer>().enabled = false;
			goHorizon.GetComponent<MeshCollider>().enabled = false;

			//Create the priority script
			FogVolumePriority fogVolumePrio = goClouds.AddComponent<FogVolumePriority>();
			fogVolumePrio.GameCamera = mainCamera;
			fogVolumePrio.FogOrderCameraAbove = 4;
			fogVolumePrio.FogOrderCameraBelow = -1;
			fogVolumePrio.thisFog = fVolume;
			fogVolumePrio.Horizon = goHorizon;

			CheckCamera ();
		}

		//Setup our clouds
		fVolume = goClouds.GetComponent<FogVolume>();

		if (fVolume != null)
		{
			clouds = fVolume;

			//Location and scale
			fVolume.transform.position = cloudsPosition;
			fVolume.fogVolumeScale = new Vector3(EnviroSkyMgr.instance.GetCamera().farClipPlane - 50f, 100f, EnviroSkyMgr.instance.GetCamera().farClipPlane - 50f);

			//Fog type and blend mode
			fVolume._FogType = FogVolume.FogType.Textured;
			fVolume._BlendMode = FogVolumeRenderer.BlendMode.TraditionalTransparency;

			//Lighting
			fVolume._AmbientColor = Color.Lerp(mainLightColor, Color.black, 0.1f);
			fVolume.useHeightGradient = true;
			fVolume.Absorption = 0.8f;
			fVolume.HeightAbsorption = 0.185f;
			fVolume.bAbsorption = true;

			fVolume.EnableInscattering = true;
			fVolume.InscatteringColor = mainLightColor;
			fVolume.InscatteringShape = 0.05f;
			fVolume.InscatteringIntensity = 0.882f;
			fVolume.InscatteringStartDistance = 0f;
			fVolume.InscatteringTransitionWideness = 1f;

			fVolume._DirectionalLighting = true;
			fVolume.LightExtinctionColor = Color.Lerp(mainLightColor, Color.black, 0.8f);
			fVolume._DirectionalLightingDistance = 0.0008f;
			fVolume.DirectLightingShadowDensity = 6f;
			fVolume.DirectLightingShadowSteps = 1;

			//Renderer
			fVolume.NoiseIntensity = 1f;
			fVolume.SceneCollision = true;
			fVolume.Iterations = 500;
			fVolume.IterationStep = 100;
			fVolume._OptimizationFactor = 0.0000005f;

			fVolume.GradMin = 0.19f;
			fVolume.GradMax = 0.06f;
			fVolume.GradMin2 = -0.25f;
			fVolume.GradMax2 = 0.21f;

			//Noise
			fVolume.EnableNoise = true;
			fVolume._3DNoiseScale = 0.15f;
			fVolume.Speed = new Vector4(0.49f, 0f, 0f, 0f);
			fVolume.Vortex = 0.47f;
			fVolume.RotationSpeed = 0f;
			fVolume.rotation = 324f;
			fVolume._VortexAxis = FogVolume.VortexAxis.Y;
			fVolume.Coverage = 2.44f;
			fVolume.NoiseContrast = 12.9f;
			fVolume.NoiseDensity = 0.2f;
			fVolume.Octaves = 3;
			fVolume.BaseTiling = 150f;
			fVolume._BaseRelativeSpeed = 0.85f;
			fVolume.DetailTiling = 285.3f;
			fVolume._DetailRelativeSpeed = 16.6f;
			fVolume.DetailDistance = 5000f;
			fVolume._NoiseDetailRange = 0.337f;
			fVolume._DetailMaskingThreshold = 8f;
			fVolume._Curl = 0.364f;

			//Other
			fVolume.DrawOrder = 4;
			fVolume._PushAlpha = 1.5f;
			fVolume._ztest = UnityEngine.Rendering.CompareFunction.LessEqual;

			if(fogMode != CurrentFogMode.Textured)
				fVolume.CreateSurrogate = true;
			else
				fVolume.CreateSurrogate = false;
			
			CheckCamera ();
		}
	}


	private void CheckCamera ()
	{
		if (EnviroSkyMgr.instance.GetCamera() != null) {
			FogVolumeRenderer renderer = EnviroSkyMgr.instance.GetCamera().gameObject.GetComponent<FogVolumeRenderer> ();

			if (renderer == null)
				renderer = EnviroSkyMgr.instance.GetCamera().gameObject.AddComponent<FogVolumeRenderer> ();

			if (renderer != null) {
				renderer._Downsample = 3;

				if (fogMode == CurrentFogMode.Textured) {
					renderer.GenerateDepth = true;
					if(clouds != null)
					clouds.CreateSurrogate = false;
				} else {
					renderer.GenerateDepth = false;
					clouds.CreateSurrogate = true;
				}
			}
		}
	}
#endif
}
