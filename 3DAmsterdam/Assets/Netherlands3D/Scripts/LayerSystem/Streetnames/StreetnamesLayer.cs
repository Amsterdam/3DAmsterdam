using ConvertCoordinates;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Netherlands3D.Utilities;
using System.Globalization;
using TMPro;

namespace Netherlands3D.LayerSystem
{
	public class StreetNamesLayer : Layer
	{
		public GameObject TextObject;
		private string baseURL = "https://geodata.nationaalgeoregister.nl/kadastralekaart/wfs/v4_0?service=WFS&version=2.0.0&request=GetFeature&TypeNames=kadastralekaartv4:openbareruimtenaam&&propertyName=plaatsingspunt,tekst,hoek,relatieveHoogteligging,openbareRuimteType&outputformat=geojson&srs=EPSG:28992&bbox=";//121000,488000,122000,489000";
		
		[SerializeField]
		private float offsetFromGround = 10.0f;

		[SerializeField]
		private int maxSpawnsPerFrame = 100;

		public override void LayerToggled()
		{
			base.LayerToggled();
			gameObject.SetActive(isEnabled);
		}

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
			string url = $"{baseURL}{tileChange.X},{tileChange.Y},{(tileChange.X + tileSize)},{(tileChange.Y + tileSize)}";
			Debug.Log(url);

			var streetnameRequest = UnityWebRequest.Get(url);
			tile.runningWebRequest = streetnameRequest;
			yield return streetnameRequest.SendWebRequest();

			if (streetnameRequest.result == UnityWebRequest.Result.Success)
			{	
				GeoJSON customJsonHandler = new GeoJSON(streetnameRequest.downloadHandler.text);
				yield return null;
				Vector3 startpoint;
				Vector3 endpoint;
				int parseCounter = 0;

				while (customJsonHandler.GotoNextFeature())
				{
					parseCounter++;
					if ((parseCounter % maxSpawnsPerFrame) == 0) yield return null;
					float angle = customJsonHandler.getPropertyFloatValue("hoek");
					string name = customJsonHandler.getPropertyStringValue("tekst");

					List<double> coordinates = customJsonHandler.getGeometryLineString();

					double[] coordinate = customJsonHandler.getGeometryPoint2DDouble();
					startpoint = CoordConvert.RDtoUnity(new Vector2RD(coordinate[0], coordinate[1]));
					startpoint.y = offsetFromGround;
					var textObject = Instantiate(TextObject);
					textObject.transform.SetParent(tile.gameObject.transform,true);
					textObject.GetComponent<TextMeshPro>().text = name;
					textObject.transform.position = startpoint;
					textObject.transform.Rotate(Vector3.left, -90,Space.Self);
					textObject.transform.Rotate(Vector3.up, angle,Space.World);
				}
				yield return null;
			}
			callback(tileChange);
		}
	}
}