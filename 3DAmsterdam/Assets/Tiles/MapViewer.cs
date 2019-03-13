using BruTile;
using Terrain.ExtensionMethods;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Terrain.Tiles;
using UnityEngine;
using UnityEngine.Networking;
using System.Diagnostics;
using System;
using ConvertCoordinates;
using SimpleJSON;




namespace Terrain
{

    public class MapViewer : MonoBehaviour
    {
        [SerializeField]
        private readonly Extent extent = new Extent(4.88, 52.36, 4.92, 52.38); //part of Amsterdam, Netherlands in Latitude/Longitude (GPS) coordinates as boundingbox.

        [SerializeField]
        private int zoomLevel = 14;
        [SerializeField]
        private GameObject placeholderTile;
        [SerializeField]
        private GameObject TileContainer;
        [SerializeField]
        private string terrainUrl = @"https://saturnus.geodan.nl/tomt/data/tiles/{z}/{x}/{y}.terrain?v=1.0.0";
        ///[SerializeField]
        public string textureUrl = "https://saturnus.geodan.nl/mapproxy/bgt/service?crs=EPSG%3A3857&service=WMS&version=1.1.1&request=GetMap&styles=&format=image%2Fjpeg&layers=bgt&bbox={xMin}%2C{yMin}%2C{xMax}%2C{yMax}&width=256&height=256&srs=EPSG%3A4326";

        //public string textureUrl = @"https://map.data.amsterdam.nl/maps/bgt?REQUEST=GetMap&VERSION=1.1.0&SERVICE=wms&styles=&layer=bgt&format=image%2Fpng&bbox={xMin}%2C{yMin}%2C{xMax}%2C{yMax}&width=256&height=256&srs=EPSG%3A4326";

        //public string textureUrl = "https://geodata.nationaalgeoregister.nl/luchtfoto/rgb/wms?styles=&layers=Actueel_ortho25&service=WMS&request=GetMap&format=image%2Fpng&version=1.1.0&bbox={xMin}%2C{yMin}%2C{xMax}%2C{yMax}&width=256&height=512&crs=EPSG%3A4326&srs=EPSG%3A4326";
        [SerializeField]
        private string buildingsUrl = @"https://saturnus.geodan.nl/tomt/data/buildingtiles_adam/tiles/{id}.b3dm";
        private const int tilesize = 180;

        public string TOP10URL = "https://acc.3d.amsterdam.nl/webmap/gebouwen/top10/{id}.geojson";
        public string BAGURL = "https://acc.3d.amsterdam.nl/webmap/gebouwen/{id}.geojson";
        public GameObject Top10Container;
        public Material GebouwMateriaal;
        readonly Dictionary<Vector3, GameObject> tileDb = new Dictionary<Vector3, GameObject>();
        Dictionary<Vector3, GameObject> top10Db = new Dictionary<Vector3, GameObject>();

        const int maxParallelRequests = 8;
        Queue<downloadRequest> gebouwenQueue = new Queue<downloadRequest>();
        Queue<downloadRequest> downloadQueue = new Queue<downloadRequest>();
        
        Dictionary<string, downloadRequest> pendingQueue = new Dictionary<string, downloadRequest>(maxParallelRequests);

        public struct modeldata
            {
            public string modelnaam;
            public double lon;
            public double lat;
            public string BAGid;
            }

        public List<modeldata> CustomModels = new List<modeldata>();
 
        //debuggin'
        private Stopwatch sw = new Stopwatch();
        int processedTileDebugCounter = 0;
        
        public enum TileService
        {
            WMS,
            QM,
            Top10
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

        private void Start()
        {
            // set WGS84-coordinates of "de dam" in Amsterdam to correspond with unity-coordinates 0,0
            CoordConvert.ReferenceWGS84 = new Vector3WGS(4.892504f, 52.373043f, 0);

            /// lijst met custom modellen inlezen en opslaan in de list CustomModels
            TextAsset modellijst = new TextAsset();
            modellijst = Resources.Load<TextAsset>("3Dmodellen/gebouwencoordinatenlijst");
            string tekst = modellijst.text;
            string[] linesInFile = tekst.Split('\n');
            for (int i = 3; i < linesInFile.Length; i++)
            {
                string[] regeldelen = linesInFile[i].Split(',');
                modeldata gegevens = new modeldata();
                gegevens.modelnaam = regeldelen[0];
                double dbl;
                double.TryParse(regeldelen[1], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out dbl);
                gegevens.lat = dbl;
                double.TryParse(regeldelen[2], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out dbl);
                gegevens.lon = dbl;
                gegevens.BAGid = regeldelen[3];
                CustomModels.Add(gegevens);
            }

        }

        private GameObject DrawPlaceHolder( Vector3 t)
        {
            var tile = Instantiate(placeholderTile);
            tile.transform.parent = TileContainer.transform;
            tile.name = $"tile/{t.x.ToString()}/{t.y.ToString()}/{t.z.ToString()}";
            tile.transform.position = GetTilePosition(t);
            tile.transform.localScale = new Vector3(ComputeScaleFactorX(int.Parse(t.z.ToString())), 1, ComputeScaleFactorY(int.Parse(t.z.ToString())));
            return tile;
        }

        
        /// <summary>
        /// TilePosition in Unity
        /// </summary>
        /// <param name="index">TileIndex</param>
        /// <param name="tileRange">TileRange</param>
        /// <returns>TilePosition in Unity</returns>
        private Vector3 GetTilePosition(Vector3 index)
        {

            double tegelbreedte = tilesize / Math.Pow(2, int.Parse(index.z.ToString())); //TileSize in Degrees
            double originX = ((index.x+0.5) * tegelbreedte) - 180;
            double originY = ((index.y+0.5) * tegelbreedte) - 90;
            return CoordConvert.WGS84toUnity(originX, originY);
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
                tileDb[tileId].GetComponent<MeshFilter>().sharedMesh = terrainTile.GetMesh(0); //height offset is manually done to nicely align height data with place holder at 0
                tileDb[tileId].GetComponent<MeshCollider>().sharedMesh = tileDb[tileId].GetComponent<MeshFilter>().sharedMesh;
                tileDb[tileId].transform.localScale = new Vector3(ComputeScaleFactorX((int) tileId.z), 1, ComputeScaleFactorY((int)tileId.z));
                }
            }
            else
            {
                UnityEngine.Debug.LogError("Tile: [" + tileId.x + " " + tileId.y + "] Error loading height data");
            }

            pendingQueue.Remove(url);
            processedTileDebugCounter++;

            if (pendingQueue.Count == 0)
                UnityEngine.Debug.Log("finished: with max queue size " + maxParallelRequests + ". Time: " + sw.Elapsed.TotalMilliseconds + " miliseconds. Total requests: " + processedTileDebugCounter);
        }

        private float ComputeScaleFactorX(int z)
        {
           return (float)(CoordConvert.UnitsPerDegreeX / Math.Pow(2, z));
        }

        private float ComputeScaleFactorY(int z)
        {
            return (float)(CoordConvert.UnitsPerDegreeY / Math.Pow(2, z));
        }

        private IEnumerator requestWMSTile(string url, Vector3 tileId)
        {
         
            UnityWebRequest www = UnityWebRequestTexture.GetTexture(url);
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
                UnityEngine.Debug.LogError("Tile: [" + tileId.x + " " + tileId.y + "] Error loading texture data");
            }

            pendingQueue.Remove(url);
            processedTileDebugCounter++;

            if (pendingQueue.Count == 0)
                UnityEngine.Debug.Log("finished: with max queue size " + maxParallelRequests + ". Time: " + sw.Elapsed.TotalMilliseconds + " miliseconds. Total requests: " + processedTileDebugCounter);
        }

        private IEnumerator requestTop10(string url, Vector3 id)
        {
            UnityWebRequest www = UnityWebRequest.Get(url);

            yield return www.SendWebRequest();
            if (top10Db.ContainsKey(id))
            {
                GameObject container = top10Db[id];

                if (!www.isNetworkError && !www.isHttpError)
                {
                    
                    var gebouwen = JSON.Parse(www.downloadHandler.text);

                        for (int i = 0; i < gebouwen["features"].Count; i++)
                        {
                            if (i % 50 == 0)
                            {
                                yield return null;
                            }
                            try
                            {
                            string naam = gebouwen["features"][i]["properties"]["PAND_ID"].Value;
                            if (naam == "")
                            {
                                CreeerGebouw(gebouwen["features"][i], container);
                            }
                            else
                            {
                                if (LoadModel(naam, container) == false)
                                {
                                    CreeerGebouw(gebouwen["features"][i], container);
                                };
                            }
                            }
                            catch (Exception)
                            {
                                //Destroy(gevel);

                            }


                        
                    }
                }
                else
                {
                    UnityEngine.Debug.Log("kan bestand niet downloaden van " + url);
                }
            }
            pendingQueue.Remove(url);
        }

        public void Update()
        {

            Extent Camext = CamExtent();
            
            UpdateTerrainTiles(Camext);
            UpdateTop10(Camext);

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
                    case TileService.WMS:
                        StartCoroutine(requestWMSTile(request.Url, request.TileId));
                        break;
                }
            }
            if (downloadQueue.Count == 0)
            {
                if (pendingQueue.Count < maxParallelRequests && gebouwenQueue.Count > 0)
                {
                    var request = gebouwenQueue.Dequeue();
                    pendingQueue.Add(request.Url, request);

                    //fire request
                    switch (request.Service)
                    {

                        case TileService.Top10:
                            StartCoroutine(requestTop10(request.Url, request.TileId));
                            break;
                    }
                }
            }

        }

        private Extent CamExtent()
        {
            // Zoomlevel en maximale kijkafstand bepalen op basis van de hoogte van de Camera
            Vector3 Camlocation = Camera.main.transform.localPosition;
            zoomLevel = 13;
            float Maxafstand = 10000;
            if (Camlocation.y < 1600) { zoomLevel = 14; Maxafstand = 6400; }
            if (Camlocation.y < 800) { zoomLevel = 15; Maxafstand = 3200; }
            if (Camlocation.y < 400) { zoomLevel = 16; Maxafstand = 1600; }
            if (Camlocation.y < 200) { zoomLevel = 17; Maxafstand = 800; }
            Maxafstand = 10000;
            // bepalen welke UnityCoordinaten zichtbaar zijn in de hoeken van het scherm
            Vector3[] hoeken = new Vector3[4];
            hoeken[0] = GetHoekpunt("LinksBoven");
            hoeken[1] = GetHoekpunt("RechtsBoven");
            hoeken[2] = GetHoekpunt("Rechtsonder");
            hoeken[3] = GetHoekpunt("Linksonder");

            // de maximale en minimale X- en Z-waarde van de zichtbare coordinaten bepalen
            Vector3 UnityMax = new Vector3(-9999999, -9999999, -99999999);
            Vector3 UnityMin = new Vector3(9999999, 9999999, 9999999);
            for (int i = 0; i < 4; i++)
            {
                if (hoeken[i].x < UnityMin.x) { UnityMin.x = hoeken[i].x; }
                if (hoeken[i].z < UnityMin.z) { UnityMin.z = hoeken[i].z; }
                if (hoeken[i].x > UnityMax.x) { UnityMax.x = hoeken[i].x; }
                if (hoeken[i].z > UnityMax.z) { UnityMax.z = hoeken[i].z; }
            }

            //// maximale en minimale X- en Z-waarden aanpassen aan de maximale zichtafastand
            //if (UnityMin.x < Camlocation.x - Maxafstand) { UnityMin.x = Camlocation.x - Maxafstand; }
            //if (UnityMin.z < Camlocation.z - Maxafstand) { UnityMin.z = Camlocation.z - Maxafstand; }
            //if (UnityMax.x > Camlocation.x + Maxafstand) { UnityMax.x = Camlocation.x + Maxafstand; }
            //if (UnityMax.z > Camlocation.z + Maxafstand) { UnityMax.z = Camlocation.z + Maxafstand; }

            // Maximale en Minimale X- en Z-unitywaarden omrekenen naar WGS84
            Vector3WGS WGSMin = CoordConvert.UnitytoWGS84(UnityMin);
            Vector3WGS WGSMax = CoordConvert.UnitytoWGS84(UnityMax);

            // de maximale en minimale WGS84-coordinaten uitbreiden met 1 tegelafmeting
            //double tegelbreedte = tilesize / Math.Pow(2, zoomLevel); //TileSize in Degrees
            //WGSMin.lon = WGSMin.lon - tegelbreedte;
            //WGSMax.lon = WGSMax.lon + (1.5*tegelbreedte);
            //WGSMax.lat = WGSMax.lat + tegelbreedte;
            //WGSMin.lat = WGSMin.lat - tegelbreedte;

            // gebied waarbinnen data geladen moet worden
            Extent Tempextent = new Extent(WGSMin.lon, WGSMin.lat, WGSMax.lon, WGSMax.lat);
            return Tempextent;
        }

        public void UpdateTop10(Extent TempExtent)
        {
            // WGS-extent omzetten naar RD-extent
            Vector3RD RDmin = CoordConvert.WGS84toRD(TempExtent.MinX, TempExtent.MinY);
            Vector3RD RDmax = CoordConvert.WGS84toRD(TempExtent.MaxX, TempExtent.MaxY);
            TempExtent = new Extent(RDmin.x, RDmin.y, RDmax.x, RDmax.y);

            //1. uitzoeken welke tegels nodig zijn
            List<Vector3> Top10Nodig = new List<Vector3>();
            int X0 = ((int)Math.Floor(TempExtent.MinX / 1000) * 1000);
            int Y0 = ((int)Math.Floor(TempExtent.MinY / 1000) * 1000);
            int X1 = ((int)Math.Ceiling(TempExtent.MaxX / 1000) * 1000);
            int Y1 = ((int)Math.Ceiling(TempExtent.MaxY / 1000) * 1000);

            
                for (int deltax = X0; deltax <= X1; deltax += 1000)
                {
                    for (int deltay = Y0; deltay <= Y1; deltay += 1000)
                    {
                        Vector3 Hart = new Vector3(deltax + 500, deltay + 500, 0);
                        Vector3 HartUnity = CoordConvert.RDtoUnity(Hart);
                        Vector3 Verschil = Camera.main.transform.localPosition - HartUnity;
                        double afstand = Math.Sqrt(Math.Pow(Verschil.x,2)+Math.Pow(Verschil.y,2)+Math.Pow(Verschil.z,2));
                        double maxafstand = 8000;
                        double minafstand = 1000;
                        if (minafstand < afstand && afstand < maxafstand)
                        {
                            Top10Nodig.Add(new Vector3(deltax, deltay, 0));
                        }
                        if(afstand<minafstand)
                        {
                        Top10Nodig.Add(new Vector3(deltax, deltay, 1));
                        Top10Nodig.Add(new Vector3(deltax+500, deltay, 1));
                        Top10Nodig.Add(new Vector3(deltax, deltay+500, 1));
                        Top10Nodig.Add(new Vector3(deltax+500, deltay+500, 1));
                    }
                    }
                }
            

            
            // bepalen welke reeds geladentegels niet meer nodig zijn en verwijderd kunnen worden
            Vector3[] Top10DBKeys = top10Db.Keys.ToArray();
            bool nodig = false;
            foreach (Vector3 V in Top10DBKeys)
            {
                nodig = false;
                foreach (var t in Top10Nodig)
                {
                    if (t == V)
                    {
                        nodig = true;
                    }
                }
                if (nodig == false)
                {
                    // te verwijderen tegels toevoegen aan ee lijst met te verwijderen tegels

                    Destroy(top10Db[V]);
                    // te verwijderen tegels verwijderen uit de lijst met geladen tegels
                    top10Db.Remove(V);
                }
            }

            // tegels die niet meer nodig zijn uit de downloadqueue verwijderen
            downloadRequest[] reqs = gebouwenQueue.ToArray();

            Queue<downloadRequest> tempQueue = new Queue<downloadRequest>();
            foreach (downloadRequest req in reqs)
            {
                if (req.Service == TileService.Top10)
                {
                    nodig = false;
                    foreach (var t in Top10Nodig)
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
            gebouwenQueue = tempQueue;

            //nieuwe tiles toevoegen aan TileDB
            //immediately draw placeholder tile and fire request for texture and height. Depending on which one returns first, update place holder.
            foreach (var t in Top10Nodig)
            {
                //draw placeholder tile

                if (top10Db.ContainsKey(t) == false) // alleen verdergaan als de tegel nog niet in de lijst met geladentegels staat.
                {
                    GameObject gebouwtegel;
                    if (t.z == 0)
                    {
                        gebouwtegel = new GameObject("top10/" + t.x + "_" + t.y);
                        gebouwtegel.transform.parent = Top10Container.transform;
                        //gebouwtegel.name = "top10/" + t.x + "_" + t.y;
                    }
                    else
                    {
                        gebouwtegel = new GameObject("BAG/" + t.x + "_" + t.y);
                        gebouwtegel.transform.parent = Top10Container.transform;
                        //gebouwtegel.name = "BAG/" + t.x + "_" + t.y;
                    }
                    
                    top10Db.Add(t, gebouwtegel);

                    //get tile texture data

                    string wmsUrl;
                    if (t.z == 0)
                    {
                        wmsUrl = TOP10URL.Replace("{id}", gebouwtegel.name);
                    }
                    else
                    {
                        wmsUrl = BAGURL.Replace("{id}", gebouwtegel.name);
                    }
                    gebouwenQueue.Enqueue(new downloadRequest(wmsUrl, TileService.Top10, t));


                }

            }



        }

        public void UpdateTerrainTiles(Extent Tempextent)
        {

            // bepalen welke tegels geladen moeten worden
            var schema = new TmsGlobalGeodeticTileSchema();
            bool doorgaan = true;

            var tiles = schema.GetTileInfos(Tempextent, "10").ToList();
            List<Vector3> TileKeys = new List<Vector3>();
            foreach(var t in tiles)
            {
                Vector3 td = new Vector3(t.Index.Col,t.Index.Row,int.Parse(t.Index.Level));
                TileKeys.Add( td);
            }

            while (doorgaan)
                {
                doorgaan = false;
                    foreach(Vector3 t in TileKeys)
                    {
                    Extent subtileExtent = TileTransform.TileToWorld(new TileRange(int.Parse(t.x.ToString()), int.Parse(t.y.ToString())), t.z.ToString(), schema);
                    Vector3WGS locatieWGS = new Vector3WGS();
                    locatieWGS.lon = subtileExtent.CenterX;
                    locatieWGS.lat = subtileExtent.CenterY;
                    locatieWGS.h = CoordConvert.ReferenceWGS84.h;
                    Vector3 LocatieUnity = CoordConvert.WGS84toUnity(locatieWGS);
                    Vector3 afstand3D = new Vector3();
                    afstand3D = LocatieUnity - Camera.main.transform.localPosition;
                    double afstand = Math.Sqrt(Math.Pow(afstand3D.x, 2) + Math.Pow(afstand3D.y, 2) + Math.Pow(afstand3D.z, 2));
                    double minafstand = 50 * Math.Pow(2,(18-t.z));
                    if(afstand < minafstand && t.z <17)
                    {
                        Vector3 toevoeging;
                        if (locatieWGS.lon > Tempextent.MinX && locatieWGS.lat > Tempextent.MinY)
                        {
                            toevoeging = new Vector3(t.x * 2, t.y * 2, t.z + 1);
                            TileKeys.Add(toevoeging);
                        }
                        if (locatieWGS.lon < Tempextent.MaxX && locatieWGS.lat > Tempextent.MinY)
                        {
                            toevoeging = new Vector3((t.x * 2) + 1, t.y * 2, t.z + 1);
                            TileKeys.Add(toevoeging);
                        }
                        if (locatieWGS.lon > Tempextent.MinX && locatieWGS.lat < Tempextent.MaxY)
                        {
                            toevoeging = new Vector3(t.x * 2, (t.y * 2) + 1, t.z + 1);
                            TileKeys.Add(toevoeging);
                        }
                        if (locatieWGS.lon < Tempextent.MaxX && locatieWGS.lat < Tempextent.MaxY)
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


            // bepalen welke reeds geladentegels niet meer nodig zijn en dezen verwijderen
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
                    // gameobject verwijderen
                    
                    Destroy(tileDb[V]);
                    //verwijderen uit de lijst met geladen tegels
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
                    Extent subtileExtent = TileTransform.TileToWorld(new TileRange(int.Parse(t.x.ToString()), int.Parse(t.y.ToString())), t.z.ToString(), schema);
                    string   wmsUrl = textureUrl.Replace("{xMin}", subtileExtent.MinX.ToString()).Replace("{yMin}", subtileExtent.MinY.ToString()).Replace("{xMax}", subtileExtent.MaxX.ToString()).Replace("{yMax}", subtileExtent.MaxY.ToString()).Replace(",", ".");
                    if (zoomLevel==17)
                    {
                        wmsUrl = wmsUrl.Replace("width=256", "width=1024");
                        wmsUrl = wmsUrl.Replace("height=256", "height=1024");
                    }
                    
                    downloadQueue.Enqueue(new downloadRequest(wmsUrl, TileService.WMS, t));

                    //get tile height data (
                    var qmUrl = terrainUrl.Replace("{x}", t.x.ToString()).Replace("{y}", t.y.ToString()).Replace("{z}", int.Parse(t.z.ToString()).ToString());
                    downloadQueue.Enqueue(new downloadRequest(qmUrl, TileService.QM, t));
                }

            }
        }

        private Vector3 GetHoekpunt(string hoek)
        {

            Vector2 Screenpos = new Vector2();
            if (hoek == "LinksBoven")
            {
                Screenpos.x = 0;
                Screenpos.y = Camera.main.pixelHeight - 1;
            }
            if (hoek == "RechtsBoven")
            {
                Screenpos.x = Camera.main.pixelWidth - 1;
                Screenpos.y = Camera.main.pixelHeight - 1;
            }
            if (hoek == "LinksOnder")
            {
                Screenpos.x = 0;
                Screenpos.y = 0;
            }
            if (hoek == "RechtsOnder")
            {
                Screenpos.x = Camera.main.pixelWidth - 1;
                Screenpos.y = 0;
            }
            Vector3 output = new Vector3();
            Vector3 linkerbovenhoekA;
            linkerbovenhoekA = Camera.main.ScreenToWorldPoint(new Vector3(Screenpos.x, Screenpos.y, 10));
            Vector3 linkerbovenhoekB;
            linkerbovenhoekB = Camera.main.ScreenToWorldPoint(new Vector3(Screenpos.x, Screenpos.y, 3010));


            Vector3 richting = linkerbovenhoekA - linkerbovenhoekB;
            float factor;
            if (richting.y < 0)
            {
                factor = 1;
            }
            else
            {
                factor = ((Camera.main.transform.localPosition.y-40) / richting.y);
            }




            output.x = Camera.main.transform.localPosition.x - (factor * richting.x);
            output.y = Camera.main.transform.localPosition.y - (factor * richting.y);
            output.z = Camera.main.transform.localPosition.z - (factor * richting.z);



            return output;
        }

        bool LoadModel(string bagid, GameObject container)
        {
            bool isaanwezig = false;
            foreach(modeldata m in CustomModels)
            {
                if (m.BAGid == bagid)
                {
                    isaanwezig = true;
                    
                    GameObject pand = Resources.Load("3Dmodellen/Paleis_op_de_Dam") as GameObject;
                    GameObject Defpand = Instantiate(pand);
                    Defpand.transform.parent = container.transform;
                    Vector3 Locatie = CoordConvert.WGS84toUnity(m.lon, m.lat);
                    Locatie.y = 45.31f;
                    Defpand.transform.localPosition = Locatie;
                }
            }
            return isaanwezig;
        }
            

        void CreeerGebouw(JSONNode pand, GameObject container)

        {
            //pandid bepalen
            string naam = pand["properties"]["PAND_ID"].Value;
            if(naam == "")
            {
                naam = pand["properties"]["gml_id"].Value;
            }
            GameObject gevel;
            gevel = new GameObject("gebouw");
            try
            {
                 
                gevel.transform.parent = container.transform;

                gevel.name = naam;
                //gevelhoogte bepalen
                string tekst = pand["properties"]["mediaan_ho"].Value;
                //tekst = tekst.Replace(".", ",");
                double dbl;
                double.TryParse(tekst, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out dbl);
                float Dakhoogte = (float)dbl;
                float Gevelhoogte;
                //UnityEngine.Debug.Log(Gevelhoogte);
                //UnityEngine.Debug.Log(pand["geometry"]["coordinates"]);
                //UnityEngine.Debug.Log(pand["geometry"]["coordinates"][0][0]);
                string polytype = pand["geometry"]["type"].Value;
                JSONNode grondvlak;
                if (polytype == "MultiPolygon")
                {
                    grondvlak = pand["geometry"]["coordinates"][0][0];
                }
                else
                {
                    grondvlak = pand["geometry"]["coordinates"][0];
                }

                int coordinaten = grondvlak.Count;

                //3dcoordinaten van grondvlak, Hoogte wijzigen zodat het dakvlak wordt
                // ten opzichte van V3Origin
                List<Vector3> V3coordinaten = new List<Vector3>();

                tekst = grondvlak[0][0].Value;
                double.TryParse(tekst, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out dbl);
                double X = dbl;

                tekst = grondvlak[0][1].Value;
                double.TryParse(tekst, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out dbl);
                double Y = dbl;

                tekst = pand["properties"]["minimum_ho"].Value;
                double.TryParse(tekst, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out dbl);
                double Z = dbl;
                Gevelhoogte = (float)(Dakhoogte - Z + 1f);
                Vector3 Coord = new Vector3(0, 0, 0);
                Vector3RD V3Origin = new Vector3RD(X, Y, Z);
                gevel.transform.localPosition = CoordConvert.RDtoUnity(V3Origin);


                for (int i = 0; i < coordinaten - 1; i++)
                {
                    tekst = grondvlak[i][0].Value;
                    double.TryParse(tekst, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out dbl);
                    dbl -= V3Origin.x;
                    X = dbl;

                    tekst = grondvlak[i][1].Value;
                    double.TryParse(tekst, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out dbl);
                    dbl -= V3Origin.y;
                    Y = dbl;

                    Coord = new Vector3((float)X, Dakhoogte, (float)Y);

                    V3coordinaten.Add(Coord);

                }

                //2dcoordinaten van grondvlak voor triangulatie
                Vector2[] V2coordinaten = new Vector2[V3coordinaten.Count];
                for (int i = 0; i < V3coordinaten.Count; i++)
                {
                    V2coordinaten[i] = new Vector2(V3coordinaten[i].x, V3coordinaten[i].z);
                }

                //triangulatie
                Triangulator tr = new Triangulator(V2coordinaten);
                int[] indices = tr.Triangulate();

                //List maken van de TRiangles
                List<int> Tris = new List<int>();
                for (int i = 0; i < indices.Length; i++)
                {
                    Tris.Add(indices[i]);
                }

                //gevels toevoegen
                int aantalcoordinaten = V3coordinaten.Count;
                for (int i = 0; i < aantalcoordinaten; i++)
                {
                    V3coordinaten.Add(V3coordinaten[i]);
                    if (i < aantalcoordinaten)
                    {
                        V3coordinaten.Add(V3coordinaten[i + 1]);
                    }
                    else
                    {
                        V3coordinaten.Add(V3coordinaten[0]);
                    }

                    int startnummer = aantalcoordinaten + (i * 4);
                    //punt1
                    Vector3 loc = new Vector3();
                    loc.x = V3coordinaten[i].x;
                    loc.y = V3coordinaten[i].y - Gevelhoogte;
                    loc.z = V3coordinaten[i].z;
                    V3coordinaten.Add(loc);
                    Tris.Add(startnummer);
                    Tris.Add(startnummer + 2);
                    Tris.Add(startnummer + 1);
                    //punt2
                    loc = new Vector3();
                    loc.x = V3coordinaten[i + 1].x;
                    loc.y = V3coordinaten[i + 1].y - Gevelhoogte;
                    loc.z = V3coordinaten[i + 1].z;
                    V3coordinaten.Add(loc);
                    Tris.Add(startnummer + 1);
                    Tris.Add(startnummer + 2);
                    Tris.Add(startnummer + 3);
                }

                //mesh toeveogen aan gameobject 
                //try
                //{
                // Create the mesh
                Mesh msh = new Mesh();
                msh.vertices = V3coordinaten.ToArray();
                msh.triangles = Tris.ToArray();
                msh.RecalculateNormals();
                msh.RecalculateBounds();

                gevel.AddComponent(typeof(MeshRenderer));
                MeshFilter filter = gevel.AddComponent(typeof(MeshFilter)) as MeshFilter;
                filter.mesh = msh;

                gevel.GetComponent<MeshRenderer>().material = GebouwMateriaal;
                MeshCollider mc = gevel.AddComponent<MeshCollider>();
                mc.sharedMesh = msh;
                //}
                //catch (System.Exception)
                //{


                //}
            }
            
            catch
            {
                Destroy(gevel);
            }
        }
    }


}
public class Triangulator
{
    private List<Vector2> m_points = new List<Vector2>();

    public Triangulator(Vector2[] points)
    {
        m_points = new List<Vector2>(points);
    }

    public int[] Triangulate()
    {
        List<int> indices = new List<int>();

        int n = m_points.Count;
        if (n < 3)
            return indices.ToArray();

        int[] V = new int[n];
        if (Area() > 0)
        {
            for (int v = 0; v < n; v++)
                V[v] = v;
        }
        else
        {
            for (int v = 0; v < n; v++)
                V[v] = (n - 1) - v;
        }

        int nv = n;
        int count = 2 * nv;
        for (int v = nv - 1; nv > 2;)
        {
            if ((count--) <= 0)
                return indices.ToArray();

            int u = v;
            if (nv <= u)
                u = 0;
            v = u + 1;
            if (nv <= v)
                v = 0;
            int w = v + 1;
            if (nv <= w)
                w = 0;

            if (Snip(u, v, w, nv, V))
            {
                int a, b, c, s, t;
                a = V[u];
                b = V[v];
                c = V[w];
                indices.Add(a);
                indices.Add(b);
                indices.Add(c);
                for (s = v, t = v + 1; t < nv; s++, t++)
                    V[s] = V[t];
                nv--;
                count = 2 * nv;
            }
        }

        indices.Reverse();
        return indices.ToArray();
    }

    private float Area()
    {
        int n = m_points.Count;
        float A = 0.0f;
        for (int p = n - 1, q = 0; q < n; p = q++)
        {
            Vector2 pval = m_points[p];
            Vector2 qval = m_points[q];
            A += pval.x * qval.y - qval.x * pval.y;
        }
        return (A * 0.5f);
    }

    private bool Snip(int u, int v, int w, int n, int[] V)
    {
        int p;
        Vector2 A = m_points[V[u]];
        Vector2 B = m_points[V[v]];
        Vector2 C = m_points[V[w]];
        if (UnityEngine.Mathf.Epsilon > (((B.x - A.x) * (C.y - A.y)) - ((B.y - A.y) * (C.x - A.x))))
            return false;
        for (p = 0; p < n; p++)
        {
            if ((p == u) || (p == v) || (p == w))
                continue;
            Vector2 P = m_points[V[p]];
            if (InsideTriangle(A, B, C, P))
                return false;
        }
        return true;
    }

    private bool InsideTriangle(Vector2 A, Vector2 B, Vector2 C, Vector2 P)
    {
        float ax, ay, bx, by, cx, cy, apx, apy, bpx, bpy, cpx, cpy;
        float cCROSSap, bCROSScp, aCROSSbp;

        ax = C.x - B.x; ay = C.y - B.y;
        bx = A.x - C.x; by = A.y - C.y;
        cx = B.x - A.x; cy = B.y - A.y;
        apx = P.x - A.x; apy = P.y - A.y;
        bpx = P.x - B.x; bpy = P.y - B.y;
        cpx = P.x - C.x; cpy = P.y - C.y;

        aCROSSbp = ax * bpy - ay * bpx;
        cCROSSap = cx * apy - cy * apx;
        bCROSScp = bx * cpy - by * cpx;

        return ((aCROSSbp >= 0.0f) && (bCROSScp >= 0.0f) && (cCROSSap >= 0.0f));
    }
}