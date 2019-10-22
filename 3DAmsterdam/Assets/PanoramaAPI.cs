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
    GameObject thumbnail;
    GameObject instantiated;

    public beweging cameraManager;
    public GameObject exitText;
    public GameObject miniMap;
    public Toggle toggle;
    List<GameObject> panoramaInstantiated;
    private Quaternion Camrotatie;

    bool panoramaWatch = false;

    public void WorldClick()
    {
        
        panoramaInstantiated = new List<GameObject>();
        hitPunt = new Vector3();
        panoramaWatch = false;
        locaties = new string[2];
        toggle.SetIsOnWithoutNotify(false);
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

        if (panoramaWatch)
        {
            
            if (Physics.Raycast(ray, out hit))
            {
                //if (hit.transform.tag=="panormaInstantiated")
                if (Vector3.Distance(hit.point, fotoLocatie) <= 200f)
                {
                    if (Input.GetMouseButtonDown(0))
                    {
                        Camrotatie = Camera.main.transform.rotation;
                        SpawnPanorama();
                        cameraManager.canMove = false;
                        cameraManager.zoomSpeed = 0f;
                        exitText.SetActive(true);
                        miniMap.SetActive(false);
                        toggle.isOn = false;
                        
                    }
                }
                else
                {
                    Destroy(instantiated);
                }
            }

            if (Input.GetKey(KeyCode.Escape))
            {
               

                cameraManager.canMove = true;
                cameraManager.zoomSpeed = 0.5f;
                exitText.SetActive(false);
                worldSphere.transform.position = new Vector3(0, -2000, 0);
                miniMap.SetActive(true);
                panoramaWatch = false;
                Destroy(instantiated);
                Camera.main.transform.rotation = Camrotatie ;
            }
        }
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

        string apiText = requestOutput["_embedded"]["panoramas"].ToString();

        int startIndex = 1288;
        int length = 32;
        string coordinaten = apiText.Substring(startIndex, length);

        locaties = coordinaten.Split(',');

        lon = double.Parse(locaties[0],System.Globalization.CultureInfo.InvariantCulture);
        lat = double.Parse(locaties[1], System.Globalization.CultureInfo.InvariantCulture); ;

        fotoLocatie = CoordConvert.WGS84toUnity(lon, lat);

        instantiated = Instantiate(canvas, fotoLocatie + new Vector3(0, 150, 0), Quaternion.identity);
        instantiated.tag = "panoramaInstantiated";

        canvas.SetActive(true);

        startIndex = 150;
        length = 126;

        string imageLink = apiText.Substring(startIndex, length);

        WWW imageTexture = new WWW(imageLink);

        yield return imageTexture;

        urlImage = imageTexture.texture;

        Renderer sphereRend = worldSphere.GetComponent<Renderer>();
        sphereRend.material = new Material(shader);
        sphereRend.material.mainTexture = urlImage;
        sphereRend.material.renderQueue = 3001;

        panoramaWatch = true;

        StopAllCoroutines();
    }

    public void SpawnPanorama()
    {
        worldSphere.transform.localScale += new Vector3(50, 50, 50);
        worldSphere.transform.position = Camera.main.transform.position;
    }
}
