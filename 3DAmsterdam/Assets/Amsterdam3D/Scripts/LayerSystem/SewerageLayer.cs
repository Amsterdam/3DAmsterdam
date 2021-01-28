using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ConvertCoordinates;
using UnityEngine.Networking;
using System.Globalization;
using System;

namespace LayerSystem
{
    public class SewerageLayer : Layer
    {
		private const string sewerPipesWfsUrl = "https://api.data.amsterdam.nl/v1/wfs/rioolnetwerk/?SERVICE=WFS&VERSION=2.0.0&REQUEST=GetFeature&outputFormat=geojson&srsname=epsg:4258&typeName=rioolleidingen&bbox=";
		private const string sewerManholesWfsUrl = "https://api.data.amsterdam.nl/v1/wfs/rioolnetwerk/?SERVICE=WFS&VERSION=2.0.0&REQUEST=GetFeature&outputFormat=geojson&srsname=epsg:4258&typeName=rioolknopen&bbox=";
		[SerializeField]
		private Material sharedMaterial;
		[SerializeField]
		private Amsterdam3D.Sewerage.SewerLineSpawner sewerPipeSpawner;
		[SerializeField]
		private Amsterdam3D.Sewerage.SewerManholeSpawner sewerManholeSpawner;
		[SerializeField]
		private RuntimeMaskSphere runtimeMaskSphere;
		private const int maxSpawnsPerFrame = 100;
		private const int maxParsesPerFrame = 500;
		private float napOffset = 0;

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
			Generate(tileChange, tile, callback);
			return tile;
		}



		public void Generate(TileChange tileChange,Tile tile, System.Action<TileChange> callback = null)
		{
			napOffset = (float)(0 - CoordConvert.referenceRD.z);
			Vector3RD boundingBoxMinimum = new Vector3RD(tileChange.X,tileChange.Y,napOffset);
			Vector3RD boundingBoxMaximum = new Vector3RD(tileChange.X+tileSize, tileChange.Y+tileSize, napOffset); ;
			
			StartCoroutine(GetSewerLinesInBoundingBox(tileChange,tile, boundingBoxMinimum, boundingBoxMaximum, callback));
		}

		IEnumerator GetSewerLinesInBoundingBox(TileChange tileChange, Tile tile, Vector3RD boundingBoxMinimum, Vector3RD boundingBoxMaximum, System.Action<TileChange> callback = null)
		{
			string escapedUrl = sewerPipesWfsUrl;
			escapedUrl += UnityWebRequest.EscapeURL((boundingBoxMinimum.x).ToString(CultureInfo.InvariantCulture) + "," + (boundingBoxMinimum.y).ToString(CultureInfo.InvariantCulture) + "," + (boundingBoxMaximum.x).ToString(CultureInfo.InvariantCulture) + "," + (boundingBoxMaximum.y).ToString(CultureInfo.InvariantCulture));
			var sewerageRequest = UnityWebRequest.Get(escapedUrl);

			yield return sewerageRequest.SendWebRequest();
			if (!sewerageRequest.isNetworkError && !sewerageRequest.isHttpError)
			{
				//Replace multidimensional arrays with strings. JsonUtility doesnt support it (yet)   
				string dataString = sewerageRequest.downloadHandler.text.Replace("[[", "\"").Replace("]]", "\"");
				var sewerLines = JsonUtility.FromJson<SewerLines>(dataString);
				for (int i = 0; i < sewerLines.features.Length; i++)
				{
					//Smear out the heavy parsing over a few frames, to avoid spikes and memory issues in WebGL
					if ((i % maxParsesPerFrame) == 0) yield return new WaitForEndOfFrame();

					var feature = sewerLines.features[i];
					Vector3[] pointCoordinate = SplitToCoordinatesArray(feature.geometry.coordinates, feature.properties.bob_beginpunt, feature.properties.bob_eindpunt);
					feature.geometry.unity_coordinates = pointCoordinate;
				}

				yield return new WaitForEndOfFrame();

				StartCoroutine(SpawnLineObjects(sewerLines, tileChange,boundingBoxMinimum, boundingBoxMaximum,tile,callback));
			}
			else
            { //callback if weberror
				Debug.Log("sewerlinedata not found");
				callback(tileChange);
            }
			//We have a new network now that can start to spawn. Clear the old objects.
			//ClearNetwork();

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
				string dataString = sewerageRequest.downloadHandler.text;
				var sewerManholes = JsonUtility.FromJson<SewerManholes>(dataString);

				yield return new WaitForEndOfFrame();
				StartCoroutine(SpawnManholeObjects(sewerManholes,tileChange,boundingBoxMinimum,boundingBoxMaximum,tile,callback));
			}
            else
            {
				callback(tileChange);
			}
			yield return null;
		}
		private IEnumerator SpawnManholeObjects(SewerManholes sewerManholes, TileChange tileChange, Vector3RD boundingBoxMinimum, Vector3RD boundingBoxMaximum, Tile tile, System.Action<TileChange> callback = null)
		{
			tile.gameObject.SetActive(isEnabled);
			SewerManholes.Feature sewerManholeFeature;
			for (int i = 0; i < sewerManholes.features.Length; i++)
			{
				//Speedy way to check if the string is not a 'Knikpunt'
				if (sewerManholes.features[i].properties.objectsoort.Length == 8) continue;

				if ((i % maxSpawnsPerFrame) == 0) yield return new WaitForEndOfFrame();

				sewerManholeFeature = sewerManholes.features[i];
				sewerManholeSpawner.CreateManhole(
					CoordConvert.WGS84toUnity(new Vector3WGS(
						sewerManholeFeature.geometry.coordinates[0],
						sewerManholeFeature.geometry.coordinates[1],
						(float.Parse(sewerManholeFeature.properties.putdekselhoogte, CultureInfo.InvariantCulture) + napOffset)
						)
					)
					,1.50f,
					tile.gameObject
				);
			}
			CombineSewerage(tileChange,tile,callback);

			tile.gameObject.SetActive(isEnabled);
			yield return null;
		}
		 
		public GameObject CombineSewerage(TileChange tileChange, Tile tile, System.Action<TileChange> callback = null)
		{
			//Do not try to combine if our gameobject was already destroyed.
			if (!tile.gameObject) return null;

			//Determine meshes to combine
			MeshFilter[] meshFilters = tile.gameObject.GetComponentsInChildren<MeshFilter>(true);
			CombineInstance[] combine = new CombineInstance[meshFilters.Length];
			int i = 0;
			while (i < meshFilters.Length)
			{
				combine[i].mesh = meshFilters[i].sharedMesh;
				combine[i].transform = meshFilters[i].transform.localToWorldMatrix;
				meshFilters[i].gameObject.SetActive(false);

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
			foreach (MeshFilter child in meshFilters)
			{
				Destroy(child.gameObject);
			}
			callback(tileChange);
			return newCombinedTile;
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
