using UnityEngine;
using UnityEngine.Events;

namespace Netherlands3D.LayerSystem
{
	[CreateAssetMenu(fileName = "TileHandlerConfig", menuName = "ScriptableObjects/TileHandlerConfig", order = 0)]
	[System.Serializable]
	public class TileHandlerConfig : ScriptableObject
	{
		[HideInInspector]
		public UnityEvent dataChanged = new UnityEvent();

		public Binarymeshlayer[] binaryMeshLayers;
		public Geojsonlayer[] geoJsonLayers;

		public void OnValidate()
		{
			dataChanged.Invoke();
		}

		[System.Serializable]
		public class Binarymeshlayer
		{
			public string layerName;
			public bool selectableSubobjects;
			public Lod[] lods;
			public int[] materialLibraryIndices;
			public bool maskable;
			public bool visible;
		}
		[System.Serializable]
		public class Geojsonlayer
		{
			public string layerName;
			public string sourcePath;
			public bool drawOutlines;
			public string lineColor;
			public float lineWidth;
			public bool filterUniqueNames;
			public bool faceCamera;
			public string positionSourceType;
			public Text[] texts;
			public bool visible;
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
			public int[] offset;
		}
	}
}