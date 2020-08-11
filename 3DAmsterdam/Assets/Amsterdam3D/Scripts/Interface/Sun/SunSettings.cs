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

    public void Start()
    {
        //Get the gps coordinates for our world centre
        var coordinates = CoordConvert.UnitytoWGS84(Vector3.zero);
        longitude = coordinates.lon;
        latitude = coordinates.lat;

        ResetTimeToNow();

        sunDragWheel.deltaTurn += SunPositionChangedFromWheel;

        //Receive changes from our input fields
        timeInput.addedOffset += ChangedTime;
        dayInput.addedOffset += ChangedDay;
        monthInput.addedOffset += ChangedMonth;
        yearInput.addedOffset += ChangedYear;
    }

    private void ChangedTime(int withOffset)
    {
        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
        {
            dateTimeNow = dateTimeNow.AddMinutes(withOffset);
        }
        else
        {
            dateTimeNow = dateTimeNow.AddHours(withOffset);
        }
        UpdateNumeralInputs();
    }

    /// <summary>
    /// Set the time of the day directly as string
    /// </summary>
    /// <param name="input">Time of the day as string, for example "13:37"</param>
    public void SetTime(string input)
    {
        string[] time = input.Split(':');
        if (time.Length > 1 && int.TryParse(time[0], out int hour) && int.TryParse(time[0], out int minute))
        {
            dateTimeNow = new DateTime(dateTimeNow.Year, dateTimeNow.Month, dateTimeNow.Day, hour, minute, dateTimeNow.Second);
        }
        SetSunPosition();
        UpdateNumeralInputs();
    }

    /// <summary>
    /// Set the day of the month nr directly based on a string input
    /// </summary>
    /// <param name="input">Day of the month input number as string, for example "1" for the first day of the month</param>
    public void SetDay(string input)
    {
        int dayNumber = 0;
        if(int.TryParse(input, out dayNumber))
        {
            dateTimeNow = new DateTime(dateTimeNow.Year, dateTimeNow.Month, dayNumber, dateTimeNow.Hour, dateTimeNow.Minute, dateTimeNow.Second);
        }
        SetSunPosition();
        UpdateNumeralInputs();
    }
    /// <summary>
    /// Set the month nr directly based on a string input
    /// </summary>
    /// <param name="input">Month input number as string, for example "8" for august</param>
    public void SetMonth(string input)
    {
        int monthNumber = 0;
        if (int.TryParse(input, out monthNumber))
        {
            dateTimeNow = new DateTime(dateTimeNow.Year, monthNumber, dateTimeNow.Day, dateTimeNow.Hour, dateTimeNow.Minute, dateTimeNow.Second);
        }
        SetSunPosition();
        UpdateNumeralInputs();
    }
    /// <summary>
    /// Set the year nr directly based on a string input
    /// </summary>
    /// <param name="input">Year input number as string, for example "2020"</param>
    public void SetYear(string input)
    {
        int yearNumber = 0;
        if (int.TryParse(input, out yearNumber))
        {
            dateTimeNow = new DateTime(yearNumber, dateTimeNow.Month, dateTimeNow.Day, dateTimeNow.Hour, dateTimeNow.Minute, dateTimeNow.Second);
        }
        SetSunPosition();
        UpdateNumeralInputs();
    }

    private void ChangedDay(int withOffset)
    {
        dateTimeNow = dateTimeNow.AddDays(withOffset);
        UpdateNumeralInputs();
    }
    private void ChangedMonth(int withOffset)
    {
        dateTimeNow = dateTimeNow.AddMonths(withOffset);
        UpdateNumeralInputs();
    }
    private void ChangedYear(int withOffset)
    {
        dateTimeNow = dateTimeNow.AddYears(withOffset);
        UpdateNumeralInputs();
    }

    private void UpdateNumeralInputs()
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
                UpdateNumeralInputs();
            }
            SetSunPosition();
        }
    }

    private void SunPositionChangedFromWheel(float delta){
        //Convert flat rotation to expected time/sun position
        dateTimeNow = dateTimeNow.AddHours(delta*0.01f);

        SetSunPosition();
        UpdateNumeralInputs();
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
        if(SpeedMultiplier > maxSpeedMultiply) SpeedMultiplier = maxSpeedMultiply;
    }
    public void FastBackward()
    {
        SpeedMultiplier /= 10;
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
        
        UpdateNumeralInputs();
    }

}
