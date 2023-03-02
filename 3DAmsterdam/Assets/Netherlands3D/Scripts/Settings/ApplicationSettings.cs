using Netherlands3D.Cameras;
using Netherlands3D.Events;
using Netherlands3D.Interface;
using Netherlands3D.Interface.Layers;
using Netherlands3D.Interface.Minimap;
using Netherlands3D.Interface.SidePanel;
using Netherlands3D.JavascriptConnection;
using Netherlands3D.TileSystem;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;

namespace Netherlands3D.Settings {
    public class ApplicationSettings : MonoBehaviour
    {
		[DllImport("__Internal")]
		private static extern bool IsMobile();

		[SerializeField]
		private bool forceMobileDevice = false;
		private bool isMobileDevice = false;

		[SerializeField]
		private int benchmarkFrames = 30;

		[SerializeField]
		private Transform mobileCameraStartPosition;

		public static ApplicationSettingsProfile settings;

		[SerializeField]
		private bool enableDebugLogsOnWebGL = false;

		[SerializeField]
        private ApplicationSettingsProfile[] settingsProfilesTemplates;
		private int selectedTemplate = 0;

		private const string playerPrefKey = "applicationSettings";

		//Dependencies we fetch at start
        private InterfaceLayers interfaceLayers;
        private MapViewer minimap;
        private Fps fpsCounter;
		private TileHandler tileHandler;
		private CanvasSettings canvasSettings;
		private WaterReflectionCamera waterReflectionCamera;

        private Rendering.RenderSettings renderSettings;

		[SerializeField]
		private BinaryMeshLayer terrainLayer;

        [SerializeField]
        private GameObject stats;

		public static ApplicationSettings Instance;

		public bool IsMobileDevice { get => isMobileDevice; set => isMobileDevice = value; }

		[SerializeField]
		private BoolEvent toggleBetaFeatures;

		private void Awake()
		{
			Instance = this;
			IsMobileDevice = forceMobileDevice;

			interfaceLayers = FindObjectOfType<InterfaceLayers>();
			tileHandler = FindObjectOfType<TileHandler>();
			minimap = FindObjectOfType<MapViewer>();
			fpsCounter = FindObjectOfType<Fps>();
			canvasSettings = FindObjectOfType<CanvasSettings>();
			waterReflectionCamera = FindObjectOfType<WaterReflectionCamera>(true);

#if !UNITY_EDITOR && UNITY_WEBGL
			IsMobileDevice = IsMobile();
#endif

#if !UNITY_EDITOR && UNITY_WEBGL
            Debug.unityLogger.logEnabled = enableDebugLogsOnWebGL;
#else
			Debug.unityLogger.logEnabled = true;
#endif

		}

		void Start()
		{
			renderSettings = Rendering.RenderSettings.Instance;
			StartupSettings();
		}

		private void StartupSettings()
		{
			//Load up and apply our loaded/selected settings profile
			CreateSettingsProfile(settingsProfilesTemplates[selectedTemplate]);

			if (!forceMobileDevice && PlayerPrefs.HasKey(playerPrefKey))
			{
				Debug.Log("Loaded custom user settings");
				LoadSavedSettings();
			}
			else if (IsMobileDevice)
			{
				Debug.Log("Mobile application settings");
				ApplyMobileSpecificSettings();
			}
            else
            {
				Debug.Log("Optimising settings based on performance");
				RunBenchmark();
            }

			//Load up our enviroment, optimised for a mobile device
			EnviromentSettings.Instance.ApplyEnviroment(isMobileDevice);

			//Apply but do not save
			ApplySettings(false); 
		}

        private void RunBenchmark()
        {
			StartCoroutine(PerformanceBenchmark());
        }

		private IEnumerator PerformanceBenchmark()
        {
			var currentTime = Time.timeSinceLevelLoad;
            for (int i = 0; i < benchmarkFrames; i++)
            {
				yield return new WaitForEndOfFrame();
			}
			var benchmarkFramesRenderTime = Time.timeSinceLevelLoad - currentTime;

			if(benchmarkFramesRenderTime>1)
            {
				settings.antiAliasing = false;
            }
            else
            {
				settings.antiAliasing = true;
			}
			ApplySettings();
		}

		private void ApplyMobileSpecificSettings()
		{
			IsMobileDevice = true;
			
			//Select the mobile settings slot
			selectedTemplate = 3;
			
			//Only enable lod 0 for the terrainlayer
			if(terrainLayer)
				terrainLayer.Datasets[1].enabled = false;

			CreateSettingsProfile(settingsProfilesTemplates[selectedTemplate]);

			//Place camera at the mobile starting position ( closer to the ground )
			CameraModeChanger.Instance.ActiveCamera.transform.SetPositionAndRotation(mobileCameraStartPosition.position, mobileCameraStartPosition.rotation);

			Selector.Instance.AllowDelayedSubObjectSelections = false;
		}

		private void CreateSettingsProfile(ApplicationSettingsProfile templateProfile)
		{
			if (settings) Destroy(settings);

			//Start with a copy of the selected base profile so we do not alter the templates
			settings = Instantiate(templateProfile);
			settings.name = "UserProfile";
			settings.applicationVersion = Application.version;
		}

		public void OpenSettingsPanel()
		{
			//Interface options
			PropertiesPanel.Instance.OpenSettings();
			PropertiesPanel.Instance.AddTitle("Interface");
			PropertiesPanel.Instance.AddActionCheckbox("Toon kaart", settings.drawMap, (toggle) => {
				settings.drawMap = toggle;
				ApplySettings();
			});
			PropertiesPanel.Instance.AddActionCheckbox("Toon FPS teller", settings.drawFPS, (toggle) => {
				settings.drawFPS = toggle;
				ApplySettings();
			});

			PropertiesPanel.Instance.AddActionCheckbox("Toon Experimentele functies", settings.showExperimentalFeatures, (toggle) => {
				settings.showExperimentalFeatures = toggle;
				ApplySettings();
			});


			PropertiesPanel.Instance.AddLabel("Interface schaal:");
			PropertiesPanel.Instance.AddActionSlider("0.75x", "2x", 0.75f, 2.00f, settings.canvasScale, (value) => {
				settings.canvasScale = value * 100.0f;
				ApplySettings();
			}, false, "Interface schaal");

			//Graphic options
			PropertiesPanel.Instance.AddTitle("Grafisch");
			PropertiesPanel.Instance.AddLabel("Algemene instelling:");
			//Fill our dropdown using the templates and their titles
			List<string> profileNames = new List<string>();
			foreach (ApplicationSettingsProfile profile in settingsProfilesTemplates)
			{
				if (profile.mobileProfile && !IsMobileDevice) continue;
				profileNames.Add(profile.profileName);
			}

			PropertiesPanel.Instance.AddActionDropdown(profileNames.ToArray(), (action) =>
			{
				print("Selected template " + action);
				selectedTemplate = profileNames.IndexOf(action);
				settings = Instantiate(settingsProfilesTemplates[selectedTemplate]);

				ApplySettings();
				OpenSettingsPanel(); //Simply force a reload of the settings panel to apply all new overrides
			}, profileNames[selectedTemplate]);

			PropertiesPanel.Instance.AddSpacer(20);
			PropertiesPanel.Instance.AddActionCheckbox("Antialiasing", settings.antiAliasing, (toggle) => {
				settings.antiAliasing = toggle;
				ApplySettings();
			});
			PropertiesPanel.Instance.AddLabel("Camera zoom factor:");
			PropertiesPanel.Instance.AddActionSlider("1x", "3x", 1.0f, 3.0f, settings.cameraFov, (value) => {
				settings.cameraFov = Mathf.Lerp(60.0f,20.0f,Mathf.InverseLerp(1.0f,3.0f,value));
				ApplySettings();
			}, false, "Camera zoom factor");
			PropertiesPanel.Instance.AddLabel("Render resolutie:");
			PropertiesPanel.Instance.AddActionSlider("25%", "100%", 25f, 100f, settings.renderResolution, (value) => {
				settings.renderResolution = value * 0.01f;
				ApplySettings();
			}, false, "Render resolutie");
			PropertiesPanel.Instance.AddLabel("Schaduw detail:");
			PropertiesPanel.Instance.AddActionSlider("Laag (Uit)", "Hoog", 0, 3, settings.shadowQuality, (value) => {
				settings.shadowQuality = (int)value;
				ApplySettings();
			}, true, "Schaduw detail");
			PropertiesPanel.Instance.AddLabel("Zichtbaarheid afstand:");
			PropertiesPanel.Instance.AddActionSlider("Dichtbij", "Ver", 0.1f, 3f, settings.lodDistanceMultiplier, (value) => {
				settings.lodDistanceMultiplier = value;
				ApplySettings();
			}, false, "Zichtbaarheid afstand");

			PropertiesPanel.Instance.AddSpacer(20);
			PropertiesPanel.Instance.AddTitle("Detail 3D objecten");
			PropertiesPanel.Instance.AddActionCheckbox("Automatisch detail", settings.autoLOD, (toggle) => {
				settings.autoLOD = toggle;
				ApplySettings();
				OpenSettingsPanel();
			});
			if (!settings.autoLOD) { 
				PropertiesPanel.Instance.AddLabel("Maximaal detail niveau:");
				PropertiesPanel.Instance.AddActionSlider("Laag", "Hoog", 1, 2, settings.maxLOD, (value) => {
					settings.maxLOD = (int)value;
					ApplySettings();
				}, true, "Maximaal LOD (Level of detail) gebouwen");
			}
			

			PropertiesPanel.Instance.AddSpacer(20);
			PropertiesPanel.Instance.AddTitle("Extra");
			PropertiesPanel.Instance.AddActionCheckbox("Effecten", settings.postProcessingEffects, (toggle) => {
				settings.postProcessingEffects = toggle;
				ApplySettings();
            });
			PropertiesPanel.Instance.AddActionCheckbox("Ambient Occlusion", settings.ambientOcclusion, (toggle) => {
				settings.ambientOcclusion = toggle;
				ApplySettings();
			});

			PropertiesPanel.Instance.AddActionCheckbox("Live reflecties", settings.realtimeReflections, (toggle) => {
				settings.realtimeReflections = toggle;
				ApplySettings();
				OpenSettingsPanel();
			});

			if (settings.realtimeReflections)
			{
				PropertiesPanel.Instance.AddLabel("Live reflectie resolutie:");
				PropertiesPanel.Instance.AddActionSlider("5%", "100%", 5f, 100f, settings.reflectionsRenderResolution, (value) =>
				{
					settings.reflectionsRenderResolution = value * 0.01f;
					ApplySettings();
				}, false, "Render resolutie van reflecties");
			}
			PropertiesPanel.Instance.AddTitle("Invoer");
			PropertiesPanel.Instance.AddLabel("Gevoeligheid camera draaien:");
			PropertiesPanel.Instance.AddActionSlider("Langzaam", "Snel", 0.1f, 2.0f, settings.rotateSensitivity, (value) => {
				settings.rotateSensitivity = value;
				ApplySettings();
			}, false, "Gevoeligheid camera draaien");

			PropertiesPanel.Instance.AddSeperatorLine();
			PropertiesPanel.Instance.AddSpacer(20);

			PropertiesPanel.Instance.AddActionButtonText("Herstel alle kleuren", (action) => {
				interfaceLayers.ResetAllLayerMaterialColors();
			});
			PropertiesPanel.Instance.AddActionButtonText("Herstel alle instellingen", (action) => {
				selectedTemplate = 0;
				settings = Instantiate(settingsProfilesTemplates[selectedTemplate]);
				ApplySettings();
				OpenSettingsPanel(); //Just regenerate this panel with new values.
            });

			PropertiesPanel.Instance.AddSeperatorLine();
			PropertiesPanel.Instance.AddCustomPrefab(stats);
        }

        public void ApplySettings(bool save = true)
        {
            //Currently we only use the quality settings files for shadow quality differences
            //3 = 2045, 2 = 1024, 1=514, 0=Off
            QualitySettings.SetQualityLevel(settings.shadowQuality, true);

            fpsCounter.ToggleVisualFPS(settings.drawFPS);
			
			minimap.gameObject.SetActive(settings.drawMap);

			toggleBetaFeatures.InvokeStarted(settings.showExperimentalFeatures);

			canvasSettings.ChangeCanvasScale(settings.canvasScale * 0.01f);
            renderSettings.ChangeCameraFOV(settings.cameraFov);
            renderSettings.SetRenderScale(settings.renderResolution);
            renderSettings.ToggleReflections(settings.realtimeReflections);
            renderSettings.TogglePostEffects(settings.postProcessingEffects);
            renderSettings.ToggleAA(settings.antiAliasing);
			renderSettings.ToggleAO(settings.ambientOcclusion);

			tileHandler.SetMaxDistanceMultiplier(settings.lodDistanceMultiplier);
			tileHandler.SetLODMode((settings.autoLOD) ? 0 : settings.maxLOD);

			waterReflectionCamera.Downscale = settings.reflectionsRenderResolution;

			if (save) SaveSettings();
        }

        [ContextMenu("Save application settings")]
        public void SaveSettings()
        {
            PlayerPrefs.SetString(playerPrefKey, JsonUtility.ToJson(settings));
        }

        [ContextMenu("Load application settings")]
        public void LoadSavedSettings()
        {
            JsonUtility.FromJsonOverwrite(PlayerPrefs.GetString(playerPrefKey), settings);
			ApplySettings();
        }

		[ContextMenu("Reset settings")]
		public void ResetSettings()
		{
			PlayerPrefs.DeleteKey(playerPrefKey);
		}


	}
}