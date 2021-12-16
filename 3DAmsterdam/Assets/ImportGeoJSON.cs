using Netherlands3D.Events;
using Netherlands3D.Utilities;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class ImportGeoJSON : MonoBehaviour
{
	[Header("Listen to events:")]
    [SerializeField]
    private StringEvent filesImportedEvent;

	[Header("Trigger events:")]
	[SerializeField]
    private BoolEvent clearDataBaseEvent;
	[SerializeField]
	private Vector3ListsEvent drawGeometryEvent;

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
		if(!File.Exists(file))
		{
			Debug.Log($"{file} not found");
			return;
		}

		var json = File.ReadAllText(file);
		GeoJSON geojson = new GeoJSON(json);
		float count = 0;
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
					p.y = count;
					list.Add(p);
				}

				polyList.Add(list);

				drawGeometryEvent.started?.Invoke(polyList);
			}
		}
		clearDataBaseEvent.started.Invoke(true);
	}
}
