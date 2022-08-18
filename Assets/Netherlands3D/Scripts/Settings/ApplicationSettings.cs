using Netherlands3D.Events;
using Netherlands3D.Interface;
using Netherlands3D.Interface.Layers;
//using Netherlands3D.Interface.Minimap;
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

        private Rendering.RenderSettings renderSettings;

        [SerializeField]
        private GameObject stats;

		public static ApplicationSettings Instance;

		void Start()
		{
			renderSettings = GetComponent<Rendering.RenderSettings>();

			LoadTemplate(settingsProfilesTemplates[selectedTemplate]);

			//Load previous or auto detect optimal settings
			if (PlayerPrefs.HasKey(playerPrefKey))
			{
				LoadSettings();
			}		
		}

		private void LoadTemplate(ApplicationSettingsProfile templateProfile)
		{
			//Start with a copy of the selected base profile so we do not alter the templates
			settings = Instantiate(templateProfile);
			settings.name = "UserProfile";
			settings.applicationVersion = Application.version;
		}

		public void OpenSettingsPanel()
        {
			//Interface options
			ServiceLocator.GetService<PropertiesPanel>().OpenSettings();
			ServiceLocator.GetService<PropertiesPanel>().AddTitle("Interface");
			ServiceLocator.GetService<PropertiesPanel>().AddActionCheckbox("Toon kaart", settings.drawMap, (toggle) => {
				settings.drawMap = toggle;
				ApplySettings();
            });
			ServiceLocator.GetService<PropertiesPanel>().AddActionCheckbox("Toon FPS teller", settings.drawFPS, (toggle) => {
				settings.drawFPS = toggle;
				ApplySettings();
            });

			ServiceLocator.GetService<PropertiesPanel>().AddActionCheckbox("Toon Experimentele functies", settings.showExperimentelFeatures, (toggle) => {
				settings.showExperimentelFeatures = toggle;
				ApplySettings();
			});


			ServiceLocator.GetService<PropertiesPanel>().AddLabel("Interface schaal");
			ServiceLocator.GetService<PropertiesPanel>().AddActionSlider("1x", "2x", 1.0f, 2.0f, settings.canvasDPI, (value) => {
				settings.canvasDPI = value;
				ApplySettings();
            },false,"Interface schaal");

			//Graphic options
			ServiceLocator.GetService<PropertiesPanel>().AddTitle("Grafisch");
            ServiceLocator.GetService<PropertiesPanel>().AddLabel("Algemene instelling:");
			//Fill our dropdown using the templates and their titles
			List<string> profileNames = new List<string>();
			foreach (ApplicationSettingsProfile profile in settingsProfilesTemplates)
				profileNames.Add(profile.profileName);

			ServiceLocator.GetService<PropertiesPanel>().AddActionDropdown(profileNames.ToArray(), (action)=>
            {
				print("Selected template " + action);
				selectedTemplate = profileNames.IndexOf(action);
				settings = Instantiate(settingsProfilesTemplates[selectedTemplate]);

				ApplySettings();
				OpenSettingsPanel(); //Simply force a reload of the settings panel to apply all new overrides
            }, profileNames[selectedTemplate]);

			ServiceLocator.GetService<PropertiesPanel>().AddSpacer(20);
			ServiceLocator.GetService<PropertiesPanel>().AddActionCheckbox("Antialiasing", settings.antiAliasing, (toggle) => {
				settings.antiAliasing = toggle;
				ApplySettings();
			});
			ServiceLocator.GetService<PropertiesPanel>().AddLabel("Render resolutie:");
			ServiceLocator.GetService<PropertiesPanel>().AddActionSlider("25%", "100%", 0.25f, 1.0f, settings.renderResolution, (value) => {
				settings.renderResolution = value;
				ApplySettings();
			},false, "Render resolutie");
			ServiceLocator.GetService<PropertiesPanel>().AddLabel("Schaduw detail:");
			ServiceLocator.GetService<PropertiesPanel>().AddActionSlider("Laag (Uit)", "Hoog", 0, 3, settings.shadowQuality, (value) => {
				settings.shadowQuality = (int)value;
				ApplySettings();
			}, true, "Schaduw detail");

			ServiceLocator.GetService<PropertiesPanel>().AddSpacer(20);
			ServiceLocator.GetService<PropertiesPanel>().AddTitle("Extra");
			ServiceLocator.GetService<PropertiesPanel>().AddActionCheckbox("Effecten", settings.postProcessingEffects, (toggle) => {
				settings.postProcessingEffects = toggle;
				ApplySettings();
            });
			ServiceLocator.GetService<PropertiesPanel>().AddActionCheckbox("Ambient Occlusion", settings.ambientOcclusion, (toggle) => {
				settings.ambientOcclusion = toggle;
				ApplySettings();
			});

			ServiceLocator.GetService<PropertiesPanel>().AddActionCheckbox("Live reflecties", settings.realtimeReflections, (toggle) => {
				settings.realtimeReflections = toggle;
				ApplySettings();
            });

			ServiceLocator.GetService<PropertiesPanel>().AddTitle("Invoer");
			ServiceLocator.GetService<PropertiesPanel>().AddLabel("Gevoeligheid camera draaien:");
			ServiceLocator.GetService<PropertiesPanel>().AddActionSlider("Langzaam", "Snel", 0.1f, 2.0f, settings.rotateSensitivity, (value) => {
				settings.rotateSensitivity = value;
				ApplySettings();
			}, false, "Gevoeligheid camera draaien");

			ServiceLocator.GetService<PropertiesPanel>().AddSeperatorLine();
			ServiceLocator.GetService<PropertiesPanel>().AddSpacer(20);

			ServiceLocator.GetService<PropertiesPanel>().AddActionButtonText("Herstel alle kleuren", (action) => {
				interfaceLayers.ResetAllLayerMaterialColors();
			});
			ServiceLocator.GetService<PropertiesPanel>().AddActionButtonText("Herstel alle instellingen", (action) => {
				selectedTemplate = 0;
				settings = Instantiate(settingsProfilesTemplates[selectedTemplate]);
				ApplySettings();
				OpenSettingsPanel(); //Just regenerate this panel with new values.
            });

			ServiceLocator.GetService<PropertiesPanel>().AddSeperatorLine();
			ServiceLocator.GetService<PropertiesPanel>().AddCustomPrefab(stats);
        }

        public void ApplySettings()
        {
            //Currently we only use the quality settings files for shadow quality differences
            //3 = 2045, 2 = 1024, 1=514, 0=Off
            QualitySettings.SetQualityLevel(settings.shadowQuality, true);

			ToggleActiveEvent.Raise(settings.showExperimentelFeatures);
			
            renderSettings.SetRenderScale(settings.renderResolution);            
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