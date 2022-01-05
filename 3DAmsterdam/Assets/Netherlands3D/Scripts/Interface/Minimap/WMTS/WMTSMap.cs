/*
*  Copyright (C) X Gemeente
*                X Amsterdam
*                X Economic Services Departments
*
*  Licensed under the EUPL, Version 1.2 or later (the "License");
*  You may not use this work except in compliance with the License.
*  You may obtain a copy of the License at:
*
*    https://github.com/Amsterdam/3DAmsterdam/blob/master/LICENSE.txt
*
*  Unless required by applicable law or agreed to in writing, software
*  distributed under the License is distributed on an "AS IS" basis,
*  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or
*  implied. See the License for the specific language governing
*  permissions and limitations under the License.
*/
using Netherlands3D.Core;
using Netherlands3D.Cameras;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Netherlands3D.Interface.Minimap
{
	[HelpURL("https://portal.opengeospatial.org/files/?artifact_id=35326")]
	public class WMTSMap : MonoBehaviour
	{
		private double minimapTopLeftX = -285401.92;
		private double minimapTopLeftY = 903401.92;

		private Dictionary<int, Dictionary<Vector2, MapTile>> mapTileLayers;

		[SerializeField]
		private RectTransform pointer;
		[SerializeField]
		private RectTransform fov;

		[SerializeField]
		private int startIdentifier = 5;
		private int layerIdentifier = 5;

		private float boundsTilesX = 0;
		private float boundsTilesY = 0;

		private float tileOffsetX = 0;
		private float tileOffsetY = 0;

		private float tileSize = 256;
		private float baseTileSize = 256;
		private double tileSizeInMeters = 0;
		private float startMeterInPixels = 0;

		private double divide = 0;

		private double pixelInMeters = 0.00028;
		private double scaleDenominator = 12288000; //Zero zoomlevel is 1:12288000 
		private double mapSizeInMeters = 0;

		private MapViewer parentMapViewer;
		private RectTransform viewerTransform;
		private RectTransform mapTransform;

		private Vector2 layerTilesOffset = Vector2.zero;

		private float boundsWidthInMeters;
		private float boundsHeightInMeters;

		[SerializeField]
		private bool centerPointerInView;
		public bool CenterPointerInView { get => centerPointerInView; set => centerPointerInView = value; }

		[SerializeField]
		private MinimapConfig minimapConfig;

		[SerializeField]
		private bool clampWithinParent = true;

		private void Start()
		{
			layerIdentifier = startIdentifier;

			//Use settingsprofile values
			tileSize = minimapConfig.TileMatrixSet.TileSize;
			pixelInMeters = minimapConfig.TileMatrixSet.PixelInMeters;
			scaleDenominator = minimapConfig.TileMatrixSet.ScaleDenominator;

			//Coverage of our application bounds
			boundsWidthInMeters = (float)Config.activeConfiguration.TopRightRD.x - (float)Config.activeConfiguration.BottomLeftRD.x;
			boundsHeightInMeters = (float)Config.activeConfiguration.TopRightRD.y - (float)Config.activeConfiguration.BottomLeftRD.y;

			baseTileSize = tileSize;

			mapTileLayers = new Dictionary<int, Dictionary<Vector2, MapTile>>();

			parentMapViewer = GetComponentInParent<MapViewer>();
			viewerTransform = parentMapViewer.transform as RectTransform;
			mapTransform = transform as RectTransform;

			//Calculate map width in meters based on zoomlevel 0 setting values
			mapSizeInMeters = baseTileSize * pixelInMeters * scaleDenominator;

			DetermineTopLeftOrigin();

			CalculateGridScaling();
			ActivateMapLayer();

			//Calculate base meters in pixels to do calculations converting local coordinates to meters
			startMeterInPixels = (float)tileSizeInMeters / (float)baseTileSize;

			pointer.gameObject.SetActive(true);
		}

		private void DetermineTopLeftOrigin()
		{
			switch(minimapConfig.TileMatrixSet.minimapOriginAlignment){
				case TileMatrixSet.OriginAlignment.BottomLeft:
					minimapTopLeftX = minimapConfig.TileMatrixSet.Origin.x;
					minimapTopLeftY = minimapConfig.TileMatrixSet.Origin.y + mapSizeInMeters;
					break;
				default:
					minimapTopLeftX = minimapConfig.TileMatrixSet.Origin.x;
					minimapTopLeftY = minimapConfig.TileMatrixSet.Origin.y;
					break;
			}
		}

		public void ClickedMap(PointerEventData eventData)
		{
			//The point we clicked on the map in local coordinates
			Vector3 localClickPosition = transform.InverseTransformPoint(eventData.position);
			
			//Distance in meters from top left corner of this map
			var meterX = localClickPosition.x * startMeterInPixels;
			var meterY = localClickPosition.y * startMeterInPixels;

			var RDcoordinate = CoordConvert.RDtoUnity(new Vector3RD
			{
				x = (float)Config.activeConfiguration.BottomLeftRD.x + meterX,
				y = (float)Config.activeConfiguration.TopRightRD.y + meterY,
				z = 0.0
			});
			RDcoordinate.y = CameraModeChanger.Instance.ActiveCamera.transform.position.y;

			print(RDcoordinate);

			CameraModeChanger.Instance.ActiveCamera.transform.position = RDcoordinate;
		}

		/// <summary>
		/// Position a RectTransform object on the map using RD coordinates
		/// Handy if you want to place markers/location indicators on the minimap.
		/// </summary>
		/// <param name="targetObject">RectTransform object to be placed</param>
		/// <param name="targetPosition">RD coordinate to place the object</param>
		public void PositionObjectOnMap(RectTransform targetObject, Vector3RD targetPosition)
		{		
			targetObject.transform.localScale = Vector3.one / mapTransform.localScale.x;
			targetObject.transform.localPosition = DeterminePositionOnMap(targetPosition);
		}

		/// <summary>
		/// Return the local unity map coordinates
		/// </summary>
		/// <param name="sourceRDPosition">The source RD position</param>
		/// <returns></returns>
		public Vector3 DeterminePositionOnMap(Vector3RD sourceRDPosition)
		{
			var meterX = sourceRDPosition.x - (float)Config.activeConfiguration.BottomLeftRD.x;
			var meterY = sourceRDPosition.y - (float)Config.activeConfiguration.TopRightRD.y;

			var pixelX = meterX / startMeterInPixels;
			var pixelY = meterY / startMeterInPixels;

			return new Vector3((float)pixelX, (float)pixelY);
		}

		/// <summary>
		/// The zoomlevel of the viewer. Not to be confused with the map identifier.
		/// The viewer starts at zoom level 0, our map identifier can start at a different identifier.
		/// </summary>
		/// <param name="viewerZoom">The viewer zoomlevel</param>
		public void Zoomed(int viewerZoom)
		{
			tileSize = baseTileSize / Mathf.Pow(2, viewerZoom);

			layerIdentifier = startIdentifier + viewerZoom;
			CalculateGridScaling();
			ActivateMapLayer();
		}

		private void CalculateGridScaling()
		{
			divide = Mathf.Pow(2, layerIdentifier);
			tileSizeInMeters = mapSizeInMeters / divide;

			//The tile 0,0 its top left does not align with our region top left. So here we determine the offset.
			layerTilesOffset = new Vector2(
				((float)Config.activeConfiguration.BottomLeftRD.x - (float)minimapTopLeftX) / (float)tileSizeInMeters,
				((float)minimapTopLeftY - (float)Config.activeConfiguration.TopRightRD.y) / (float)tileSizeInMeters
			);

			//Based on tile numbering type
			tileOffsetX = Mathf.Floor(layerTilesOffset.x);
			tileOffsetY = Mathf.Floor(layerTilesOffset.y);

			//Store the remaining value to offset layer
			layerTilesOffset.x -= tileOffsetX;
			layerTilesOffset.y -= tileOffsetY;

			//Calculate the amount of tiles needed for our app bounding box
			boundsTilesX = Mathf.CeilToInt(boundsWidthInMeters / (float)tileSizeInMeters);
			boundsTilesY = Mathf.CeilToInt(boundsHeightInMeters / (float)tileSizeInMeters);
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
			Clamp();

			//Continiously check if tiles of the active layer identifier should be loaded
			ShowLayerTiles(mapTileLayers[layerIdentifier]);
			MovePointer();
		}

		public void Clamp()
		{
			var maxPositionXInUnits = -(boundsWidthInMeters / startMeterInPixels) * transform.localScale.x;
			var maxPositionYInUnits = (boundsHeightInMeters / startMeterInPixels) * transform.localScale.x;

			this.transform.localPosition = new Vector3(
				Mathf.Clamp(this.transform.localPosition.x, maxPositionXInUnits + viewerTransform.sizeDelta.x, 0),
				Mathf.Clamp(this.transform.localPosition.y, viewerTransform.sizeDelta.y, maxPositionYInUnits),
				0
			);
		}

		private void MovePointer()
		{
			fov.SetAsLastSibling(); //Fov is on top of map
			pointer.SetAsLastSibling(); //Pointer is on top of fov

			PositionObjectOnMap(pointer, CoordConvert.UnitytoRD(CameraModeChanger.Instance.ActiveCamera.transform.position));

			if(CenterPointerInView)
			{
				this.transform.localPosition = -pointer.localPosition * mapTransform.localScale.x + (Vector3)viewerTransform.sizeDelta*0.5f;
			}
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
			for (int x = 0; x <= boundsTilesX; x++)
			{
				for (int y = 0; y <= boundsTilesY; y++)
				{
					Vector2 tileKey;

					//Tile position within this container
					float xPosition = (x * tileSize) - (layerTilesOffset.x * tileSize);
					float yPosition = -((y * tileSize) - (layerTilesOffset.y * tileSize));

					//Origin alignment determines the way we count our grid
					switch (minimapConfig.TileMatrixSet.minimapOriginAlignment)
					{
						case TileMatrixSet.OriginAlignment.BottomLeft:
							tileKey = new Vector2(x + tileOffsetX, (float)(divide-1) - (y + tileOffsetY));
							break;
						case TileMatrixSet.OriginAlignment.TopLeft:
						default:
							tileKey = new Vector2(x + tileOffsetX, y + tileOffsetY);
							break;
					}

					//Tile position to check if they are in viewer
					float compareXPosition = xPosition * mapTransform.localScale.x + mapTransform.transform.localPosition.x;
					float compareYPosition = yPosition * mapTransform.localScale.x + mapTransform.transform.localPosition.y;

					//Is this tile within the viewer rectangle?
					bool xWithinView = (compareXPosition+baseTileSize > 0 && compareXPosition < viewerTransform.sizeDelta.x);
					bool yWithinView = (compareYPosition > 0 && compareYPosition-baseTileSize < viewerTransform.sizeDelta.y);

					if (xWithinView && yWithinView)
					{
						if (!tileList.ContainsKey(tileKey))
						{
							var newTileObject = new GameObject();
							var mapTile = newTileObject.AddComponent<MapTile>();
							mapTile.Initialize(this.transform, layerIdentifier, tileSize, xPosition, yPosition, tileKey, minimapConfig);

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
