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
		private List<List<Vector3Int>> tileDistances = new List<List<Vector3Int>>();

		[SerializeField]
		private List<TileChange> pendingTileChanges = new List<TileChange>();
		[SerializeField]
		private List<TileChange> activeTileChangesView = new List<TileChange>();

		private Dictionary<Vector3Int, TileChange> activeTileChanges = new Dictionary<Vector3Int, TileChange>();
		// key : Vector3Int where
		//                  x = bottomleft x-coordinate in RD
		//                  y = bottomleft y-coordinate in RD
		//                  z = layerIndex.

		private Vector4 viewRange = new Vector4();
		// x= minimum X-coordinate in RD
		// y= minimum Y-coordinate in RD
		// z= size in X-direction in M.
		// w= size in Y-direction in M.
		public ICameraExtents cameraExtents;
		private Vector3Int cameraPosition;
		private Extent previousCameraViewExtent;
		private Vector3RD bottomLeft;
		private Vector3RD topRight;
		private Vector3RD cameraPositionRD;

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
			#if UNITY_EDITOR
			activeTileChangesView = activeTileChanges.Values.ToList();
			#endif

			UpdateViewRange();
			GetTilesizes();
			GetPossibleTiles();

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

		private void UpdateViewRange()
		{
			bottomLeft = CoordConvert.WGS84toRD(cameraExtents.GetExtent().MinX, cameraExtents.GetExtent().MinY);
			topRight = CoordConvert.WGS84toRD(cameraExtents.GetExtent().MaxX, cameraExtents.GetExtent().MaxY);

			viewRange.x = (float)bottomLeft.x;
			viewRange.y = (float)bottomLeft.y;
			viewRange.z = (float)(topRight.x - bottomLeft.x);
			viewRange.w = (float)(topRight.y - bottomLeft.y);

			cameraPositionRD = CoordConvert.UnitytoRD(cameraExtents.GetPosition());
			cameraPosition.x = (int)cameraPositionRD.x;
			cameraPosition.y = (int)cameraPositionRD.y;
			cameraPosition.z = (int)cameraPositionRD.z;
		}
		private void GetTilesizes()
		{
			int tilesize;
			tileSizes.Clear();
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
		}
		private void GetPossibleTiles()
		{
			tileDistances.Clear();

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
						tileList.Add(new Vector3Int(x, y, (int)GetTileDistanceSquared(tileID)));
					}
				}
				tileDistances.Add(tileList);
			}
		}
		private float GetTileDistanceSquared(Vector3Int tileID)
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
                    if (LOD==-1 && !layer.tiles.ContainsKey(tileKey))
                    {
						continue;
                    }
					else if (layer.tiles.ContainsKey(tileKey))
					{
						int activeLOD = layer.tiles[tileKey].LOD;
						if (LOD == -1)
						{
							TileChange tileChange = new TileChange();
							tileChange.action = TileAction.Remove;
							tileChange.X = tileKey.x;
							tileChange.Y = tileKey.y;
							tileChange.layerIndex = layerIndex;
							tileChange.priorityScore = int.MaxValue;
							AddTileChange(tileChange, layerIndex);
						}
						else if (activeLOD > LOD)
						{
							TileChange tileChange = new TileChange();
							tileChange.action = TileAction.Downgrade;
							tileChange.X = tileKey.x;
							tileChange.Y = tileKey.y;
							tileChange.layerIndex = layerIndex;
							tileChange.priorityScore = CalculatePriorityScore(layer.layerPriority, activeLOD - 1);
							AddTileChange(tileChange, layerIndex);
						}
						else if (activeLOD < LOD)
						{
							TileChange tileChange = new TileChange();
							tileChange.action = TileAction.Upgrade;
							tileChange.X = tileKey.x;
							tileChange.Y = tileKey.y;
							tileChange.layerIndex = layerIndex;
							tileChange.priorityScore = (5000 - (int)tileDistance.magnitude) + CalculatePriorityScore(layer.layerPriority, activeLOD + 1);
							AddTileChange(tileChange, layerIndex);
						}
					}
					else
					{
						TileChange tileChange = new TileChange();
						tileChange.action = TileAction.Create;
						tileChange.X = tileKey.x;
						tileChange.Y = tileKey.y;
						tileChange.priorityScore = (6000 - (int)tileDistance.magnitude);
						tileChange.layerIndex = layerIndex;
						AddTileChange(tileChange, layerIndex);
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
		private int CalculatePriorityScore(int layerPriority, int lod)
		{
			return (10 * lod) - layerPriority;
		}
		private void RemoveOutOfViewTiles()
		{
			List<Vector3Int> neededTileSizesDistance;
			List<Vector2Int> neededTileSizes = new List<Vector2Int>();
			List<Vector2Int> activeTiles;
			for (int layerIndex = 0; layerIndex < layers.Count; layerIndex++)
			{
				Layer layer = layers[layerIndex];
				if (layer.gameObject.activeSelf == false) { continue; }
				int tilesizeIndex = tileSizes.IndexOf(layer.tileSize);
				neededTileSizesDistance = tileDistances[tilesizeIndex];
				neededTileSizes.Clear();
				foreach (var neededTileSize in neededTileSizesDistance)
				{
					neededTileSizes.Add(new Vector2Int(neededTileSize.x, neededTileSize.y));
				}

				activeTiles = new List<Vector2Int>(layer.tiles.Keys);
				foreach (Vector2Int activeTile in activeTiles)
				{
					if (neededTileSizes.Contains(activeTile) == false)
					{
						TileChange tileChange = new TileChange();
						tileChange.action = TileAction.Remove;
						tileChange.X = activeTile.x;
						tileChange.Y = activeTile.y;
						tileChange.layerIndex = layerIndex;
						tileChange.priorityScore = int.MaxValue;
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