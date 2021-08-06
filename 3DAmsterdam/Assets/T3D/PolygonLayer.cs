using ConvertCoordinates;
using Netherlands3D.LayerSystem;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using UnityEngine;
using UnityEngine.Networking;

public class PolygonLayer : Layer
{
	public Material LineMaterial;


    public override void HandleTile(TileChange tileChange, Action<TileChange> callback = null)
    {
		TileAction action = tileChange.action;
		var tileKey = new Vector2Int(tileChange.X, tileChange.Y);
		switch (action)
		{
			case TileAction.Create:
				Tile newTile = CreateNewTile(tileChange, callback);
				tiles.Add(tileKey, newTile);
				break;
			case TileAction.Remove:
				InteruptRunningProcesses(tileKey);
				RemoveTile(tileChange, callback);
				return;
			default:
				callback(tileChange);
				break;
		}
	}

	private Tile CreateNewTile(TileChange tileChange, System.Action<TileChange> callback = null)
	{
		Tile tile = new Tile();
		tile.LOD = 0;
		tile.tileKey = new Vector2Int(tileChange.X, tileChange.Y);
		tile.layer = transform.gameObject.GetComponent<Layer>();
		tile.gameObject = new GameObject("perceel-" + tileChange.X + "_" + tileChange.Y);
		tile.gameObject.transform.parent = transform.gameObject.transform;
		tile.gameObject.layer = tile.gameObject.transform.parent.gameObject.layer;
		tile.gameObject.SetActive(false);
		Generate(tileChange, tile, callback);
		return tile;
	}

	public void Generate(TileChange tileChange, Tile tile, System.Action<TileChange> callback = null)
	{
		tile.runningCoroutine = StartCoroutine(BuildLineNetwork(tileChange, tile, callback));
	}

	IEnumerator BuildLineNetwork(TileChange tileChange, Tile tile, Action<TileChange> callback = null)
	{
		var bbox = tile.tileKey.x + "," + tile.tileKey.y + "," + (tile.tileKey.x + tileSize) + "," + (tile.tileKey.y + tileSize);
		string url = $"https://geodata.nationaalgeoregister.nl/kadastralekaart/wfs/v4_0?SERVICE=WFS&REQUEST=GetFeature&VERSION=2.0.0&TYPENAMES=kadastralekaartv4:perceel&STARTINDEX=0&COUNT=1000&SRSNAME=urn:ogc:def:crs:EPSG::28992&BBOX={bbox},urn:ogc:def:crs:EPSG::28992&outputFormat=json";

		List<Vector2[]> list = new List<Vector2[]>();

		tile.runningWebRequest = UnityWebRequest.Get(url);
		yield return tile.runningWebRequest.SendWebRequest();

		if (tile.runningWebRequest.result == UnityWebRequest.Result.Success)
		{
			using (JsonTextReader reader = new JsonTextReader(new StringReader(tile.runningWebRequest.downloadHandler.text)))
			{
				reader.SupportMultipleContent = true;
				var serializer = new JsonSerializer();
				JsonModels.WebFeatureService.WFSRootobject wfs = serializer.Deserialize<JsonModels.WebFeatureService.WFSRootobject>(reader);

				yield return null;

				foreach (var feature in wfs.features)
				{
					List<Vector2> polygonList = new List<Vector2>();

					var coordinates = feature.geometry.coordinates;
					foreach (var points in coordinates)
					{
						foreach (var point in points)
						{
							polygonList.Add(new Vector2(point[0], point[1]));
						}
					}
					list.Add(polygonList.ToArray());
				}

			}

		}


		StartCoroutine (RenderPolygons(list, LineMaterial, tile));

		//Finaly activate our new tile gameobject (if layer is not disabled)
		tile.gameObject.SetActive(isEnabled);

		yield return null;
		callback(tileChange);
	}


	IEnumerator RenderPolygons(List<Vector2[]> polygons, Material material, Tile tile)
	{
		List<Vector2> vertices = new List<Vector2>();
		List<int> indices = new List<int>();

		int count = 0;
		foreach (var list in polygons)
		{
			for (int i = 0; i < list.Length - 1; i++)
			{
				indices.Add(count + i);
				indices.Add(count + i + 1);
			}
			count += list.Length;
			vertices.AddRange(list);			
		}

		GameObject newgameobject = new GameObject();
		newgameobject.transform.transform.SetParent(tile.gameObject.transform, false); ;
		MeshFilter filter = newgameobject.AddComponent<MeshFilter>();
		newgameobject.AddComponent<MeshRenderer>().material = material;

		yield return null;

		var mesh = new Mesh();
		mesh.vertices = vertices.Select(o => CoordConvert.RDtoUnity(o)).ToArray();
		mesh.SetIndices(indices.ToArray(), MeshTopology.Lines, 0);
		filter.sharedMesh = mesh;
	}

	private void RemoveTile(TileChange tileChange, System.Action<TileChange> callback = null)
	{
		var tileKey = new Vector2Int(tileChange.X, tileChange.Y);
		if (tiles.ContainsKey(tileKey))
		{
			Tile tile = tiles[tileKey];
			if (tile.gameObject)
			{
				MeshFilter[] meshFilters = tile.gameObject.GetComponentsInChildren<MeshFilter>();

				foreach (var meshfilter in meshFilters)
				{
					Destroy(meshfilter.sharedMesh);
				}

				Destroy(tile.gameObject);
			}

			tiles.Remove(tileKey);
		}
		callback(tileChange);
	}

}
