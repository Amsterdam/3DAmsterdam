using ConvertCoordinates;
using Netherlands3D.LayerSystem;
using Netherlands3D.Sun;
using Netherlands3D.T3D.Uitbouw;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;



public class HandleButtonsT3D : MonoBehaviour
{
    public Button ButtonOmgeving;
    public Button ButtonZon;
    public Button ButtonMinusHour;
    public Button ButtonAddHour;
    public Button ButtonZoomIn;
    public Button ButtonZoomOut;
    public Button ButtonVolgende;

    public Toggle TogglePositieIsGoed;
    public Toggle TogglePositieIsNietGoed;

    public Layer BuildingsLayer;
    public Layer TerrainLayer;
    public GameObject Zonnepaneel;
    public Texture2D RotateIcon;

    public DropUp MaandenDropup;

    public GameObject Step1U;
    public GameObject Step2aU;
    public GameObject Step2bU;

    public GameObject Step1D;
    public GameObject Step2D;

    //Sun related
    public Text DagText;
    public Text MaandText;
    public Text TijdText;

    private DateTime dateTimeNow;
    private double longitude;
    private double latitude;

    bool? positieGoed = null;

    List<string> months = new List<string>()
    {
        "JAN","FEB","MRT","APR","MEI","JUN","JUL","AUG","SEP","OKT","NOV","DEC"
    };


    void Start()
    {
        MetadataLoader.Instance.BuildingMetaDataLoaded += BuildingMetaDataLoaded;

        ButtonOmgeving.onClick.AddListener(ToggleBuildings);
        ButtonZon.onClick.AddListener(ToggleZonnepaneel);
        ButtonMinusHour.onClick.AddListener(MinusHour);
        ButtonAddHour.onClick.AddListener(AddHour);
        ButtonZoomIn.onClick.AddListener(ZoomIn);
        ButtonZoomOut.onClick.AddListener(ZoomOut);
        ButtonVolgende.onClick.AddListener(Volgende);

        TogglePositieIsGoed.onValueChanged.AddListener((value) =>
        {
            if (value == true)
            {
                TogglePositieIsNietGoed.isOn = false;
                positieGoed = true;
            }
            else positieGoed = null;
            
            
        });

        TogglePositieIsNietGoed.onValueChanged.AddListener((value) =>
        {
            if (value == true)
            {
                TogglePositieIsGoed.isOn = false;
                positieGoed = false;                
            }
            else positieGoed = null;
        });

        //Cursor.SetCursor(RotateIcon, Vector2.zero, CursorMode.Auto);

        //Sun related
        dateTimeNow = DateTime.Now;        
        var coordinates = CoordConvert.UnitytoWGS84(Vector3.zero);
        longitude = coordinates.lon;
        latitude = coordinates.lat;

        UpdateTijd();

        MaandenDropup.SetItems(months, dateTimeNow.Month-1, SetMonth);
    }

    private void BuildingMetaDataLoaded(object source, ObjectDataEventArgs args)
    {
        if (MetadataLoader.UploadedModel)
            Step1U.SetActive(true);
        else
            Step1D.SetActive(true);
    }

    void SetMonth(int month)
    {
        dateTimeNow = new DateTime(dateTimeNow.Year, month+1, dateTimeNow.Day, dateTimeNow.Hour, dateTimeNow.Minute, 0);
        UpdateSun();
        UpdateTijd();      
    }

    void ToggleBuildings()
    {        
        BuildingsLayer.isEnabled = !BuildingsLayer.isEnabled;
        TerrainLayer.isEnabled = !TerrainLayer.isEnabled;
    }

    #region Sun related
    void ToggleZonnepaneel()
    {
        Zonnepaneel.SetActive(!Zonnepaneel.active);
    }

    void AddHour()
    {
        dateTimeNow = new DateTime(dateTimeNow.Year, dateTimeNow.Month, dateTimeNow.Day, dateTimeNow.Hour, 0, 0);
        dateTimeNow =  dateTimeNow.AddHours(1);
        
        UpdateTijd();
        UpdateSun();
        Debug.Log(dateTimeNow);
    }

    void MinusHour()
    {
        dateTimeNow = new DateTime(dateTimeNow.Year, dateTimeNow.Month, dateTimeNow.Day, dateTimeNow.Hour, 0, 0);
        dateTimeNow = dateTimeNow.AddHours(-1);
        UpdateTijd();
        UpdateSun();
        Debug.Log(dateTimeNow);
    }
    
    void ZoomIn()
    {
        Debug.Log("zoomin");
    }

    void ZoomOut()
    {
        Debug.Log("zoomout");
    }

    void Volgende()
    {
        if (positieGoed == null) return;

        if (positieGoed == true)
        {
            Step1U.SetActive(false);
            Step2bU.SetActive(true);
        }
        else
        {
            Step1U.SetActive(false);
            Step2aU.SetActive(true);
        }

        var volgendeLabel = ButtonVolgende.GetComponentInChildren<Text>();
        volgendeLabel.text = "Opslaan";
    }

    private void UpdateTijd()
    {
        DagText.text = $"{dateTimeNow.Day}";
        MaandText.text = GetMonthString(dateTimeNow.Month-1);        
        TijdText.text = $"{ dateTimeNow:HH:mm}";
    }



    void UpdateSun()
    {
        var angles = new Vector3();
        double alt;
        double azi;
        SunPosition.CalculateSunPosition(dateTimeNow, (double)latitude, (double)longitude, out azi, out alt);
        angles.x = (float)alt * Mathf.Rad2Deg;
        angles.y = (float)azi * Mathf.Rad2Deg;

        EnviromentSettings.SetSunAngle(angles);
    }

    string GetMonthString(int monthIndex)
    {
        return months[monthIndex];
    }



    #endregion

}
