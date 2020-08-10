using Amsterdam3D.Sun;
using ConvertCoordinates;
using System;
using UnityEngine;
using UnityEngine.UI;

public class SunSettings : MonoBehaviour
{
    private double longitude;
    private double latitude;

    [SerializeField]
    private SunVisuals sun;

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

    [SerializeField]
    private Text speedMultiplierText;
    private int speedMultiplier = 1;
    public int SpeedMultiplier
    {
        get
        {
            return speedMultiplier;
        }
        set
        {
            speedMultiplier = value;
            print("x" + speedMultiplier.ToString());
            speedMultiplierText.text = "x" + speedMultiplier.ToString();
        }
    }

    private bool useTimedSun = true;
    private bool paused = false;

    [SerializeField]
    private Button playButton;
    [SerializeField]
    private Button pauseButton;

    private DateTime dateTimeNow;

    private string previousTimeString = "";

    public void Start()
    {
        ResetTimeToNow();

        //Get the gps coordinates for our world centre
        var coordinates = CoordConvert.UnitytoWGS84(Vector3.zero);
        longitude = coordinates.lon;
        latitude = coordinates.lat;

        sunDragWheel.changedDirection += SunPositionChangedFromWheel;

        //Receive changes from our input fields
        timeInput.addedOffset += ChangedTime;
        dayInput.addedOffset += ChangedDay;
        monthInput.addedOffset += ChangedMonth;
        yearInput.addedOffset += ChangedYear;
    }


    private void ChangedTime(int withOffset)
    {
        if (Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift))
        {
            dateTimeNow = dateTimeNow.AddMinutes(withOffset);
        }
        else
        {
            dateTimeNow = dateTimeNow.AddHours(withOffset);
        }
        UpdateInputs();
    }
    private void ChangedDay(int withOffset)
    {
        dateTimeNow = dateTimeNow.AddDays(withOffset);
        UpdateInputs();
    }
    private void ChangedMonth(int withOffset)
    {
        dateTimeNow = dateTimeNow.AddMonths(withOffset);
        UpdateInputs();
    }
    private void ChangedYear(int withOffset)
    {
        dateTimeNow = dateTimeNow.AddYears(withOffset);
        UpdateInputs();
    }

    private void UpdateInputs()
    {
        timeInput.SetInputText(dateTimeNow.ToString("HH:mm"));
        dayInput.SetInputText(dateTimeNow.Day.ToString());
        monthInput.SetInputText(dateTimeNow.Month.ToString());
        yearInput.SetInputText(dateTimeNow.Year.ToString());

        sunDragWheel.SetUpDirection(new Vector3(sun.transform.forward.x, sun.transform.forward.y, 0.0f));
    }

    private void LateUpdate()
    {
        if (useTimedSun)
        {
            //Simply tick time forward
            if (!paused)
            {
                dateTimeNow = dateTimeNow.AddSeconds(Time.deltaTime * SpeedMultiplier);
            }

            if (dateTimeNow.ToString("HH:mm") != previousTimeString)
            {
                previousTimeString = dateTimeNow.ToString("HH:mm");
                UpdateInputs();
            }
            SetSunPosition();
        }
    }

    private void SunPositionChangedFromWheel(float rotation){
        //Convert flat rotation to expected time/sun position
        SetSunPosition();
    }
    private void SetSunPosition()
    {
        var angles = new Vector3();
        double alt;
        double azi;
        SunPosition.CalculateSunPosition(dateTimeNow, (double)latitude, (double)longitude, out azi, out alt);
        angles.x = (float)alt * Mathf.Rad2Deg;
        angles.y = (float)azi * Mathf.Rad2Deg;     

        sun.UpdateVisuals(angles);
    }

    public void ToggleTimedSun(bool timedSun)
    {
        useTimedSun = timedSun;
        ResetTimeToNow();
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
        SpeedMultiplier = 1;
        EnablePausePlayButtons();
    }

    private void EnablePausePlayButtons()
    {
        playButton.gameObject.SetActive(paused);
        pauseButton.gameObject.SetActive(!paused);
    }

    public void ResetTimeToNow()
    {
        dateTimeNow = DateTime.Now;
        SpeedMultiplier = 1;
        paused = false;

        UpdateInputs();
        EnablePausePlayButtons();
    }

}
