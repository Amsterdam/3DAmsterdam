using ConvertCoordinates;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Networking;
using UnityEngine.UI;

public class Map : MonoBehaviour
{
    [SerializeField]
    private string tilesUrl = "https://t1.data.amsterdam.nl/topo_rd/{zoom}/{x}/{y}.png";

    private int minZoom = 6;
    private int maxZoom = 9;

    private int zoom = 6;
    private int x = 0; //8426
    private int y = 0; //to 5392

    private int gridColumns = 2;

    [SerializeField]
    private float meterScale = 0.001f;

    private Dictionary<int, GameObject> zoomLevelContainers;

    private void Start()
    {
        LoadGridTiles();
        zoomLevelContainers = new Dictionary<int, GameObject>();
    }

    public void LoadTilesInView()
    {
        //calculate what tiles we need to load
        //just check what zoomlevel we are at, and if the points in that grid level are in the view.
        //load those
    }

    private void LoadGridTiles()
    {        
        for (int i = 0; i < gridColumns; i++)
        {
            for (int j = 0; j < gridColumns; j++)
            {
                StartCoroutine(LoadTile(zoom, x + i, y + j));
            }
        }
    }

    public void ZoomIn(bool useMousePosition = false)
    {
        if (zoom < maxZoom)
        {
            zoom++;
            gridColumns *= 2;

            LoadGridTiles();
        }
    }
    public void ZoomOut()
    {
        if (zoom > minZoom)
        {
            zoom--;
            gridColumns /= 2;

            LoadGridTiles();
        }
    }

    public void OffsetBy(Vector3 offset)
    {
        this.transform.Translate(this.transform.position.x - offset.x, offset.y - Input.mousePosition.y, 0.0f);
    }

    private IEnumerator LoadTile(int zoom, int x, int y)
    {
        var solvedUrl = tilesUrl.Replace("{zoom}", zoom.ToString()).Replace("{x}", x.ToString()).Replace("{y}", y.ToString());
        UnityWebRequest www = UnityWebRequestTexture.GetTexture(solvedUrl);
        yield return www.SendWebRequest();
        Debug.Log("Load: " + solvedUrl);
        if (www.isNetworkError || www.isHttpError)
        {
            Debug.Log(www.error);
        }
        else
        {
            GenerateZoomLevelParent(zoom);
            if (zoomLevelContainers.TryGetValue(zoom, out GameObject zoomLevelParent))
            {
                var keyName = zoom + "/" + x + "/" + y;
                Texture texture = ((DownloadHandlerTexture)www.downloadHandler).texture;

                GenerateTile(x, y, zoomLevelParent, keyName, texture);
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
    private static void GenerateTile(int x, int y, GameObject zoomLevelParent, string keyName, Texture texture)
    {
        var newMapTile = new GameObject();
        newMapTile.name = keyName;

        var gridCoordinates = new Vector3(x, y, 0.0f);

        var rawImage = newMapTile.AddComponent<RawImage>();
        rawImage.texture = texture;
        rawImage.rectTransform.pivot = Vector2.zero;

        rawImage.rectTransform.sizeDelta = new Vector2(1.0f, 1.0f);
        newMapTile.transform.SetParent(zoomLevelParent.transform, false);
        newMapTile.transform.localPosition = gridCoordinates;
    }
}
