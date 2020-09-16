using BruTile;
using Terrain.ExtensionMethods;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Terrain.Tiles;
using UnityEngine;
using UnityEngine.Networking;

using System;
using ConvertCoordinates;
using Amsterdam3D.CameraMotion;

public class TileLoader : MonoBehaviour
{
    private Boolean UpdateTerrainTilesFinished = true;
    public Material DeFaultMaterial;
    public ICameraExtents CV;
    private Extent previousCameraViewExtent = new Extent(0, 0, 0, 0);
    [SerializeField] private string dataFolder = "terrain";
    private string terrainUrl;
    public string textureUrl = "https://map.data.amsterdam.nl/cgi-bin/mapserv?map=/srv/mapserver/topografie.map&REQUEST=GetMap&VERSION=1.1.0&SERVICE=wms&styles=&layers=basiskaart-zwartwit&format=image%2Fpng&bbox={xMin}%2C{yMin}%2C{xMax}%2C{yMax}&width=256&height=256&srs=EPSG%3A4326&crs=EPSG%3A4326";
   
    public GameObject placeholderTile;
    private const int tilesize = 180;

    readonly Dictionary<Vector3, GameObject> activeTiles = new Dictionary<Vector3, GameObject>();
    Dictionary<Vector3, GameObject> TeVerwijderenTiles = new Dictionary<Vector3, GameObject>();
    const int maxParallelRequests = 8;
    Queue<downloadRequest> downloadQueue = new Queue<downloadRequest>();
    public Dictionary<string, downloadRequest> activeDownloads = new Dictionary<string, downloadRequest>(maxParallelRequests);

    public enum TileService
    {
        WMS,
        QM
    }

    public struct downloadRequest
    {

        public string Url;
        public TileService Service;
        public Vector3 TileId;

        public downloadRequest(string url, TileService service, Vector3 tileId)
        {
            Url = url;
            Service = service;
            TileId = tileId;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        CV = CameraControls.Instance.camera.GetComponent<CameraView>();
        terrainUrl = Constants.BASE_DATA_URL + dataFolder + "/{z}/{x}/{y}.terrain";
    }

    // Update is called once per frame
    void Update()
    {
        RemoveTiles();

        if (HasCameraViewChanged())
        {
            if (UpdateTerrainTilesFinished)
            {
                previousCameraViewExtent = CV.GetExtent();
                StartCoroutine(UpdateTerrainTiles(previousCameraViewExtent));
            }
        }
        
        // Continue downloading from queue
        if (activeDownloads.Count < maxParallelRequests && downloadQueue.Count > 0)
        {
            var request = downloadQueue.Dequeue();

            if(!activeDownloads.ContainsKey(request.Url))
                activeDownloads.Add(request.Url, request);

            //fire request

            StartCoroutine(requestQMTile(request.Url, request.TileId));

        }
    }

    bool HasCameraViewChanged()
    {
        bool cameraviewChanged = false;
        if (previousCameraViewExtent.CenterX != CV.GetExtent().CenterX|| previousCameraViewExtent.CenterY != CV.GetExtent().CenterY)
        {
            cameraviewChanged=true;
        }
        return cameraviewChanged;
    }

    private GameObject DrawPlaceHolder(Vector3 t)
    {
        var tile = Instantiate(placeholderTile);
        
        tile.transform.parent = transform;
        tile.name = $"tile/{t.x.ToString()}/{t.y.ToString()}/{t.z.ToString()}";
        tile.transform.position = GetTilePosition(t);
        
        tile.transform.localScale = new Vector3(ComputeScaleFactorX(int.Parse(t.z.ToString())), 1, ComputeScaleFactorY(int.Parse(t.z.ToString())));
        return tile;
    }


    /// <summary>
    /// TilePosition in Unity
    /// </summary>
    /// <param name="index">TileIndex</param>
    /// <returns>TilePosition in Unity</returns>
    private Vector3 GetTilePosition(Vector3 index)
    {

        double tegelbreedte = tilesize / Math.Pow(2, int.Parse(index.z.ToString())); //TileSize in Degrees
        Vector3WGS origin = new Vector3WGS();
        origin.lon = ((index.x + 0.5) * tegelbreedte) - 180;
        origin.lat = ((index.y + 0.5) * tegelbreedte) - 90;
        origin.h = 0 - CoordConvert.ReferenceRD.z;
        return CoordConvert.WGS84toUnity(origin);
    }

    public void UpdateTerrainTextures(string newTextureURL)
    {
        textureUrl = newTextureURL;
        List<KeyValuePair<Vector3,GameObject>> temp = activeTiles.ToList();
        StopAllCoroutines();
        for (int i = 0; i < temp.Count; i++)
        {
           StartCoroutine(UpdateTerrainTexture(textureUrl, temp[i].Key));
        }
    }

    private IEnumerator UpdateTerrainTexture(string url, Vector3 tileId)
    {
        DownloadHandlerBuffer handler = new DownloadHandlerBuffer();
        var meshRenderer = activeTiles[tileId].GetComponent<MeshRenderer>();
        var baseMap = meshRenderer.material.GetTexture("_BaseMap");
        if (url.Length == 0)
        {
            //If the url is empty, simply clear the texture slot
            meshRenderer.material.SetTexture("_BaseMap", null);
            if (baseMap) Destroy(baseMap);
            yield break;
        }

        //get tile texture data
        var schema = new Terrain.TmsGlobalGeodeticTileSchema();
        Extent subtileExtent = TileTransform.TileToWorld(new TileRange(int.Parse(tileId.x.ToString()), int.Parse(tileId.y.ToString())), tileId.z.ToString(), schema);
        string wmsUrl = textureUrl.Replace("{xMin}", subtileExtent.MinX.ToString()).Replace("{yMin}", subtileExtent.MinY.ToString()).Replace("{xMax}", subtileExtent.MaxX.ToString()).Replace("{yMax}", subtileExtent.MaxY.ToString()).Replace(",", ".");
        if (tileId.z == 17)
        {
            wmsUrl = wmsUrl.Replace("width=256", "width=1024").Replace("height=256", "height=1024");
        }
        UnityWebRequest www = UnityWebRequestTexture.GetTexture(wmsUrl);
        yield return www.SendWebRequest();

        if (!www.isNetworkError && !www.isHttpError)
        {
            if (activeTiles.ContainsKey(tileId))
            {
                Destroy(baseMap);

                var loadedTexture = ((DownloadHandlerTexture)www.downloadHandler).texture;
                loadedTexture.wrapMode = TextureWrapMode.Clamp;
                meshRenderer.material.SetTexture("_BaseMap", loadedTexture);
            }
        }
        else
        {
            Debug.LogWarning("Tile: [" + tileId.x + " " + tileId.y + "] Error loading texture data");
        }
    }

    private IEnumerator requestQMTile(string url, Vector3 tileId)
    {
        DownloadHandlerBuffer handler = new DownloadHandlerBuffer();
        TerrainTile terrainTile;

        ///QM-tile downloaden
        UnityWebRequest www = new UnityWebRequest(url);

        www.downloadHandler = handler;
        yield return www.SendWebRequest();

        if (!www.isNetworkError && !www.isHttpError)
        {
            //get data
            MemoryStream stream = new MemoryStream(www.downloadHandler.data);

            //parse into tile data structure
            terrainTile = TerrainTileParser.Parse(stream);

            //update tile with height data
            if (activeTiles.ContainsKey(tileId))
            {
                var meshFilter = activeTiles[tileId].GetComponent<MeshFilter>();
                meshFilter.mesh.vertices = terrainTile.GetVertices(0);
                meshFilter.mesh.triangles = terrainTile.GetTriangles(0);
                meshFilter.mesh.uv = terrainTile.GetUV(0);
                meshFilter.mesh.RecalculateNormals();

                activeTiles[tileId].AddComponent<MeshCollider>().sharedMesh = meshFilter.sharedMesh;
                activeTiles[tileId].transform.localScale = new Vector3(ComputeScaleFactorX((int)tileId.z), 1, ComputeScaleFactorY((int)tileId.z));
                Vector3 loc = activeTiles[tileId].transform.localPosition;
                loc.y = 0;
                activeTiles[tileId].transform.localPosition = loc;
                activeTiles[tileId].layer = 9;
            }
        }
        else
        {
            Debug.Log("Tile: [" + tileId.z + "/" + tileId.x + "/" + tileId.y + "] Error loading height data");
            Debug.Log(www.error);
        }

        if (textureUrl != "")
        {
            var schema = new Terrain.TmsGlobalGeodeticTileSchema();
            Extent subtileExtent = TileTransform.TileToWorld(new TileRange(int.Parse(tileId.x.ToString()), int.Parse(tileId.y.ToString())), tileId.z.ToString(), schema);
            string wmsUrl = textureUrl.Replace("{xMin}", subtileExtent.MinX.ToString()).Replace("{yMin}", subtileExtent.MinY.ToString()).Replace("{xMax}", subtileExtent.MaxX.ToString()).Replace("{yMax}", subtileExtent.MaxY.ToString()).Replace(",", ".");
            if (tileId.z >= 17)
            {
                wmsUrl = wmsUrl.Replace("width=256", "width=1024").Replace("height=256", "height=1024");
            }
            www = UnityWebRequestTexture.GetTexture(wmsUrl);
            yield return www.SendWebRequest();

            if (!www.isNetworkError && !www.isHttpError)
            {
                if (activeTiles.ContainsKey(tileId))
                {
                    var meshRenderer = activeTiles[tileId].GetComponent<MeshRenderer>();
                    Destroy(meshRenderer.material.GetTexture("_BaseMap"));

                    var loadedTexture = ((DownloadHandlerTexture)www.downloadHandler).texture;
                    loadedTexture.wrapMode = TextureWrapMode.Clamp;
                    meshRenderer.material.SetTexture("_BaseMap", loadedTexture);
                }
            }
            else
            {
                Debug.LogWarning("Tile: [" + tileId.x + " " + tileId.y + "] Error loading texture data: " + wmsUrl);
            }
        }

        activeDownloads.Remove(url);
    }

    private float ComputeScaleFactorX(int z)
    {
        return (float)(CoordConvert.UnitsPerDegreeX / Math.Pow(2, z));
    }

    private float ComputeScaleFactorY(int z)
    {
        return (float)(CoordConvert.UnitsPerDegreeY / Math.Pow(2, z));
    }


    

    private List<Vector3> SetBasicTilekeys(Extent TempExtent)
    {
        double TegelAfmeting = 180 / (Math.Pow(2, 13)); //tegelafmeting in graden bij zoomniveau 13;
        int tegelMinX = (int)Math.Floor((TempExtent.MinX + 180) / TegelAfmeting);
        int tegelMaxX = (int)Math.Floor((TempExtent.MaxX + 180) / TegelAfmeting);
        int tegelMinY = (int)Math.Floor((TempExtent.MinY + 90) / TegelAfmeting);
        int tegelMaxY = (int)Math.Floor((TempExtent.MaxY + 90) / TegelAfmeting);

        List<Vector3> TileKeys = new List<Vector3>();
        for (int X = tegelMinX; X < tegelMaxX + 1; X++)
        {

            for (int Y = tegelMinY; Y < tegelMaxY + 1; Y++)
            {
                Vector3 td = new Vector3(X, Y, 13);
                TileKeys.Add(td);
            }
        }
        return TileKeys;
    }

    private double GetMinimumDistance(float X, float Y, float Zoomlevel)
    {
        double TegelAfmeting = 180 / (Math.Pow(2, Zoomlevel)); //tegelafmeting in graden bij zoomniveau 10;
        double lon = (TegelAfmeting * X) - 180;
        double Lat = (TegelAfmeting * Y) - 90;
        double H = CoordConvert.ReferenceWGS84.h;
        Vector3 LocatieUnity = new Vector3();
        LocatieUnity = CoordConvert.WGS84toUnity(lon, Lat);
        LocatieUnity.x = (float)((lon - CoordConvert.ReferenceWGS84.lon) * CoordConvert.UnitsPerDegreeX);
        LocatieUnity.y = (float)CoordConvert.ReferenceWGS84.h;
        LocatieUnity.z = (float)((Lat - CoordConvert.ReferenceWGS84.lat) * CoordConvert.UnitsPerDegreeY);
        //LocatieUnity = CoordConvert.WGS84toUnity(locatieWGS);
        Vector3 afstand3D = LocatieUnity - CameraControls.Instance.camera.transform.localPosition;
        double afstand = Math.Sqrt(Math.Pow(afstand3D.x, 2) + Math.Pow(afstand3D.y, 2) + Math.Pow(afstand3D.z, 2));
        return afstand;
    }
    IEnumerator UpdateTerrainTiles(Extent Tempextent)
    {
        UpdateTerrainTilesFinished = false;
        List<Vector3> requiredTileKeys = SetBasicTilekeys(Tempextent);
        bool subTilesAdded = true;
        var schema = new Terrain.TmsGlobalGeodeticTileSchema();
        while (subTilesAdded)
        {
            yield return null;
            subTilesAdded = false;
            foreach (Vector3 tileKey in requiredTileKeys)
            {
                
                double tileSizeInDegrees = 180 / (Math.Pow(2, tileKey.z));

                double tileDistance = GetMinimumDistance(tileKey.x + 0.5f, tileKey.y + 0.5f, tileKey.z);
                double minafstand = 50* Math.Pow(2, (19 - tileKey.z));
                if (tileDistance < minafstand && tileKey.z < 18)
                {
                    AddSubtilesToTileKeys(tileKey, requiredTileKeys);
                    requiredTileKeys.Remove(tileKey);
                    subTilesAdded = true;
                    break;
                }

            }
        }
        requiredTileKeys.Reverse();

        yield return null;

        // bepalen welke reeds geladentegels niet meer nodig zijn en deze toevoegen aan TeVerwijderenTiles
        Vector3[] activeTileKeys = activeTiles.Keys.ToArray();
        bool tileIsRequired = false;
        foreach (Vector3 activeTile in activeTileKeys)
        {
            tileIsRequired = false;
            foreach (var requiredTileKey in requiredTileKeys)
            {
                if (requiredTileKey == activeTile)
                {
                    tileIsRequired = true;
                }
            }
            if (tileIsRequired == false)
            {
                if (TeVerwijderenTiles.ContainsKey(activeTile) == false)
                {
                    TeVerwijderenTiles.Add(activeTile, activeTiles[activeTile]);
                }
                
            }
        }

        // update downloadqueue
        downloadRequest[] downloadRequests = downloadQueue.ToArray();
        Queue<downloadRequest> tempQueue = new Queue<downloadRequest>();
        foreach (downloadRequest downloadRequest in downloadRequests)
        {
                tileIsRequired = false;
                foreach (var requiredTileKey in requiredTileKeys)
                {
                    if (requiredTileKey == downloadRequest.TileId)
                    {
                        tileIsRequired = true;
                        tempQueue.Enqueue(downloadRequest);
                    }
                }
        }
        downloadQueue = tempQueue;

        // Add new Tiles to ActiveTiles
        foreach (var newTileKey in requiredTileKeys)
        {
            //draw placeholder tile

            if (activeTiles.ContainsKey(newTileKey) == false) 
            {
                GameObject tile = DrawPlaceHolder(newTileKey);
                activeTiles.Add(newTileKey, tile);
                var qmUrl = terrainUrl.Replace("{x}", newTileKey.x.ToString()).Replace("{y}", newTileKey.y.ToString()).Replace("{z}", int.Parse(newTileKey.z.ToString()).ToString());
                downloadQueue.Enqueue(new downloadRequest(qmUrl, TileService.QM, newTileKey));
            }

        }
        UpdateTerrainTilesFinished = true;
    }


    void AddSubtilesToTileKeys(Vector3 parentTile, List<Vector3> TileKeys)
    {
        Vector3 newTile;

        // Add bottom-left tile.
        newTile = new Vector3(parentTile.x * 2, parentTile.y * 2, parentTile.z + 1);
        if (IsInsideExtent(newTile, CV.GetExtent()))
        {
            TileKeys.Add(newTile);
        }

        // Add botton-riht tile.
        newTile = new Vector3((parentTile.x * 2) + 1, parentTile.y * 2, parentTile.z + 1);
        if (IsInsideExtent(newTile, CV.GetExtent()))
        {

            TileKeys.Add(newTile);
        }
        // Add top-left tile.
        newTile = new Vector3(parentTile.x * 2, (parentTile.y * 2) + 1, parentTile.z + 1);
        if (IsInsideExtent(newTile, CV.GetExtent()))
        {
            TileKeys.Add(newTile);
        }

        // Add top-right tile
        newTile = new Vector3((parentTile.x * 2) + 1, (parentTile.y * 2) + 1, parentTile.z + 1);
        if (IsInsideExtent(newTile, CV.GetExtent()))
        {

            TileKeys.Add(newTile);
        }
    }

    Boolean IsInsideExtent(Vector3 tiledata, Extent BBox)
    {
        Boolean isbinnen = false;
        
        double TegelAfmeting = 180 / (Math.Pow(2, tiledata.z)); //tegelafmeting in graden bij zoomniveau 10;
        double lon = (TegelAfmeting * tiledata.x) - 180;
        double Lat = (TegelAfmeting * tiledata.y) - 90;
        Extent Subtileextent = new Extent((TegelAfmeting * tiledata.x) - 180, (TegelAfmeting * tiledata.y) - 90, (TegelAfmeting * (tiledata.x+1)) - 180, (TegelAfmeting * (tiledata.y+1)) - 90);

        // check Linkerbovenhoek
        if (Subtileextent.MaxY>BBox.MinY && Subtileextent.MinY<BBox.MaxY && Subtileextent.MaxX > BBox.MinX && Subtileextent.MinX < BBox.MaxX) { isbinnen = true; };


        return isbinnen;
    }

   

    private bool IsReplacementTileInDownloadQueue(Vector3 currentTileKey, downloadRequest[] downloadQueue )
    {
        bool replacementInQueue = false;

        Vector3 expectedBiggerTileKey = new Vector3(
            UnityEngine.Mathf.Floor(currentTileKey.x / 2),
            UnityEngine.Mathf.Floor(currentTileKey.y / 2), 
            currentTileKey.z - 1
            );
        Vector3 expectedSmallerTileKey = new Vector3(currentTileKey.x * 2, currentTileKey.y * 2, currentTileKey.z + 1);

        //check if smaller or bigger tilekey is in downloadqueue
        foreach (var downloadQueueItem in downloadQueue)
        {
            if (downloadQueueItem.TileId == expectedBiggerTileKey) { replacementInQueue = true; }
            if (downloadQueueItem.TileId == expectedSmallerTileKey) { replacementInQueue = true; }
            if (downloadQueueItem.TileId == (expectedSmallerTileKey + new Vector3(1, 0, 0))) { replacementInQueue = true; }
            if (downloadQueueItem.TileId == (expectedSmallerTileKey + new Vector3(0, 1, 0))) { replacementInQueue = true; }
            if (downloadQueueItem.TileId == (expectedSmallerTileKey + new Vector3(1, 1, 0))) { replacementInQueue = true; }
        }
        //check if smaller or bigger tilekey is in activeDownloadList
        foreach (var activeDownloadItem in activeDownloads)
        {
            if (activeDownloadItem.Value.TileId == expectedBiggerTileKey) { replacementInQueue = true; }
            if (activeDownloadItem.Value.TileId == expectedSmallerTileKey) { replacementInQueue = true; }
            if (activeDownloadItem.Value.TileId == (expectedSmallerTileKey + new Vector3(1, 0, 0))) { replacementInQueue = true; }
            if (activeDownloadItem.Value.TileId == (expectedSmallerTileKey + new Vector3(0, 1, 0))) { replacementInQueue = true; }
            if (activeDownloadItem.Value.TileId == (expectedSmallerTileKey + new Vector3(1, 1, 0))) { replacementInQueue = true; }
        }

        return replacementInQueue;
    }

    void RemoveTiles()
    {
        Vector3[] tilesToBeRemoved = TeVerwijderenTiles.Keys.ToArray();
        downloadRequest[] downloadRequests = downloadQueue.ToArray();

        foreach (Vector3 tileToBeRemoved in tilesToBeRemoved)
        {
            if (IsReplacementTileInDownloadQueue(tileToBeRemoved,downloadRequests)==false)
            {
                Destroy(TeVerwijderenTiles[tileToBeRemoved].GetComponent<MeshRenderer>().material.GetTexture("_BaseMap"));
                Destroy(TeVerwijderenTiles[tileToBeRemoved].GetComponent<MeshRenderer>().material);
                Destroy(TeVerwijderenTiles[tileToBeRemoved].GetComponent<MeshFilter>().mesh);
                Destroy(TeVerwijderenTiles[tileToBeRemoved]);
                TeVerwijderenTiles.Remove(tileToBeRemoved);
                activeTiles.Remove(tileToBeRemoved);
            }
        }


    }


}
