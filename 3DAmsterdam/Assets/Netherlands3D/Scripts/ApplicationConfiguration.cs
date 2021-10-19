﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Netherlands3D
{
    [ExecuteInEditMode]
    public class ApplicationConfiguration : MonoBehaviour
    {
        [Tooltip("Specific configuration file with custom external data paths and viewer options.\nTo create a config file right click in the assets menu and select ScriptableObjects/ConfigurationFile")]

        [SerializeField]
        private ConfigurationFile configurationFile;

        [Header("Optional acceptance config")]
        [SerializeField]
        private ConfigurationFile acceptanceConfigurationFile;

		public ConfigurationFile ConfigurationFile { get => configurationFile; }

		void Awake()
        {   
            #if DEVELOPMENT
            Config.activeConfiguration = (acceptanceConfigurationFile) ? acceptanceConfigurationFile : configurationFile;
#elif PRODUCTION
            Config.activeConfiguration = configurationFile;
#endif
            Debug.Log("Loaded config: " + Config.activeConfiguration.name);
        }
    }
}
