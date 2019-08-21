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

    public TextMeshProUGUI naam;
    public TextMeshProUGUI bouwjaar;
    public TextMeshProUGUI BAGID;
    public TextMeshProUGUI verblijfsobjecten;
    public WWW request;
    public WWW verblijfRequest;
    public TMP_Dropdown dropDown;
    int pageNumber;

    JSONNode requestOutput;
    JSONNode verblijfOutput;

    List<string> verblijfList;

    public void Start()
    {
        dropDown.ClearOptions();
        verblijfList = new List<string>();
        pageNumber = 1;
    }

    public IEnumerator OnResponse(WWW req)
    {
        yield return req;

        requestOutput = JSON.Parse(req.text);

        naam.GetComponent<TextMeshProUGUI>().text = requestOutput["pandnaam"];
        bouwjaar.GetComponent<TextMeshProUGUI>().text = requestOutput["oorspronkelijk_bouwjaar"];
        BAGID.GetComponent<TextMeshProUGUI>().text = requestOutput["pandidentificatie"];
        verblijfsobjecten.GetComponent<TextMeshProUGUI>().text = requestOutput["verblijfsobjecten"]["count"];

        Debug.Log("Einde 1");
    }

    public IEnumerator AdressLoader(WWW req)
    {
        yield return req;

        Debug.Log(req.error);

        verblijfOutput = JSON.Parse(req.text);

        if (pageNumber == 1)
        {
            verblijfList.Capacity = verblijfOutput["count"];
        }
        
        if(verblijfOutput["links"]["next"]["href"] != "null")
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

            verblijfURL = verblijfOutput["links"]["next"]["href"];
            //verblijfURL = "https://api.data.amsterdam.nl/bag/verblijfsobject/?format=api" + "&page=" + pageNumber + "&panden__id=0363100012185598";
            verblijfRequest = new WWW(verblijfURL);

            StartCoroutine(AdressLoader(verblijfRequest));

        }
        else if(verblijfOutput["links"]["next"]["href"] == "null")
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
