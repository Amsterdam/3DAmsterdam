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
    public GameObject coordinaatPanel;
    GameObject thumbnail;
    GameObject instantiated;
    public GameObject exitPanorama;

    public beweging cameraManager;
    public GameObject miniMap;
    public Toggle toggle;

    bool panoramaWatch = false;

    public void WorldClick()
    {
        hitPunt = new Vector3();
        panoramaWatch = false;
        locaties = new string[2];
        StartCoroutine(ClickPhase());
    }

    public void Update()
    {
        ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out hit))
        {
            if (Input.GetMouseButtonDown(0) && EventSystem.current.currentSelectedGameObject == null)
            {
                hitPunt = hit.point;
            }
        }
    }
    
    public void ExitPanorama()
    {
        StopAllCoroutines();

        toggle.interactable = true;
        cameraManager.canMove = true;
        cameraManager.zoomSpeed = 0.5f;
        cameraManager.dragFactor = 2f;
        worldSphere.transform.position = new Vector3(0, -2000, 0);
        miniMap.SetActive(true);
        panoramaWatch = false;
        coordinaatPanel.SetActive(true);
        exitPanorama.SetActive(false);
    }

    public IEnumerator ClickPhase()
    {
        yield return new WaitUntil(() => hitPunt != new Vector3(0,0,0));

        wgs = CoordConvert.UnitytoWGS84(hitPunt);

        string wgsLon;
        string wgsLat;

        wgsLon = wgs.lon.ToString().Replace(',', '.');
        wgsLat = wgs.lat.ToString().Replace(',', '.');

        StartCoroutine(Panoramas(new WWW("https://api.data.amsterdam.nl/panorama/panoramas/?near="+wgsLon+","+wgsLat+"&radius=100&srid=4326&newest_in_range=true&limit_results=1")));
                                        
    }

    public IEnumerator Panoramas(WWW req)
    {
        yield return req;

        requestOutput = JSON.Parse(req.text);

        if (panoramaWatch == false)
        {
            string apiText = requestOutput["_embedded"]["panoramas"].ToString();

            int startIndex = 1288;
            int length = 32;
            string coordinaten = apiText.Substring(startIndex, length);

            locaties = coordinaten.Split(',');

            lon = double.Parse(locaties[0], System.Globalization.CultureInfo.InvariantCulture);
            lat = double.Parse(locaties[1], System.Globalization.CultureInfo.InvariantCulture); ;

            fotoLocatie = CoordConvert.WGS84toUnity(lon, lat);

            canvas.SetActive(true);

            startIndex = 150;
            length = 126;

            string imageLink = apiText.Substring(startIndex, length);

            WWW imageTexture = new WWW(imageLink);

            yield return imageTexture;

            instantiated = Instantiate(canvas, fotoLocatie + new Vector3(0, 100, 0), Quaternion.identity);
            instantiated.tag = "panoramaInstantiated";

            urlImage = imageTexture.texture;

            Renderer sphereRend = worldSphere.GetComponent<Renderer>();
            sphereRend.material = new Material(shader);
            sphereRend.material.mainTexture = urlImage;
            sphereRend.material.renderQueue = 3001;

            panoramaWatch = true;

            if (panoramaWatch)
            {
                toggle.isOn = false;

                  if (Input.GetMouseButtonDown(0))
                  {
                        GameObject[] instants = GameObject.FindGameObjectsWithTag("panoramaInstantiated");

                        foreach (GameObject x in instants)
                        {
                            DestroyImmediate(x);
                        }

                        toggle.interactable = false;
                        exitPanorama.SetActive(true);
                        SpawnPanorama();
                        cameraManager.canMove = false;
                        cameraManager.zoomSpeed = 0f;
                        cameraManager.dragFactor = 0f;
                        miniMap.SetActive(false);
                        coordinaatPanel.SetActive(false);
                  }
            }
        }

        StopAllCoroutines();
    }

    public void SpawnPanorama()
    {
        StopAllCoroutines();
        worldSphere.transform.localScale += new Vector3(50, 50, 50);
        worldSphere.transform.position = Camera.main.transform.position;
    }
}
