using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BruTile;
using ConvertCoordinates;
using System.Linq;
using UnityEngine.Networking;
using Amsterdam3D.CameraMotion;
namespace LayerSystem
{
	
	public class TileHandler : MonoBehaviour
	{
		/// <summary>
		/// if true, prevents all layers from updating tiles
		/// downloading data continues if already started
		/// </summary>
		public bool pauseLoading
		{
            set
            {
                foreach (Layer layer in layers)
                {
					layer.pauseLoading = value;
                }
            }
		}
		public int maximumConcurrentDownloads = 5;

		public List<Layer> layers = new List<Layer>();
		private List<int> tileSizes = new List<int>();
		/// <summary>
		/// contains, for each tilesize in tileSizes, al list with tilecoordinates an distance to camera
		/// X,Y is bottom-left coordinate of tile in RD (for example 121000,480000)
		/// Z is distance-squared to camera in m 
		/// </summary>
		public List<List<Vector3Int>> tileDistances = new List<List<Vector3Int>>();

		/// <summary>
		/// list of tilechanges, ready to be processed
		/// </summary>
		[SerializeField]
		private List<TileChange> pendingTileChanges = new List<TileChange>();
		[SerializeField]	//list with active tilechanges, for debugging
		private List<TileChange> activeTileChangesView = new List<TileChange>();

		/// <summary>
		/// dictionary with tilechanges that are curently being processed
		/// Key: 
		///		X,Y is bottom-left coordinate of tile in RD (for example 121000,480000)
		///		Z is the Layerindex of the tile
		/// </summary>
		private Dictionary<Vector3Int, TileChange> activeTileChanges = new Dictionary<Vector3Int, TileChange>();

		/// <summary>
		/// area that is visible
		/// X, Y is bottom-left coordinate in RD (for example 121000,480000)
		/// Z width of area(RD-X-direction) in M
		/// W length of area(RD-Y-direction) in M
		/// </summary>
		private Vector4 viewRange = new Vector4();


		public ICameraExtents cameraExtents;
		/// <summary>
		/// postion of camera in RDcoordinates rounded to nearest integer
		/// </summary>
		private Vector3Int cameraPosition;
		



		private Vector2Int tileKey;

		public static int runningTileDataRequests = 0;

		public void OnCameraChanged()
		{
			cameraExtents = CameraModeChanger.Instance.CurrentCameraExtends;
		}

		void Start()
		{
			pauseLoading = false;
			cameraExtents = CameraModeChanger.Instance.CurrentCameraExtends;
			CameraModeChanger.Instance.OnFirstPersonModeEvent += OnCameraChanged;
			CameraModeChanger.Instance.OnGodViewModeEvent += OnCameraChanged;
		}
		void Update()
		{
			//for debugging
			activeTileChangesView = activeTileChanges.Values.ToList();

			viewRange = GetViewRange(cameraExtents);
			cameraPosition = getCameraPosition(cameraExtents);
			tileSizes=GetTilesizes();
			tileDistances = GetTileDistances(tileSizes, viewRange, cameraPosition);

			pendingTileChanges.Clear();
			RemoveOutOfViewTiles();
			GetTileChanges();

			if (pendingTileChanges.Count == 0) { return; }

			if (activeTileChanges.Count < maximumConcurrentDownloads)
			{
				TileChange highestPriorityTileChange = FindHighestPriorityTileChange();
				Vector3Int tilekey = new Vector3Int(highestPriorityTileChange.X, highestPriorityTileChange.Y, highestPriorityTileChange.layerIndex);
				if (activeTileChanges.ContainsKey(tilekey) == false)
				{
					activeTileChanges.Add(tilekey, highestPriorityTileChange);
					pendingTileChanges.Remove(highestPriorityTileChange);
					layers[highestPriorityTileChange.layerIndex].HandleTile(highestPriorityTileChange,TileHandled);
				}
				else if (activeTileChanges.TryGetValue(tilekey, out TileChange existingTileChange))
				{
					//Change running tile changes to more important ones
					Debug.Log("Upgrading existing");
					if (existingTileChange.priorityScore < highestPriorityTileChange.priorityScore)
					{
						activeTileChanges[tilekey] = highestPriorityTileChange;
						pendingTileChanges.Remove(highestPriorityTileChange);
					}
				}
			}
		}

		public void TileHandled(TileChange handledTileChange)
        {
			TileAction action = handledTileChange.action;
			activeTileChanges.Remove(new Vector3Int(handledTileChange.X, handledTileChange.Y, handledTileChange.layerIndex));
		}

		/// <summary>
		/// uses CameraExtent
		/// updates the variable viewrange
		/// updates the variable cameraPositionRD
		/// updates the variable cameraPosition
		/// </summary>
		private Vector4 GetViewRange(ICameraExtents cameraExtents)
		{
			var bottomLeft = CoordConvert.WGS84toRD(cameraExtents.GetExtent().MinX, cameraExtents.GetExtent().MinY);
			var topRight = CoordConvert.WGS84toRD(cameraExtents.GetExtent().MaxX, cameraExtents.GetExtent().MaxY);
			Vector4 viewRange = new Vector4();
			viewRange.x = (float)bottomLeft.x;
			viewRange.y = (float)bottomLeft.y;
			viewRange.z = (float)(topRight.x - bottomLeft.x);
			viewRange.w = (float)(topRight.y - bottomLeft.y);

			return viewRange;
		}

		private Vector3Int getCameraPosition(ICameraExtents cameraExtents)
        {
			var cameraPositionRD = CoordConvert.UnitytoRD(cameraExtents.GetPosition());
			Vector3Int cameraPosition = new Vector3Int();
			cameraPosition.x = (int)cameraPositionRD.x;
			cameraPosition.y = (int)cameraPositionRD.y;
			cameraPosition.z = (int)cameraPositionRD.z;
			return cameraPosition;
		}
		
		/// <summary>
		/// create a list of unique tilesizes used by all the layers
		/// save the list in variable tileSizes
		/// </summary>
		private List<int> GetTilesizes()
		{
			int tilesize;
			List<int> tileSizes = new List<int>();
			foreach (Layer layer in layers)
			{
				if (layer.isEnabled == true)
				{
					tilesize = layer.tileSize;
					if (tileSizes.Contains(tilesize) == false)
					{
						tileSizes.Add(tilesize);
					}
				}
			}
			return tileSizes;
		}
		
		private List<List<Vector3Int>> GetTileDistances(List<int> tileSizes, Vector4 viewRange, Vector3Int cameraPosition)
		{
			List<List<Vector3Int>> tileDistances = new List<List<Vector3Int>>();
		
			int startX;
			int startY;
			int endX;
			int endY;

			List<Vector3Int> tileList;
			foreach (int tileSize in tileSizes)
			{
				tileList = new List<Vector3Int>();
				startX = (int)Math.Floor(viewRange.x / tileSize) * tileSize;
				startY = (int)Math.Floor(viewRange.y / tileSize) * tileSize;
				endX = (int)Math.Ceiling((viewRange.x + viewRange.z) / tileSize) * tileSize;
				endY = (int)Math.Ceiling((viewRange.y + viewRange.w) / tileSize) * tileSize;

				for (int x = startX; x <= endX; x += tileSize)
				{
					for (int y = startY; y <= endY; y += tileSize)
					{
						Vector3Int tileID = new Vector3Int(x, y, tileSize);
						tileList.Add(new Vector3Int(x, y, (int)GetTileDistanceSquared(tileID,cameraPosition)));

					}
				}
				tileDistances.Add(tileList);
			}
			return tileDistances;
		}


		private float GetTileDistanceSquared(Vector3Int tileID, Vector3Int cameraPosition)
		{
			float distance = 0;
			int centerOffset = (int)tileID.z / 2;
			Vector3Int center = new Vector3Int(tileID.x + centerOffset, tileID.y + centerOffset, 0);
			float delta = center.x - cameraPosition.x;
			distance += (delta * delta);
			delta = center.y - cameraPosition.y;
			distance += (delta * delta);
			delta = cameraPosition.z * cameraPosition.z;
			distance += (delta);
			
			return distance;
		}
		
		
		private void GetTileChanges()
		{
			for (int layerIndex = 0; layerIndex < layers.Count; layerIndex++)
			{
				Layer layer = layers[layerIndex];
				if (layer.isEnabled == false) { continue; }
				int tilesizeIndex = tileSizes.IndexOf(layer.tileSize);
				foreach (Vector3Int tileDistance in tileDistances[tilesizeIndex])
				{
					tileKey = new Vector2Int(tileDistance.x, tileDistance.y);
					int LOD = CalculateLOD(tileDistance, layer);
					if (layer.tiles.ContainsKey(tileKey))
					{
						int activeLOD = layer.tiles[tileKey].LOD;
						if (LOD == -1)
						{
							TileChange tileChange = new TileChange();
							tileChange.action = TileAction.Remove;
							tileChange.X = tileKey.x;
							tileChange.Y = tileKey.y;
							tileChange.layerIndex = layerIndex;
							tileChange.priorityScore = CalculatePriorityScore(layer.layerPriority, 0, tileDistance.z, TileAction.Remove);
							AddTileChange(tileChange, layerIndex);
						}
						else if (activeLOD > LOD)
						{
							TileChange tileChange = new TileChange();
							tileChange.action = TileAction.Downgrade;
							tileChange.X = tileKey.x;
							tileChange.Y = tileKey.y;
							tileChange.layerIndex = layerIndex;
							tileChange.priorityScore = CalculatePriorityScore(layer.layerPriority, activeLOD - 1,tileDistance.z,TileAction.Downgrade);
							AddTileChange(tileChange, layerIndex);
						}
						else if (activeLOD < LOD)
						{
							TileChange tileChange = new TileChange();
							tileChange.action = TileAction.Upgrade;
							tileChange.X = tileKey.x;
							tileChange.Y = tileKey.y;
							tileChange.layerIndex = layerIndex;
							tileChange.priorityScore = CalculatePriorityScore(layer.layerPriority, activeLOD + 1, tileDistance.z, TileAction.Upgrade);
							AddTileChange(tileChange, layerIndex);
						}
					}
					else
					{
                        if (LOD !=-1)
                        {
						TileChange tileChange = new TileChange();
						tileChange.action = TileAction.Create;
						tileChange.X = tileKey.x;
						tileChange.Y = tileKey.y;
							tileChange.priorityScore = CalculatePriorityScore(layer.layerPriority, 0, tileDistance.z, TileAction.Create);
						tileChange.layerIndex = layerIndex;
						AddTileChange(tileChange, layerIndex);
						}
					}
				}
			}
		}

		private void AddTileChange(TileChange tileChange, int layerIndex)
		{

			//don't add a tilechange if the tile has an active tilechange already
			
			Vector3Int activekey = new Vector3Int(tileChange.X, tileChange.Y, tileChange.layerIndex);
			if (activeTileChanges.ContainsKey(activekey))
			{
				return;
			}
			bool tileIspending = false;
			for (int i = pendingTileChanges.Count - 1; i >= 0; i--)
			{
				if (pendingTileChanges[i].X == tileChange.X && pendingTileChanges[i].Y == tileChange.Y && pendingTileChanges[i].layerIndex == tileChange.layerIndex)
                {
					tileIspending = true;
                }
			}

            //Replace running tile changes with this one if priority is higher
            if (tileIspending==false)
            {		
				pendingTileChanges.Add(tileChange);
			}
		}

		private int CalculateLOD(Vector3Int tiledistance, Layer layer)
		{
			int lod = -1;

			foreach (DataSet dataSet in layer.Datasets)
			{
				if (dataSet.maximumDistanceSquared > (tiledistance.z))
				{
					lod = dataSet.lod;
				}
			}
			return lod;

		}
		private int CalculatePriorityScore(int layerPriority, int lod, int distanceSquared, TileAction action)
		{
			float distanceFactor = ((5000f * 5000f) / distanceSquared);
			int priority = 1;
            switch (action)
            {
                case TileAction.Create:
					priority = (int)((10*(lod+layerPriority))*distanceFactor);
                    break;
                case TileAction.Upgrade:
					priority = (int)((1 * (lod + layerPriority)) * distanceFactor);
					break;
                case TileAction.Downgrade:
					priority= (int)((0.5 * (lod + layerPriority)) * distanceFactor);
					break;
                case TileAction.Remove:
					priority = (int)((100 * (lod + layerPriority)) * distanceFactor);
					break;
                default:
                    break;
            }
			return priority;
		}
		
		/// <summary>
		/// 
		/// </summary>
		private void RemoveOutOfViewTiles()
		{
			List<Vector3Int> neededTiles;
			List<Vector2Int> neededTileKeys = new List<Vector2Int>();
			List<Vector2Int> activeTiles; // list of currently active tiles
			for (int layerIndex = 0; layerIndex < layers.Count; layerIndex++)
			{
				// create a list of tilekeys for the tiles that are within the viewrange
				Layer layer = layers[layerIndex];
				if (layer.gameObject.activeSelf == false) { continue; }
				int tilesizeIndex = tileSizes.IndexOf(layer.tileSize);
				neededTiles = tileDistances[tilesizeIndex];
				neededTileKeys.Clear();
				foreach (var neededTile in neededTiles)
				{
					neededTileKeys.Add(new Vector2Int(neededTile.x, neededTile.y));
				}


				activeTiles = new List<Vector2Int>(layer.tiles.Keys);
				// check for each active tile if the key is in the list of tilekeys within the viewrange
				foreach (Vector2Int activeTile in activeTiles)
				{
					if (neededTileKeys.Contains(activeTile) == false) // if the tile is not within the viewrange, set it up for removal
					{
						TileChange tileChange = new TileChange();
						tileChange.action = TileAction.Remove;
						tileChange.X = activeTile.x;
						tileChange.Y = activeTile.y;
						tileChange.layerIndex = layerIndex;
						tileChange.priorityScore = int.MaxValue; // set the priorityscore to maximum
						AddTileChange(tileChange,layerIndex);
					}
				}

			}
		}
		private TileChange FindHighestPriorityTileChange()
		{
			TileChange highestPriorityTileChange = pendingTileChanges[0];
			float highestPriority = highestPriorityTileChange.priorityScore;

			for (int i = 1; i < pendingTileChanges.Count; i++)
			{
				if (pendingTileChanges[i].priorityScore > highestPriority)
				{
					highestPriorityTileChange = pendingTileChanges[i];
					highestPriority = highestPriorityTileChange.priorityScore;
				}
			}
			return highestPriorityTileChange;
		}
	}
	[Serializable]
	public class DataSet
	{
		public string Description;
		public int lod;
		public string path;
		public float maximumDistance;
		[HideInInspector]
		public float maximumDistanceSquared;
		public bool enabled = true;
	}

	public class Tile
	{
		public Layer layer;
		public int LOD;
		public GameObject gameObject;
		public AssetBundle assetBundle;
		public Vector2Int tileKey;
	}
	[Serializable]
	public class TileChange
	{
		public TileAction action;
		public int priorityScore;
		public int layerIndex;
		public int X;
		public int Y;
	}
	public enum TileAction
	{
		Create,
		Upgrade,
		Downgrade,
		Remove
	}
}