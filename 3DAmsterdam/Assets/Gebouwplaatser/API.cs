using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SimpleJSON;
using TMPro;

public class API : MonoBehaviour
{
    public string bagID;

    private string URL;

    public TextMeshProUGUI naam;
    public TextMeshProUGUI bouwjaar;
    public TextMeshProUGUI BAGID;
    public TextMeshProUGUI verblijfsobjecten;
    public WWW request;

    JSONNode requestOutput;

    public void FixedUpdate()
    {
        URL = "https://api.data.amsterdam.nl/bag/pand/" + bagID;
        request = new WWW(URL);        
    }

    public IEnumerator OnResponse(WWW req)
    {
        yield return req;

        requestOutput = JSON.Parse(req.text);

        naam.GetComponent<TextMeshProUGUI>().text = requestOutput["pandnaam"];
        bouwjaar.GetComponent<TextMeshProUGUI>().text = requestOutput["oorspronkelijk_bouwjaar"];
        BAGID.GetComponent<TextMeshProUGUI>().text = requestOutput["pandidentificatie"];
        verblijfsobjecten.GetComponent<TextMeshProUGUI>().text = requestOutput["verblijfsobjecten"]["count"];
    }
}
