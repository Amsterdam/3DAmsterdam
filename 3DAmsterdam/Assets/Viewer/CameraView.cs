
using UnityEngine;
using ConvertCoordinates;
using BruTile;

/// <summary>
/// bepaalt elke frame welk deel van amsterdam in beeld is en stelt in als public Brutile.Extent in WGS84 beschikbaar
/// </summary>
public class CameraView : MonoBehaviour
{
    //TekenZichtlijnen is voor debugging
    //4 lijnen worden getekend van de camera naar de hoekpunten van het zichtveld op maaiveldniveau
    //1 contour wordt getekend tussen de 4 lijen op maaiveldniveau
    public bool TekenZichtlijnen = false;
    public GameObject lijn1;    //gameobject met linerenderer met 2 points
    public GameObject lijn2;    //gameobject met linerenderer met 2 points
    public GameObject lijn3;    //gameobject met linerenderer met 2 points
    public GameObject lijn4;    //gameobject met linerenderer met 2 points
    public GameObject contour;  //gameobject met linerenderer met 4 points

    Vector3[] Contourcoordinaten;
    Vector3[] hoeken;
    Vector3 Camlocation;
    public Extent CameraExtent;
    // Start is called before the first frame update
    void Start()
    {
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
        // locatie van de camera bepalen
        Camlocation = transform.localPosition;
        
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

        if (TekenZichtlijnen)
        {
            Contourcoordinaten = new Vector3[5];
            Contourcoordinaten[0] = new Vector3(UnityMin.x, hoeken[0].y+10, UnityMin.z);
            Contourcoordinaten[1] = new Vector3(UnityMax.x, hoeken[0].y+10, UnityMin.z);
            Contourcoordinaten[2] = new Vector3(UnityMax.x, hoeken[0].y+10, UnityMax.z);
            Contourcoordinaten[3] = new Vector3(UnityMin.x, hoeken[0].y+10, UnityMax.z);
            Contourcoordinaten[4] = new Vector3(UnityMin.x, hoeken[0].y+10, UnityMin.z);

            setLineRenderer();
        }
        

        // Maximale en Minimale X- en Z-unitywaarden omrekenen naar WGS84
        Vector3WGS WGSMin = CoordConvert.UnitytoWGS84(UnityMin);
        Vector3WGS WGSMax = CoordConvert.UnitytoWGS84(UnityMax);

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


        Vector3 linkerbovenhoekA; //coordinaat op 10 eenheden van het scherm
        linkerbovenhoekA = Camera.main.ScreenToWorldPoint(new Vector3(Screenpos.x, Screenpos.y, 10));
        Vector3 linkerbovenhoekB;//coordinaat op 3010 eenheden van het scherm
        linkerbovenhoekB = Camera.main.ScreenToWorldPoint(new Vector3(Screenpos.x, Screenpos.y, 3010));
        
        // de richting van de lijn bepalen
        Vector3 richting = linkerbovenhoekA - linkerbovenhoekB;
        float factor; //factor waarmee de Richtingvector vermenigvuldigd moet worden om op het maaiveld te stoppen
        if (richting.y < 260) //wanneer de Richtingvector omhooggaat deze factor op 1 instellen
        {
            factor = 1;
        }
        else
        {
            factor = ((Camera.main.transform.localPosition.y - 40) / richting.y); //factor bepalen t.o.v. maaiveld (aanname maaiveld op 0 NAP = ca 40 Unityeenheden in Y-richting)
        }

        // uiteindelijke X, Y, en Z locatie bepalen waar de zichtlijn eindigt.
        output.x = Camera.main.transform.localPosition.x - (factor * richting.x);
        output.y = Camera.main.transform.localPosition.y - (factor * richting.y);
        output.z = Camera.main.transform.localPosition.z - (factor * richting.z);

        return output;
    }

}
