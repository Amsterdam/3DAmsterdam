using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using ConvertCoordinates;
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
            string gpstekst = "WGS84: " + " X: " + ((int)wgscoords.lon).ToString() + "  Y: " + ((int)wgscoords.lat).ToString();
            GPScoordinaten.GetComponent<TextMeshProUGUI>().text = gpstekst;
            Vector3RD RDcoords = CoordConvert.UnitytoRD(hit.point);
            string rdtekst = "RD: " + " X: " + ((int)RDcoords.x).ToString() + "  Y: " + ((int)RDcoords.y).ToString() + "  Z: " + ((int)RDcoords.z).ToString();
            RDcoordinaten.GetComponent<TextMeshProUGUI>().text = rdtekst;
        }
    }
}
