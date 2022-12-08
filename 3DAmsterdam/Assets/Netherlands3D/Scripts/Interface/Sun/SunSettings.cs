using Netherlands3D.Sun;
using Netherlands3D.Core;
using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using Netherlands3D.Interface;
using TMPro;

public class SunSettings : MonoBehaviour
{
    private double longitude;
    private double latitude;

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
    private int maxYearInput = 2100;

    [SerializeField]
    private TextMeshProUGUI speedMultiplierText;
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
    [SerializeField]
    private int maxSpeedMultiply = 10000;

    private bool useTimedSun = true;
    private bool paused = false;

    [SerializeField]
    private Button playButton;
    [SerializeField]
    private Button pauseButton;

    private DateTime dateTimeNow;

    private string previousTimeString = "";
    private bool usingSharedSceneTime = false;

    public void Start()
    {
        SetGPSCoordinates();

        //Receive changes from wheel input
        sunDragWheel.deltaTurn += SunPositionChangedFromWheel;

        //Receive changes from our input fields
        timeInput.addedOffset += ChangedTime;
        dayInput.addedOffset += ChangedDay;
        monthInput.addedOffset += ChangedMonth;
        yearInput.addedOffset += ChangedYear;

        //Check if we loaded a shared scene, and got our time from there.
        if (!usingSharedSceneTime || dateTimeNow == DateTime.MinValue) 
            ResetTimeToNow();
    }

    private void SetGPSCoordinates()
    {
        //Get the GPS coordinates for our world centre
        var coordinates = CoordConvert.UnitytoWGS84(Vector3.zero);
        longitude = coordinates.lon;
        latitude = coordinates.lat;
    }

    private void ChangedTime(int withOffset)
    {
        if (Selector.doingMultiselect)
        {
            dateTimeNow = dateTimeNow.AddMinutes(withOffset);
        }
        else
        {
            dateTimeNow = dateTimeNow.AddHours(withOffset);
        }
        UpdateNumericInputs();
    }

    public string GetDateTimeAsString()
    {
        return dateTimeNow.ToString();
    }
    public void SetDateTimeFromString(string dateTimeString)
    {
        dateTimeNow = DateTime.Now;
        DateTime.TryParse(dateTimeString, out dateTimeNow);
        usingSharedSceneTime = true;

        //Check if the shared datetime was set (otherwise, it will be the min datetime value)
        if (dateTimeNow != DateTime.MinValue)
        {
            SetGPSCoordinates();
            UpdateNumericInputs();
            ChangeSunPosition();
        }
    }

    /// <summary>
    /// Set the time of the day directly as string
    /// </summary>
    /// <param name="input">Time of the day as string, for example "13:37"</param>
    public void SetTime(string input)
    {
        string[] time = input.Split(':');
        if (time.Length > 1 && int.TryParse(time[0], out int hour) && int.TryParse(time[1], out int minute))
        {
            print("Parsed time: " + hour + ":" + minute);
            hour = Mathf.Clamp(hour, 0, 23);
            minute = Mathf.Clamp(minute, 0, 59);
            dateTimeNow = new DateTime(dateTimeNow.Year, dateTimeNow.Month, dateTimeNow.Day, hour, minute, dateTimeNow.Second);
        }
        ChangeSunPosition();
    }

    /// <summary>
    /// Set the day of the month nr directly based on a string input
    /// </summary>
    /// <param name="input">Day of the month input number as string, for example "1" for the first day of the month</param>
    public void SetDay(string input)
    {
        if(int.TryParse(input, out int dayNumber))
        {
            print("Parsed day: " + dayNumber);
            dayNumber = Mathf.Clamp(dayNumber, 1, DateTime.DaysInMonth(dateTimeNow.Year, dateTimeNow.Month));
            dateTimeNow = new DateTime(dateTimeNow.Year, dateTimeNow.Month, dayNumber, dateTimeNow.Hour, dateTimeNow.Minute, dateTimeNow.Second);
        }
        ChangeSunPosition();
    }
    /// <summary>
    /// Set the month nr directly based on a string input
    /// </summary>
    /// <param name="input">Month input number as string, for example "8" for august</param>
    public void SetMonth(string input)
    {
        print("End edit. " + input);
        if (int.TryParse(input, out int monthNumber))
        {
            monthNumber = Mathf.Clamp(monthNumber, 1, 12);
            dateTimeNow = new DateTime(dateTimeNow.Year, monthNumber, dateTimeNow.Day, dateTimeNow.Hour, dateTimeNow.Minute, dateTimeNow.Second);
        }
        ChangeSunPosition();
    }
    /// <summary>
    /// Set the year nr directly based on a string input
    /// </summary>
    /// <param name="input">Year input number as string, for example "2020"</param>
    public void SetYear(string input)
    {
        if (int.TryParse(input, out int yearNumber))
        {
            yearNumber = Mathf.Clamp(yearNumber, 0, maxYearInput);
            dateTimeNow = new DateTime(yearNumber, dateTimeNow.Month, dateTimeNow.Day, dateTimeNow.Hour, dateTimeNow.Minute, dateTimeNow.Second);
        }
        ChangeSunPosition();
    }

    private void ChangedDay(int withOffset)
    {
        dateTimeNow = dateTimeNow.AddDays(withOffset);
        ChangeSunPosition();
    }
    private void ChangedMonth(int withOffset)
    {
        dateTimeNow = dateTimeNow.AddMonths(withOffset);
        ChangeSunPosition();
    }
    private void ChangedYear(int withOffset)
    {
        dateTimeNow = dateTimeNow.AddYears(withOffset);
        if (dateTimeNow.Year > maxYearInput || dateTimeNow.Year < 0)
            dateTimeNow = new DateTime(Mathf.Clamp(dateTimeNow.Year,0, maxYearInput), dateTimeNow.Month, dateTimeNow.Day, dateTimeNow.Hour, dateTimeNow.Minute, dateTimeNow.Second);

        ChangeSunPosition();
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
                UpdateNumericInputs();
            }
            ChangeSunPosition();
        }
    }

    private void SunPositionChangedFromWheel(float delta){
        //Convert flat rotation to expected time/sun position
        dateTimeNow = dateTimeNow.AddHours(delta*0.01f);

        ChangeSunPosition();
    }
    private void ChangeSunPosition()
    {
        var angles = new Vector3();
        double alt;
        double azi;
        SunPosition.CalculateSunPosition(dateTimeNow, (double)latitude, (double)longitude, out azi, out alt);
        angles.x = (float)alt * Mathf.Rad2Deg;
        angles.y = (float)azi * Mathf.Rad2Deg;

        EnviromentSettings.SetSunAngle(angles);

        UpdateWheelAccordingToSun();
        UpdateNumericInputs();
    }

    /// <summary>
    /// Changes the text values of our input fields to nicely readable date/time values, and rotate our wheel according to the sun
    /// </summary>
    private void UpdateNumericInputs()
    {
        timeInput.SetInputText(dateTimeNow.ToString("HH:mm"));
        dayInput.SetInputText(dateTimeNow.Day.ToString());
        monthInput.SetInputText(dateTimeNow.Month.ToString());
        yearInput.SetInputText(dateTimeNow.Year.ToString());
    }

    private void UpdateWheelAccordingToSun()
    {
        sunDragWheel.SetUpDirection(new Vector3(EnviromentSettings.sun.transform.forward.x, EnviromentSettings.sun.transform.forward.y, 0.0f));
    }

    public void ToggleTimedSun(bool timedSun)
    {
        useTimedSun = timedSun;
        ResetTimeToNow();
    }

    public void FastForward()
    {
        SpeedMultiplier *= 10;
        if(SpeedMultiplier > maxSpeedMultiply) SpeedMultiplier = maxSpeedMultiply;
    }
    public void FastBackward()
    {
        SpeedMultiplier /= 10;
        if(SpeedMultiplier == 0) SpeedMultiplier = 1;
        if (SpeedMultiplier < -maxSpeedMultiply) SpeedMultiplier = -maxSpeedMultiply;
    }
    public void TogglePausePlay()
    {
        paused = !paused;
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
        
        ChangeSunPosition();
    }

}
