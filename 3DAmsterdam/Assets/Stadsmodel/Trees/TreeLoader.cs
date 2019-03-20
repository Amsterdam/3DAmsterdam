using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BruTile;
using UnityEngine.Networking;
using ConvertCoordinates;
using SimpleJSON;
using System;
using System.Linq;


public class TreeLoader : MonoBehaviour
{
    private CameraView CV;
    private string TreeURL = "https://map.data.amsterdam.nl/maps/bgtobjecten?SERVICE=wfs&version=2.0.0&request=GetFeature&typeName=BGTPLUS_VGT_boom&srsName=epsg:4326&outputFormat=geojson&bbox={xmin},{ymin},{xmax},{ymax}";
    Dictionary<Vector3, GameObject> treeDb = new Dictionary<Vector3, GameObject>();
    Queue<Boomdata> DownloadQueue = new Queue<Boomdata>();
    List<Vector3> PendingQueue = new List<Vector3>();
    public GameObject Boomcontainer;
    public GameObject BoomPrefab;
    public int MaxParallelDownloads = 5;
    public int MaxAfstand = 1000;
    struct Boomdata
    {
        public GameObject Container;
        public Vector3 Locatie;


        public Boomdata(GameObject container, Vector3 locatie)
        {
            Container = container;
            Locatie = locatie;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        CV = Camera.main.GetComponent<CameraView>();
        //Vector3WGS min = CoordConvert.RDtoWGS84(121000, 486000);
        //Vector3WGS max = CoordConvert.RDtoWGS84(122000, 487000);

        //DownloadQueue.Enqueue(new Boomdata(Boomcontainer, 121000, 486000, 122000, 487000));
    }

    // Update is called once per frame
    void Update()
    {
        UpdateTrees(CV.CameraExtent);
        if (DownloadQueue.Count>0 && PendingQueue.Count<MaxParallelDownloads)
        {
            Boomdata dwnload = DownloadQueue.Dequeue();
            PendingQueue.Add(dwnload.Locatie);
            StartCoroutine(Download(dwnload));
        }

    }
    private IEnumerator Download(Boomdata test)
    {

        string downloadstring = TreeURL;
        downloadstring = downloadstring.Replace("{xmin}", test.Locatie.x.ToString());
        downloadstring = downloadstring.Replace("{ymin}", test.Locatie.y.ToString());
        downloadstring = downloadstring.Replace("{xmax}", (test.Locatie.x+1000).ToString());
        downloadstring = downloadstring.Replace("{ymax}", (test.Locatie.y+1000).ToString());
        Debug.Log(downloadstring);
        UnityWebRequest www = UnityWebRequest.Get(downloadstring);

        yield return www.SendWebRequest();
        if (!www.isNetworkError && !www.isHttpError)
        {
            Debug.Log(www.downloadHandler.text);
            var bomen = JSON.Parse(www.downloadHandler.text);

            for (int i = 0; i < bomen["features"].Count; i++)
            {
                if (i % 20 == 0)
                {
                    yield return null;
                }

                string stringX = bomen["features"][i]["geometry"]["coordinates"][0].Value;
                string stringY = bomen["features"][i]["geometry"]["coordinates"][1].Value;
                Vector3WGS wgsloc = new Vector3WGS();
                double dbl;
                double.TryParse(stringX, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out dbl);

                wgsloc.lon = dbl;
                double.TryParse(stringY, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out dbl);
                wgsloc.lat = dbl;
                Vector3 Locatie = CoordConvert.WGS84toUnity(wgsloc);
                GameObject Boom = Instantiate(BoomPrefab);
                
                Boom.transform.parent = test.Container.transform;
                Vector3 RaycastVanaf = Locatie;
                RaycastVanaf.y = 100;
                RaycastHit hit;
                int layerMask = 1 << 8;
                if (Physics.Raycast(RaycastVanaf, Vector3.down, out hit, Mathf.Infinity,layerMask))
                {
                    Boom.transform.localPosition = hit.point;
                }
                else
                {
                    RaycastVanaf.y = 0 - (float)CoordConvert.ReferenceRD.z;
                    Boom.transform.localPosition = RaycastVanaf;
                }
            }
        }
        else
        {
            Debug.Log("kan bestand niet downloaden van " + downloadstring);
        }
        PendingQueue.Remove(test.Locatie);
    }

    void UpdateTrees(Extent TempExtent)
    {
        // WGS-extent omzetten naar RD-extent
        Vector3RD RDmin = CoordConvert.WGS84toRD(TempExtent.MinX, TempExtent.MinY);
        Vector3RD RDmax = CoordConvert.WGS84toRD(TempExtent.MaxX, TempExtent.MaxY);
        TempExtent = new Extent(RDmin.x, RDmin.y, RDmax.x, RDmax.y);

        //1. uitzoeken welke tegels nodig zijn
        List<Vector3> BomenNodig = new List<Vector3>();
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
                double afstand = Math.Sqrt(Math.Pow(Verschil.x, 2) + Math.Pow(Verschil.y, 2) + Math.Pow(Verschil.z, 2));
                if (afstand < MaxAfstand)
                {
                    BomenNodig.Add(new Vector3(deltax, deltay, 0));
                }
            }
        }
        // bepalen welke reeds geladentegels niet meer nodig zijn en verwijderd kunnen worden
        Vector3[] Top10DBKeys = treeDb.Keys.ToArray();
        bool nodig = false;
        foreach (Vector3 V in Top10DBKeys)
        {
            nodig = false;
            foreach (var t in BomenNodig)
            {
                if (t == V)
                {
                    nodig = true;
                }
            }
            if (nodig == false)
            {
                // te verwijderen tegels toevoegen aan ee lijst met te verwijderen tegels

                Destroy(treeDb[V]);
                // te verwijderen tegels verwijderen uit de lijst met geladen tegels
                treeDb.Remove(V);
            }
        }
        // tegels die niet meer nodig zijn uit de downloadqueue verwijderen
        Boomdata[] reqs = DownloadQueue.ToArray();

        Queue<Boomdata> tempQueue = new Queue<Boomdata>();
        foreach (Boomdata req in reqs)
        {

                nodig = false;
                foreach (var t in BomenNodig)
                {
                    if (t == req.Locatie)
                    {
                        nodig = true;
                        tempQueue.Enqueue(req);
                    }
                }
        }
        DownloadQueue = tempQueue;

        //nieuwe tiles toevoegen aan TileDB
        //immediately draw placeholder tile and fire request for texture and height. Depending on which one returns first, update place holder.
        foreach (var t in BomenNodig)
        {
            //draw placeholder tile

            if (treeDb.ContainsKey(t) == false) // alleen verdergaan als de tegel nog niet in de lijst met geladentegels staat.
            {
                GameObject bomentegel;

                    bomentegel = new GameObject("bomen/" + t.x + "_" + t.y);
                    bomentegel.transform.parent = transform;

                treeDb.Add(t, bomentegel);

                //get tile texture data


                DownloadQueue.Enqueue(new Boomdata(bomentegel,t));


            }

        }
    }
}
