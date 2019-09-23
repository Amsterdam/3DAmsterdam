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
     * Kijk in de API in een radius van 100 meter welke coördinaten hier het dichtstbij zijn en kies de meest dichtstbijzijnde CHECK
     * Laat het canvas zien op de plek van aanklikken CHECK
     * Geef de panorama weer van de locatie naar keuze. Einde Coroutine.
     * 
     * 
     */
    [HideInInspector]
    public Vector3WGS wgs;

    JSONNode requestOutput;
    Vector3 location;
    Vector3 fotoLocatie;
    string[] locaties;
    RaycastHit hit;
    Ray ray;
    Vector3 hitPunt;

    double lon;
    double lat;

    Texture2D urlImage;
    public GameObject worldSphere;
    public GameObject canvas;
    public Shader shader;

    public void WorldClick()
    {
        locaties = new string[2];
        StartCoroutine(ClickPhase());
    }

    public void Update()
    {
        ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if(Physics.Raycast(ray, out hit))
        {
            if (Input.GetMouseButtonDown(0) && EventSystem.current.currentSelectedGameObject == null)
            {
                hitPunt = hit.point;
            }
        }      
    }
    
    public IEnumerator ClickPhase()
    {
        yield return new WaitForSeconds(3);

        Debug.Log("Hit.point:" + hitPunt);           

        yield return new WaitUntil(() => hitPunt != null);

        wgs = CoordConvert.UnitytoWGS84(hitPunt);

        string wgsLon;
        string wgsLat;

        wgsLon = wgs.lon.ToString().Replace(',', '.');
        wgsLat = wgs.lat.ToString().Replace(',', '.');

        Debug.Log("WgsLon: " + wgsLon + " Wgslat: " + wgsLat);
        StartCoroutine(Panoramas(new WWW("https://api.data.amsterdam.nl/panorama/panoramas/?near="+wgsLon+","+wgsLat+"&radius=100&srid=4326&newest_in_range=true&limit_results=1")));
                                        
    }

    public IEnumerator Panoramas(WWW req)
    {
        yield return req;

        requestOutput = JSON.Parse(req.text);

        string apiText = requestOutput["_embedded"]["panoramas"].ToString();

        Debug.Log(apiText);

        int startIndex = 1288;
        int length = 32;
        string coordinaten = apiText.Substring(startIndex, length);

        locaties = coordinaten.Split(',');

        lon = double.Parse(locaties[0].Replace('.', ','));
        lat = double.Parse(locaties[1].Replace('.', ','));

        Debug.Log(lon + " " + lat);

        fotoLocatie = CoordConvert.WGS84toUnity(lon, lat);

        Instantiate(canvas, fotoLocatie + new Vector3(0, 200, 0), Quaternion.identity);
        canvas.SetActive(true);

        startIndex = 150;
        length = 126;

        string imageLink = apiText.Substring(startIndex, length);

        Debug.Log("ImageLink: " + imageLink);

        WWW imageTexture = new WWW(imageLink);

        yield return imageTexture;

        //urlImage = GameObject.Find("RawImage");
        //urlImage.GetComponent<RawImage>().texture = urlTexture;

        //Debug.Log("DONE!");

        urlImage = imageTexture.texture;

        Renderer sphereRend = worldSphere.GetComponent<Renderer>();
        sphereRend.material = new Material(shader);
        sphereRend.material.mainTexture = urlImage;
        sphereRend.material.renderQueue = 3001;
        

        worldSphere.transform.localScale += new Vector3(50, 50, 50);
        worldSphere.transform.position = Camera.main.transform.position;

        yield return new WaitForSeconds(1);

        //canvas.GetComponent<Renderer>().enabled = false;
    }
}
