using ConvertCoordinates;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Netherlands3D.Utilities;

namespace Netherlands3D.LayerSystem
{
	public class StreetnamesLayer : Layer
	{
		public GameObject TextObject;
		private string baseURL = "https://geodata.nationaalgeoregister.nl/kadastralekaart/wfs/v4_0?service=WFS&version=2.0.0&request=GetFeature&TypeNames=kadastralekaartv4:openbareruimtenaam&&propertyName=plaatsingspunt,tekst,hoek,relatieveHoogteligging,openbareRuimteType&outputformat=geojson&srs=EPSG:28992&bbox=";//121000,488000,122000,489000";
		public override void HandleTile(TileChange tileChange, System.Action<TileChange> callback = null)
		{
			TileAction action = tileChange.action;
			var tileKey = new Vector2Int(tileChange.X, tileChange.Y);
			switch (action)
			{
				case TileAction.Create:
					Tile newTile = CreateNewTile(tileKey);
					tiles.Add(tileKey, newTile);
					newTile.runningCoroutine = StartCoroutine(DownloadStreetNameData(tileChange, newTile, callback));
					break;
				case TileAction.Upgrade:
					tiles[tileKey].LOD++;
					//callback(tileChange);
					break;
				case TileAction.Downgrade:
					tiles[tileKey].LOD--;
					//callback(tileChange);
					break;
				case TileAction.Remove:
					InteruptRunningProcesses(tileKey);
					RemoveGameObjectFromTile(tileKey);
					tiles.Remove(tileKey);
					callback(tileChange);
					return;
				default:
					break;
			}
			
		}



		private Tile CreateNewTile(Vector2Int tileKey)
		{
			Tile tile = new Tile();
			tile.LOD = 0;
			tile.tileKey = tileKey;
			tile.layer = transform.gameObject.GetComponent<Layer>();
			tile.gameObject = new GameObject();
			tile.gameObject.transform.parent = transform.gameObject.transform;
			tile.gameObject.layer = tile.gameObject.transform.parent.gameObject.layer;
			tile.gameObject.transform.position = CoordConvert.RDtoUnity(tileKey);

			return tile;
		}
		private void RemoveGameObjectFromTile(Vector2Int tileKey)
		{
			if (tiles.ContainsKey(tileKey))
			{

				Tile tile = tiles[tileKey];
				if (tile == null)
				{
					return;
				}
				if (tile.gameObject == null)
				{
					return;
				}
				MeshFilter mf = tile.gameObject.GetComponent<MeshFilter>();
				if (mf != null)
				{
					DestroyImmediate(tile.gameObject.GetComponent<MeshFilter>().sharedMesh, true);
				}
				Destroy(tiles[tileKey].gameObject);

			}
		}
			private IEnumerator DownloadStreetNameData(TileChange tileChange, Tile tile, System.Action<TileChange> callback = null)
		{
			string url = baseURL + (tileChange.X.ToString() + "," + tileChange.Y.ToString() + "," + (tileChange.X + tileSize).ToString() + "," + (tileChange.Y + tileSize).ToString());
			Debug.Log(url);

			var streetnameRequest = UnityWebRequest.Get(url);

			
			yield return streetnameRequest.SendWebRequest();

			if (!streetnameRequest.isNetworkError && !streetnameRequest.isHttpError)
			{
				
				GeoJSON customJsonHandler = new GeoJSON(streetnameRequest.downloadHandler.text);
				yield return null;
				Vector3 startpoint;
				Vector3 endpoint;
				int parseCounter = 0;

				while (customJsonHandler.GotoNextFeature())
				{
					parseCounter++;
					//if ((parseCounter % maxParsesPerFrame) == 0) yield return null;
					float angle = customJsonHandler.getPropertyFloatValue("hoek");
					string name = customJsonHandler.getPropertyStringValue("tekst");

					List<double> coordinates = customJsonHandler.getGeometryLineString();
					double[] coordinate = customJsonHandler.getGeometryPoint2DDouble();
					startpoint = CoordConvert.RDtoUnity(new Vector2RD(coordinate[0], coordinate[1]));
					var To = Instantiate(TextObject);
					To.transform.parent = tile.gameObject.transform;
					To.GetComponent<TextMesh>().text = name;
					To.transform.position = startpoint;
					To.transform.Rotate(Vector3.up, angle,Space.World);

				}

				yield return null;
			}
			callback(tileChange);
		}
	}
}