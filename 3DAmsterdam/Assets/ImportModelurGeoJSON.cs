using Netherlands3D.Events;
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
	private Vector3ListsEvent OnPolygonsParsed;
	[SerializeField]
	private Vector3Event onCentroidCalculated;

	private List<Dictionary<string, object>> propertyList = new List<Dictionary<string, object>>();

	private void Awake()
	{
		filesImportedEvent.started.AddListener(FileImported);
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
		if (!file.Contains("/"))
		{
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
		float count = 0;

		Vector3 centroid = Vector3.zero;
		int amountOfPoints = 0;
		while (geojson.GotoNextFeature())
		{
			count++;

			var polygons = geojson.GetPolygon();
			var properties = geojson.GetProperties();
			propertyList.Add(properties);

			foreach (var polygon in polygons)
			{
				List<IList<Vector3>> polyList = new List<IList<Vector3>>();
				List<Vector3> list = new List<Vector3>();
				foreach (var point in polygon)
				{
					var p = ConvertCoordinates.CoordConvert.WGS84toUnity(point.x, point.y);
					p.y = count; //Use y as object index
					list.Add(p);

					centroid += p;
					amountOfPoints++;
				}

				polyList.Add(list);


				OnPolygonsParsed.started?.Invoke(polyList);
			}
		}

		centroid /= amountOfPoints;
		centroid.y = 0;
		onCentroidCalculated.started.Invoke(centroid);

		onDoneParsing.started.Invoke(true);
	}
}
