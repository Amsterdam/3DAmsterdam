using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
public class ImportBOG : ImportAPI
{
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(CallAPI("https://api.data.amsterdam.nl/bag/v1.1/pand/", "0363100012165761", 0));
    }

    public override IEnumerator CallAPI(string apiUrl, string bogIndexInt, int resultIndex)
    {
        // voegt data ID en url samen tot één geheel
        string url = apiUrl + bogIndexInt + "/";
        Debug.Log(url);
        // stuurt een HTTP request naar de pagina
        var request = UnityWebRequest.Get(url);
        {
            yield return request.SendWebRequest();

            if (request.isDone && !request.isHttpError)
            {
                // vangt de data op in text bestand.
                dataResult = request.downloadHandler.text;
                // print hier de data maar je kan vanuit hier hem door sturen en gebruiken.
                Debug.Log(JsonUtility.FromJson<Rootobject>(dataResult).date_modified.ToString());
            }
        }
    }

}



[System.Serializable]
public class Rootobject
{
    public _Links _links = new _Links();
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
    public object type_woonobject;
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
}
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
    public _Links1 _links = new _Links1();
    public string _display;
    public string id;
    public string dataset;
}
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
[System.Serializable]
public class _Buurt
{
    public _Links2 _links = new _Links2();
    public string _display;
    public string code;
    public string naam;
    public string dataset;
}
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
[System.Serializable]
public class _Buurtcombinatie
{
    public _Links3 _links = new _Links3();
    public string _display;
    public string naam;
    public string vollcode;
    public string dataset;
}
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
[System.Serializable]
public class _Stadsdeel
{
    public _Links4 _links = new _Links4();
    public string _display;
    public string code;
    public string naam;
    public string dataset;
}
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
[System.Serializable]
public class _Gemeente
{
    public string _display;
    public _Links5 _links = new _Links5();
    public string naam;
    public string code;
    public string dataset;
}
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
