using Amsterdam3D.Interface;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Amsterdam3D.Settings {
    public class ApplicationSettings : MonoBehaviour
    {
        public ApplicationSettingsProfile settings;

        [SerializeField]
        private ApplicationSettingsProfile[] settingsProfilesTemplates;

        private const string playerPrefKey = "applicationSettings";

        [SerializeField]
        private Map minimap;

        [SerializeField]
        private Fps fpsCounter;

        private CanvasSettings canvasSettings;
        private Rendering.RenderSettings renderSettings;

        void Awake()
        {
            canvasSettings = GetComponent<CanvasSettings>();
            renderSettings = GetComponent<Rendering.RenderSettings>();

            //Start with a copy of the selected base profile so we do not alter the templates
            settings = Instantiate(settings);
            settings.name = "UserProfile";
        }

        public void OpenSettingsPanel()
        {
            ObjectProperties.Instance.OpenPanel("Settings", true);
            ObjectProperties.Instance.AddTitle("Interface");
            ObjectProperties.Instance.AddActionCheckbox("Toon kaart", (toggle) => {
                settings.drawMap = toggle;
                ApplySettings();
            });
            ObjectProperties.Instance.AddActionCheckbox("Toon FPS teller", (toggle) => {
                settings.drawFPS = toggle;
                ApplySettings();
            });
            ObjectProperties.Instance.AddLabel("Interface schaal");
            ObjectProperties.Instance.AddActionSlider("1x", "2x", 1.0f, 2.0f, (value) => {
                settings.canvasDPI = value;
                ApplySettings();
            });
            ObjectProperties.Instance.AddLabel("Interface schaal");
            ObjectProperties.Instance.AddActionSlider("1x", "2x", 1.0f, 2.0f, (value) => {
                settings.canvasDPI = value;
                ApplySettings();
            });
            ObjectProperties.Instance.AddTitle("Grafisch");
        }

        public void ApplySettings()
        {
            fpsCounter.ToggleVisualFPS(settings.drawFPS);
            minimap.gameObject.SetActive(settings.drawMap);
            canvasSettings.ChangeCanvasScale(settings.canvasDPI);
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
        }
    }
}