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

    private string pandURL;
    private string verblijfURL;
    bool adresAanvraag = false;

    public TextMeshProUGUI naam;
    public TextMeshProUGUI bouwjaar;
    public TextMeshProUGUI BAGID;
    public TextMeshProUGUI verblijfsobjecten;
    public WWW request;
    private WWW verblijfRequest;
    public TMP_Dropdown dropDown;

    JSONNode requestOutput;
    JSONNode verblijfOutput;

    public void FixedUpdate()
    {
        pandURL = "https://api.data.amsterdam.nl/bag/pand/" + bagID;
        request = new WWW(pandURL);
     
        if (adresAanvraag)
        {
            verblijfURL = "https://api.data.amsterdam.nl/bag/verblijfsobject/?panden__id=" + bagID;
            verblijfRequest = new WWW(verblijfURL);

            StartCoroutine(AdressLoader(verblijfRequest));
            adresAanvraag = false;
        }
    }

    public IEnumerator OnResponse(WWW req)
    {
        yield return req;

        requestOutput = JSON.Parse(req.text);

        naam.GetComponent<TextMeshProUGUI>().text = requestOutput["pandnaam"];
        bouwjaar.GetComponent<TextMeshProUGUI>().text = requestOutput["oorspronkelijk_bouwjaar"];
        BAGID.GetComponent<TextMeshProUGUI>().text = requestOutput["pandidentificatie"];
        verblijfsobjecten.GetComponent<TextMeshProUGUI>().text = requestOutput["verblijfsobjecten"]["count"];

        adresAanvraag = true;
    }

    public IEnumerator AdressLoader(WWW req)
    {
        List<string> verblijfList = new List<string>();

        yield return req;

        verblijfOutput = JSON.Parse(req.text);

        verblijfList.Capacity = verblijfOutput["count"];

        Debug.Log(verblijfList.Capacity);

        for (int i = 0; i < verblijfList.Count; i++)
        {
            verblijfList[i] = verblijfOutput["results"][i]["_display"];    
        }

        dropDown.ClearOptions();
        dropDown.AddOptions(verblijfList);      
    }
}
