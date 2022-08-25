﻿using ConvertCoordinates;
using Netherlands3D.Cameras;
using Netherlands3D.Interface;
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
    public Button ButtonToggleRotateFirstperson;

    public GameObject BuildingsLayer;    
    public GameObject Zonnepaneel;

    public DropUp MaandenDropup;

    //Sun related
    public Text DagText;
    public Text MaandText;
    public Text TijdText;

    private DateTime dateTimeNow;
    private double longitude;
    private double latitude;

    private float zoomSpeed = 150;

    List<string> months = new List<string>()
    {
        "JAN","FEB","MRT","APR","MEI","JUN","JUL","AUG","SEP","OKT","NOV","DEC"
    };


    void Start()
    {
        ButtonOmgeving.onClick.AddListener(ToggleBuildings);
        ButtonZon.onClick.AddListener(ToggleZonnepaneel);
        ButtonMinusHour.onClick.AddListener(MinusHour);
        ButtonAddHour.onClick.AddListener(AddHour);
        ButtonZoomIn.onClick.AddListener(ZoomIn);
        ButtonZoomOut.onClick.AddListener(ZoomOut);
        ButtonToggleRotateFirstperson.onClick.AddListener(ToggleRotateFirstperson);

        //Cursor.SetCursor(RotateIcon, Vector2.zero, CursorMode.Auto);

        //Sun related
        dateTimeNow = DateTime.Now;
        var coordinates = CoordConvert.UnitytoWGS84(Vector3.zero);
        longitude = coordinates.lon;
        latitude = coordinates.lat;

        UpdateTijd();

        MaandenDropup.SetItems(months, dateTimeNow.Month - 1, SetMonth);
    }

    void SetMonth(int month)
    {
        dateTimeNow = new DateTime(dateTimeNow.Year, month + 1, dateTimeNow.Day, dateTimeNow.Hour, dateTimeNow.Minute, 0);
        UpdateSun();
        UpdateTijd();
    }

    void ToggleBuildings()
    {
        //BuildingsLayer.isEnabled = !BuildingsLayer.isEnabled;
        BuildingsLayer.SetActive(!BuildingsLayer.activeSelf);
        
        RestrictionChecker.ActivePerceel.SetPerceelActive(!BuildingsLayer.activeSelf);
    }

    #region Sun related
    void ToggleZonnepaneel()
    {
        Zonnepaneel.SetActive(!Zonnepaneel.activeInHierarchy);
    }

    void AddHour()
    {
        dateTimeNow = new DateTime(dateTimeNow.Year, dateTimeNow.Month, dateTimeNow.Day, dateTimeNow.Hour, 0, 0);
        dateTimeNow = dateTimeNow.AddHours(1);

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
        if (ServiceLocator.GetService<CameraModeChanger>().CurrentMode == CameraMode.TopDown)
            ServiceLocator.GetService<CameraModeChanger>().ActiveCamera.orthographicSize -= 5;
        else
            ServiceLocator.GetService<CameraModeChanger>().ActiveCamera.transform.position = ServiceLocator.GetService<CameraModeChanger>().ActiveCamera.transform.position + (Time.deltaTime * ServiceLocator.GetService<CameraModeChanger>().ActiveCamera.transform.forward * zoomSpeed);
    }

    void ZoomOut()
    {
        if (ServiceLocator.GetService<CameraModeChanger>().CurrentMode == CameraMode.TopDown)
            ServiceLocator.GetService<CameraModeChanger>().ActiveCamera.orthographicSize += 5;
        else
            ServiceLocator.GetService<CameraModeChanger>().ActiveCamera.transform.position = ServiceLocator.GetService<CameraModeChanger>().ActiveCamera.transform.position - (Time.deltaTime * ServiceLocator.GetService<CameraModeChanger>().ActiveCamera.transform.forward * zoomSpeed);
    }

    void ToggleRotateFirstperson()
    {
        var newMode = ServiceLocator.GetService<CameraModeChanger>().CurrentMode == CameraMode.GodView ? CameraMode.StreetView : CameraMode.GodView;
        ServiceLocator.GetService<CameraModeChanger>().SetCameraMode(newMode);
        var tooltiptrigger = ButtonToggleRotateFirstperson.GetComponent<TooltipTrigger>();

        tooltiptrigger.TooltipText = newMode == CameraMode.StreetView ? "Roteren" : "Lopen";
    }

    private void UpdateTijd()
    {
        DagText.text = $"{dateTimeNow.Day}";
        MaandText.text = GetMonthString(dateTimeNow.Month - 1);
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

    //called by the UI button events in the inspector
    public void AllowUitbouwMovement(bool enable)
    {
        RestrictionChecker.ActiveUitbouw.GetComponent<UitbouwMovement>().SetAllowMovement(enable);
        RestrictionChecker.ActiveUitbouw.GetComponent<UitbouwRotation>().SetAllowRotation(enable);
    }

    #endregion
}