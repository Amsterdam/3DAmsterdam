using ConvertCoordinates;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Netherlands3D.Interface.Minimap
{
	[HelpURL("http://example.com/docs/MyComponent.html")]
	public class WMTSMap : MonoBehaviour
	{
		private double topLeftRDCoordinateX = -285401.92;
		private double topLeftRDCoordinateY = 903401.92;

		private Dictionary<int, Dictionary<Vector2, MapTile>> mapTileLayers;

		[SerializeField]
		private RectTransform pointer;

		[SerializeField]
		private int startIdentifier = 5;
		private int layerIdentifier = 5;

		[SerializeField]
		private int maxIdentifier = 14;

		private float totalTilesX = 0;
		private float totalTilesY = 0;

		private float tileOffsetX = 0;
		private float tileOffsetY = 0;

		[SerializeField]
		private float tileSize = 256;
		private float baseTileSize = 256;
		private double tileSizeInMeters = 0;

		//Source: https://portal.opengeospatial.org/files/?artifact_id=35326
		private double pixelInMeters = 0.00028;
		private double scaleDenominator = 12288000; //Zero zoomlevel is 1:12288000 

		private MapViewer parentMapViewer;
		private RectTransform viewerTransform;
		private RectTransform mapTransform;

		[SerializeField]
		private double mapWidthInMeters = 0;

		private Vector2 layerTilesOffset = Vector2.zero;

		//config EPSG:28992
		/*
		 * <ScaleDenominator>12288000.0</ScaleDenominator>
			<TopLeftCorner>-285401.92 903401.92</TopLeftCorner>
			<TileWidth>256</TileWidth>
			<TileHeight>256</TileHeight>
*/
		private void Start()
		{
			layerIdentifier = startIdentifier;
			baseTileSize = tileSize;

			mapTileLayers = new Dictionary<int, Dictionary<Vector2, MapTile>>();

			parentMapViewer = GetComponentInParent<MapViewer>();
			viewerTransform = parentMapViewer.transform as RectTransform;
			mapTransform = parentMapViewer.transform as RectTransform;
			mapWidthInMeters = tileSize * pixelInMeters * scaleDenominator;
			print($"mapWidthInMeters = {tileSize} * {pixelInMeters} * {scaleDenominator}");

			CalculateGridScaling();
			ActivateMapLayer();
		}

		public void PositionObjectOnMap(RectTransform targetObject, Vector2RD targetPosition)
		{
			
		}
		public void Zoomed(int viewerZoom)
		{
			tileSize = baseTileSize / Mathf.Pow(2, viewerZoom);

			layerIdentifier = startIdentifier + viewerZoom;
			CalculateGridScaling();
			ActivateMapLayer();
		}

		private void CalculateGridScaling()
		{
			tileSizeInMeters = mapWidthInMeters / Mathf.Pow(2, layerIdentifier);

			layerTilesOffset = new Vector2(
				((float)Config.activeConfiguration.BottomLeftRD.x - (float)topLeftRDCoordinateX) / (float)tileSizeInMeters,
				((float)topLeftRDCoordinateY - (float)Config.activeConfiguration.TopRightRD.y) / (float)tileSizeInMeters
			);

			//Based on tile numbering type
			tileOffsetX = Mathf.Floor(layerTilesOffset.x);
			tileOffsetY = Mathf.Floor(layerTilesOffset.y);

			//Store the remaining value to offset layer
			layerTilesOffset.x -= tileOffsetX;
			layerTilesOffset.y -= tileOffsetY;

			//Coverage of our application bounds
			var spanXInMeters = (float)Config.activeConfiguration.TopRightRD.x - (float)Config.activeConfiguration.BottomLeftRD.x;
			var spanYInMeters = (float)Config.activeConfiguration.TopRightRD.y - (float)Config.activeConfiguration.BottomLeftRD.y;
			totalTilesX = Mathf.CeilToInt(spanXInMeters / (float)tileSizeInMeters);
			totalTilesY = Mathf.CeilToInt(spanYInMeters / (float)tileSizeInMeters);
		}

		private void RemoveOtherLayers()
		{
			List<int> mapTileKeys = new List<int>(mapTileLayers.Keys);
			foreach (int layerKey in mapTileKeys)
			{
				//Remove all layers behind top layer except the first, and the one right below our layer
				if((layerKey < layerIdentifier-1 && layerKey != startIdentifier) || layerKey > layerIdentifier)
				{
					foreach(var tile in mapTileLayers[layerKey])
					{
						Destroy(tile.Value.gameObject);
					}
					mapTileLayers.Remove(layerKey);
				}
			}
		}

		private void Update()
		{
			//Continiously check if tiles of the active layer identifier should be loaded
			ShowLayerTiles(mapTileLayers[layerIdentifier]);
			MovePointer();
		}

		private void MovePointer()
		{
			pointer.SetAsLastSibling(); //Pointer is on top of map
		}

		private void ActivateMapLayer()
		{
			RemoveOtherLayers();
	
			Dictionary<Vector2, MapTile> tileList;
			if (!mapTileLayers.ContainsKey(layerIdentifier))
			{
				tileList = new Dictionary<Vector2, MapTile>();
				mapTileLayers.Add(layerIdentifier, tileList);
			}
			else
			{
				tileList = mapTileLayers[layerIdentifier];
			}
		}

		private void ShowLayerTiles(Dictionary<Vector2, MapTile> tileList)
		{
			for (int x = 0; x <= totalTilesX; x++)
			{
				for (int y = 0; y <= totalTilesY; y++)
				{
					var tileKey = new Vector2(x + tileOffsetX, y + tileOffsetY);

					//Tile position within this container
					float xPosition = (x * tileSize) - (layerTilesOffset.x * tileSize);
					float yPosition = -((y * tileSize) - (layerTilesOffset.y * tileSize)); 

					//Check if this tile rect would overlap with our viewer rectangle
					Rect tileRect = new Rect(mapTransform.position.x + xPosition, mapTransform.position.y + yPosition, baseTileSize, baseTileSize);
					Rect viewRect = new Rect(parentMapViewer.transform.position.x, parentMapViewer.transform.position.y, viewerTransform.rect.width, viewerTransform.rect.height);

					if (viewRect.Overlaps(tileRect,true))
					{
						if (!tileList.ContainsKey(tileKey))
						{
							var newTileObject = new GameObject();
							var mapTile = newTileObject.AddComponent<MapTile>();
							mapTile.Initialize(this.transform, layerIdentifier, tileSize, xPosition, yPosition, tileKey, true);

							tileList.Add(tileKey, mapTile);
						}
					}
					else if (tileList.ContainsKey(tileKey))
					{
						Destroy(tileList[tileKey].gameObject);
						tileList.Remove(tileKey);
					}
				}
			}
		}
	}
}
