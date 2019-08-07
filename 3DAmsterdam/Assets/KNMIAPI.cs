using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SimpleJSON;

public class KNMIAPI : MonoBehaviour
{
    private string URL;
    private WWW request;
    JSONNode requestOutput;
    private string huidigWeer;
    private bool runTime = false;

    public void HuidigWeer()
    {    
        URL = "https://meteoserver.nl/api/liveweer.php?locatie=Amsterdam&key=a84d8b8009";
        WWW request = new WWW(URL);

        runTime = true;

        StartCoroutine(OnResponse(request));
    }

    public IEnumerator OnResponse(WWW req)
    {
        while (runTime)
        {
            yield return req;

            requestOutput = JSON.Parse(req.text);

            huidigWeer = requestOutput["liveweer"][0]["image"].Value;
            Debug.Log(huidigWeer);

            switch (huidigWeer)
            {
                case "bewolkt":
                    EnviroSkyMgr.instance.ChangeWeather(3);
                    break;

                case "zwaarbewolkt":
                    EnviroSkyMgr.instance.ChangeWeather(6);
                    break;

                case "buien":
                    EnviroSkyMgr.instance.ChangeWeather(7);
                    break;

                case "zon":
                    EnviroSkyMgr.instance.ChangeWeather(0);
                    break;
            }

            yield return new WaitForSeconds(7200);
        }   
    }
}
