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
using ConvertCoordinates;
using Netherlands3D.Cameras;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Netherlands3D.Interface.Minimap
{
	[HelpURL("https://portal.opengeospatial.org/files/?artifact_id=35326")]
	public class WMTSMap : MonoBehaviour, IPointerClickHandler
	{
		private double minimapOriginX = -285401.92;
		private double minimapOriginY = 903401.92;

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

		private float tileSize = 256;
		private float baseTileSize = 256;
		private double tileSizeInMeters = 0;

		private double divide = 0;

		private double pixelInMeters = 0.00028;
		private double scaleDenominator = 12288000; //Zero zoomlevel is 1:12288000 

		private MapViewer parentMapViewer;
		private RectTransform viewerTransform;
		private RectTransform mapTransform;

		[SerializeField]
		private double mapWidthInMeters = 0;

		private Vector2 layerTilesOffset = Vector2.zero;

		float boundsWidthInMeters;
		float boundsHeightInMeters;

		[SerializeField]
		private bool centerPointerInView;
		public bool CenterPointerInView { get => centerPointerInView; set => centerPointerInView = value; }

		private void Start()
		{
			layerIdentifier = startIdentifier;

			//Use application config minimap settings
			tileSize = Config.activeConfiguration.minimapTileSize;
			pixelInMeters = Config.activeConfiguration.minimapPixelInMeters;
			scaleDenominator = Config.activeConfiguration.minimapScaleDenominator;
			minimapOriginX = Config.activeConfiguration.minimapOrigin.x;
			minimapOriginY = Config.activeConfiguration.minimapOrigin.y;

			//Coverage of our application bounds
			boundsWidthInMeters = (float)Config.activeConfiguration.TopRightRD.x - (float)Config.activeConfiguration.BottomLeftRD.x;
			boundsHeightInMeters = (float)Config.activeConfiguration.TopRightRD.y - (float)Config.activeConfiguration.BottomLeftRD.y;

			baseTileSize = tileSize;

			mapTileLayers = new Dictionary<int, Dictionary<Vector2, MapTile>>();

			parentMapViewer = GetComponentInParent<MapViewer>();
			viewerTransform = parentMapViewer.transform as RectTransform;
			mapTransform = transform as RectTransform;

			mapWidthInMeters = baseTileSize * pixelInMeters * scaleDenominator;
			print($"mapWidthInMeters = {baseTileSize} * {pixelInMeters} * {scaleDenominator}");

			CalculateGridScaling();
			ActivateMapLayer();
		}

		public void OnPointerClick(PointerEventData eventData)
		{
			/*Vector3 localClickPosition = transform.InverseTransformPoint(eventData.position);
			print(localClickPosition);
			var RDcoordinate = CoordConvert.RDtoUnity(new Vector3RD
			{
				x = (float)Config.activeConfiguration.BottomLeftRD.x + localClickPosition.x / pixelInMeters / divide,
				y = 0,
				z = 0.0
			});
			RDcoordinate.y = CameraModeChanger.Instance.ActiveCamera.transform.position.y;
			print(RDcoordinate);

			CameraModeChanger.Instance.ActiveCamera.transform.position = RDcoordinate;*/
		}

		public void PositionObjectOnMap(RectTransform targetObject, Vector3RD targetPosition)
		{		
			var meterX = targetPosition.x - (float)Config.activeConfiguration.BottomLeftRD.x;
			var meterY = targetPosition.y - (float)Config.activeConfiguration.TopRightRD.y;

			var pixelX = meterX * pixelInMeters * divide / mapTransform.localScale.x;
			var pixelY = meterY * pixelInMeters * divide / mapTransform.localScale.x;

			targetObject.transform.localScale = Vector3.one / mapTransform.localScale.x;
			targetObject.transform.localPosition = new Vector3((float)pixelX, (float)pixelY);
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
			divide = Mathf.Pow(2, layerIdentifier);
			tileSizeInMeters = mapWidthInMeters / divide;

			layerTilesOffset = new Vector2(
				((float)Config.activeConfiguration.BottomLeftRD.x - (float)minimapOriginX) / (float)tileSizeInMeters,
				((float)minimapOriginY - (float)Config.activeConfiguration.TopRightRD.y) / (float)tileSizeInMeters
			);

			//Based on tile numbering type
			tileOffsetX = Mathf.Floor(layerTilesOffset.x);
			tileOffsetY = Mathf.Floor(layerTilesOffset.y);

			//Store the remaining value to offset layer
			layerTilesOffset.x -= tileOffsetX;
			layerTilesOffset.y -= tileOffsetY;

			totalTilesX = Mathf.CeilToInt(boundsWidthInMeters / (float)tileSizeInMeters);
			totalTilesY = Mathf.CeilToInt(boundsHeightInMeters / (float)tileSizeInMeters);
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

			PositionObjectOnMap(pointer, CoordConvert.UnitytoRD(CameraModeChanger.Instance.ActiveCamera.transform.position));

			if(CenterPointerInView)
			{
				this.transform.localPosition = -pointer.localPosition* mapTransform.localScale.x + (Vector3)viewerTransform.sizeDelta*0.5f;
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
			for (int x = 0; x <= totalTilesX; x++)
			{
				for (int y = 0; y <= totalTilesY; y++)
				{
					var tileKey = new Vector2(x + tileOffsetX, y + tileOffsetY);

					//Tile position within this container
					float xPosition = (x * tileSize) - (layerTilesOffset.x * tileSize);
					float yPosition = -((y * tileSize) - (layerTilesOffset.y * tileSize));

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
