using Amsterdam3D.Sun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class SunSettings : MonoBehaviour
{
    [SerializeField]
    private DirectionalLight sunDirectionalLight;

    [SerializeField]
    private SunDragWheel sunDragWheel;

    [SerializeField]
    private LimitedNumericInput timeInput;

    [SerializeField]
    private LimitedNumericInput dayInput;
    [SerializeField]
    private LimitedNumericInput monthInput;
    [SerializeField]
    private LimitedNumericInput yearInput;

    private Text speedMultiplierText;
    private int speedMultiplier = 1;

    private bool useTimedSun = false;
    private bool paused = false;

    [SerializeField]
    private Image coverTimeSettings;

    private DateTime dateTimeNow;

    public int SpeedMultiplier {
        get {
            return speedMultiplier;
        }
        set {
            speedMultiplier = value;
            speedMultiplierText.text = speedMultiplier.ToString();
        }
    }

    public void Start()
    {
        sunDragWheel.changedDirection += SunPositionChangedFromOutside;
    }

    private void Update()
    {
        if (useTimedSun && !paused)
        {
            dateTimeNow.AddMinutes(Time.deltaTime * SpeedMultiplier);

            timeInput.Value = dateTimeNow.Hour*60 + dateTimeNow.Minute; //This value is counted in 'minute of the day'

            dayInput.Value = dateTimeNow.Day;
            monthInput.Value = dateTimeNow.Month;
            yearInput.Value = dateTimeNow.Year;
        }
    }

    private void SunPositionChangedFromOutside(float rotation){
        //Convert flat rotation to expected time/sun position
        Debug.Log("Rotate sun..");
    }

    public void ToggleTimedSun(bool timedSun)
    {
        useTimedSun = timedSun;
        ResetTimeToNow();
        coverTimeSettings.gameObject.SetActive(!useTimedSun);
    }

    public void FastForward()
    {
        SpeedMultiplier *= 10;
        if(SpeedMultiplier > 1000) SpeedMultiplier = 1000;
    }
    public void FastBackward()
    {
        SpeedMultiplier /= 10;
        if (SpeedMultiplier < 1) SpeedMultiplier = 1;
    }
    public void TogglePausePlay()
    {
        paused = !paused;
    }
    
    public void ResetTimeToNow()
    {
        dateTimeNow = DateTime.Now;
    }

}
