using UnityEngine;
using UnityEngine.Events;

namespace Netherlands3D.LayerSystem
{
	[CreateAssetMenu(fileName = "TileHandlerConfig", menuName = "ScriptableObjects/TileHandlerConfig", order = 0)]
	[System.Serializable]
	public class TileHandlerConfig : ScriptableObject
	{
		public string configFile = "TileHandlerConfig.json";

		[HideInInspector]
		public UnityEvent dataChanged = new UnityEvent();

		public Binarymeshlayer[] binaryMeshLayers;
		public Geojsonlayer[] geoJsonLayers;

		public void OnValidate()
		{
			if(Application.isPlaying)
				dataChanged.Invoke();
		}

		[System.Serializable]
		public class Binarymeshlayer
		{
			public string layerName = "Binary meshes layer";
			public int priority = 1;
			public bool selectableSubobjects;
			public Lod[] lods;
			public int[] materialLibraryIndices;
			public bool visible = true;
		}
		[System.Serializable]
		public class Geojsonlayer
		{
			public string layerName = "GeoJson layer";
			public int priority = 1;
			public string sourcePath = "";
			public bool drawOutlines = true;
			public bool overlay = true;
			public string lineColor = "#D26432";
			public float lineWidth = 5.0f;
			public bool filterUniqueNames = true;
			public string positionSourceType = "Point";
			public string autoOrientationMode = "FaceCamera";
			public string angleProperty = "";
			public int drawDistance = 1000;
			public Text[] texts;
			public bool visible = true;
		}
		[System.Serializable]
		public class Lod
		{
			public string LOD;
			public string sourcePath;
			public int drawDistance;
		}
		[System.Serializable]

		public class Text
		{
			public string propertyName;
			public float size;
			public float[] offset;
		}
	}
}