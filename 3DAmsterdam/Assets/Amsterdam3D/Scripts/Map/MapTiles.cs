using ConvertCoordinates;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace Amsterdam3D.Interface
{
    public class MapTiles : MonoBehaviour
    {
        private int minZoom = 6;
        private int maxZoom = 12;

        [SerializeField]
        private int startCellX = 28;
        [SerializeField]
        private int startCellY = 32;

        [SerializeField]
        private RectTransform pointer;

        private Vector2 mapBottomLeftRDCoordinates;
        private Vector2 mapTopRightRDCoordinates;

        [SerializeField]
        private int zoom = 6;
        [SerializeField]
        private int gridCells = 3;

        public Vector2 MapBottomLeftRDCoordinates { get => mapBottomLeftRDCoordinates; }
        public Vector2 MapTopRightRDCoordinates { get => mapTopRightRDCoordinates; }

        private Vector3 bottomLeftUnityCoordinates, topRightUnityCoordinates;
        public Vector3 BottomLeftUnityCoordinates { get => bottomLeftUnityCoordinates; }
        public Vector3 TopRightUnityCoordinates { get => topRightUnityCoordinates; }

        public int Zoom { get => zoom; }
        public int GridCells { get => gridCells; }
        public int StartCellX { get => startCellX; }
        public int StartCellY { get => startCellY; }

        private float keyTileSize;
        private float distanceX;
        private float distanceY;
        private float tileOffsetX;
        private float tileOffsetY;

        private RectTransform tilesDraggableContainer;
        private RectTransform viewBoundsArea;
        private RectTransform zoomLevelParent;

        [SerializeField]
        private int tilePixelSize = 256;

        [SerializeField]
        private int maxTilesToLoad = 6;

        private Dictionary<int, GameObject> zoomLevelContainers;
        private Dictionary<Vector2, MapTile> loadedTiles;

        public void Initialize(RectTransform view, RectTransform drag)
        {
            loadedTiles = new Dictionary<Vector2, MapTile>();

            tilesDraggableContainer = drag;
            viewBoundsArea = view;
            zoomLevelContainers = new Dictionary<int, GameObject>();
            zoomLevelParent = GetZoomLevelParent(Zoom);

            CalculateMapCoordinates();
            LoadTilesInView();
        }
        private void CalculateMapCoordinates()
        {
            var gridCellTileSize = Constants.MINIMAP_RD_ZOOM_0_TILESIZE / Mathf.Pow(2, Zoom);
            mapBottomLeftRDCoordinates = new Vector2(Constants.MINIMAP_RD_BOTTOMLEFT_X + (gridCellTileSize * StartCellX), Constants.MINIMAP_RD_BOTTOMLEFT_Y + (gridCellTileSize * StartCellY));
            mapTopRightRDCoordinates = new Vector2(Constants.MINIMAP_RD_BOTTOMLEFT_X + (gridCellTileSize * (StartCellX + gridCells)), Constants.MINIMAP_RD_BOTTOMLEFT_Y + (gridCellTileSize * (StartCellY + gridCells)));

            bottomLeftUnityCoordinates = CoordConvert.RDtoUnity(new Vector3(mapBottomLeftRDCoordinates.x, mapBottomLeftRDCoordinates.y, 0.0f));
            topRightUnityCoordinates = CoordConvert.RDtoUnity(new Vector3(mapTopRightRDCoordinates.x, mapTopRightRDCoordinates.y, 0.0f));

            CalculateGridOffset();
        }
        private void CalculateGridOffset()
        {
            keyTileSize = Constants.MINIMAP_RD_ZOOM_0_TILESIZE / Mathf.Pow(2, Zoom);

            distanceX = MapBottomLeftRDCoordinates.x - Constants.MINIMAP_RD_BOTTOMLEFT_X;
            distanceY = MapBottomLeftRDCoordinates.y - Constants.MINIMAP_RD_BOTTOMLEFT_Y;
            tileOffsetX = Mathf.Floor(distanceX / keyTileSize);
            tileOffsetY = Mathf.Floor(distanceY / keyTileSize);
        }

        public void LoadTilesInView()
        {
            // Calculate new offset for grid
            Debug.Log($"zoom:{Zoom}, keyTileSize: {keyTileSize}, distanceX: {distanceX}, distanceY: {distanceY}, tileOffsetX: {tileOffsetX}, , tileOffsetY: {tileOffsetY}");

            for (int x = 0; x < GridCells; x++)
            {
                for (int y = 0; y < GridCells; y++)
                {
                    var key = new Vector2(tileOffsetX + x, tileOffsetY + y);
                    var tileSize = tilePixelSize * zoomLevelParent.transform.localScale.x;
                   
                    //TODO: Refactor to something faster for checking overlap of quads
                    Rect tileRect = new Rect(
                       tilesDraggableContainer.anchoredPosition.x + ((x * tileSize * tilesDraggableContainer.localScale.x)) + (tileSize/2.0f),
                       tilesDraggableContainer.anchoredPosition.y + ((y * tileSize * tilesDraggableContainer.localScale.x)) + (tileSize/2.0f),
                       tileSize * tilesDraggableContainer.localScale.x*2.0f, tileSize * tilesDraggableContainer.localScale.x * 2.0f
                    );
                    var tileIsInView = tileRect.Overlaps(viewBoundsArea.rect);

                    if ((tileIsInView || zoom == minZoom) && !loadedTiles.ContainsKey(key))
                    {
                        var newTileObject = new GameObject();
                        var mapTile = newTileObject.AddComponent<MapTile>();
                        mapTile.Initialize(zoomLevelParent, viewBoundsArea, Zoom, tilePixelSize, x, y, key);
                        loadedTiles.Add(key, mapTile);
                    }
                    else if (zoom != minZoom && !tileIsInView && loadedTiles.Count > maxTilesToLoad && loadedTiles.ContainsKey(key) && loadedTiles.TryGetValue(key, out MapTile mapTile))
                    {
                        loadedTiles.Remove(key);
                        if(mapTile)
                            Destroy(mapTile.gameObject);
                    }
                }
            }
        }

        private void ClearZoomLevelContainers()
        {
            //Remove all zoomlevel containers with images that are not the base zoomlevel, and are two levels down
            //from current zoomlevel, or above.
            var itemsToRemove = zoomLevelContainers.Where(f => (f.Key < zoom - 1 || f.Key > zoom) && f.Key != minZoom).ToArray();
            foreach (var zoomLevelContainer in itemsToRemove)
            {
                print("DESTROYING");
                Destroy(zoomLevelContainer.Value);
                zoomLevelContainers.Remove(zoomLevelContainer.Key);   
            }

            //Clear current level loaded tiles list
            loadedTiles.Clear();
        }

        public void ZoomIn(bool useMousePosition = true)
        {
            if (Zoom < maxZoom)
            {
                zoom++;
                gridCells *= 2;
                ZoomTowardsLocation(useMousePosition);
                CalculateGridOffset();
                zoomLevelParent = GetZoomLevelParent(Zoom);
                ClearZoomLevelContainers();
                LoadTilesInView();
            }
        }

        public void ZoomOut(bool useMousePosition = true)
        {
            if (Zoom > minZoom)
            {
                zoom--;
                gridCells /= 2;
                ZoomTowardsLocation(useMousePosition);
                CalculateGridOffset();
                zoomLevelParent = GetZoomLevelParent(Zoom);
                ClearZoomLevelContainers();
                LoadTilesInView();
            }
        }

        private void ZoomTowardsLocation(bool useMouse = true)
        {
            var zoomTarget = Vector3.zero;
            if (useMouse)
            {
                zoomTarget = Input.mousePosition;
            }
            else
            {
                zoomTarget = viewBoundsArea.position + new Vector3(viewBoundsArea.sizeDelta.x * 0.5f, viewBoundsArea.sizeDelta.y * 0.5f);
            }

            var zoomFactor = Mathf.Pow(2.0f,(Zoom - minZoom));
            ScaleOverOrigin(tilesDraggableContainer.gameObject, zoomTarget, Vector3.one * zoomFactor);

            //Match pointer scale to resized container
            pointer.localScale = Vector3.one / tilesDraggableContainer.localScale.x;
        }

        public void ScaleOverOrigin(GameObject target, Vector3 scaleOrigin, Vector3 newScale)
        {
            var targetPosition = target.transform.position;
            var origin = scaleOrigin;
            var newOrigin = targetPosition - origin;
            var relativeScale = newScale.x / target.transform.localScale.x;
            var finalPosition = origin + newOrigin * relativeScale;

            target.transform.localScale = newScale;
            target.transform.position = finalPosition;
        }

        private RectTransform GetZoomLevelParent(int zoom)
        {
            if (!zoomLevelContainers.ContainsKey(zoom))
            {
                var newZoomLevelParent = new GameObject().AddComponent<RectTransform>();
                newZoomLevelParent.name = zoom.ToString();
                zoomLevelContainers.Add(zoom, newZoomLevelParent.gameObject);
                newZoomLevelParent.pivot = Vector2.zero;
                newZoomLevelParent.localScale = Vector3.one * Mathf.Pow(2, -(zoom - minZoom));
                newZoomLevelParent.SetParent(transform, false);
                return newZoomLevelParent;
            }
            else if (zoomLevelContainers.TryGetValue(zoom, out GameObject existingZoomLevelParent))
            {
                return existingZoomLevelParent.GetComponent<RectTransform>();
            }
            return null;
        }
    }
}