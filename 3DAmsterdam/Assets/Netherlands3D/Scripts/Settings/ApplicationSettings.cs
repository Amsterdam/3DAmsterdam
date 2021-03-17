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

        public ApplicationSettingsProfile settings;

        [SerializeField]
        private ApplicationSettingsProfile[] settingsProfilesTemplates;

        private const string playerPrefKey = "applicationSettings";

        [SerializeField]
        private InterfaceLayers interfaceLayers;

        [SerializeField]
        private Map minimap;

        [SerializeField]
        private Fps fpsCounter;

        private CanvasSettings canvasSettings;
        private Rendering.RenderSettings renderSettings;

        [SerializeField]
        private GameObject stats;

        void Start()
        {
            canvasSettings = GetComponent<CanvasSettings>();
            renderSettings = GetComponent<Rendering.RenderSettings>();

            //Start with a copy of the selected base profile so we do not alter the templates
            settings = Instantiate(settings);
            settings.name = "UserProfile";
            settings.applicationVersion = Application.version;

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

		private void DetermineOptimalSettings()
		{
			settings.canvasDPI = canvasSettings.DetectPreferedCanvasScale();
		}

		public void OpenSettingsPanel()
        {
			//Interface options
			Interface.SidePanel.Properties.Instance.OpenPanel("Instellingen", true , 10.0f);
			Interface.SidePanel.Properties.Instance.AddTitle("Interface");
			Interface.SidePanel.Properties.Instance.AddActionCheckbox("Toon kaart", settings.drawMap, (toggle) => {
				settings.drawMap = toggle;
				ApplySettings();
            });
			Interface.SidePanel.Properties.Instance.AddActionCheckbox("Toon FPS teller", settings.drawFPS, (toggle) => {
				settings.drawFPS = toggle;
				ApplySettings();
            });
			Interface.SidePanel.Properties.Instance.AddLabel("Interface schaal");
			Interface.SidePanel.Properties.Instance.AddActionSlider("1x", "2x", 1.0f, 2.0f, settings.canvasDPI, (value) => {
				settings.canvasDPI = value;
				ApplySettings();
            });
			Interface.SidePanel.Properties.Instance.AddActionButtonBig("Herstel alle kleuren", (action) => {
				interfaceLayers.ResetAllLayerMaterialColors();
            });

			//Graphic options
			Interface.SidePanel.Properties.Instance.AddTitle("Grafisch");
			Interface.SidePanel.Properties.Instance.AddActionCheckbox("Effecten", settings.postProcessingEffects, (toggle) => {
				settings.postProcessingEffects = toggle;
				ApplySettings();
            });
			Interface.SidePanel.Properties.Instance.AddActionCheckbox("Antialiasing", settings.antiAliasing, (toggle) => {
				settings.antiAliasing = toggle;
				ApplySettings();
            });
			Interface.SidePanel.Properties.Instance.AddActionCheckbox("Reflecties", settings.realtimeReflections, (toggle) => {
				settings.realtimeReflections = toggle;
				ApplySettings();
            });
			Interface.SidePanel.Properties.Instance.AddLabel("Render resolutie:");
			Interface.SidePanel.Properties.Instance.AddActionSlider("25%", "100%", 0.25f, 1.0f, settings.renderResolution, (value) => {
				settings.renderResolution = value;
				ApplySettings();
            });
			Interface.SidePanel.Properties.Instance.AddLabel("Schaduw detail:");
			Interface.SidePanel.Properties.Instance.AddActionSlider("Laag (Uit)", "Hoog", 0, 3, settings.shadowQuality, (value) => {
				settings.shadowQuality = (int)value;
				ApplySettings();
            }, true);

			Interface.SidePanel.Properties.Instance.AddActionButtonBig("Herstel instellingen", (action) => {
				settings = Instantiate(settingsProfilesTemplates[0]);
				ApplySettings();
				OpenSettingsPanel(); //Just regenerate this panel with new values.
            });

			Interface.SidePanel.Properties.Instance.AddSeperatorLine();
			Interface.SidePanel.Properties.Instance.AddCustomPrefab(stats);
        }

        public void ApplySettings()
        {
            //Currently we only use the quality settings files for shadow quality differences
            //3 = 2045, 2 = 1024, 1=514, 0=Off
            QualitySettings.SetQualityLevel(settings.shadowQuality, true);

            fpsCounter.ToggleVisualFPS(settings.drawFPS);
            minimap.gameObject.SetActive(settings.drawMap);
            canvasSettings.ChangeCanvasScale(settings.canvasDPI);

            renderSettings.SetRenderScale(settings.renderResolution);
            renderSettings.ToggleReflections(settings.realtimeReflections);
            renderSettings.TogglePostEffects(settings.postProcessingEffects);
            renderSettings.ToggleAA(settings.antiAliasing);

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