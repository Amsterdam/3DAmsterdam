using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using ConvertCoordinates;
using UnityEngine.UI;
public class RunningCoordinates : MonoBehaviour
{
    public GameObject GPScoordinaten;
    public GameObject RDcoordinaten;
    // Start is called before the first frame update
    void Start()
    {
        //GPScoordinaten.GetComponent<TextMeshProUGUI>().text = "werkt";
        //GPScoordinaten.text = "werkt";
    }

    // Update is called once per frame
    void Update()
    {
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out hit))
        {
            Vector3WGS wgscoords = CoordConvert.UnitytoWGS84(hit.point);
            string gpstekst = "WGS84: " + " lon: " + (wgscoords.lon).ToString("F7") + "  lat: " + (wgscoords.lat).ToString("F7");
            GPScoordinaten.GetComponent<Text>().text = gpstekst;
            Vector3RD RDcoords = CoordConvert.UnitytoRD(hit.point);
            string rdtekst = "RD: " + " X: " + (RDcoords.x).ToString("F") + "  Y: " + (RDcoords.y).ToString("F") + "  Z: " + (RDcoords.z).ToString("F");
            RDcoordinaten.GetComponent<Text>().text = rdtekst;
        }
    }
}
