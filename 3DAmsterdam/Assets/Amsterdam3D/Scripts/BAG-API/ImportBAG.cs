using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Linq;

public class ImportBAG : ImportAPI
{
    public static ImportBAG Instance = null;
    [HideInInspector]
    public Pand.Rootobject hoofdData;

    public string numberIndicatorURL = "https://api.data.amsterdam.nl/bag/v1.1/nummeraanduiding/?pand=";
    public string numberIndicatorInstanceURL = "https://api.data.amsterdam.nl/bag/v1.1/nummeraanduiding/";
    public string monumentURL = "https://api.data.amsterdam.nl/monumenten/monumenten/?betreft_pand=";

    private void Awake()
    {
        // creates a singleton which can be called on every corner
        if (Instance == null)
        {
            Instance = this;
        }
    }

    public IEnumerator CallAPI(string apiUrl, string bagIndexInt, RetrieveType type)
    {
       
        // adds data id and url in one string
        string url = apiUrl + bagIndexInt;
        // send http request
        var request = UnityWebRequest.Get(url);
        {
            yield return request.SendWebRequest();

            if (request.isDone && !request.isHttpError)
            {
                // catches the data
                dataResult = request.downloadHandler.text;

                switch (type)
                {
                    case RetrieveType.Pand:
                        // retrieves premises
                        hoofdData = JsonUtility.FromJson<Pand.Rootobject>(dataResult);
                        StartCoroutine(CallAPI(numberIndicatorURL, bagIndexInt, RetrieveType.NummeraanduidingList));
                        break;

                    case RetrieveType.NummeraanduidingList:
                        // adds the number instance to the premises, (basic things such as zip, construction year etc) 
                        hoofdData += JsonUtility.FromJson<Pand.Rootobject>(dataResult);

                        foreach (Pand.PandResults result in hoofdData.results)
                        {
                            StartCoroutine(CallAPI(numberIndicatorInstanceURL, result.landelijk_id, RetrieveType.NummeraanduidingInstance));
                        }

                        // sorteert de array alfabetisch en numeriek
                        var tempPandResults = from pand in hoofdData.results
                                              orderby pand._display // _display is the adres name, sorts it from A - Z 0 - 999
                                              select pand;

                        // replaces the premises results list and sorts them
                        hoofdData.results = tempPandResults.ToArray<Pand.PandResults>();
                        // shows results in the list
                        DisplayBAGData.Instance.ShowData(hoofdData);
                        break;

                    case RetrieveType.NummeraanduidingInstance:
                        // adds the number instances to the premises and retrieves all info such as zip, adress number etc for the current chosen premises
                        Pand.PandInstance tempPand = JsonUtility.FromJson<Pand.PandInstance>(dataResult);
                        if (hoofdData.results != null){ 
                            foreach (Pand.PandResults result in hoofdData.results)
                            {
                                if (result.landelijk_id == tempPand.nummeraanduidingidentificatie)
                                {
                                    // checks if the id matches and then adds the data
                                    result.nummeraanduiding = tempPand;
                                    // adds accomodation data to the current adress
                                    StartCoroutine(CallAPI(result.nummeraanduiding.verblijfsobject, "", RetrieveType.VerblijfsobjectInstance));
                                }
                            }
                         }
                        // retrieves monument data from this premises
                        StartCoroutine(CallAPI(monumentURL, hoofdData.pandidentificatie, RetrieveType.Monumenten));
                        
                        break;

                    case RetrieveType.VerblijfsobjectInstance:
                        // adds accommodation data for this adress 
                        Pand.VerblijfsInstance tempVerblijf = JsonUtility.FromJson<Pand.VerblijfsInstance>(dataResult);
                        if (hoofdData.results != null)
                        {
                            foreach (Pand.PandResults result in hoofdData.results)
                            {
                                if (result?.nummeraanduiding._display == tempVerblijf?._display)
                                {
                                    // adds accommodation data for this adress 
                                    result.verblijfsobject = tempVerblijf;
                                }
                            }
                        }
                        break;

                    case RetrieveType.Monumenten:
                        // adds monument data to this premises
                        Pand.Monumenten tempMonument = JsonUtility.FromJson<Pand.Monumenten>(dataResult);
                        hoofdData.monumenten = tempMonument;
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
    NummeraanduidingList,
    VerblijfsobjectList,
    VerblijfsobjectInstance,
    Monumenten
}

