using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleJSON;
using ConvertCoordinates;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class PanoramaAPI : MonoBehaviour
{
    /* Plan van aanpak:
     * Raycast naar de grond CHECK
     * Haal coördinaten op van de hit.point CHECK
     * Haal de coördinaten door martijn zijn script, daar krijg je lon/lat uit CHECK
     * Kijk in de API in een radius van 100 meter welke coördinaten hier het dichtstbij zijn CHECK
     * Geef deze coördinaten weer op de kaart met een stipje of animatie.
     * Laat de persoon klikken op zo'n stipje
     * Geef de panorama weer van de locatie naar keuze. Einde Coroutine.
     * 
     * 
     */

    public Vector3WGS wgs;
    JSONNode requestOutput;
    Vector3 location;
    List<string> Locaties;
    RaycastHit hit;
    Ray ray;

    // Start is called before the first frame update
    public void WorldClick()
    {    
        Locaties = new List<string>();
        StartCoroutine(ClickPhase());
    }

    public void Update()
    {
        ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Input.GetMouseButtonDown(0) && EventSystem.current.currentSelectedGameObject == null)
        {
            hit.point = Input.mousePosition;
        }
    }
    
    public IEnumerator ClickPhase()
    {
        yield return new WaitForSeconds(5);

        Debug.Log("Hit.point:" + hit.point);           

        yield return new WaitUntil(() => hit.point != null);

        wgs = CoordConvert.UnitytoWGS84(hit.point);

        //wgs.lon = Mathf.Round((float)wgs.lon);
        //wgs.lat = Mathf.Round((float)wgs.lat);

        Debug.Log("WgsLon: " + wgs.lon + " Wgslat: " + wgs.lat);
        StartCoroutine(Panoramas(new WWW("https://api.data.amsterdam.nl/panorama/panoramas/?near=" + wgs.lon + "," + wgs.lat + "&radius=100&srid=4326&newest_in_range=true&limit_results=1")));
              
    }

    public IEnumerator Panoramas(WWW req)
    {
        yield return req;

        requestOutput = JSON.Parse(req.text);

        Locaties.Add(requestOutput["_embedded"]["panoramas"]["geometry"]["coordinates"]);
       
        Debug.Log("Locatie in de lijst: " + Locaties[0]);
    }
}
