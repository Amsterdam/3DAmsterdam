using Netherlands3D.Events;
using Netherlands3D.LayerSystem;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

public class LayersLoader : MonoBehaviour
{
    [SerializeField]
    private string externalConfigPath = "layersConfig.json";

    [SerializeField]
    private TileHandlerConfig configuration;

    void Start()
    {
       configuration.dataChanged.AddListener(ConstructLayers);
       StartCoroutine(LoadExternalConfig());
    }

    private void ConstructLayers()
    {
        var tileHandler = gameObject.AddComponent<TileHandler>();
        foreach (var binaryMeshLayer in configuration.binaryMeshLayers)
        {
            var newLayer = new GameObject().AddComponent<BinaryMeshLayer>();
        }
        foreach(var geoJsonLayer in configuration.geoJsonLayers)
        {
            
		}
	}


    public void LoadConfig(string jsonConfig)
    {
        JsonUtility.FromJsonOverwrite(jsonConfig, configuration);
        configuration.dataChanged.Invoke();
    }

	IEnumerator LoadExternalConfig()
	{
        var streamingAssetsConfigPath = Application.streamingAssetsPath + "/" + externalConfigPath;

#if UNITY_WEBGL && !UNITY_EDITOR
        UnityWebRequest webRequest = UnityWebRequest.Get(streamingAssetsConfigPath);
        
        yield return webRequest.SendWebRequest();
        if (webRequest.result == UnityWebRequest.Result.Success)
        {
            LoadConfig(webRequest.downloadHandler.text);
        }
        else
        {
            Debug.Log($"Could not load {externalConfigPath}");
        }
        yield return null;
#else
        if (!File.Exists(streamingAssetsConfigPath))
        { 
            Debug.Log($"Could not load {externalConfigPath}");
            yield break;
        }
        LoadConfig(File.ReadAllText(streamingAssetsConfigPath));
        yield return null;
#endif      
    }
}
