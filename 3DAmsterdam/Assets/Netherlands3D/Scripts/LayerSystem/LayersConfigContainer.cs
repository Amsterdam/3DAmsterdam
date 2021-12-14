using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Netherlands3D.LayerSystem
{
    public class LayersConfigContainer : ScriptableObject
    {
        [SerializeField]
        TileHandlerConfig configuration;

        public void LoadConfig(string jsonConfig)
        {
            JsonUtility.FromJsonOverwrite(jsonConfig, configuration);
        }
    }
}