using ConvertCoordinates;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Networking;
using UnityEngine.UI;

[System.Serializable]
public struct LoadTile{
    public GameObject loadedGameObject;
    public IEnumerator loadingProgress;
}

public class Map : MonoBehaviour
{
    [SerializeField]
    private string tilesUrl = "https://t1.data.amsterdam.nl/topo_rd/{zoom}/{x}/{y}.png";

    private int minZoom = 6;
    private int maxZoom = 16;

    private int zoom = 6;

    private int gridCells = 3;

    private RectTransform tilesDraggableContainer;
    private RectTransform viewBoundsArea;

    [SerializeField]
    private int tilePixelSize = 256;

    private Dictionary<int, GameObject> zoomLevelContainers;
    private Dictionary<Vector2, LoadTile> currentLevelTiles;

    private void Start()
    {
        zoomLevelContainers = new Dictionary<int, GameObject>();
        currentLevelTiles = new Dictionary<Vector2, LoadTile>();

        GetComponent<RectTransform>().localScale = new Vector2(tilePixelSize, tilePixelSize);
        LoadTilesInView();
    }

    public void LoadTilesInView()
    {
        for (int x = 0; x < gridCells; x++)
        {
            for (int y = 0; y < gridCells; y++)
            {
                var key = new Vector2(x, y);

                Rect tileRect = new Rect(
                    tilesDraggableContainer.anchoredPosition.x + (x * tilePixelSize),
                    tilesDraggableContainer.anchoredPosition.y + (y * tilePixelSize), 
                    tilePixelSize, tilePixelSize
                );
               

                var alreadyLoaded = currentLevelTiles.ContainsKey(key);
                var tileIsInView = tileRect.Overlaps(viewBoundsArea.rect);

                Debug.Log(tileRect + " overlap? " + viewBoundsArea.rect + " ->" + tileIsInView);


                if (tileIsInView && !alreadyLoaded)
                {
                    var tileLoad = new LoadTile();
                    var newTileObject = new GameObject();
                    newTileObject.name = x + "/" + y;
                    tileLoad.loadedGameObject = newTileObject;

                    //If we are in view, load this tile (if wel arent already loading)
                    IEnumerator progress = LoadTile(zoom, x, y, tileLoad);               
                    //Add this to our dictionary

                    tileLoad.loadingProgress = progress;
                    currentLevelTiles.Add(key, tileLoad);

                    StartCoroutine(progress);
                }
                else if(alreadyLoaded && currentLevelTiles.TryGetValue(key, out LoadTile loadedTile))
                {
                    StopCoroutine(loadedTile.loadingProgress);
                    Destroy(loadedTile.loadedGameObject);
                    currentLevelTiles.Remove(key);
                }

                //remove two levels below, and all top ones
            }
        }
    }

    public void SetMapAreas(RectTransform view, RectTransform drag)
    {
        tilesDraggableContainer = drag;
        viewBoundsArea = view;
    }

    /*private void LoadGridTiles()
    {        
        for (int i = 0; i < gridColumns; i++)
        {
            for (int j = 0; j < gridColumns; j++)
            {
                StartCoroutine(LoadTile(zoom, i, j));
            }
        }
    }*/

    public void ZoomIn(bool useMousePosition = false)
    {
        if (zoom < maxZoom)
        {
            zoom++;
            gridCells *= 2;
            this.transform.localScale = tilePixelSize * (Vector3.one * (zoom - minZoom + 1));
            LoadTilesInView();
        }
    }
    public void ZoomOut(bool useMousePosition = false)
    {
        if (zoom > minZoom)
        {
            zoom--;
            gridCells /= 2;
            this.transform.localScale = tilePixelSize * (Vector3.one * (zoom - minZoom + 1));
            LoadTilesInView();
        }
    }

    public void OffsetBy(Vector3 offset)
    {
        this.transform.Translate(this.transform.position.x - offset.x, offset.y - Input.mousePosition.y, 0.0f);
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
            GenerateZoomLevelParent(zoom);
            if (zoomLevelContainers.TryGetValue(zoom, out GameObject zoomLevelParent))
            {
                Texture texture = ((DownloadHandlerTexture)www.downloadHandler).texture;
                LoadedTexture(x, y, zoomLevelParent, targetLoadTile.loadedGameObject, texture);
            }
        }
    }
    private void GenerateZoomLevelParent(int zoom)
    {
        if (!zoomLevelContainers.ContainsKey(zoom))
        {
            var newZoomLevelParent = new GameObject().AddComponent<RectTransform>();
            newZoomLevelParent.name = zoom.ToString();
            zoomLevelContainers.Add(zoom, newZoomLevelParent.gameObject);
            newZoomLevelParent.pivot = Vector2.zero;
            newZoomLevelParent.localScale = Vector3.one * Mathf.Pow(2,-(zoom-minZoom));
            newZoomLevelParent.SetParent(transform,false);
        }
    }
    private static void LoadedTexture(int x, int y, GameObject zoomLevelParent, GameObject newMapTile, Texture texture)
    {
        var gridCoordinates = new Vector3(x, y, 0.0f);

        var rawImage = newMapTile.AddComponent<RawImage>();
        rawImage.texture = texture;
        rawImage.rectTransform.pivot = Vector2.zero;
        rawImage.rectTransform.sizeDelta = Vector2.one;
        newMapTile.transform.SetParent(zoomLevelParent.transform, false);
        newMapTile.transform.localPosition = gridCoordinates;
    }
}
