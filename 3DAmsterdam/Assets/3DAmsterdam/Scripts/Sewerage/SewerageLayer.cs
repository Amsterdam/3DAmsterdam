using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Globalization;
using System;
using Netherlands3D.TileSystem;
using Netherlands3D.Utilities;
using Netherlands3D.Core;
using Netherlands3D;
using System.Linq;

namespace Amsterdam3D.Sewerage
{
	public enum SewerageApiType { Amsterdam, Pdok }

	public class SewerageLayer : Layer
    {		
		[SerializeField]
		private Material sharedMaterial;
		[SerializeField]
		private SewerLineSpawner sewerPipeSpawner;
		[SerializeField]
		private SewerManholeSpawner sewerManholeSpawner;

		private const int maxSpawnsPerFrame = 50;
		private const int maxParsesPerFrame = 50;
		private float napOffset = 0;
		[SerializeField]
		private int maxSimultaneous = 1;
		[SerializeField]
		private SewerageObjectPool SewerLineObjectPool;
		[SerializeField]
		private SewerageObjectPool manHoleObjectPool;

		private string diameterString = null;

		private Dictionary<Coroutine, bool> coroutinesWaiting;

		public string DiameterString {get {
				if (diameterString == null){
					diameterString = Config.activeConfiguration.sewerageApiType == SewerageApiType.Amsterdam ? "diameter" : "BreedteLeiding";
				}
				return diameterString;
			}}

		private string bobBeginPuntString = null;
		public string BobBeginPuntString { get {
				if(bobBeginPuntString == null)
                {
					bobBeginPuntString = Config.activeConfiguration.sewerageApiType == SewerageApiType.Amsterdam ? "bob_beginpunt" : "BobBeginpuntLeiding";
				}
				return bobBeginPuntString;
			}}

		private string bobEindPuntString = null;
		public string BobEindPuntString { get {
                if (bobEindPuntString == null)
                {
					bobEindPuntString = Config.activeConfiguration.sewerageApiType == SewerageApiType.Amsterdam ? "bob_eindpunt" : "BobEindpuntLeiding";
				}
				return bobEindPuntString;
			}}

		private string putdekselhoogteString = null;
		public string PutdekselhoogteString { get {
                if (putdekselhoogteString == null)
                {
					putdekselhoogteString=  Config.activeConfiguration.sewerageApiType == SewerageApiType.Amsterdam ? "putdekselhoogte" : "Maaiveldhoogte";
				}
				return putdekselhoogteString;
			}}


		private void Awake()
		{
			coroutinesWaiting = new Dictionary<Coroutine, bool>();
		}

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
			tile.gameObject = new GameObject("sewerage-"+tileChange.X + "_" + tileChange.Y);
			tile.gameObject.transform.parent = transform.gameObject.transform;
			tile.gameObject.layer = tile.gameObject.transform.parent.gameObject.layer;
			tile.gameObject.SetActive(false);
			Generate(tileChange, tile, callback);
			return tile;
		}

		public override void InteruptRunningProcesses(Vector2Int tileKey)
		{
			if (!tiles.ContainsKey(tileKey)) return;

			if (tiles[tileKey].runningWebRequest != null)
				tiles[tileKey].runningWebRequest.Abort();

			if (tiles[tileKey].runningCoroutine != null)
			{
				if (coroutinesWaiting.ContainsKey(tiles[tileKey].runningCoroutine))
					coroutinesWaiting.Remove(tiles[tileKey].runningCoroutine);
				StopCoroutine(tiles[tileKey].runningCoroutine);
			}
		}

		public void Generate(TileChange tileChange,Tile tile, System.Action<TileChange> callback = null)
		{
			napOffset = Config.activeConfiguration.zeroGroundLevelY;
			Vector3RD boundingBoxMinimum = new Vector3RD(tileChange.X,tileChange.Y,napOffset);
			Vector3RD boundingBoxMaximum = new Vector3RD(tileChange.X+tileSize, tileChange.Y+tileSize, napOffset); ;
			
			tile.runningCoroutine = StartCoroutine(GetSewerLinesInBoundingBox(tileChange,tile, boundingBoxMinimum, boundingBoxMaximum, callback));
			coroutinesWaiting.Add(tile.runningCoroutine, true);
		}

		IEnumerator GetSewerLinesInBoundingBox(TileChange tileChange, Tile tile, Vector3RD boundingBoxMinimum, Vector3RD boundingBoxMaximum, System.Action<TileChange> callback = null)
		{
			yield return new WaitForEndOfFrame();
			var ownCoroutine = tile.runningCoroutine;
			yield return new WaitUntil(() => CoroutineSlotsAvailable());
			coroutinesWaiting[ownCoroutine] = false;

			string escapedUrl = Config.activeConfiguration.sewerPipesWfsUrl;
			escapedUrl += UnityWebRequest.EscapeURL($"{boundingBoxMinimum.x.ToInvariant()},{boundingBoxMinimum.y.ToInvariant()},{boundingBoxMaximum.x.ToInvariant()},{boundingBoxMaximum.y.ToInvariant()}");

			var sewerageRequest = UnityWebRequest.Get(escapedUrl);

			tile.runningWebRequest = sewerageRequest;
			yield return sewerageRequest.SendWebRequest();
			tile.runningWebRequest = null;

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
					double diameter = customJsonHandler.GetPropertyFloatValue(DiameterString);
					double bobBeginPunt = customJsonHandler.GetPropertyFloatValue(BobBeginPuntString);

					List<GeoJSONPoint> coordinates = customJsonHandler.GetGeometryLineString();
					endpoint = GetUnityPoint(coordinates[0].x, coordinates[0].y, bobBeginPunt + Config.activeConfiguration.zeroGroundLevelY);

					for (int i = 1; i < coordinates.Count; i ++)
					{
						startpoint = endpoint;
						double bobEindPunt = customJsonHandler.GetPropertyFloatValue(BobEindPuntString);

						endpoint = GetUnityPoint(coordinates[i].x, coordinates[i].y, bobEindPunt + Config.activeConfiguration.zeroGroundLevelY);
						sewerPipeSpawner.CreateSewerLine(startpoint, endpoint, diameter, tile.gameObject);
					}

				}
				yield return GetSewerManholesInBoundingBox(tileChange, boundingBoxMinimum, boundingBoxMaximum, tile, callback);
			}
			else
			{ //callback if weberror
				Debug.Log("sewerlinedata not found");
				callback(tileChange);
			}
			
			if(coroutinesWaiting.ContainsKey(ownCoroutine))
				coroutinesWaiting.Remove(ownCoroutine);

			yield return null;
		}

		private bool CoroutineSlotsAvailable()
		{
			return coroutinesWaiting.Count((coroutine) => coroutine.Value == false) < maxSimultaneous;
		}

		IEnumerator GetSewerManholesInBoundingBox(TileChange tileChange, Vector3RD boundingBoxMinimum, Vector3RD boundingBoxMaximum, Tile tile, System.Action<TileChange> callback = null)
		{
			string escapedUrl = Config.activeConfiguration.sewerManholesWfsUrl;
			escapedUrl += UnityWebRequest.EscapeURL($"{boundingBoxMinimum.x.ToInvariant()},{boundingBoxMinimum.y.ToInvariant()},{boundingBoxMaximum.x.ToInvariant()},{boundingBoxMaximum.y.ToInvariant()}");

			var sewerageRequest = UnityWebRequest.Get(escapedUrl);
			tile.runningWebRequest = sewerageRequest;
			yield return sewerageRequest.SendWebRequest();
			tile.runningWebRequest = null;

			if (!sewerageRequest.isNetworkError && !sewerageRequest.isHttpError)
			{
				yield return SpawnManHoleObjects(sewerageRequest.downloadHandler.text, tileChange, tile, callback);
            }
            else
            {
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
                
				if (customJsonHandler.PropertyValueStringEquals("objectsoort", "Knikpunt") == false)
                {
					point2D = customJsonHandler.GetGeometryPoint2DDouble();

					double putdekselhoogte = customJsonHandler.GetPropertyFloatValue(PutdekselhoogteString);
					point = GetUnityPoint(point2D[0], point2D[1], putdekselhoogte + Config.activeConfiguration.zeroGroundLevelY);

					sewerManholeSpawner.CreateManhole(point, 1.50f, tile.gameObject);
				}
			}
			yield return CombineSewerage(tileChange, tile, callback);
		}
		private IEnumerator CombineSewerage(TileChange tileChange, Tile tile, System.Action<TileChange> callback = null)
		{
			int parseCounter = 0;
			//Do not try to combine if our gameobject was already destroyed.
			if (!tile.gameObject)
			{
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

		public Vector3 GetUnityPoint(double x, double y, double z)
		{
			if (Config.activeConfiguration.sewerageApiType == SewerageApiType.Amsterdam)
			{
				return CoordConvert.WGS84toUnity(new Vector3WGS(x, y, z + Config.activeConfiguration.zeroGroundLevelY));
			}
			else
			{
				return CoordConvert.RDtoUnity(new Vector3RD(x, y, z + Config.activeConfiguration.zeroGroundLevelY));
			}
		}

	}
}