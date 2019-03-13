using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ConvertCoordinates;
using BruTile;
using UnityEditor;

public class CameraView : MonoBehaviour
{
    public GameObject lijn1;
    public GameObject lijn2;
    public GameObject lijn3;
    public GameObject lijn4;
    public GameObject contour;
    Vector3[] Contourcoordinaten;
    Vector3[] hoeken;
    Vector3 Camlocation;
    public Extent CameraExtent;
    // Start is called before the first frame update
    void Start()
    {
        CoordConvert.ReferenceWGS84 = new Vector3WGS(4.892504f, 52.373043f, 0);
        CameraExtent = CamExtent();
    }

    // Update is called once per frame
    void Update()
    {
        CameraExtent = CamExtent();
    }
    public void setLineRenderer()
    {
        var points = new Vector3[2];
        points[0] = Camlocation;
        points[1] = hoeken[0];
        LineRenderer LR = lijn1.GetComponent<LineRenderer>();
        LR.SetPositions(points);
        points[1] = hoeken[1];
        LR = lijn2.GetComponent<LineRenderer>();
        LR.SetPositions(points);
        points[1] = hoeken[2];
        LR = lijn3.GetComponent<LineRenderer>();
        LR.SetPositions(points);
        points[1] = hoeken[3];
        LR = lijn4.GetComponent<LineRenderer>();
        LR.SetPositions(points);
        LR = contour.GetComponent<LineRenderer>();
        LR.SetPositions(Contourcoordinaten);
    }
    private Extent CamExtent()
    {
        // Zoomlevel en maximale kijkafstand bepalen op basis van de hoogte van de Camera
        Camlocation = Camera.main.transform.localPosition;
        int zoomLevel;
        zoomLevel = 13;
        float Maxafstand = 10000;
        if (Camlocation.y < 1600) { zoomLevel = 14; Maxafstand = 6400; }
        if (Camlocation.y < 800) { zoomLevel = 15; Maxafstand = 3200; }
        if (Camlocation.y < 400) { zoomLevel = 16; Maxafstand = 1600; }
        if (Camlocation.y < 200) { zoomLevel = 17; Maxafstand = 800; }
        Maxafstand = 10000;
        // bepalen welke UnityCoordinaten zichtbaar zijn in de hoeken van het scherm
        hoeken = new Vector3[4];
        hoeken[0] = GetHoekpunt("LinksBoven");
        
        hoeken[1] = GetHoekpunt("RechtsBoven");
        hoeken[2] = GetHoekpunt("RechtsOnder");
        hoeken[3] = GetHoekpunt("LinksOnder");
        
        // de maximale en minimale X- en Z-waarde van de zichtbare coordinaten bepalen
        Vector3 UnityMax = new Vector3(-9999999, -9999999, -99999999);
        Vector3 UnityMin = new Vector3(9999999, 9999999, 9999999);
        for (int i = 0; i < 4; i++)
        {
            if (hoeken[i].x < UnityMin.x) { UnityMin.x = hoeken[i].x; }
            if (hoeken[i].z < UnityMin.z) { UnityMin.z = hoeken[i].z; }
            if (hoeken[i].x > UnityMax.x) { UnityMax.x = hoeken[i].x; }
            if (hoeken[i].z > UnityMax.z) { UnityMax.z = hoeken[i].z; }
        }
        Contourcoordinaten = new Vector3[5];
        Contourcoordinaten[0] = new Vector3(UnityMin.x, hoeken[0].y+10, UnityMin.z);
        Contourcoordinaten[1] = new Vector3(UnityMax.x, hoeken[0].y+10, UnityMin.z);
        Contourcoordinaten[2] = new Vector3(UnityMax.x, hoeken[0].y+10, UnityMax.z);
        Contourcoordinaten[3] = new Vector3(UnityMin.x, hoeken[0].y+10, UnityMax.z);
        Contourcoordinaten[4] = new Vector3(UnityMin.x, hoeken[0].y+10, UnityMin.z);

        //setLineRenderer();


        //// maximale en minimale X- en Z-waarden aanpassen aan de maximale zichtafastand
        //if (UnityMin.x < Camlocation.x - Maxafstand) { UnityMin.x = Camlocation.x - Maxafstand; }
        //if (UnityMin.z < Camlocation.z - Maxafstand) { UnityMin.z = Camlocation.z - Maxafstand; }
        //if (UnityMax.x > Camlocation.x + Maxafstand) { UnityMax.x = Camlocation.x + Maxafstand; }
        //if (UnityMax.z > Camlocation.z + Maxafstand) { UnityMax.z = Camlocation.z + Maxafstand; }

        // Maximale en Minimale X- en Z-unitywaarden omrekenen naar WGS84
        Vector3WGS WGSMin = CoordConvert.UnitytoWGS84(UnityMin);
        Vector3WGS WGSMax = CoordConvert.UnitytoWGS84(UnityMax);

        // de maximale en minimale WGS84-coordinaten uitbreiden met 1 tegelafmeting
        //double tegelbreedte = tilesize / Math.Pow(2, zoomLevel); //TileSize in Degrees
        //WGSMin.lon = WGSMin.lon - tegelbreedte;
        //WGSMax.lon = WGSMax.lon + (1.5*tegelbreedte);
        //WGSMax.lat = WGSMax.lat + tegelbreedte;
        //WGSMin.lat = WGSMin.lat - tegelbreedte;

        // gebied waarbinnen data geladen moet worden
        Extent Tempextent = new Extent(WGSMin.lon, WGSMin.lat, WGSMax.lon, WGSMax.lat);
        return Tempextent;
    }

    private Vector3 GetHoekpunt(string hoek)
    {

        Vector2 Screenpos = new Vector2();
        if (hoek == "LinksBoven")
        {
            Screenpos.x = Camera.main.pixelRect.xMin;
            
            Screenpos.y = Camera.main.pixelRect.yMax;
        }
        if (hoek == "RechtsBoven")
        {
            Screenpos.x = Camera.main.pixelRect.xMax;
            Screenpos.y = Camera.main.pixelRect.yMax;
        }
        if (hoek == "LinksOnder")
        {
            Screenpos.x = Camera.main.pixelRect.xMin;
            Screenpos.y = Camera.main.pixelRect.yMin;
        }
        if (hoek == "RechtsOnder")
        {
            Screenpos.x = Camera.main.pixelRect.xMax;
            Screenpos.y = Camera.main.pixelRect.yMin;
        }
        Vector3 output = new Vector3();
        Vector3 linkerbovenhoekA;
        linkerbovenhoekA = Camera.main.ScreenToWorldPoint(new Vector3(Screenpos.x, Screenpos.y, 10));
        Vector3 linkerbovenhoekB;
        linkerbovenhoekB = Camera.main.ScreenToWorldPoint(new Vector3(Screenpos.x, Screenpos.y, 3010));
        

        Vector3 richting = linkerbovenhoekA - linkerbovenhoekB;
        float factor;
        if (richting.y < 0)
        {
            factor = 1;
        }
        else
        {
            factor = ((Camera.main.transform.localPosition.y - 40) / richting.y);
        }




        output.x = Camera.main.transform.localPosition.x - (factor * richting.x);
        output.y = Camera.main.transform.localPosition.y - (factor * richting.y);
        output.z = Camera.main.transform.localPosition.z - (factor * richting.z);
        


        return output;
    }

}
