using ConvertCoordinates;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Netherlands3D.Utilities;
using System.Globalization;
using TMPro;
using Netherlands3D.Interface;

namespace Netherlands3D.LayerSystem
{
	public class GeoJSONTextLayer : Layer
	{
		public GameObject TextObject;

		[SerializeField]
		private string geoJsonUrl = "https://geodata.nationaalgeoregister.nl/kadastralekaart/wfs/v4_0?service=WFS&version=2.0.0&request=GetFeature&TypeNames=kadastralekaartv4:openbareruimtenaam&&propertyName=plaatsingspunt,tekst,hoek,relatieveHoogteligging,openbareRuimteType&outputformat=geojson&srs=EPSG:28992&bbox=";//121000,488000,122000,489000";
		
		[SerializeField]
		private float offsetFromGround = 10.0f;

		[SerializeField]
		private int maxSpawnsPerFrame = 100;

		[SerializeField]
		private string textProperty = "tekst";

		[SerializeField]
		private PositionSourceType positionSourceType = PositionSourceType.Point;

		[SerializeField]
		private AutoOrientationMode autoOrientationMode = AutoOrientationMode.AutoFlip;

		[Header("Optional:")]
		[SerializeField]
		private bool readAngleFromProperty = false;
		[SerializeField]
		private string angleProperty = "hoek";

		private enum AutoOrientationMode
		{
			None,
			FaceToCamera,
			AutoFlip
		}

		private enum PositionSourceType
		{
			Point,
			MultiPolygonCentroid
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
					newTile.runningCoroutine = StartCoroutine(DownloadTextNameData(tileChange, newTile, callback));
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
		private IEnumerator DownloadTextNameData(TileChange tileChange, Tile tile, System.Action<TileChange> callback = null)
		{
			string url = $"{geoJsonUrl}{tileChange.X},{tileChange.Y},{(tileChange.X + tileSize)},{(tileChange.Y + tileSize)}";

			var streetnameRequest = UnityWebRequest.Get(url);
			tile.runningWebRequest = streetnameRequest;
			yield return streetnameRequest.SendWebRequest();

			if (streetnameRequest.result == UnityWebRequest.Result.Success)
			{	
				GeoJSON customJsonHandler = new GeoJSON(streetnameRequest.downloadHandler.text);
				yield return null;
				Vector3 locationPoint = default;
				int featureCounter = 0;

				while (customJsonHandler.GotoNextFeature())
				{
					featureCounter++;
					if ((featureCounter % maxSpawnsPerFrame) == 0) yield return null;

					string textPropertyValue = customJsonHandler.getPropertyStringValue(textProperty);

					if (textPropertyValue.Length > 1)
					{
						var textObject = Instantiate(TextObject);
						textObject.transform.SetParent(tile.gameObject.transform, true);
						textObject.GetComponent<TextMeshPro>().text = textPropertyValue;

						//Determine text position by either a geometry point node, or the centroid of a geometry MultiPolygon node
						switch (positionSourceType)
						{
							case PositionSourceType.Point:
								double[] coordinate = customJsonHandler.getGeometryPoint2DDouble();
								locationPoint = CoordConvert.RDtoUnity(new Vector2RD(coordinate[0], coordinate[1]));
								locationPoint.y = offsetFromGround;

								//Turn the text object so it faces up
								textObject.transform.Rotate(Vector3.left, -90, Space.Self);

								break;
							case PositionSourceType.MultiPolygonCentroid:
								List<double> coordinates = customJsonHandler.getGeometryMultiPolygonString();
								double centroidX = 0;
								double centroidY = 0;
								for (int i = 0; i < coordinates.Count; i++)
								{
									if (i % 2 == 0)
									{
										centroidX += coordinates[i];
										centroidY += coordinates[i + 1];
									}
								}

								locationPoint = CoordConvert.RDtoUnity(new Vector2RD(centroidX / (coordinates.Count / 2), centroidY / (coordinates.Count / 2)));
								locationPoint.y = offsetFromGround;
								break;
						}
						textObject.transform.position = locationPoint;

						//Determine how the spawned texts auto orientate
						switch (autoOrientationMode)
						{
							case AutoOrientationMode.FaceToCamera:
								textObject.AddComponent<FaceToCamera>().HideDistance = 200;
								break;
							case AutoOrientationMode.AutoFlip:
								if (readAngleFromProperty)
								{
									float angle = customJsonHandler.getPropertyFloatValue(angleProperty);
									textObject.transform.Rotate(Vector3.up, angle, Space.World);
								}
								textObject.AddComponent<FlipToCamera>();
								break;
							case AutoOrientationMode.None:
							default:
								break;
						}
					}
				}
				yield return null;
			}
			callback(tileChange);
		}
	}
}