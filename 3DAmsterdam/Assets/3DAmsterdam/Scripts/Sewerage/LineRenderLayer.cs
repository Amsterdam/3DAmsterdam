using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ConvertCoordinates;
using UnityEngine.Networking;
using System.Globalization;
using System;
using Netherlands3D.LayerSystem;
using Netherlands3D.Utilities;
using Netherlands3D;
using System.Linq;

namespace Amsterdam3D.Sewerage
{
	public class LineRenderLayer : Layer
    {
		[SerializeField]
		private ColorPalette nlcsColorPalette;

		[SerializeField]
		private Material lineRendererMaterial;

		public override void OnDisableTiles(bool isenabled) { }

		public override void HandleTile(TileChange tileChange, System.Action<TileChange> callback = null)
        {
			TileAction action = tileChange.action;
			var tileKey = new Vector2Int(tileChange.X, tileChange.Y);
			switch (action)
			{
				case TileAction.Create:
					Tile newTile = CreateNewTile(tileChange,callback);
					tiles.Add(tileKey, newTile);
					break;
				case TileAction.Remove:
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
			tile.gameObject = new GameObject("cablesAndPipes-"+tileChange.X + "_" + tileChange.Y);
			tile.gameObject.transform.parent = transform.gameObject.transform;
			tile.gameObject.layer = tile.gameObject.transform.parent.gameObject.layer;
			tile.gameObject.SetActive(false);
			Generate(tileChange, tile, callback);
			return tile;
		}

		public void Generate(TileChange tileChange,Tile tile, System.Action<TileChange> callback = null)
		{
			StartCoroutine(BuildInfrastructure(tileChange, tile,callback));
		}

		IEnumerator BuildInfrastructure(TileChange tileChange, Tile tile, Action<TileChange> callback = null)
		{
			var bbox = tile.tileKey.x + "," + tile.tileKey.y + "," + (tile.tileKey.x + tileSize) + "," + (tile.tileKey.y + tileSize);
			int startIndex = 0;
			var pagesRemaining = true;
			int maxResultsCount = 5000;

			var mesh = new Mesh();
			List<int> indices = new List<int>();
			List<Vector3> vertices = new List<Vector3>();
			List<Color> colors = new List<Color>();

			while (pagesRemaining)
			{
				var url = $"https://api.data.amsterdam.nl/v1/wfs/leidingeninfrastructuur/?SERVICE=WFS&VERSION=2.0.0&REQUEST=GetFeature&TYPENAMES=ligging_lijn_totaal&OUTPUTFORMAT=application/json&BBox={bbox}&count={maxResultsCount}&startIndex={startIndex}";
				Debug.Log(url);
				using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
				{
					yield return webRequest.SendWebRequest();

					if (webRequest.result == UnityWebRequest.Result.Success)
					{
						GeoJSON customJsonHandler = new GeoJSON(webRequest.downloadHandler.text);

						//Determine if there is a page after this
						if (customJsonHandler.geoJSONString.LastIndexOf("\"title\":\"next page\"") > -1)
						{
							startIndex += maxResultsCount - 1;
						}
						else
						{
							pagesRemaining = false;
						}

						yield return null;

						while (customJsonHandler.GotoNextFeature())
						{
							//Get the parameters that make up our feature line
							List<double> coordinates = customJsonHandler.getGeometryLineString();
							float depth = customJsonHandler.getPropertyFloatValue("diepte");
							var lineColor = GetNLCSColor(customJsonHandler.getPropertyStringValue("thema"));

							//Min. of two points? This is a line we can draw!
							if (coordinates.Count > 1)
							{
								//For every two coordinates
								for (int i = 0; i < coordinates.Count/2; i+=2)
								{
									var point = CoordConvert.RDtoUnity(new Vector3((float)coordinates[i], (float)coordinates[i + 1], depth));
									vertices.Add(point);
									colors.Add(lineColor);
									indices.Add(vertices.Count-1);
								}								
							}
						}
					}
					else
					{
						pagesRemaining = false;
					}
				}
			}

			Debug.Log("Constructing line mesh");

			yield return null;

			//All lines are read into mesh. Lets finish it
			mesh.vertices = vertices.ToArray();
			mesh.colors = colors.ToArray();
			mesh.SetIndices(indices, MeshTopology.Lines, 0, true);

			tile.gameObject.AddComponent<MeshFilter>().sharedMesh = mesh;
			tile.gameObject.AddComponent<MeshRenderer>().material = lineRendererMaterial;

			tile.gameObject.SetActive(true);

			callback(tileChange);
		}

		private Color GetNLCSColor(string theme)
		{
			//return color based on template
			return nlcsColorPalette.colors[UnityEngine.Random.Range(0, nlcsColorPalette.colors.Count)].color;
		}

		private void RemoveTile(TileChange tileChange, System.Action<TileChange> callback = null)
		{
			var tileKey = new Vector2Int(tileChange.X, tileChange.Y);
			if (tiles.ContainsKey(tileKey))
			{
				Tile tile = tiles[tileKey];
				if (tile.gameObject)
				{
					MeshFilter[] meshFilters = tile.gameObject.GetComponents<MeshFilter>();

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
}