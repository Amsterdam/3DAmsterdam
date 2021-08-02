using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Netherlands3D.Interface.Minimap
{
	public class WMTSMapLayer : MonoBehaviour
	{
		private double topLeftRDCoordinateX = -285401.92;
		private double topLeftRDCoordinateY = 903401.92;

		private Dictionary<Vector2, MapTile> loadedTiles;

		[SerializeField]
		private int zoom = 0;
		private int minZoom = 6;

		private int totalTilesX = 0;
		private int totalTilesY = 0;

		private int tileOffsetX = 0;
		private int tileOffsetY = 0;

		[SerializeField]
		private int tileSize = 256;

		//Source: https://portal.opengeospatial.org/files/?artifact_id=35326
		private double pixelInMeters = 0.00028;
		private double scaleDenominator = 12288000; //Zero zoomlevel is 1:12288000 

		private void Start()
		{
			loadedTiles = new Dictionary<Vector2, MapTile>();

			CalculateGridOffset();
			LoadTilesInView();
		}

		private void CalculateGridOffset()
		{
			var keyTileSizeInMeters = tileSize * (scaleDenominator / Mathf.Pow(2, zoom)) * pixelInMeters;
			print($"keyTileSizeInMeters {keyTileSizeInMeters}");

			tileOffsetX = Mathf.FloorToInt(((float)Config.activeConfiguration.BottomLeftRD.x - (float)topLeftRDCoordinateX) / (float)keyTileSizeInMeters);

			//Based on tile numbering type
			tileOffsetY = Mathf.FloorToInt(((float)topLeftRDCoordinateY - (float)Config.activeConfiguration.TopRightRD.y) / (float)keyTileSizeInMeters);

			//Should do offset of viewer here too
			var maxXSpanXInMeters = (float)Config.activeConfiguration.TopRightRD.x - (float)topLeftRDCoordinateX;
			var maxXSpanYInMeters = (float)topLeftRDCoordinateY - (float)Config.activeConfiguration.BottomLeftRD.y;
			print($"maxXSpanXInMeters {maxXSpanXInMeters}");
			print($"maxXSpanYInMeters {maxXSpanYInMeters}");

			totalTilesX = Mathf.FloorToInt(maxXSpanXInMeters / (float)keyTileSizeInMeters);
			totalTilesY = Mathf.FloorToInt(maxXSpanYInMeters / (float)keyTileSizeInMeters);
			print($"totalTilesX {totalTilesX}");
			print($"totalTilesX {totalTilesY}");

			print($"tileOffsetX {tileOffsetX}");
			print($"tileOffsetY {tileOffsetY}");
		}

		public void LoadTilesInView()
		{
			for (int x = tileOffsetX; x <= totalTilesX; x++)
			{
				for (int y = tileOffsetY; y <= totalTilesY; y++)
				{
					var tileKey = new Vector2(x, y);

					var newTileObject = new GameObject();
					var mapTile = newTileObject.AddComponent<MapTile>();
					mapTile.Initialize(this.transform, zoom, 256, x-tileOffsetX, -(y-tileOffsetY), tileKey, (zoom == minZoom));
					loadedTiles.Add(tileKey, mapTile);
				}
			}
		}
	}
}
