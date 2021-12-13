using Netherlands3D.Events;
using Netherlands3D.LayerSystem;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

[HelpURL("https://3d.amsterdam.nl/netherlands3d/help#LayersLoader")]
public class LayersLoader : MonoBehaviour
{
	[SerializeField]
	private TileHandlerConfig configuration;
	private TileHandler tileHandler;
	
	[Header("Visuals")]
	[SerializeField]
	private Material[] materialSlots;
	[SerializeField]
	private GameObject textIntersectPrefab;
	[SerializeField]
	private GameObject textOverlayPrefab;
	[SerializeField]
	private Material lineShader;
	void Start()
	{
		configuration.dataChanged.AddListener(ConstructLayers);
		StartCoroutine(LoadExternalConfig());
	}

	private void ConstructLayers()
	{
		tileHandler = gameObject.AddComponent<TileHandler>();

		foreach (var binaryMeshLayer in configuration.binaryMeshLayers)
		{
			var newLayer = new GameObject().AddComponent<BinaryMeshLayer>();
			newLayer.transform.SetParent(this.transform);
			newLayer.layerPriority = binaryMeshLayer.priority;
			newLayer.name = binaryMeshLayer.layerName;
			if (binaryMeshLayer.selectableSubobjects)
			{
				newLayer.gameObject.AddComponent<SelectSubObjects>();
			}

			foreach(var materialIndex in binaryMeshLayer.materialLibraryIndices)
			{
				newLayer.DefaultMaterialList.Add(materialSlots[materialIndex]);
			}

			for(int i = 0; i< binaryMeshLayer.lods.Length; i++)
			{
				var lod = binaryMeshLayer.lods[i];
				newLayer.Datasets.Add(
					new DataSet()
					{
						lod = i,
						maximumDistance = lod.drawDistance,
						path = lod.sourcePath
					}
				); 
			}
			tileHandler.layers.Add(newLayer);
		}

		foreach (var geoJsonLayer in configuration.geoJsonLayers)
		{
			var newLayer = new GameObject().AddComponent<GeoJSONTextLayer>();
			newLayer.transform.SetParent(this.transform);
			newLayer.name = geoJsonLayer.layerName;
			newLayer.textPrefab = (geoJsonLayer.overlay) ? textOverlayPrefab : textIntersectPrefab;
			newLayer.lineRenderMaterial = lineShader;

			ColorUtility.TryParseHtmlString(geoJsonLayer.lineColor, out Color lineColor);
			newLayer.lineColor = lineColor;
			newLayer.layerPriority = geoJsonLayer.priority;
			newLayer.geoJsonUrl = geoJsonLayer.sourcePath;
			newLayer.drawGeometry = geoJsonLayer.drawOutlines;
			newLayer.filterUniqueNames = geoJsonLayer.filterUniqueNames;
			if (geoJsonLayer.angleProperty != "")
			{
				newLayer.readAngleFromProperty = true;
				newLayer.angleProperty = geoJsonLayer.angleProperty;
			}
			newLayer.SetAutoOrientationMode(geoJsonLayer.autoOrientationMode);
			newLayer.SetPositionSourceType(geoJsonLayer.positionSourceType);
			newLayer.Datasets.Add(new DataSet() { maximumDistance = geoJsonLayer.drawDistance });
			foreach (var text in geoJsonLayer.texts)
			{
				newLayer.textsAndSizes.Add(new GeoJSONTextLayer.TextsAndSize()
				{
					textPropertyName = text.propertyName,
					drawWithSize = text.size,
					offset = text.offset[1]
				});
			}

			tileHandler.layers.Add(newLayer);
		}
	}

	public void LoadConfig(string jsonConfig)
	{
		JsonUtility.FromJsonOverwrite(jsonConfig, configuration);
		configuration.dataChanged.Invoke();
	}

	IEnumerator LoadExternalConfig()
	{
		var streamingAssetsConfigPath = Application.streamingAssetsPath + "/" + configuration.configFile;
		Debug.Log($"Loading layers config file: {streamingAssetsConfigPath}");
#if UNITY_WEBGL && !UNITY_EDITOR
        UnityWebRequest webRequest = UnityWebRequest.Get(streamingAssetsConfigPath);
        
        yield return webRequest.SendWebRequest();
        if (webRequest.result == UnityWebRequest.Result.Success)
        {
            LoadConfig(webRequest.downloadHandler.text);
        }
        else
        {
            Debug.Log($"Could not load {configuration.configFilePath}");
        }
        yield return null;
#else
		if (!File.Exists(streamingAssetsConfigPath))
		{
			Debug.Log($"Could not load {configuration.configFile}");
			yield break;
		}
		LoadConfig(File.ReadAllText(streamingAssetsConfigPath));
		yield return null;
#endif
	}
}
