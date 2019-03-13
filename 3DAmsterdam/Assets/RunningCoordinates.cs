using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ConvertCoordinates;
using System;



public class RunningCoordinates : MonoBehaviour
{
    public GameObject Lat;
    public GameObject lon;
    public GameObject wgsAlt;
    public GameObject RDX;
    public GameObject RDY;
    public GameObject RDZ;


    // Start is called before the first frame update
    void Start()
    {
        Lat.GetComponent<Text>().text = "hallo";
    }

    // Update is called once per frame
    void Update()
    {
        RaycastHit hit;
        int layerMaskBuildings = 1 << 9;
        int LayermaskTerrain = 1 << 8;
        int layerMask = layerMaskBuildings + LayermaskTerrain;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out hit,Mathf.Infinity,layerMask))
        {
            Vector3 position = hit.point;
            Vector3WGS gps = CoordConvert.UnitytoWGS84(position);
            Lat.GetComponent<Text>().text = "Lat:" + Math.Round(gps.lat,7);
            lon.GetComponent<Text>().text = "Lon:" + Math.Round(gps.lon, 7);
            wgsAlt.GetComponent<Text>().text = "h:" + Math.Round(position.y,2);
            Vector3RD rd = CoordConvert.UnitytoRD(position);
            RDX.GetComponent<Text>().text = "X:" + Math.Round(rd.x, 2);
            RDY.GetComponent<Text>().text = "Y:" + Math.Round(rd.y, 2);
            RDZ.GetComponent<Text>().text = "h:" + Math.Round(rd.z, 2);

            //debug rd-rotation
            //double rotatie = CoordConvert.RDRotation(rd);
            //RDZ.GetComponent<Text>().text = "h:" + Math.Round(rotatie, 5);

        }
    }
}
