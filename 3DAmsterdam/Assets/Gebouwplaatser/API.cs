using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SimpleJSON;
using TMPro;
using UnityEngine.UI.Extensions;

public class API : MonoBehaviour
{
    public string bagID;

    public string pandURL;

    public string verblijfURL;

    [HideInInspector]
    public TextMeshProUGUI naam;
    [HideInInspector]
    public TextMeshProUGUI bouwjaar;
    [HideInInspector]
    public TextMeshProUGUI BAGID;
    [HideInInspector]
    public TextMeshProUGUI verblijfsobjecten;

    public WWW request;
    public WWW verblijfRequest;

    [HideInInspector]
    public TMP_Dropdown dropDown;

    int pageNumber;

    public GetBagIDs martijnsScript;

    JSONNode requestOutput;
    JSONNode verblijfOutput;

    List<string> verblijfList;

    public void Begin(string bagIDstring)
    {
        verblijfList = new List<string>();
        pageNumber = 1;

        bagID = bagIDstring;

        StartCoroutine(OnResponse(new WWW("https://api.data.amsterdam.nl/bag/pand/" + bagID)));
        StartCoroutine(AdressLoader(new WWW("https://api.data.amsterdam.nl/bag/verblijfsobject/?panden__id=" + bagID)));
        
    }

    public IEnumerator OnResponse(WWW req)
    {
        yield return req;

        requestOutput = JSON.Parse(req.text);

        foreach (string bagid in martijnsScript.Bagids)
        {
            this.gameObject.GetComponent<TekstVeldScript>().naam.text = requestOutput["pandnaam"];
            this.gameObject.GetComponent<TekstVeldScript>().bouwjaar.text = requestOutput["oorspronkelijk_bouwjaar"];
            this.gameObject.GetComponent<TekstVeldScript>().BAGID.text = requestOutput["pandidentificatie"];
            this.gameObject.GetComponent<TekstVeldScript>().label.text = requestOutput["verblijfsobjecten"]["count"];
        }
    }

    public IEnumerator AdressLoader(WWW req)
    {
        yield return req;

        Debug.Log(req.error);

        verblijfOutput = JSON.Parse(req.text);

        if (pageNumber == 1)
        {
            dropDown.ClearOptions();
            verblijfList.Capacity = verblijfOutput["count"];
        }
        
        if(verblijfOutput["_links"]["next"]["href"] != null)
        {
            for (int i = 0; i < verblijfOutput["results"].Count; i++)
            {
                verblijfList.Add(verblijfOutput["results"][i]["_display"]);
            }

            dropDown.AddOptions(verblijfList);
            verblijfList.Clear();
            Debug.Log("Aantal options: " + dropDown.options.Count);
            Debug.Log(pageNumber);

            pageNumber++;

            verblijfURL = verblijfOutput["_links"]["next"]["href"];
            verblijfRequest = new WWW(verblijfURL);

            StartCoroutine(AdressLoader(verblijfRequest));

        }
        else
        {
            for (int i = 0; i < verblijfOutput["results"].Count; i++)
            {
                verblijfList.Add(verblijfOutput["results"][i]["_display"]);
            }

            dropDown.AddOptions(verblijfList);
            Debug.Log("Aantal options: " + dropDown.options.Count);
            verblijfList.Clear();

            Debug.Log(pageNumber);
            Debug.Log("Klaar met Loopen");
            Debug.Log(dropDown.options);
        }
    }
}
