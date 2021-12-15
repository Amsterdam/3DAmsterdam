using Netherlands3D.Events;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class ImportGeoJSON : MonoBehaviour
{
    [SerializeField]
    private StringEvent filesImportedEvent;

    [SerializeField]
    private BoolEvent clearDataBaseEvent;

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

	}
}
