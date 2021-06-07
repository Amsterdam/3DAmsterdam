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
			StartCoroutine(BuildInfrastructure(tileChange));
		}

		IEnumerator BuildInfrastructure(TileChange tileChange)
		{
			var bbox = tileChange.X + "," + tileChange.Y + "," + (tileChange.X + tileSize) + "," + (tileChange.Y + tileSize);
			int startIndex = 0;
			var pagesRemaining = true;

			var mesh = new Mesh();
			List<Vector3> vertices = new List<Vector3>();
			List<Vector3> colors = new List<Vector3>();

			while (pagesRemaining)
			{
				var url = $"https://api.data.amsterdam.nl/v1/wfs/leidingeninfrastructuur/?SERVICE=WFS&VERSION=2.0.0&REQUEST=GetFeature&TYPENAMES=ligging_lijn_totaal&OUTPUTFORMAT=application/gml+xml&BBox={bbox}&count=5000&startIndex={startIndex}";
				using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
				{
					yield return webRequest.SendWebRequest();

					if (webRequest.result == UnityWebRequest.Result.Success)
					{
						GeoJSON customJsonHandler = new GeoJSON(webRequest.downloadHandler.text);
						yield return null;

						Vector3 startPoint;
						Vector3 endPoint;

						while (customJsonHandler.GotoNextFeature())
						{
							List<double> coordinates = customJsonHandler.getGeometryLineString();
							float depth = customJsonHandler.getPropertyFloatValue("diepte");
							//var lineColor = customJsonHandler.getPropertyFloatValue
							var lineColor = GetNLCSColor(customJsonHandler.getPropertyStringValue("thema"));

							startPoint = CoordConvert.RDtoUnity(new Vector3((float)coordinates[0]- tileChange.X, (float)coordinates[1]-tileChange.Y, depth));
							endPoint = CoordConvert.RDtoUnity(new Vector3((float)coordinates[0], (float)coordinates[1], depth));

							vertices.Add(startPoint);
							colors.Add(endPoint);
						}
					}
					else
					{
						pagesRemaining = false;
					}
				}
			}

			mesh.SetIndices(new int[] { 0, 1, 2, 3, 4, 5 }, MeshTopology.Lines, 0, true);
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