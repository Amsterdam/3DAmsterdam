using Amsterdam3D.Interface;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Amsterdam3D.Settings {
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
            ObjectProperties.Instance.OpenPanel("Instellingen", true , 10.0f);
            ObjectProperties.Instance.AddTitle("Interface");
            ObjectProperties.Instance.AddActionCheckbox("Toon kaart", settings.drawMap, (toggle) => {
                settings.drawMap = toggle;
                ApplySettings();
            });
            ObjectProperties.Instance.AddActionCheckbox("Toon FPS teller", settings.drawFPS, (toggle) => {
                settings.drawFPS = toggle;
                ApplySettings();
            });
            ObjectProperties.Instance.AddLabel("Interface schaal");
            ObjectProperties.Instance.AddActionSlider("1x", "2x", 1.0f, 2.0f, settings.canvasDPI, (value) => {
                settings.canvasDPI = value;
                ApplySettings();
            });
            ObjectProperties.Instance.AddActionButtonBig("Herstel alle kleuren", (action) => {
                interfaceLayers.ResetAllLayerMaterialColors();
            });

            //Graphic options
            ObjectProperties.Instance.AddTitle("Grafisch");
            ObjectProperties.Instance.AddActionCheckbox("Effecten", settings.postProcessingEffects, (toggle) => {
                settings.postProcessingEffects = toggle;
                ApplySettings();
            });
            ObjectProperties.Instance.AddActionCheckbox("Antialiasing", settings.antiAliasing, (toggle) => {
                settings.antiAliasing = toggle;
                ApplySettings();
            });
            ObjectProperties.Instance.AddLabel("Render resolutie:");
            ObjectProperties.Instance.AddActionSlider("25%", "100%", 0.25f, 1.0f, settings.renderResolution, (value) => {
                settings.renderResolution = value;
                ApplySettings();
            });
            ObjectProperties.Instance.AddLabel("Schaduw detail:");
            ObjectProperties.Instance.AddActionSlider("Laag (Uit)", "Hoog", 0, 3, settings.shadowQuality, (value) => {
                settings.shadowQuality = (int)value;
                ApplySettings();
            }, true);

            ObjectProperties.Instance.AddActionButtonBig("Herstel instellingen", (action) => {
                settings = Instantiate(settingsProfilesTemplates[0]);
                ApplySettings();
                OpenSettingsPanel(); //Just regenerate this panel with new values.
            });

            ObjectProperties.Instance.AddSeperatorLine();
            ObjectProperties.Instance.AddCustomPrefab(stats);
        }

        public void ApplySettings()
        {
            fpsCounter.ToggleVisualFPS(settings.drawFPS);
            minimap.gameObject.SetActive(settings.drawMap);
            canvasSettings.ChangeCanvasScale(settings.canvasDPI);

            renderSettings.SetRenderScale(settings.renderResolution);
            renderSettings.ToggleReflections(settings.realtimeReflections);
            renderSettings.TogglePostEffects(settings.postProcessingEffects);
            renderSettings.ToggleAA(settings.antiAliasing);

            //Currently we only use the quality settings files for shadow quality differences
            //3 = 2045, 2 = 1024, 1=514, 0=Off
            QualitySettings.SetQualityLevel(settings.shadowQuality, true);

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