using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class ImportWKBP : ImportAPI
{
    public static ImportWKBP Instance = null;
    public WKBP.RootBeperkingen wkbpData;

    private void Awake()
    {
        // maak een singleton zodat je deze class contant kan aanroepen vanuit elke hoek
        if (Instance == null)
        {
            Instance = this;
        }
    }

    public IEnumerator CallWKBP(Pand.PandResults tempVerblijf)
    {
        string wkbpURL = "https://api.data.amsterdam.nl/wkpb/beperking/?verblijfsobjecten__id=" + tempVerblijf.verblijfsobject.verblijfsobjectidentificatie;
        string wkbpResult = "";
        WKBP.RootBeperkingen wkbpBeperkingen = new WKBP.RootBeperkingen();
        var WKPBRequest = UnityWebRequest.Get(wkbpURL);
        {
            yield return WKPBRequest.SendWebRequest();

            if (WKPBRequest.isDone && !WKPBRequest.isHttpError)
            {
                // vangt de data op in text bestand.
                wkbpResult = WKPBRequest.downloadHandler.text;

                wkbpBeperkingen = JsonUtility.FromJson<WKBP.RootBeperkingen>(wkbpResult);


                tempVerblijf.verblijfsobject.wkbpBeperkingen = wkbpBeperkingen;
            }

        }

        for (int i = 0; i < wkbpBeperkingen.results.Length; i++)
        {
            string wkbpInstanceURL = wkbpBeperkingen.results[i]._links.self.href;
            string wkbpInstanceResult = "";
            var WKPBInstanceRequest = UnityWebRequest.Get(wkbpInstanceURL);
            {
                yield return WKPBInstanceRequest.SendWebRequest();

                if (WKPBInstanceRequest.isDone && !WKPBInstanceRequest.isHttpError)
                {
                    // vangt de data op in text bestand.
                    wkbpInstanceResult = WKPBInstanceRequest.downloadHandler.text;
                }
            }
            WKBP.Beperking beperkingInstance = JsonUtility.FromJson<WKBP.Beperking>(wkbpInstanceResult);
            wkbpBeperkingen.results[i].beperking = beperkingInstance;
            tempVerblijf.verblijfsobject.wkbpBeperkingen = wkbpBeperkingen;
            wkbpData = wkbpBeperkingen;
        }

    }
}

[System.Serializable]
public class WKBP
{
    //https://api.data.amsterdam.nl/wkpb/beperking/?verblijfsobjecten__id=0363010000802188
    [System.Serializable]
    public class RootBeperkingen
    {
        //public _Links _links;
        public int count;
        public Result[] results;
    }
    /*
    public class _Links
    {
        public Self self;
        public Next next;
        public Previous previous;
    }

    public class Self
    {
        public string href;
    }

    public class Next
    {
        public string href;
    }

    public class Previous
    {
        public object href;
    }
    */
    [System.Serializable]
    public class Result
    {
        public BeperkingLink _links = new BeperkingLink();
        public Beperking beperking = new Beperking();
        public string _display;
        public int inschrijfnummer;
    }
    [System.Serializable]
    public class BeperkingLink
    {
        public BeperkingURL self = new BeperkingURL();
    }
    [System.Serializable]
    public class BeperkingURL
    {
        public string href;
    }
    [System.Serializable]
    public class Beperking
    {
        //public _Links _links;
        public string _display;
        public string id;
        public string inschrijfnummer;
        public string datum_in_werking;
        public string datum_einde;
        public Kadastrale_Objecten kadastrale_objecten = new Kadastrale_Objecten();
        public Documenten documenten = new Documenten();
        public Beperkingcode beperkingcode = new Beperkingcode();
    }
    /*
        public class _Links
        {
            public Self self;
        }

        public class Self
        {
            public string href;
        }
        */
    [System.Serializable]
    public class Kadastrale_Objecten
    {
        public int count;
        public string href;
    }
    [System.Serializable]
    public class Documenten
    {
        public int count;
        public string href;
    }
    [System.Serializable]
    public class Beperkingcode
    {
        public string code;
        public string omschrijving;
    }

}