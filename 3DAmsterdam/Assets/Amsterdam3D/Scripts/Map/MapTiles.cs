using ConvertCoordinates;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Remoting.Messaging;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Networking;
using UnityEngine.UI;

[System.Serializable]
public struct LoadTile{
    public GameObject loadedGameObject;
    public IEnumerator loadingProgress;
    public RawImage rawImage;
}

public class MapTiles : MonoBehaviour
{
    [SerializeField]
    private string tilesUrl = "https://t1.data.amsterdam.nl/topo_rd/{zoom}/{x}/{y}.png";

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
        GetComponent<RectTransform>().localScale = new Vector2(tilePixelSize, tilePixelSize);

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

                Rect tileRect = new Rect(
                    tilesDraggableContainer.anchoredPosition.x + (x * tilePixelSize),
                    tilesDraggableContainer.anchoredPosition.y + (y * tilePixelSize), 
                    tilePixelSize, tilePixelSize
                );
               
                //var tileIsInView = tileRect.Overlaps(viewBoundsArea.rect);
                
                //Create new tile object
                var newTileObject = new GameObject();
                var rawImage = newTileObject.AddComponent<RawImage>();

                newTileObject.name = key.x + "/" + key.y;

                //Posotion it in our parent according to x an y grid
                newTileObject.transform.SetParent(zoomLevelParent, false);
                newTileObject.transform.localPosition = new Vector3(x, y, 0);
                rawImage.rectTransform.pivot = Vector2.zero;
                rawImage.rectTransform.sizeDelta = Vector2.one;
                rawImage.enabled = false;

                //Construct for our dictionary object (to avoid double loading)
                var tileLoad = new LoadTile();
                tileLoad.rawImage = rawImage;
                tileLoad.loadedGameObject = newTileObject;              

                IEnumerator progress = LoadTile(Zoom, (int)key.x, (int)key.y, tileLoad);               
                //Add this to our dictionary

                tileLoad.loadingProgress = progress;

                StartCoroutine(progress);
            }
        }
    }

    public void ZoomIn(bool useMousePosition = false)
    {
        if (Zoom < maxZoom)
        {
            zoom++;
            gridCells *= 2;
            this.transform.localScale = tilePixelSize * (Vector3.one * (Zoom - minZoom + 1));
            LoadTilesInView();
        }
    }
    public void ZoomOut(bool useMousePosition = false)
    {
        if (Zoom > minZoom)
        {
            zoom--;
            gridCells /= 2;
            this.transform.localScale = tilePixelSize * (Vector3.one * (Zoom - minZoom + 1));
            LoadTilesInView();
        }
    }

    private IEnumerator LoadTile(int zoom, int x, int y, LoadTile targetLoadTile)
    {
        var solvedUrl = tilesUrl.Replace("{zoom}", zoom.ToString()).Replace("{x}", x.ToString()).Replace("{y}", y.ToString());
        UnityWebRequest www = UnityWebRequestTexture.GetTexture(solvedUrl);
        yield return www.SendWebRequest();

        if (www.isNetworkError || www.isHttpError)
        {
            Debug.Log(www.error);
        }
        else
        {
            if (zoomLevelContainers.TryGetValue(zoom, out GameObject zoomLevelParent))
            {
                Texture texture = ((DownloadHandlerTexture)www.downloadHandler).texture;
                targetLoadTile.rawImage.texture = texture;
                targetLoadTile.rawImage.enabled = true;
            }
        }
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
        else if (zoomLevelContainers.TryGetValue(zoom, out GameObject existingZoomLevelParent)){ 
            return existingZoomLevelParent.GetComponent<RectTransform>();
        }
        return null;
    }
}
