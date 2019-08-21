using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleJSON;
using ConvertCoordinates;
using UnityEngine.Networking;
/// <summary>
/// Zoekt BagIds op op basis van RDcoordinaten
/// </summary>
public class GetBagIDs :MonoBehaviour
{
    public List<string> Bagids = new List<string>();        //gevonden BagIds
    public bool Bezig = false;                              //geeft aan of het zoeken nar BAGids bezig is
    public bool HasError = false;                           //geeft aan of er bij het zoeken naar BagIDs fouten zijn opgetreden
    public SelectBuilding api;                                         //het Script dat verdergaat met de gevonden BagIDs


    private WFSStatus Status = WFSStatus.ReadyToStart;
    private Errortype FoutType;
    private bool StapBezig = false;

    private Vector3 LocatieUnity;



    //Zoekinstellingen
    private int Servernr = 0;
    //eerste server waar gezocht moet worden:
    private string BagURL1 = "https://map.data.amsterdam.nl/maps/bag?service=wfs&version=2.0.0&outputFormat=geojson&CRS=EPSG::4326&request=GetFeature&Typenames=pand&bbox={xmin},{ymin},{xmax},{ymax}&srs=EPSG::4326";
    //tweede server waar gezocht moet worden, als de eerste server niet beschikbaar is
    private string BagURL2 = "https://geodata.nationaalgeoregister.nl/bag/wfs?service=wfs&version=2.0.0&outputFormat=json&CRS=EPSG::4326&request=GetFeature&Typenames=pand&bbox={xmin},{ymin},{xmax},{ymax}&srs=EPSG::4326";
    //straal waarbinnen een bagpand gezocht moet worden in meters
    private int Zoekstraal;
    private int ZoekstraalStandaard = 1;
    private int ZoekstraalGroot = 5;


    private JSONNode WFSData;   //de wfsdata die terugkomt van de server

    private RaycastHit hit;     
    private Ray ray;
    // het type foutmelding dat de server geeft
    private enum Errortype
    {
        GeenLocatie,
        GeenNetwerk,
        GeenServer,
        GeenPanden
    }

    private enum WFSStatus
    {
        ReadyToStart,
        GettingWFSData,
        GotWFSData,
        UsingWFSData,
        HasError,
        FInished
    }

    private void Update()
    {
        //als geklikt wordt en er nog niet gezocht wordt naar BagIDs
        if (Input.GetMouseButtonDown(0) && Bezig==false)
        {
            ray = Camera.main.ScreenPointToRay(Input.mousePosition); 
            if (Physics.Raycast(ray, out hit, LayerMask.GetMask("Panden"))) //raycast naar layer Panden
            {
                //Hitpoint opslaan en coroutine starten
                LocatieUnity = hit.point;
                StartCoroutine(GetBagID());
            }
        }
    }


    private IEnumerator GetBagID()
    {
        //instellingen op standard zetten
        Bezig = true;
        Zoekstraal = ZoekstraalStandaard;

        // data zoeken met kleine straal op eerste server
        StapBezig = true;
        Status = WFSStatus.GettingWFSData;
        StartCoroutine(GetWFSData());
        while (StapBezig){yield return null;}

        //als geen netwerkverbinding, stoppen
        if (Status == WFSStatus.HasError && FoutType == Errortype.GeenNetwerk)
        {
            HasError = true;
            yield break;
        }

        // als server niet beschikbaar, tweede server proberen
        if (Status == WFSStatus.HasError && FoutType == Errortype.GeenServer)
        {
            Servernr = 1;
            StapBezig = true;
            Status = WFSStatus.GettingWFSData;
            StartCoroutine(GetWFSData());
            while (StapBezig) { yield return null; }
        }

        //als geen gebouwen gevonden, groter zoekstraal proberen
        if (Status == WFSStatus.HasError && FoutType == Errortype.GeenPanden)
        {
            Zoekstraal = ZoekstraalGroot;
            StapBezig = true;
            Status = WFSStatus.GettingWFSData;
            StartCoroutine(GetWFSData());
            while (StapBezig) { yield return null; }

            //als nog steeds geen gebouwen gevonden, stoppen
            if (Status == WFSStatus.HasError && FoutType == Errortype.GeenPanden)
            {
                HasError = true;
                yield break;
            }

        }

        //wfsdata uitlezen
        ReadBAGIDS();

        //gevonden bagid naar api-script sturen
        //api.locatie = hit.point;
        if (Bagids.Count>0)
        {
            api.SelectHoogbouw(Bagids[0]);
        }

           

        //aangeven dat zoekactie gereed is
        Bezig = false;

        
    }
    private void ReadBAGIDS()
    {
        Bagids = new List<string>();
        int aantalGebouwen = WFSData["features"].Count;
        if (aantalGebouwen == 0) //serverantwoord zonder panden onderscheppen
        {
            Status = WFSStatus.HasError;
            FoutType = Errortype.GeenPanden;
            return;
        }
        for (int i = 0; i < WFSData["features"].Count; i++) // alle features in serverantwoord doorlopen en bagid bepalen
        {
            if (Servernr == 0) //bagid bepalen bij serverantwoord van server1
            {
                Bagids.Add(WFSData["features"][i]["properties"]["id"]);

            }
            else //bagid bepalen bij serverantwoord van server2
            {
                Bagids.Add("0" + WFSData["features"][i]["properties"]["identificatie"]);
            }
        }
        if (Status != WFSStatus.HasError)
        {
            Status = WFSStatus.FInished;
        }
    }
    private IEnumerator GetWFSData()
    {
        //url bepalen
        string url = "";
        switch (Servernr)
        {
            case 0:
                url = BagURL1;
                break;
            case 1:
                url = BagURL2;
                break;
            default:
                break;
        }

        //zoeklocatie omrekenen naar RD-coordinaten
        Vector3RD posRD = CoordConvert.UnitytoRD(LocatieUnity);

        //zoekgebied bepalen aan de hand van zoekstraal
        int radius = Zoekstraal;
        int minx = (int)posRD.x - radius;
        int maxx = (int)posRD.x + radius;
        int miny = (int)posRD.y - radius;
        int maxy = (int)posRD.y + radius;

        //downloadstring maken
        string downloadstring = url;
        downloadstring = downloadstring.Replace("{xmin}", minx.ToString());
        downloadstring = downloadstring.Replace("{ymin}", miny.ToString());
        downloadstring = downloadstring.Replace("{xmax}", maxx.ToString());
        downloadstring = downloadstring.Replace("{ymax}", maxy.ToString());

        //dataverzoek sturen
        UnityWebRequest www = UnityWebRequest.Get(downloadstring);

        //wachten op antwoord van de server
        yield return www.SendWebRequest();

        if (www.isNetworkError) //netwerkerror onderscheppen (geen internet)
        {
            Status = WFSStatus.HasError;
            FoutType = Errortype.GeenNetwerk;
        }
        else if (www.isHttpError) //httperror onderscheppen (server down)
        {
            Status = WFSStatus.HasError;
            FoutType = Errortype.GeenServer;
        }
        else //wfsdata opslaan
        {
            WFSData = JSON.Parse(www.downloadHandler.text);
            Status = WFSStatus.GotWFSData;
        }
        StapBezig = false; //aangeven dat servercommunicatie gereed is

    }
}
