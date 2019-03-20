using BruTile;

using System.Collections;
using System.Collections.Generic;

using System.Linq;

using UnityEngine;
using UnityEngine.Networking;

using System;
using ConvertCoordinates;
using SimpleJSON;


public static class UnityExtensions
{
    public static void RunOnChildrenRecursive(this GameObject go, Action<GameObject> action)
    {
        if (go == null) return;
        foreach (var trans in go.GetComponentsInChildren<Transform>(true))
        {
            action(trans.gameObject);
        }
    }
}


public class BuildingLoader : MonoBehaviour
{

    public string TOP10URL = "https://acc.3d.amsterdam.nl/webmap/gebouwen/top10/{id}.geojson";
    public string BAGURL = "https://acc.3d.amsterdam.nl/webmap/gebouwen/{id}.geojson";
    public string ModelURL = "https://acc.3d.amsterdam.nl/webmap/gebouwen/models/{id}";
    public Material GebouwMateriaal;
    public double Max_Afstand_Top10 = 5000;
    public double Max_Afstand_BAG = 1000;
    private CameraView CV;

    Dictionary<Vector3, GameObject> top10Db = new Dictionary<Vector3, GameObject>();
    const int maxParallelRequests = 8;
    Queue<downloadRequest> gebouwenQueue = new Queue<downloadRequest>();
    Queue<downloadRequest> downloadQueue = new Queue<downloadRequest>();
    Queue<ModelDownloadData> ModelQueue = new Queue<ModelDownloadData>();
    int AantalModelDownloads = 0;


    Dictionary<string, downloadRequest> pendingQueue = new Dictionary<string, downloadRequest>(maxParallelRequests);

    public struct ModelDownloadData
    {
        public string modelnaam;
        public GameObject gameobject;
    }

    public struct modeldata
    {
        public string modelnaam;
        public double lon;
        public double lat;
        public string BAGid;
        public double NAPHoogte;
        public GameObject gameobject;
    }

    private List<modeldata> CustomModels = new List<modeldata>();

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

    // Start is called before the first frame update
    void Start()
    {
        CV = Camera.main.GetComponent<CameraView>();

        /// lijst met custom modellen inlezen en opslaan in de list CustomModels
        //TextAsset modellijst = new TextAsset();
        //modellijst = Resources.Load<TextAsset>("3Dmodellen/gebouwencoordinatenlijst");
        //string tekst = modellijst.text;
        //string[] linesInFile = tekst.Split('\n');
        //for (int i = 3; i < linesInFile.Length; i++)
        //{
        //    string[] regeldelen = linesInFile[i].Split(';');
        //    modeldata gegevens = new modeldata();
        //    gegevens.modelnaam = regeldelen[0];
        //    double dbl;
        //    double.TryParse(regeldelen[1], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out dbl);
        //    gegevens.lat = dbl;
        //    double.TryParse(regeldelen[2], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out dbl);
        //    gegevens.lon = dbl;
        //    double.TryParse(regeldelen[3], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out dbl);
        //    gegevens.NAPHoogte = dbl;
        //    gegevens.BAGid = regeldelen[4];
        //    CustomModels.Add(gegevens);
        //}
    }

    // Update is called once per frame
    void Update()
    {
        UpdateTop10(CV.CameraExtent);

        // generieke panden donloaden
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

    public void UpdateTop10(Extent WGSExtent)
    {
        // WGS-extent omzetten naar RD-extent
        Vector3RD RDmin = CoordConvert.WGS84toRD(WGSExtent.MinX, WGSExtent.MinY);
        Vector3RD RDmax = CoordConvert.WGS84toRD(WGSExtent.MaxX, WGSExtent.MaxY);
        Extent TempExtent = new Extent(RDmin.x, RDmin.y, RDmax.x, RDmax.y);

        //1. uitzoeken welke tegels nodig zijn
        List<Vector3> Top10Nodig = new List<Vector3>();
        int X0 = ((int)Math.Floor(TempExtent.MinX / 1000) * 1000);
        int Y0 = ((int)Math.Floor(TempExtent.MinY / 1000) * 1000);
        int X1 = ((int)Math.Floor(TempExtent.MaxX / 1000) * 1000);
        int Y1 = ((int)Math.Floor(TempExtent.MaxY / 1000) * 1000);


        for (int deltax = X0; deltax <= X1; deltax += 1000)
        {
            for (int deltay = Y0; deltay <= Y1; deltay += 1000)
            {
                Vector3 Hart = new Vector3(deltax + 500, deltay + 500, 0);
                Vector3 HartUnity = CoordConvert.RDtoUnity(Hart);
                Vector3 Verschil = Camera.main.transform.localPosition - HartUnity;
                double afstand = Verschil.magnitude;
                if (Max_Afstand_BAG < afstand && afstand < Max_Afstand_Top10)
                {
                    Top10Nodig.Add(new Vector3(deltax, deltay, 0));
                }
                if (afstand < Max_Afstand_BAG)
                {
                    //controleren of bagpandtegel binnen view valt
                    Top10Nodig.Add(new Vector3(deltax, deltay, 1));
                    Top10Nodig.Add(new Vector3(deltax + 500, deltay, 1));
                    Top10Nodig.Add(new Vector3(deltax, deltay + 500, 1));
                    Top10Nodig.Add(new Vector3(deltax + 500, deltay + 500, 1));
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
                    gebouwtegel.transform.parent = transform;
                    //gebouwtegel.name = "top10/" + t.x + "_" + t.y;
                }
                else
                {
                    gebouwtegel = new GameObject("BAG/" + t.x + "_" + t.y);
                    gebouwtegel.transform.parent = transform;
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
    void CreeerGebouw(JSONNode pand, GameObject container)

    {
        //pandid bepalen
        string naam = pand["properties"]["PAND_ID"].Value;
        if (naam == "")
        {
            naam = pand["properties"]["gml_id"].Value;
        }
        
        GameObject gevel = new GameObject("gebouw");
        gevel.layer = 9;
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
            double rotatie = CoordConvert.RDRotation(V3Origin);
            gevel.transform.Rotate(new Vector3(0, (float)rotatie,0));


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
            //MeshCollider mc = gevel.AddComponent<MeshCollider>();
            //mc.sharedMesh = msh;
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
                    if (i % 20 == 0)
                    {
                        yield return null;
                    }
                    
                        string naam = gebouwen["features"][i]["properties"]["PAND_ID"].Value;
                        if (naam == "") //betreft hier een top10-pand
                        {
                            CreeerGebouw(gebouwen["features"][i], container);
                        }
                        else //betreft een BAG-pand, hier controleren of er een gedetailleerd model van bestaat
                        {
                            if (LoadModel(naam, container) == false)
                            {
                                CreeerGebouw(gebouwen["features"][i], container);
                            };
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

    bool LoadModel(string bagid, GameObject container)
    {
        
        bool isaanwezig = false;

        return isaanwezig;
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
