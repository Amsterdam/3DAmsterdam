using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Netherlands3D.Core;
using System;
using UnityEngine.Networking;
using System.IO;
using System.Globalization;

namespace Netherlands3D
{
    [ExecuteInEditMode]
    public class ApplicationConfiguration : MonoBehaviour
    {
        [SerializeField]
        private bool loadExternalConfigFile = true;
        [SerializeField]
        private string externalConfigFilePath = "/";

        [Tooltip("Specific configuration file with custom external data paths and viewer options.\nTo create a config file right click in the assets menu and select ScriptableObjects/ConfigurationFile")]

        [SerializeField]
        private ConfigurationFile configurationFile;

		public ConfigurationFile ConfigurationFile { get => configurationFile; }

        [SerializeField]
        private bool blockOtherInputInHTML = true;

        void Awake()
        {
#if !UNITY_EDITOR && UNITY_WEBGL
            // disable WebGLInput.captureAllKeyboardInput so elements in web page can handle keyboard inputs
            WebGLInput.captureAllKeyboardInput = blockOtherInputInHTML;
#endif

            CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;

            SetConfig();

            if (loadExternalConfigFile)
            {
                Config.isLoadingOverrides = true;
                LoadConfig();
			}
            else
			{
                Debug.Log("Using hardcoded config: " + Config.activeConfiguration.name);
			}
		}


        private IEnumerator LoadExternalConfig()
        {
            using (UnityWebRequest request = UnityWebRequest.Get(externalConfigFilePath))
            {
                yield return request.SendWebRequest();
                if (request.result == UnityWebRequest.Result.Success)
                {
                    Debug.Log($"Successfully downloaded external config: {externalConfigFilePath}");
                    var json = request.downloadHandler.text;
                    JsonUtility.FromJsonOverwrite(json, configurationFile);
                }
                else{
                    Debug.Log($"Could not load: {externalConfigFilePath}. Using default config.");
                }

                Config.isLoadingOverrides = false;

                SetConfig();
            }
        }
        private void SetConfig()
        {
            Config.activeConfiguration = configurationFile;
            ApplySettings();
        }

        private void ApplySettings()
		{
            CoordConvert.zeroGroundLevelY = Config.activeConfiguration.zeroGroundLevelY;
            CoordConvert.relativeCenterRD = Config.activeConfiguration.RelativeCenterRD;
        }

        [ContextMenu("Load config from json file")]
        private void LoadConfig()
        {
            StartCoroutine(LoadExternalConfig());
        }

        [ContextMenu("Write config to json file")]
        public void WriteConfigToJson()
        {
            var jsonText = JsonUtility.ToJson(configurationFile, true);
            var filePath = Application.dataPath + "/app-config.json";
            File.WriteAllText(filePath, jsonText);
            Debug.Log($"{filePath} created");
        }
    }
}
