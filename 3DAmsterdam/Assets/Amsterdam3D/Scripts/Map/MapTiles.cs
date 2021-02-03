using Amsterdam3D.CameraMotion;
using ConvertCoordinates;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace Amsterdam3D.Interface
{
    public class MapTiles : MonoBehaviour, IPointerClickHandler
    {
        private bool loadedBottomLayer = false;

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

        private int baseGridCells = 3;

        public Vector2 MapBottomLeftRDCoordinates { get => mapBottomLeftRDCoordinates; }
        public Vector2 MapTopRightRDCoordinates { get => mapTopRightRDCoordinates; }

        private Vector3 bottomLeftUnityCoordinates, topRightUnityCoordinates;
        public Vector3 BottomLeftUnityCoordinates { get => bottomLeftUnityCoordinates; }
        public Vector3 TopRightUnityCoordinates { get => topRightUnityCoordinates; }

        public int Zoom { get => zoom; }
        public int GridCells { get => gridCells; }
        public int StartCellX { get => startCellX; }
        public int StartCellY { get => startCellY; }
        public int TilePixelSize { get => tilePixelSize;  }
        public int MapPixelWidth { get => mapPixelWidth; }

        private float keyTileSize;
        private float distanceX;
        private float distanceY;
        private float tileOffsetX;
        private float tileOffsetY;

        private const float viewLoadMargin = 500.0f;

        private RectTransform tilesDraggableContainer;
        private RectTransform viewBoundsArea;
        private RectTransform zoomLevelParent;

        private const int tilePixelSize = 256; //Width/height pixels
        private float currentRelativeTileSize;
        private int mapPixelWidth;

        [SerializeField]
        private int maxTilesToLoad = 6;

        private Dictionary<int, GameObject> zoomLevelContainers;
        private Dictionary<Vector2, MapTile> loadedTiles;

        private Vector3 lastDraggedPointerPosition;

        private Canvas canvas;

        public void Initialize(RectTransform view, RectTransform drag)
        {
            canvas = transform.root.GetComponent<Canvas>();

            tilesDraggableContainer = drag;
            viewBoundsArea = view;

            mapPixelWidth = TilePixelSize * baseGridCells;

            loadedTiles = new Dictionary<Vector2, MapTile>();

            zoomLevelContainers = new Dictionary<int, GameObject>();
            zoomLevelParent = GetZoomLevelParent(Zoom);

            CalculateMapCoordinates();
            LoadTilesInView();

            pointer.localScale = Vector3.one / tilesDraggableContainer.localScale.x;
        }

        public void CenterMapOnPointer()
        {
            tilesDraggableContainer.anchoredPosition = -pointer.transform.localPosition* tilesDraggableContainer.localScale.x;

            if (Vector3.Distance(lastDraggedPointerPosition, tilesDraggableContainer.localPosition) > tilePixelSize / tilesDraggableContainer.localScale.y)
            {
                LoadTilesInView();
            }
        }

        void Update()
        {
            PutPointerOnCameraLocation();
        }

        private void PutPointerOnCameraLocation()
        {
            var cameraRDPosition = CoordConvert.UnitytoRD(CameraModeChanger.Instance.ActiveCamera.transform.position);

            var posX = Mathf.InverseLerp(mapBottomLeftRDCoordinates.x, mapTopRightRDCoordinates.x, (float)cameraRDPosition.x);
            var posY = Mathf.InverseLerp(mapBottomLeftRDCoordinates.y, mapTopRightRDCoordinates.y, (float)cameraRDPosition.y);

            pointer.anchoredPosition = new Vector3(posX * mapPixelWidth * transform.localScale.x, posY * mapPixelWidth * transform.localScale.y, 0.0f);
        }

        public void ClampInViewBounds(Vector3 targetPosition)
        {
            tilesDraggableContainer.position = new Vector3()
            {
                x = Mathf.Clamp(targetPosition.x, viewBoundsArea.position.x - (MapPixelWidth * tilesDraggableContainer.localScale.x * canvas.scaleFactor), viewBoundsArea.position.x - TilePixelSize),
                y = Mathf.Clamp(targetPosition.y, viewBoundsArea.position.y - (MapPixelWidth * tilesDraggableContainer.localScale.y * canvas.scaleFactor) + TilePixelSize, viewBoundsArea.position.y)
            };
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            Vector3 localClickPosition = transform.InverseTransformPoint(eventData.position);

            var RDcoordinate = CoordConvert.RDtoUnity(new Vector3RD
            {
                x = Mathf.Lerp(mapBottomLeftRDCoordinates.x, mapTopRightRDCoordinates.x, localClickPosition.x / mapPixelWidth),
                y = Mathf.Lerp(MapBottomLeftRDCoordinates.y, MapTopRightRDCoordinates.y, localClickPosition.y / mapPixelWidth),
                z = 0.0
            });
            RDcoordinate.y = CameraModeChanger.Instance.ActiveCamera.transform.position.y;
            CameraModeChanger.Instance.ActiveCamera.transform.position = RDcoordinate;

            //PutPointerOnCameraLocation(); //TODO: Needs a smooth transition to not be disturbing
            //CenterMapOnPointer();
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
            lastDraggedPointerPosition = tilesDraggableContainer.localPosition;
            for (int x = 0; x < GridCells; x++)
            {
                for (int y = 0; y < GridCells; y++)
                {
                    var key = new Vector2(tileOffsetX + x, tileOffsetY + y);
                    var tileIsInView = Mathf.Abs(tilesDraggableContainer.localPosition.x + (currentRelativeTileSize*x) + currentRelativeTileSize/2.0f) < viewLoadMargin && Mathf.Abs(tilesDraggableContainer.localPosition.y + (currentRelativeTileSize * y) + currentRelativeTileSize / 2.0f) < 500.0f;

                    //Only load a tile if its the initial bottom layer, or if it is in view and didnt load yet 
                    if ((zoom == minZoom && !loadedBottomLayer) || (zoom != minZoom && tileIsInView && !loadedTiles.ContainsKey(key)))
                    {
                        var newTileObject = new GameObject();
                        var mapTile = newTileObject.AddComponent<MapTile>();
                        mapTile.Initialize(zoomLevelParent, Zoom, tilePixelSize, x, y, key, (zoom == minZoom));
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
            //We only load the base layer once, because it will never be destroyed
            if(minZoom == zoom)
            {
                loadedBottomLayer = true;
            }
        }

        private void ClearZoomLevelContainers()
        {
            //Remove all zoomlevel containers with images that are not the base zoomlevel, and are two levels down
            //from current zoomlevel, or above.
            var itemsToRemove = zoomLevelContainers.Where(f => (f.Key < zoom - 1 || f.Key > zoom) && f.Key != minZoom).ToArray();
            foreach (var zoomLevelContainer in itemsToRemove)
            {
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

                StopAllCoroutines();
                StartCoroutine(DelayLoadingTiles());
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

                StopAllCoroutines();
                StartCoroutine(DelayLoadingTiles());
            }
        }

        IEnumerator DelayLoadingTiles()
        {
            yield return new WaitForSeconds(0.2f);
            ClearZoomLevelContainers();
            LoadTilesInView();
        }

        private void ZoomTowardsLocation(bool useMouse = true)
        {
            var zoomTarget = Vector3.zero;
            var zoomFactor = Mathf.Pow(2.0f, (Zoom - minZoom));

            if (useMouse)
            {
                zoomTarget = Input.mousePosition;
            }
            else
            {
                zoomTarget = viewBoundsArea.position + new Vector3(-viewBoundsArea.sizeDelta.x * 0.5f, viewBoundsArea.sizeDelta.y * 0.5f);
            }

            ScaleOverOrigin(tilesDraggableContainer.gameObject, zoomTarget, Vector3.one * zoomFactor);

            ClampInViewBounds(tilesDraggableContainer.transform.position);

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
                currentRelativeTileSize = tilePixelSize * newZoomLevelParent.transform.localScale.x * tilesDraggableContainer.localScale.x;
                return newZoomLevelParent;
            }
            else if (zoomLevelContainers.TryGetValue(zoom, out GameObject existingZoomLevelParent))
            {
                currentRelativeTileSize = tilePixelSize * existingZoomLevelParent.transform.localScale.x * tilesDraggableContainer.localScale.x;
                return existingZoomLevelParent.GetComponent<RectTransform>();
            }
            return null;
        }
    }
}