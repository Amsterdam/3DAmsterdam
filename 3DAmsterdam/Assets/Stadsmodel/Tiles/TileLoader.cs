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




public class TileLoader : MonoBehaviour
{
    private CameraView CV;
    public string terrainUrl = "https://saturnus.geodan.nl/tomt/data/tiles/{z}/{x}/{y}.terrain?v=1.0.0";
    
    //public string textureUrl = "https://saturnus.geodan.nl/mapproxy/bgt/service?crs=EPSG%3A3857&service=WMS&version=1.1.1&request=GetMap&styles=&format=image%2Fjpeg&layers=bgt&bbox={xMin}%2C{yMin}%2C{xMax}%2C{yMax}&width=256&height=256&srs=EPSG%3A4326";
    public string textureUrl = "https://map.data.amsterdam.nl/cgi-bin/mapserv?map=/srv/mapserver/topografie.map&REQUEST=GetMap&VERSION=1.1.0&SERVICE=wms&styles=&layers=basiskaart-zwartwit&format=image%2Fpng&bbox={xMin}%2C{yMin}%2C{xMax}%2C{yMax}&width=256&height=256&srs=EPSG%3A4326&crs=EPSG%3A4326";
   // public string textureUrl = "https://geodata.nationaalgeoregister.nl/luchtfoto/rgb/wms?styles=&layers=Actueel_ortho25&service=WMS&request=GetMap&format=image%2Fpng&version=1.1.0&bbox={xMin}%2C{yMin}%2C{xMax}%2C{yMax}&width=256&height=512&crs=EPSG%3A4326&srs=EPSG%3A4326";
    public GameObject placeholderTile;
    private const int tilesize = 180;

    readonly Dictionary<Vector3, GameObject> tileDb = new Dictionary<Vector3, GameObject>();
    Dictionary<Vector3, GameObject> TeVerwijderenTiles = new Dictionary<Vector3, GameObject>();
    const int maxParallelRequests = 8;
    Queue<downloadRequest> downloadQueue = new Queue<downloadRequest>();
    Dictionary<string, downloadRequest> pendingQueue = new Dictionary<string, downloadRequest>(maxParallelRequests);

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
        CV = Camera.main.GetComponent<CameraView>();
    }

    // Update is called once per frame
    void Update()
    {
        VerwijderTiles();
        Extent Tempextent = CV.CameraExtent;
        UpdateTerrainTiles(Tempextent);
        // verdergaan met downloaden uit de queue
        if (pendingQueue.Count < maxParallelRequests && downloadQueue.Count > 0)
        {
            var request = downloadQueue.Dequeue();
            pendingQueue.Add(request.Url, request);

            //fire request
            switch (request.Service)
            {
                case TileService.QM:
                    StartCoroutine(requestQMTile(request.Url, request.TileId));
                    break;
                
            }
        }
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

    private IEnumerator requestQMTile(string url, Vector3 tileId)
    {
        DownloadHandlerBuffer handler = new DownloadHandlerBuffer();
        TerrainTile terrainTile;
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
            if (tileDb.ContainsKey(tileId))
            {
                tileDb[tileId].GetComponent<MeshFilter>().sharedMesh = terrainTile.GetMesh(0); 
                //tileDb[tileId].GetComponent<MeshCollider>().sharedMesh = tileDb[tileId].GetComponent<MeshFilter>().sharedMesh;
               
                tileDb[tileId].transform.localScale = new Vector3(ComputeScaleFactorX((int)tileId.z), 1, ComputeScaleFactorY((int)tileId.z));
                Vector3 loc = tileDb[tileId].transform.localPosition;
                loc.y = 0;
                tileDb[tileId].transform.localPosition = loc;
                tileDb[tileId].layer = 8;

            }
        }
        else
        {
            UnityEngine.Debug.LogError("Tile: [" + tileId.x + " " + tileId.y + "] Error loading height data");
        }
        //get tile texture data
        var schema = new Terrain.TmsGlobalGeodeticTileSchema();
        Extent subtileExtent = TileTransform.TileToWorld(new TileRange(int.Parse(tileId.x.ToString()), int.Parse(tileId.y.ToString())), tileId.z.ToString(), schema);
        string wmsUrl = textureUrl.Replace("{xMin}", subtileExtent.MinX.ToString()).Replace("{yMin}", subtileExtent.MinY.ToString()).Replace("{xMax}", subtileExtent.MaxX.ToString()).Replace("{yMax}", subtileExtent.MaxY.ToString()).Replace(",", ".");
        if (tileId.z == 17)
        {
            wmsUrl = wmsUrl.Replace("width=256", "width=1024");
            wmsUrl = wmsUrl.Replace("height=256", "height=1024");
        }
        www = UnityWebRequestTexture.GetTexture(wmsUrl);
        yield return www.SendWebRequest();

        if (!www.isNetworkError && !www.isHttpError)
        {
            if (tileDb.ContainsKey(tileId))
            {
                Texture2D myTexture = ((DownloadHandlerTexture)www.downloadHandler).texture;
                myTexture.wrapMode = TextureWrapMode.Clamp;
                //update tile with height data
                tileDb[tileId].GetComponent<MeshRenderer>().material.mainTexture = myTexture;
            }
        }
        else
        {
            Debug.LogError("Tile: [" + tileId.x + " " + tileId.y + "] Error loading texture data");
        }

        pendingQueue.Remove(url);
    }

    private float ComputeScaleFactorX(int z)
    {
        return (float)(CoordConvert.UnitsPerDegreeX / Math.Pow(2, z));
    }

    private float ComputeScaleFactorY(int z)
    {
        return (float)(CoordConvert.UnitsPerDegreeY / Math.Pow(2, z));
    }

    public void UpdateTerrainTiles(Extent Tempextent)
    {

        // bepalen welke tegels geladen moeten worden
        var schema = new Terrain.TmsGlobalGeodeticTileSchema();
        bool doorgaan = true;

        var tiles = schema.GetTileInfos(Tempextent, "10").ToList();
        List<Vector3> TileKeys = new List<Vector3>();
        foreach (var t in tiles)
        {
            Vector3 td = new Vector3(t.Index.Col, t.Index.Row, int.Parse(t.Index.Level));
            TileKeys.Add(td);
        }

        while (doorgaan)
        {
            doorgaan = false;
            foreach (Vector3 t in TileKeys)
            {
                Extent subtileExtent = TileTransform.TileToWorld(new TileRange(int.Parse(t.x.ToString()), int.Parse(t.y.ToString())), t.z.ToString(), schema);

                Vector3WGS locatieWGS = new Vector3WGS();
                Vector3 LocatieUnity;
                Vector3 afstand3D = new Vector3();
                double afstand;
                double Werkafstand = 100000;


                locatieWGS.lon = subtileExtent.MinX;
                locatieWGS.lat = subtileExtent.MinY;
                locatieWGS.h = CoordConvert.ReferenceWGS84.h;
                LocatieUnity = CoordConvert.WGS84toUnity(locatieWGS);
                afstand3D = LocatieUnity - Camera.main.transform.localPosition;
                afstand = Math.Sqrt(Math.Pow(afstand3D.x, 2) + Math.Pow(afstand3D.y, 2) + Math.Pow(afstand3D.z, 2));

                if (afstand < Werkafstand)
                {
                    Werkafstand = afstand;
                }

                locatieWGS.lon = subtileExtent.MaxX;
                locatieWGS.lat = subtileExtent.MinY;
                locatieWGS.h = CoordConvert.ReferenceWGS84.h;
                LocatieUnity = CoordConvert.WGS84toUnity(locatieWGS);
                afstand3D = LocatieUnity - Camera.main.transform.localPosition;
                afstand = Math.Sqrt(Math.Pow(afstand3D.x, 2) + Math.Pow(afstand3D.y, 2) + Math.Pow(afstand3D.z, 2));

                if (afstand < Werkafstand)
                {
                    Werkafstand = afstand;
                }
                locatieWGS.lon = subtileExtent.MaxX;
                locatieWGS.lat = subtileExtent.MaxY;
                locatieWGS.h = CoordConvert.ReferenceWGS84.h;
                LocatieUnity = CoordConvert.WGS84toUnity(locatieWGS);
                afstand3D = LocatieUnity - Camera.main.transform.localPosition;
                afstand = Math.Sqrt(Math.Pow(afstand3D.x, 2) + Math.Pow(afstand3D.y, 2) + Math.Pow(afstand3D.z, 2));

                if (afstand < Werkafstand)
                {
                    Werkafstand = afstand;
                }

                locatieWGS.lon = subtileExtent.MinX;
                locatieWGS.lat = subtileExtent.MaxY;
                locatieWGS.h = CoordConvert.ReferenceWGS84.h;
                LocatieUnity = CoordConvert.WGS84toUnity(locatieWGS);
                afstand3D = LocatieUnity - Camera.main.transform.localPosition;
                afstand = Math.Sqrt(Math.Pow(afstand3D.x, 2) + Math.Pow(afstand3D.y, 2) + Math.Pow(afstand3D.z, 2));

                if (afstand < Werkafstand)
                {
                    Werkafstand = afstand;
                }

                double minafstand = 50 * Math.Pow(2, (18 - t.z));
                if (Werkafstand < minafstand && t.z < 17)
                {
                    Vector3 toevoeging;
                    Extent tempExtent = TileTransform.TileToWorld(new TileRange(int.Parse((t.x*2).ToString()), int.Parse((t.y*2).ToString())), (t.z+1).ToString(), schema);
                    if (IsInsideExtent(tempExtent, CV.CameraExtent))
                    {
                    toevoeging = new Vector3(t.x * 2, t.y * 2, t.z + 1);
                        TileKeys.Add(toevoeging);
                    }
                    tempExtent = TileTransform.TileToWorld(new TileRange(int.Parse(((t.x * 2)+1).ToString()), int.Parse(((t.y * 2)).ToString())), (t.z + 1).ToString(), schema);
                    if (IsInsideExtent(tempExtent, CV.CameraExtent))
                        {
                        toevoeging = new Vector3((t.x * 2) + 1, t.y * 2, t.z + 1);
                        TileKeys.Add(toevoeging);
                    }
                    tempExtent = TileTransform.TileToWorld(new TileRange(int.Parse((t.x * 2).ToString()), int.Parse(((t.y * 2) + 1).ToString())), (t.z + 1).ToString(), schema);
                    if (IsInsideExtent(tempExtent, CV.CameraExtent))
                        {
                        toevoeging = new Vector3(t.x * 2, (t.y * 2) + 1, t.z + 1);
                        TileKeys.Add(toevoeging);
                    }
                    tempExtent = TileTransform.TileToWorld(new TileRange(int.Parse(((t.x * 2) + 1).ToString()), int.Parse(((t.y * 2)+1).ToString())), (t.z + 1).ToString(), schema);
                    if (IsInsideExtent(tempExtent, CV.CameraExtent))
                        {
                        toevoeging = new Vector3((t.x * 2) + 1, (t.y * 2) + 1, t.z + 1);
                        TileKeys.Add(toevoeging);
                    }
                    TileKeys.Remove(t);
                    doorgaan = true;
                    break;
                }

            }
        }
        TileKeys.Reverse();


        // bepalen welke reeds geladentegels niet meer nodig zijn en deze toevoegen aan TeVerwijderenTiles
        Vector3[] TileDBKeys = tileDb.Keys.ToArray();
        bool nodig = false;
        foreach (Vector3 V in TileDBKeys)
        {
            nodig = false;
            foreach (var t in TileKeys)
            {
                if (t == V)
                {
                    nodig = true;
                }
            }
            if (nodig == false)
            {
                // gameobject toevoegen aan de te verwijderen lijst
                //Color kleur = new Color(255, 255, 255, 100);
                //tileDb[V].GetComponent<Material>().color = kleur;
                if (TeVerwijderenTiles.ContainsKey(V) == false)
                {
                    TeVerwijderenTiles.Add(V, tileDb[V]);
                }
                //verwijderen uit de lijst met geladen tegels
                Destroy(tileDb[V]);
                tileDb.Remove(V);
                
            }
        }

        // tegels die niet meer nodig zijn uit de downloadqueue verwijderen
        downloadRequest[] reqs = downloadQueue.ToArray();

        Queue<downloadRequest> tempQueue = new Queue<downloadRequest>();
        foreach (downloadRequest req in reqs)
        {
            if (req.Service == TileService.WMS || req.Service == TileService.QM)
            {
                nodig = false;
                foreach (var t in TileKeys)
                {
                    if (t == req.TileId)
                    {
                        nodig = true;
                        tempQueue.Enqueue(req);
                    }
                }
            }
            else tempQueue.Enqueue(req);
        }
        downloadQueue = tempQueue;

        //nieuwe tiles toevoegen aan TileDB
        //immediately draw placeholder tile and fire request for texture and height. Depending on which one returns first, update place holder.
        foreach (var t in TileKeys)
        {
            //draw placeholder tile

            if (tileDb.ContainsKey(t) == false) // alleen verdergaan als de tegel nog niet in de lijst met geladentegels staat.
            {
                GameObject tile = DrawPlaceHolder(t);
                tileDb.Add(t, tile);

                //get tile texture data
                //Extent subtileExtent = TileTransform.TileToWorld(new TileRange(int.Parse(t.x.ToString()), int.Parse(t.y.ToString())), t.z.ToString(), schema);
                //string wmsUrl = textureUrl.Replace("{xMin}", subtileExtent.MinX.ToString()).Replace("{yMin}", subtileExtent.MinY.ToString()).Replace("{xMax}", subtileExtent.MaxX.ToString()).Replace("{yMax}", subtileExtent.MaxY.ToString()).Replace(",", ".");
                //if (t.z == 17)
                //{
                //    wmsUrl = wmsUrl.Replace("width=256", "width=1024");
                //    wmsUrl = wmsUrl.Replace("height=256", "height=1024");
                //}

                //downloadQueue.Enqueue(new downloadRequest(wmsUrl, TileService.WMS, t));

                //get tile height data (
                var qmUrl = terrainUrl.Replace("{x}", t.x.ToString()).Replace("{y}", t.y.ToString()).Replace("{z}", int.Parse(t.z.ToString()).ToString());
                downloadQueue.Enqueue(new downloadRequest(qmUrl, TileService.QM, t));
            }

        }
    }

    Boolean IsInsideExtent(Extent Subtileextent, Extent BBox)
    {
        Boolean isbinnen = false;

        // check Linkerbovenhoek
        if(Subtileextent.MaxY>BBox.MinY && Subtileextent.MinY<BBox.MaxY && Subtileextent.MaxX > BBox.MinX && Subtileextent.MinX < BBox.MaxX) { isbinnen = true; };


        return isbinnen;
    }

   

    void VerwijderTiles()
    {
        Vector3[] TileDBKeys = TeVerwijderenTiles.Keys.ToArray();
        downloadRequest[] reqs = downloadQueue.ToArray();


        foreach (Vector3 V in TileDBKeys)
        {
            bool GroterAanwezig = false;
            bool KleinerAanwezig = false;
            float Xgroter = UnityEngine.Mathf.Floor(V.x / 2);
            float Ygroter = UnityEngine.Mathf.Floor(V.y / 2);
            float ZoomGroter = V.z - 1;
            Vector3 Groter = new Vector3(Xgroter, Ygroter, ZoomGroter);
            Vector3 Kleiner = new Vector3(V.x * 2, V.y * 2, V.z + 1);
            foreach (var req in reqs)
            {
                //controleren of een grotere tegel op dezelfde locatie gedouwnload moet worden
                if (req.TileId==Groter){GroterAanwezig = true;}
                if (req.TileId == Kleiner){KleinerAanwezig = true;}
                if (req.TileId == (Kleiner+new Vector3(1,0,0))) { KleinerAanwezig = true; }
                if (req.TileId == (Kleiner + new Vector3(0, 1, 0))) { KleinerAanwezig = true; }
                if (req.TileId == (Kleiner + new Vector3(1, 1, 0))) { KleinerAanwezig = true; }


            }
            foreach (var DR in pendingQueue)
            {
                if (DR.Value.TileId == Groter) { GroterAanwezig = true; }
                if (DR.Value.TileId == Kleiner) { KleinerAanwezig = true; }
                if (DR.Value.TileId == (Kleiner + new Vector3(1, 0, 0))) { KleinerAanwezig = true; }
                if (DR.Value.TileId == (Kleiner + new Vector3(0, 1, 0))) { KleinerAanwezig = true; }
                if (DR.Value.TileId == (Kleiner + new Vector3(1, 1, 0))) { KleinerAanwezig = true; }
            }

            if (GroterAanwezig==false && KleinerAanwezig==false)
            {
                Destroy(TeVerwijderenTiles[V]);
                TeVerwijderenTiles.Remove(V);
            }
        }


    }


}
