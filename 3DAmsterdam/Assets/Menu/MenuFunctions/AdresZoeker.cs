using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using SimpleJSON;
using System.Globalization;
using ConvertCoordinates;



public class AdresZoeker : MonoBehaviour
{
    public InputField Inputfield;
    bool IgnoreInpufieldChange = false;
    public Dropdown ResultatenDropdown;
    public Button[] ResultButtons = new Button[5];
    string[] ResultIDs = new string[5];

    //string SuggestieURL = "https://geodata.nationaalgeoregister.nl/locatieserver/v3/suggest?q=zoekterm%20and%20Amsterdam%20and%20type:adres&rows=5";
    string SuggestieURL = "https://geodata.nationaalgeoregister.nl/locatieserver/v3/suggest?q=zoekterm%20and%20Amsterdam%20&rows=5";
    string locatieURL = "https://geodata.nationaalgeoregister.nl/locatieserver/v3/lookup?id=zoekterm";


    public void Update()
    {
        

    }

    public void GetSuggestions(string waarde)
    {
        string inputwaarde = waarde;
        Debug.Log(inputwaarde);
        StopAllCoroutines();
        if (inputwaarde.Length>2)
        {
            StartCoroutine(FindSuggestions(inputwaarde));

        }
    }

    public void GetCoordinaat(int buttonID)
    {
        Debug.Log("knopnummer " + buttonID);
        Debug.Log("knoptekst" + ResultButtons[buttonID].GetComponentInChildren<Text>().text);
        Debug.Log("knopID" + ResultIDs[buttonID]);
        StartCoroutine(FindLocation(ResultIDs[buttonID]));
    }
    IEnumerator FindLocation(string zoekterm)
    {
        string uri =locatieURL.Replace("zoekterm", zoekterm);
        using (UnityWebRequest webRequest = UnityWebRequest.Get(uri))
        {
            // Request and wait for the desired page.
            yield return webRequest.SendWebRequest();

            if (webRequest.isNetworkError)
            {
                Debug.Log("Error: " + webRequest.error);
            }
            else
            {
                string resultaat = webRequest.downloadHandler.text;
                JSONNode N = JSON.Parse(resultaat);
                int TotaalaantalItems = N["response"]["numFound"];
                if (TotaalaantalItems == 0)
                {
                    Debug.Log("geen zoekresultaten voor " + zoekterm);
                    yield break;
                }
                string locatiedata = N["response"]["docs"][0]["centroide_ll"].ToString();
                Debug.Log(locatiedata);
                locatiedata = locatiedata.Replace("POINT(", "");
                locatiedata = locatiedata.Replace(")", "");
                locatiedata = locatiedata.Replace("\"", "");

                string lonstr = locatiedata.Split(' ')[0];
                string latstr = locatiedata.Split(' ')[1];
                Debug.Log(lonstr);
                Debug.Log(latstr);
                double lon;
                double lat;
                double.TryParse(lonstr, NumberStyles.Any, CultureInfo.InvariantCulture, out lon);
                double.TryParse(latstr, NumberStyles.Any, CultureInfo.InvariantCulture, out lat);
                Debug.Log(lon);
                Debug.Log(lat);
                Vector3 UnityLocatie = CoordConvert.WGS84toUnity(lon, lat);

                Vector3 Campositie = UnityLocatie;
                Campositie.x -= 100;
                Campositie.z -= 100;
                Campositie.y = 200;
                Camera.main.transform.position = Campositie;
                Camera.main.transform.LookAt(UnityLocatie, Vector3.up);
            }

        }
    }
    IEnumerator FindSuggestions(string zoekterm)
    {

        string gefixteZoekterm = zoekterm.Replace(" ", "%20");
        string uri = SuggestieURL.Replace("zoekterm", gefixteZoekterm);
        Debug.Log(uri);
        using (UnityWebRequest webRequest = UnityWebRequest.Get(uri))
        {
            // Request and wait for the desired page.
            yield return webRequest.SendWebRequest();

            if (webRequest.isNetworkError)
            {
                Debug.Log("Error: " + webRequest.error);
            }
            else
            {
                string resultaat = webRequest.downloadHandler.text;
                JSONNode N = JSON.Parse(resultaat);
                int TotaalaantalItems = N["response"]["numFound"];
                if (TotaalaantalItems==0)
                {
                    Debug.Log("geen zoekresultaten voor " + zoekterm);
                    yield break;
                }
                Debug.Log("Numfound = " + TotaalaantalItems);
                Debug.Log("Received: " + resultaat);
                
                int aantalItems = Mathf.Min(5, TotaalaantalItems);
                int i;
                string gevondentekst;
                string vervangtekst = "<b>" + zoekterm + "</b>";



                for (i = 0; i < aantalItems; i++)
                {
                    gevondentekst = N["response"]["docs"][i]["weergavenaam"].ToString().Replace("\"", "");
                    gevondentekst = gevondentekst.Replace(zoekterm, vervangtekst);
                    int startvanzoekterm = gevondentekst.ToLower().IndexOf(zoekterm.ToLower());
                    string gevondenzoekterm = gevondentekst.Substring(startvanzoekterm, zoekterm.Length);
                    string restvangevondentekst = gevondentekst.Substring(startvanzoekterm + zoekterm.Length);
                    string deftekst = "";
                    if (startvanzoekterm>0)
                    {
                        deftekst = gevondentekst.Substring(0, startvanzoekterm);
                    }
                    deftekst = deftekst + "<b>" + gevondenzoekterm + "</b>" + restvangevondentekst;
                    ResultButtons[i].GetComponentInChildren<Text>().text = deftekst;
                    ResultIDs[i] = N["response"]["docs"][i]["id"].ToString();
                    ResultButtons[i].interactable = true;
                }
                while (i<5)
                {
                    ResultButtons[i].GetComponentInChildren<Text>().text = "";
                    ResultButtons[i].interactable = false;
                    i++;
                }

            }
        }
    }
}
