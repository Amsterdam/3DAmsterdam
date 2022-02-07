using Netherlands3D.Events;
using Netherlands3D.Interface;
using Netherlands3D.Interface.Layers;
using Netherlands3D.Interface.Minimap;
using Netherlands3D.Interface.SidePanel;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Netherlands3D.Settings {
    public class ApplicationSettings : MonoBehaviour
    {
        [SerializeField]
        private bool automaticOptimalSettings = true;

        public static ApplicationSettingsProfile settings;

        [SerializeField]
        private ApplicationSettingsProfile[] settingsProfilesTemplates;
		private int selectedTemplate = 0;

        private const string playerPrefKey = "applicationSettings";

        [SerializeField]
        private InterfaceLayers interfaceLayers;

        [SerializeField]
        private Map minimap;

        private CanvasSettings canvasSettings;
        private Rendering.RenderSettings renderSettings;

        [SerializeField]
        private GameObject stats;

		public static ApplicationSettings Instance;

		void Start()
		{
			canvasSettings = GetComponent<CanvasSettings>();
			renderSettings = GetComponent<Rendering.RenderSettings>();

			LoadTemplate(settingsProfilesTemplates[selectedTemplate]);

			//Load previous or auto detect optimal settings
			if (PlayerPrefs.HasKey(playerPrefKey))
			{
				LoadSettings();
			}
			else if (automaticOptimalSettings && !PlayerPrefs.HasKey(playerPrefKey))
			{
				DetermineOptimalSettings();
			}
		}

		private void LoadTemplate(ApplicationSettingsProfile templateProfile)
		{
			//Start with a copy of the selected base profile so we do not alter the templates
			settings = Instantiate(templateProfile);
			settings.name = "UserProfile";
			settings.applicationVersion = Application.version;
		}

		private void DetermineOptimalSettings()
		{
			settings.canvasDPI = canvasSettings.DetectPreferedCanvasScale();
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

			PropertiesPanel.Instance.AddActionCheckbox("Toon Experimentele functies", settings.showExperimentelFeatures, (toggle) => {
				settings.showExperimentelFeatures = toggle;
				ApplySettings();
			});


			PropertiesPanel.Instance.AddLabel("Interface schaal");
			PropertiesPanel.Instance.AddActionSlider("1x", "2x", 1.0f, 2.0f, settings.canvasDPI, (value) => {
				settings.canvasDPI = value;
				ApplySettings();
            },false,"Interface schaal");

			//Graphic options
			PropertiesPanel.Instance.AddTitle("Grafisch");
            PropertiesPanel.Instance.AddLabel("Algemene instelling:");
			//Fill our dropdown using the templates and their titles
			List<string> profileNames = new List<string>();
			foreach (ApplicationSettingsProfile profile in settingsProfilesTemplates)
				profileNames.Add(profile.profileName);

			PropertiesPanel.Instance.AddActionDropdown(profileNames.ToArray(), (action)=>
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
			PropertiesPanel.Instance.AddLabel("Render resolutie:");
			PropertiesPanel.Instance.AddActionSlider("25%", "100%", 0.25f, 1.0f, settings.renderResolution, (value) => {
				settings.renderResolution = value;
				ApplySettings();
			},false, "Render resolutie");
			PropertiesPanel.Instance.AddLabel("Schaduw detail:");
			PropertiesPanel.Instance.AddActionSlider("Laag (Uit)", "Hoog", 0, 3, settings.shadowQuality, (value) => {
				settings.shadowQuality = (int)value;
				ApplySettings();
			}, true, "Schaduw detail");

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
            });

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

        public void ApplySettings()
        {
            //Currently we only use the quality settings files for shadow quality differences
            //3 = 2045, 2 = 1024, 1=514, 0=Off
            QualitySettings.SetQualityLevel(settings.shadowQuality, true);
			
			if (Config.activeConfiguration.EnableMinimap)
			{
				minimap.gameObject.SetActive(settings.drawMap);
			}

			ToggleActiveEvent.Raise(settings.showExperimentelFeatures);

			canvasSettings.ChangeCanvasScale(settings.canvasDPI);
            renderSettings.SetRenderScale(settings.renderResolution);
            renderSettings.ToggleReflections(settings.realtimeReflections);
            renderSettings.TogglePostEffects(settings.postProcessingEffects);
            renderSettings.ToggleAA(settings.antiAliasing);
			renderSettings.ToggleAO(settings.ambientOcclusion);


			SaveSettings();
        }

        [ContextMenu("Save application settings")]
        public void SaveSettings()
        {
            PlayerPrefs.SetString(playerPrefKey, JsonUtility.ToJson(settings));
        }

        [ContextMenu("Load application settings")]
        public void LoadSettings()
        {
            JsonUtility.FromJsonOverwrite(PlayerPrefs.GetString(playerPrefKey), settings);
            ApplySettings();
        }
    }
}