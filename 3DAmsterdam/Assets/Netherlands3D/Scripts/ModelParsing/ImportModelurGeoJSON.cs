using Netherlands3D.Events;
using Netherlands3D.Core;
using Netherlands3D.Utilities;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class ImportModelurGeoJSON : MonoBehaviour
{
	[Header("Listen to events:")]
    [SerializeField]
    private StringEvent filesImportedEvent;

	[Header("Trigger events:")]
	[SerializeField]
    private BoolEvent onDoneParsing;
	[SerializeField]
	private FloatEvent OnStoreyHeightDetermined;
	[SerializeField]
	private FloatEvent OnStoreyDividerHeightDetermined;
	[SerializeField]
	private Vector3ListsEvent OnPolygonParsed;
	[SerializeField]
	private Vector3ListsEvent OnStoreyDividerParsed;
	[SerializeField]
	private Vector3Event onCentroidCalculated;

	[SerializeField]
	private ObjectEvent onObjectReady;

	private List<Dictionary<string, object>> propertyList = new List<Dictionary<string, object>>();

	[SerializeField]
	private float storeyDividerHeight = 0.2f;

	[SerializeField]
	private bool combineIntoOneMesh = true;

	private Texture2D colorMappedTexture;
	private int colorSlots = 8;

	private void Awake()
	{
		filesImportedEvent.AddListenerStarted(FileImported);
	}

	private void FileImported(string files)
	{
		string[] importedFiles = files.Split(',');
		foreach (var file in importedFiles)
		{
			if (file.ToLower().EndsWith(".json") || file.ToLower().EndsWith(".geojson"))
			{
				ParseGeoJSON(file);
				return;
			}
		}
	}

	public void ParseGeoJSON(string file)
	{
		var filePath = file;
		if (!Path.IsPathRooted(filePath))
		{
			Debug.Log($"{filePath} is relative. Appended persistentDataPath.");
			filePath = Application.persistentDataPath + "/" + file;
		}

		if(!File.Exists(filePath))
		{
			Debug.Log($"{filePath} not found");
			return;
		}

		Debug.Log($"Parsing {filePath}");
		var json = File.ReadAllText(filePath);
		GeoJSON geojson = new GeoJSON(json);
		float objectIndex = 0;

		var newParent = new GameObject();
		newParent.name = "";
		Vector3 centroid = Vector3.zero;
		int amountOfPoints = 0;
		while (geojson.GotoNextFeature())
		{
			objectIndex++;

			var polygons = geojson.GetPolygon();
			var properties = geojson.GetProperties();
			propertyList.Add(properties);

			//Determine our storeys + height
			if(newParent.name == ""){
				var projectName = geojson.GetPropertyStringValue("#LayerName");
				newParent.name = projectName;
			}

			var buildingHeight = geojson.GetPropertyFloatValue("Building_Height");
			var numberOfStoreys = geojson.GetPropertyFloatValue("Number_of_Storeys");
			var groundOffset  = geojson.GetPropertyFloatValue("Ground_to_Sea_Elevation"); 
			var storeyHeight = (buildingHeight / numberOfStoreys) - storeyDividerHeight;

			OnStoreyHeightDetermined.InvokeStarted(storeyHeight);
			OnStoreyDividerHeightDetermined.InvokeStarted(storeyDividerHeight);

			//Draw all polygons as building storeys
			for (int i = 0; i < numberOfStoreys; i++)
			{
				foreach (var polygon in polygons)
				{
					DrawStorey(ref centroid, ref amountOfPoints, groundOffset, storeyHeight, i, polygon);
				}
			}
		}

		centroid /= amountOfPoints;
		centroid.y = 0;
		onCentroidCalculated.InvokeStarted(centroid);

		//Collect this building
		newParent.transform.position = centroid;
		var children = GetComponentsInChildren<Transform>();
		foreach (Transform child in children)
		{
			if (child != this.transform)
				child.SetParent(newParent.transform);
		}

		if(combineIntoOneMesh)
		{
			CombineIntoOne(newParent);
		}

		onDoneParsing.InvokeStarted(true);
		onObjectReady.InvokeStarted(newParent);
	}

	private void DrawStorey(ref Vector3 centroid, ref int amountOfPoints, float groundOffset, float storeyHeight, int i, List<GeoJSONPoint> polygon)
	{
		List<List<Vector3>> polyList = new List<List<Vector3>>();
		List<Vector3> outerContour = new List<Vector3>();

		foreach (var point in polygon)
		{
			var unityPoint = CoordConvert.WGS84toUnity(point.x, point.y);
			unityPoint.y = groundOffset + (i * storeyHeight) + (i * storeyDividerHeight);
			outerContour.Add(unityPoint);

			centroid += unityPoint;
			amountOfPoints++;
		}
		polyList.Add(outerContour);

		OnPolygonParsed.InvokeStarted(polyList);

		//Draw the same polygon shape as a storey dividing line
		for (int j = 0; j < outerContour.Count; j++)
		{
			outerContour[j] = new Vector3(outerContour[j].x, outerContour[j].y - storeyDividerHeight, outerContour[j].z);
		}

		OnStoreyDividerParsed.InvokeStarted(polyList);
	}

	private void CombineIntoOne(GameObject target){
		MeshFilter[] meshFilters = target.GetComponentsInChildren<MeshFilter>();
		CombineInstance[] combine = new CombineInstance[meshFilters.Length];

		List<Material> childMaterials = new List<Material>();

		int i = 0;
		while (i < meshFilters.Length)
		{
			combine[i].mesh = meshFilters[i].sharedMesh;
			combine[i].transform = target.transform.worldToLocalMatrix * meshFilters[i].transform.localToWorldMatrix;
			childMaterials.AddRange(meshFilters[i].gameObject.GetComponent<MeshRenderer>().sharedMaterials);
			i++;
		}

		List<Material> uniqueMaterials = childMaterials.Distinct().ToList();

		var meshFilter = target.AddComponent<MeshFilter>();
		var meshRenderer = target.AddComponent<MeshRenderer>();
		meshRenderer.materials = uniqueMaterials.ToArray();
		var newMesh = new Mesh();
		newMesh.CombineMeshes(combine,true);
		meshFilter.mesh = newMesh;

		//Cleanup
		for (int j = meshFilters.Length - 1; j >= 0; j--)
		{
			Destroy(meshFilters[j].sharedMesh);
			Destroy(meshFilters[j].gameObject);
		}
	}
}
