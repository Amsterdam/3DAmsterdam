using ConvertCoordinates;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Remoting.Messaging;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace Amsterdam3D.Interface
{
    [System.Serializable]
    public struct LoadTile
    {
        public GameObject loadedGameObject;
        public IEnumerator loadingProgress;
        public RawImage rawImage;
    }

    public class MapTiles : MonoBehaviour
    {
        private int minZoom = 6;
        private int maxZoom = 16;

        [SerializeField]
        private int startCellX = 28;
        [SerializeField]
        private int startCellY = 32;

        private Vector2 mapBottomLeftRDCoordinates;
        private Vector2 mapTopRightRDCoordinates;

        [SerializeField]
        private int zoom = 6;
        [SerializeField]
        private int gridCells = 3;

        public Vector2 MapBottomLeftRD { get => mapBottomLeftRDCoordinates; }
        public Vector2 MapTopRightRD { get => mapTopRightRDCoordinates; }

        private Vector3 bottomLeftUnityCoordinates, topRightUnityCoordinates;
        public Vector3 BottomLeftUnityCoordinates { get => bottomLeftUnityCoordinates; }
        public Vector3 TopRightUnityCoordinates { get => topRightUnityCoordinates; }

        public int Zoom { get => zoom; }
        public int GridCells { get => gridCells; }
        public int StartCellX { get => startCellX; }
        public int StartCellY { get => startCellY; }

        private RectTransform tilesDraggableContainer;
        private RectTransform viewBoundsArea;

        [SerializeField]
        private int tilePixelSize = 256;

        private Dictionary<int, GameObject> zoomLevelContainers;

        public void Initialize(RectTransform view, RectTransform drag)
        {
            tilesDraggableContainer = drag;
            viewBoundsArea = view;
            zoomLevelContainers = new Dictionary<int, GameObject>();

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
        }

        public void LoadTilesInView()
        {
            //Make sure we dont continue loading other zoom level stuff
            StopAllCoroutines();

            // Calculate new offset for grid
            var keyTileSize = Constants.MINIMAP_RD_ZOOM_0_TILESIZE / Mathf.Pow(2, Zoom);

            var distanceX = MapBottomLeftRD.x - Constants.MINIMAP_RD_BOTTOMLEFT_X;
            var distanceY = MapBottomLeftRD.y - Constants.MINIMAP_RD_BOTTOMLEFT_Y;
            var tileOffsetX = Mathf.Floor(distanceX / keyTileSize);
            var tileOffsetY = Mathf.Floor(distanceY / keyTileSize);

            Debug.Log($"zoom:{Zoom}, keyTileSize: {keyTileSize}, distanceX: {distanceX}, distanceY: {distanceY}, tileOffsetX: {tileOffsetX}, , tileOffsetY: {tileOffsetY}");

            //Make sure we have a parent for our tiles
            var zoomLevelParent = GetZoomLevelParent(Zoom);

            for (int x = 0; x < GridCells; x++)
            {
                for (int y = 0; y < GridCells; y++)
                {
                    var key = new Vector2(tileOffsetX + x, tileOffsetY + y);
                    //Create new tile object
                    var newTileObject = new GameObject();
                    newTileObject.AddComponent<MapTile>().Initialize(zoomLevelParent, viewBoundsArea, Zoom, tilePixelSize, x, y, key); 
                }
            }
        }

        public void ZoomIn(bool useMousePosition = true)
        {
            if (Zoom < maxZoom)
            {
                zoom++;
                gridCells *= 2;

                ZoomTowardsLocation(useMousePosition);
                //LoadTilesInView();
            }
        }

        public void ZoomOut(bool useMousePosition = true)
        {
            if (Zoom > minZoom)
            {
                zoom--;
                gridCells /= 2;
                ZoomTowardsLocation(useMousePosition);
                //LoadTilesInView();
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
            ScaleOverOrigin(tilesDraggableContainer.gameObject, zoomTarget, Vector3.one * (Zoom - minZoom + 1));
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