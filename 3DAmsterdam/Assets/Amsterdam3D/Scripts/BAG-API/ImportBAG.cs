using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Linq; // dit wordt gebruikt voor op lijn 49

public class ImportBAG : ImportAPI
{
    public string BAG_ID_TEST = "0363100012171966"; // dit is test data
    public Pand.Rootobject hoofdData;

    public static ImportBAG Instance = null;

    private void Awake()
    {
        // maak een singleton zodat je deze class contant kan aanroepen vanuit elke hoek
        if (Instance == null)
        {
            Instance = this;
        }
    }

    void Start()
    {
        // dit is een test, verwijder dit in de officiele build of als je hem wilts gebruiken.
        //StartCoroutine(CallAPI("https://api.data.amsterdam.nl/bag/v1.1/pand/", BAG_ID_TEST, RetrieveType.Pand));
    }
    public IEnumerator CallAPI(string apiUrl, string bogIndexInt, RetrieveType type)
    {
        // voegt data ID en url samen tot één geheel
        string url = apiUrl + bogIndexInt;
        // stuurt een HTTP request naar de pagina
        var request = UnityWebRequest.Get(url);
        {
            yield return request.SendWebRequest();

            if (request.isDone && !request.isHttpError)
            {
                // vangt de data op in text bestand.
                dataResult = request.downloadHandler.text;

                switch (type)
                {
                    case RetrieveType.Pand:
                        // haalt alleen het pand op
                        hoofdData = JsonUtility.FromJson<Pand.Rootobject>(dataResult);
                        StartCoroutine(CallAPI("https://api.data.amsterdam.nl/bag/v1.1/nummeraanduiding/?pand=", bogIndexInt, RetrieveType.NummeraanduidingList));
                        break;
                    case RetrieveType.NummeraanduidingList:
                        // voegt de nummeraanduiding toe aan het pand, (basis gegevens zoals bouwjaar, appartement nummer, etc)
                        hoofdData += JsonUtility.FromJson<Pand.Rootobject>(dataResult);

                        foreach (Pand.Result result in hoofdData.results)
                        {
                            StartCoroutine(CallAPI("https://api.data.amsterdam.nl/bag/v1.1/nummeraanduiding/", result.landelijk_id, RetrieveType.NummeraanduidingInstance));
                        }

                        // sorteert de array alfabetisch en numeriek
                        var tempPandResults = from pand in hoofdData.results
                                  orderby pand._display // _display is de adres naam, hij sorteert hem dus van A - Z 0 - 999
                                  select pand;

                        // vervangt de pand resultaten lijst met de geordende lijst
                        hoofdData.results = tempPandResults.ToArray<Pand.Result>();

                        // toont de resultaten in de lijst
                        DisplayBAGData.Instance.ShowData(hoofdData);
                        break;
                    case RetrieveType.NummeraanduidingInstance:

                        // voegt de nummeraanduiding toe aan het pand, haalt alle adres gegevens op (Postcode, huisnummer, juiste pand type etc)
                        Pand.PandInstance temp = JsonUtility.FromJson<Pand.PandInstance>(dataResult);
                        foreach (Pand.Result result in hoofdData.results)
                        {
                            if(result.landelijk_id == temp.nummeraanduidingidentificatie)
                            {
                                // voegt adres gegevens toe als het gebouwID matcht met het adres ID (vrij logisch)
                                result.adresGegevens = temp;
                            }
                        }
                        break;
                    default:
                        hoofdData = JsonUtility.FromJson<Pand.Rootobject>(dataResult);
                        break;
                }
            }
        }
    }
}
public enum RetrieveType
{
    Pand,
    NummeraanduidingInstance,
    NummeraanduidingList
}



[System.Serializable]
public class Pand
{
    [System.Serializable]
    public class Rootobject
    {
        //public _Links _links = new _Links();
        public int count;
        public Result[] results;
        public string _display;
        public string pandidentificatie;
        public string date_modified;
        public string document_mutatie;
        public string document_nummer;
        public string status;
        public float[] bbox;
        //public Geometrie geometrie; 
        public string oorspronkelijk_bouwjaar;
        public string bouwlagen;
        public string hoogste_bouwlaag;
        public string laagste_bouwlaag;
        public string pandnaam;
        public string ligging;
        public string type_woonobject;
        public Verblijfsobjecten verblijfsobjecten;
        public _Adressen _adressen;
        public _Monumenten _monumenten;
        public Bouwblok bouwblok;
        public string begin_geldigheid;
        public string einde_geldigheid;
        public _Buurt _buurt;
        public _Buurtcombinatie _buurtcombinatie;
        public _Stadsdeel _stadsdeel;
        public _Gemeente _gemeente;
        public string dataset;

        // simpele operator die de resultaten van het pand toevoegd aan het aangemaakte pand
        public static Pand.Rootobject operator +(Rootobject a, Rootobject b)
        {
            a.count = b.count;
            a.results = b.results;
            return a;
        }

    }
    /* // onnodige links?
    [System.Serializable]
    public class _Links
    {
        public Self self = new Self();
    }
    [System.Serializable]
    public class Self
    {
        public string href;
    }
    */
    /*
    [System.Serializable]
    public class Geometrie
    {
        public string type;
        public float[][][] coordinates;
    }
    */
    [System.Serializable]
    public class Verblijfsobjecten
    {
        public int count;
        public string href;
    }
    [System.Serializable]
    public class _Adressen
    {
        public string href;
    }
    [System.Serializable]
    public class _Monumenten
    {
        public string href;
    }
    [System.Serializable]
    public class Bouwblok
    {
        //public _Links1 _links = new _Links1();
        public string _display;
        public string id;
        public string dataset;
    }
    /* // onnodige links?
    [System.Serializable]
    public class _Links1
    {
        public Self1 self = new Self1();
    }
    [System.Serializable]
    public class Self1
    {
        public string href;
    }
    */
    [System.Serializable]
    public class _Buurt
    {
        //public _Links2 _links = new _Links2();
        public string _display;
        public string code;
        public string naam;
        public string dataset;
    }
    /* // onnodige links?
    [System.Serializable]
    public class _Links2
    {
        public Self2 self = new Self2();
    }
    [System.Serializable]
    public class Self2
    {
        public string href;
    }
    */
    [System.Serializable]
    public class _Buurtcombinatie
    {
        //public _Links3 _links = new _Links3();
        public string _display;
        public string naam;
        public string vollcode;
        public string dataset;
    }
    /* // Onnodige Links?
    [System.Serializable]
    public class _Links3
    {
        public Self3 self = new Self3();
    }
    [System.Serializable]
    public class Self3
    {
        public string href;
    }
    */
    [System.Serializable]
    public class _Stadsdeel
    {
        //public _Links4 _links = new _Links4();
        public string _display;
        public string code;
        public string naam;
        public string dataset;
    }
    /* // onnodige links?
    [System.Serializable]
    public class _Links4
    {
        public Self4 self = new Self4();
    }
    [System.Serializable]
    public class Self4
    {
        public string href;
    }
    */
    [System.Serializable]
    public class _Gemeente
    {
        public string _display;
        //public _Links5 _links = new _Links5();
        public string naam;
        public string code;
        public string dataset;
    }
    /* // Onnodige links?
    [System.Serializable]
    public class _Links5
    {
        public Self5 self = new Self5();
    }
    [System.Serializable]
    public class Self5
    {
        public string href;
    }
    */
    [System.Serializable]
    public class Result
    {
        //public NummerLinks _links = new NummerLinks();
        public PandInstance adresGegevens = new PandInstance();
        public string _display;
        public string landelijk_id;
        public string type_adres;
        public string vbo_status;
        public string dataset;
    }
    /* // onnodige links?
    [System.Serializable]
    public class NummerLinks
    {
        public NummerSelf self = new NummerSelf();
    }
    [System.Serializable]
    public class NummerSelf
    {
        public string href;
    }
    */
    [System.Serializable]
    public class PandInstance
    {
        //public _Links _links;
        public string _display;
        public string nummeraanduidingidentificatie;
        public string date_modified;
        public string document_mutatie;
        public string document_nummer;
        public string begin_geldigheid;
        public object einde_geldigheid;
        public string status;
        public object bron;
        public string adres;
        public string postcode;
        public int huisnummer;
        public string huisletter;
        public string huisnummer_toevoeging;
        public string type;
        //public Openbare_Ruimte openbare_ruimte;
        public string type_adres;
        public object ligplaats;
        public object standplaats;
        public string verblijfsobject;
        //public Buurt buurt;
        //public Buurtcombinatie buurtcombinatie;
        //public Gebiedsgerichtwerken gebiedsgerichtwerken;
        public object grootstedelijkgebied;
        //public Stadsdeel stadsdeel;
        //public Woonplaats woonplaats;
        public Bouwblok bouwblok;
        //public _Geometrie _geometrie;
        //public string dataset;
    }

    //public class _Links
    //{
    //    public Self self;
    //}

    //public class Self
    //{
    //    public string href;
    //}

    //public class Openbare_Ruimte
    //{
    //    public _Links1 _links;
    //    public string _display;
    //    public string landelijk_id;
    //    public string dataset;
    //}

    //public class _Links1
    //{
    //    public Self1 self;
    //}

    //public class Self1
    //{
    //    public string href;
    //}

    //public class Buurt
    //{
    //    public _Links2 _links;
    //    public string _display;
    //    public string code;
    //    public string naam;
    //    public string dataset;
    //}

    //public class _Links2
    //{
    //    public Self2 self;
    //}

    //public class Self2
    //{
    //    public string href;
    //}

    //public class Buurtcombinatie
    //{
    //    public _Links3 _links;
    //    public string _display;
    //    public string naam;
    //    public string vollcode;
    //    public string dataset;
    //}

    //public class _Links3
    //{
    //    public Self3 self;
    //}

    //public class Self3
    //{
    //    public string href;
    //}

    //public class Gebiedsgerichtwerken
    //{
    //    public _Links4 _links;
    //    public string _display;
    //    public string code;
    //    public string naam;
    //    public string dataset;
    //}

    //public class _Links4
    //{
    //    public Self4 self;
    //}

    //public class Self4
    //{
    //    public string href;
    //}

    //public class Stadsdeel
    //{
    //    public _Links5 _links;
    //    public string _display;
    //    public string code;
    //    public string naam;
    //    public string dataset;
    //}

    //public class _Links5
    //{
    //    public Self5 self;
    //}

    //public class Self5
    //{
    //    public string href;
    //}

    //public class Woonplaats
    //{
    //    public _Links6 _links;
    //    public string _display;
    //    public string landelijk_id;
    //    public string dataset;
    //}

    //public class _Links6
    //{
    //    public Self6 self;
    //}

    //public class Self6
    //{
    //    public string href;
    //}

    //public class Bouwblok
    //{
    //    public _Links7 _links;
    //    public string _display;
    //    public string id;
    //    public string dataset;
    //}

    //public class _Links7
    //{
    //    public Self7 self;
    //}

    //public class Self7
    //{
    //    public string href;
    //}

    //public class _Geometrie
    //{
    //    public string type;
    //    public float[] coordinates;
    //}
}

