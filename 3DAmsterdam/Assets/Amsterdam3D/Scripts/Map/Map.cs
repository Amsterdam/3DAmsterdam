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

    private void Start()
    {
        LoadGrid();
    }

    public void LoadTilesInView()
    {
        //calculate what tiles we need to load
        //just check what zoomlevel we are at, and if the points in that grid level are in the view.
        //load those
    }

    private void LoadGrid()
    {        
        for (int i = 0; i < gridColumns; i++)
        {
            for (int j = 0; j < gridColumns; j++)
            {
                StartCoroutine(LoadTile(zoom, x + i, y + j));
            }
        }
    }

    public void ZoomIn()
    {
        if (zoom < maxZoom)
        {
            zoom++;
            gridColumns *= 2;

            LoadGrid();
        }
    }

    public void ZoomOut()
    {
        if (zoom > minZoom)
        {
            zoom--;
            gridColumns /= 2;

            LoadGrid();
        }
    }

    private IEnumerator LoadTile(int zoom, int x, int y)
    {
        //Vector3WGS wgs = ConvertCoordinates.Vector3WGS();       

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
            var keyName = zoom + "/" + x + "/" + y;
            Texture texture = ((DownloadHandlerTexture)www.downloadHandler).texture;

            GameObject newMapTile = new GameObject();
            newMapTile.name = keyName;
            Vector3 gridCoordinates = new Vector3(x,y, 0.0f);

            RawImage rawImage = newMapTile.AddComponent<RawImage>();
            rawImage.texture = texture;
            rawImage.rectTransform.pivot = Vector2.zero;

            rawImage.rectTransform.sizeDelta = new Vector2(1.0f, 1.0f);
            newMapTile.transform.SetParent(transform, false);
            newMapTile.transform.localPosition = gridCoordinates;
        }
    }
}
