using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ConvertCoordinates;
using UnityEngine.Networking;
using System.Globalization;
using System;
using Netherlands3D.Underground;
using Netherlands3D.LayerSystem;
using Netherlands3D.Utilities;
using Netherlands3D;

namespace Amsterdam3D.Sewerage
{
	public class SewerageLayer : Layer
    {
		private const string sewerPipesWfsUrl = "https://api.data.amsterdam.nl/v1/wfs/rioolnetwerk/?SERVICE=WFS&VERSION=2.0.0&REQUEST=GetFeature&outputFormat=geojson&srsname=epsg:4258&typeName=rioolleidingen&bbox=";
		private const string sewerManholesWfsUrl = "https://api.data.amsterdam.nl/v1/wfs/rioolnetwerk/?SERVICE=WFS&VERSION=2.0.0&REQUEST=GetFeature&outputFormat=geojson&srsname=epsg:4258&typeName=rioolknopen&bbox=";
		[SerializeField]
		private Material sharedMaterial;
		[SerializeField]
		private SewerLineSpawner sewerPipeSpawner;
		[SerializeField]
		private SewerManholeSpawner sewerManholeSpawner;
		[SerializeField]
		private RuntimeMaskSphere runtimeMaskSphere;
		private const int maxSpawnsPerFrame = 50;
		private const int maxParsesPerFrame = 50;
		private float napOffset = 0;
		[SerializeField]
		private int activeCount = 0;
		private int maxSimultaneous = 1;
		[SerializeField]
		private SewerageObjectPool SewerLineObjectPool;
		[SerializeField]
		private SewerageObjectPool manHoleObjectPool;

		public override void OnDisableTiles(bool isenabled)
        {
			runtimeMaskSphere.enabled = isenabled;
        }

		public override void HandleTile(TileChange tileChange, System.Action<TileChange> callback = null)
        {
			TileAction action = tileChange.action;
			switch (action)
			{
				case TileAction.Create:
					Tile newTile = CreateNewTile(tileChange,callback);
					tiles.Add(new Vector2Int(tileChange.X, tileChange.Y), newTile);
					break;
				case TileAction.Remove:
					RemoveTile(tileChange, callback);
					tiles.Remove(new Vector2Int(tileChange.X, tileChange.Y));
					callback(tileChange);
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
			tile.gameObject = new GameObject("sewerage-"+tileChange.X + "_" + tileChange.Y);
			tile.gameObject.transform.parent = transform.gameObject.transform;
			tile.gameObject.layer = tile.gameObject.transform.parent.gameObject.layer;
			tile.gameObject.SetActive(false);
			Generate(tileChange, tile, callback);
			return tile;
		}



		public void Generate(TileChange tileChange,Tile tile, System.Action<TileChange> callback = null)
		{
			
			napOffset = Config.activeConfiguration.zeroGroundLevelY;
			Vector3RD boundingBoxMinimum = new Vector3RD(tileChange.X,tileChange.Y,napOffset);
			Vector3RD boundingBoxMaximum = new Vector3RD(tileChange.X+tileSize, tileChange.Y+tileSize, napOffset); ;
			
			StartCoroutine(GetSewerLinesInBoundingBox(tileChange,tile, boundingBoxMinimum, boundingBoxMaximum, callback));
		}

		IEnumerator GetSewerLinesInBoundingBox(TileChange tileChange, Tile tile, Vector3RD boundingBoxMinimum, Vector3RD boundingBoxMaximum, System.Action<TileChange> callback = null)
		{
			yield return null;
			yield return new WaitUntil(() => activeCount < maxSimultaneous);
			activeCount++;
			tile.gameObject.SetActive(true);
			string escapedUrl = sewerPipesWfsUrl;
			escapedUrl += UnityWebRequest.EscapeURL((boundingBoxMinimum.x).ToString(CultureInfo.InvariantCulture) + "," + (boundingBoxMinimum.y).ToString(CultureInfo.InvariantCulture) + "," + (boundingBoxMaximum.x).ToString(CultureInfo.InvariantCulture) + "," + (boundingBoxMaximum.y).ToString(CultureInfo.InvariantCulture));
			var sewerageRequest = UnityWebRequest.Get(escapedUrl);

			yield return sewerageRequest.SendWebRequest();
			if (!sewerageRequest.isNetworkError && !sewerageRequest.isHttpError)
			{
                GeoJSON customJsonHandler = new GeoJSON(sewerageRequest.downloadHandler.text);
				yield return null;
                Vector3 startpoint;
                Vector3 endpoint;
                int parseCounter = 0;
                while (customJsonHandler.GotoNextFeature())
                {
                    parseCounter++;
                    if ((parseCounter % maxParsesPerFrame) == 0) yield return null;
                    double diameter = customJsonHandler.getPropertyFloatValue("diameter");
                    double bobBeginPunt = customJsonHandler.getPropertyFloatValue("bob_beginpunt");
                    List<double> coordinates = customJsonHandler.getGeometryLineString();
                    endpoint = ConvertCoordinates.CoordConvert.WGS84toUnity(new Vector3WGS(coordinates[0], coordinates[1], bobBeginPunt + Config.activeConfiguration.zeroGroundLevelY));
                    for (int i = 2; i < coordinates.Count; i += 2)
                    {
                        startpoint = endpoint;
                        double bobEindPunt = customJsonHandler.getPropertyFloatValue("bob_eindpunt");
                        endpoint = ConvertCoordinates.CoordConvert.WGS84toUnity(new Vector3WGS(coordinates[i], coordinates[(i + 1)], bobEindPunt + Config.activeConfiguration.zeroGroundLevelY));

                        sewerPipeSpawner.CreateSewerLine(startpoint, endpoint, diameter, tile.gameObject);
                    }

                }
                StartCoroutine(GetSewerManholesInBoundingBox(tileChange, boundingBoxMinimum, boundingBoxMaximum, tile, callback));
            }
			else
            { //callback if weberror
				Debug.Log("sewerlinedata not found");
				activeCount--;
				callback(tileChange);
            }

			yield return null;
		}

		private IEnumerator SpawnLineObjects(SewerLines sewerLines, TileChange tileChange, Vector3RD boundingBoxMinimum, Vector3RD boundingBoxMaximum, Tile tile, System.Action<TileChange> callback = null)
		{
			tile.gameObject.SetActive(isEnabled);
			SewerLines.Feature sewerLineFeature;
			for (int i = 0; i < sewerLines.features.Length; i++)
			{
				if ((i % maxSpawnsPerFrame) == 0) yield return new WaitForEndOfFrame();

				sewerLineFeature = sewerLines.features[i];
				
				sewerPipeSpawner.CreateSewerLine(
					sewerLineFeature.geometry.unity_coordinates[0],
					sewerLineFeature.geometry.unity_coordinates[1],
					float.Parse(sewerLineFeature.properties.diameter),
					tile.gameObject
				);
			}

			//Lines are done spawing. Start loading and spawing the manholes.
			StartCoroutine(GetSewerManholesInBoundingBox(tileChange,boundingBoxMinimum, boundingBoxMaximum,tile,callback));

			yield return null;
		}
		IEnumerator GetSewerManholesInBoundingBox(TileChange tileChange, Vector3RD boundingBoxMinimum, Vector3RD boundingBoxMaximum, Tile tile, System.Action<TileChange> callback = null)
		{
			string escapedUrl = sewerManholesWfsUrl;
			escapedUrl += UnityWebRequest.EscapeURL((boundingBoxMinimum.x).ToString(CultureInfo.InvariantCulture) + "," + (boundingBoxMinimum.y).ToString(CultureInfo.InvariantCulture) + "," + (boundingBoxMaximum.x).ToString(CultureInfo.InvariantCulture) + "," + (boundingBoxMaximum.y).ToString(CultureInfo.InvariantCulture));
			var sewerageRequest = UnityWebRequest.Get(escapedUrl);

			yield return sewerageRequest.SendWebRequest();
			if (!sewerageRequest.isNetworkError && !sewerageRequest.isHttpError)
			{

                StartCoroutine(SpawnManHoleObjects(sewerageRequest.downloadHandler.text, tileChange, tile, callback));
            }
            else
            {
				activeCount--;
				callback(tileChange);
			}
			yield return null;
		}
		
		private IEnumerator SpawnManHoleObjects(string geoJSONtext, TileChange tileChange,Tile tile, System.Action<TileChange> callback = null)
        {
			tile.gameObject.SetActive(isEnabled);	
			GeoJSON customJsonHandler = new GeoJSON(geoJSONtext);
			yield return null;
			double[] point2D;
			Vector3 point;

			int parseCounter = 0;
			while (customJsonHandler.GotoNextFeature())
			{
				parseCounter++;
				if ((parseCounter % maxParsesPerFrame) == 0) yield return new WaitForEndOfFrame();
                if (customJsonHandler.PropertyValueStringEquals("objectsoort", "Knikpunt"))
                {
					point2D = customJsonHandler.getGeometryPoint2DDouble();

					double putdekselhoogte = customJsonHandler.getPropertyFloatValue("putdekselhoogte");
					point = ConvertCoordinates.CoordConvert.WGS84toUnity(new Vector3WGS(point2D[0], point2D[1], putdekselhoogte + Config.activeConfiguration.zeroGroundLevelY));
					sewerManholeSpawner.CreateManhole(point, 1.50f, tile.gameObject);
				}
			}
			StartCoroutine(CombineSewerage(tileChange, tile, callback));

			

		}
		private IEnumerator CombineSewerage(TileChange tileChange, Tile tile, System.Action<TileChange> callback = null)
		{
			int parseCounter = 0;
			//Do not try to combine if our gameobject was already destroyed.
			if (!tile.gameObject)
			{
				activeCount--;
				callback(tileChange);
				tile.gameObject.SetActive(isEnabled);
				yield break;
			}

			//Determine meshes to combine
			MeshFilter[] meshFilters = tile.gameObject.GetComponentsInChildren<MeshFilter>(true);
			CombineInstance[] combine = new CombineInstance[meshFilters.Length];
			int i = 0;
			while (i < meshFilters.Length)
			{
				combine[i].mesh = meshFilters[i].sharedMesh;
				combine[i].transform = meshFilters[i].transform.localToWorldMatrix;
				//meshFilters[i].gameObject.SetActive(false);

				i++;
			}

			//Own combined mesh
			GameObject newCombinedTile = new GameObject();
			newCombinedTile.name = "CombinedTile";
			newCombinedTile.transform.SetParent(tile.gameObject.transform);
			newCombinedTile.AddComponent<MeshRenderer>().material = sharedMaterial;

			Mesh newMesh = new Mesh
			{
				indexFormat = UnityEngine.Rendering.IndexFormat.UInt32
			};
			newMesh.CombineMeshes(combine);
			newCombinedTile.AddComponent<MeshFilter>().sharedMesh = newMesh;

			//Now destroy our large amount of network children.

			GameObject childObject; 
            for (int j = tile.gameObject.transform.childCount-1; j >= 0; j--)
            {
				parseCounter++;
				if ((parseCounter % maxParsesPerFrame) == 0) yield return new WaitForEndOfFrame();
				childObject = tile.gameObject.transform.GetChild(j).gameObject;

				if (childObject.name[0]=='S')
                {
					SewerLineObjectPool.ReturnObject(childObject);
                }
                else if (childObject.name[0] == 'M')
                {
					manHoleObjectPool.ReturnObject(childObject);
				}
            }
			
			callback(tileChange);
			activeCount--;
			tile.gameObject.SetActive(isEnabled);
		}

		/// <summary>
		/// Splits an unsupported multidimensional array into a Vector3 array.
		/// </summary>
		/// <param name="coordinates">The string containing the multidimensional array</param>
		/// <returns>An array of unity coordinates</returns>
		private Vector3[] SplitToCoordinatesArray(string coordinates, string startHeight, string endHeight)
		{
			string[] splitArray = coordinates.Split(new string[] { "],[" }, StringSplitOptions.None);
			List<Vector3> newVector3Array=new List<Vector3>();

			//Convert string with RD coordinates into unity coordinates
			for (int i = 0; i < splitArray.Length; i++)
			{
				string[] vector2String = splitArray[i].Split(',');
				Vector3WGS newWGSVector3 = new Vector3WGS(
						double.Parse(vector2String[0], CultureInfo.InvariantCulture),
						double.Parse(vector2String[1], CultureInfo.InvariantCulture),
						(i == 0) ? double.Parse(startHeight, CultureInfo.InvariantCulture) + napOffset : double.Parse(endHeight, CultureInfo.InvariantCulture) + napOffset
				);

				Vector3 unityCoordinate = CoordConvert.WGS84toUnity(newWGSVector3);
				newVector3Array.Add(unityCoordinate);

			}
			return newVector3Array.ToArray();
		}
		private void RemoveTile(TileChange tileChange, System.Action<TileChange> callback = null)
		{
			Tile tile = tiles[new Vector2Int(tileChange.X, tileChange.Y)];
			MeshFilter[] meshFilters = tile.gameObject.GetComponents<MeshFilter>();
            foreach (var meshfilter in meshFilters)
            {
				Destroy(meshfilter.sharedMesh);
            }
			Destroy(tile.gameObject);
			tiles.Remove(new Vector2Int(tileChange.X, tileChange.Y));
			callback(tileChange);
		}
	}
}