using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Amsterdam3D.CameraMotion;
using LayerSystem;

public class BillboardText : MonoBehaviour
{
    public bool previewAllowed = false;
    public FireworksLayer fireworksLayer;
    public Vector3 cameraStartpositie;
    public Quaternion cameraStartRotatie;
    public SunSettings sunSettings;
    private TextMesh billboardText;
    DateTime scriptDateTime;

    private Vector3 startBillboardTextScale = default;

    public bool countDownAboutToStart = false;
    public bool allowFireworks = false;
    public bool allowFireworksFlicker = false;

    public bool allowReplay = false;

    private DateTime buttonAppearTime;
    private DateTime restartCountdownTime;
    private DateTime midnightTime;
    private DateTime fireWorksFlickerTime;

    private DateTime fireWorksEndTime;
    private DateTime restartAllowedTime;

    private TimeSpan verschil;

    private int days;
    private int hours;
    private int minutes;
    private int seconds;

    // Start is called before the first frame update
    void Start()
    {
        restartCountdownTime = new DateTime(2020, 12, 31, 23, 59, 50);
        buttonAppearTime = new DateTime(2020, 12, 31, 23, 59, 0);
        midnightTime = new DateTime(2021, 1, 1, 0, 0, 0);
        fireWorksFlickerTime = new DateTime(2021, 1, 1, 0, 0, 2);

        fireWorksEndTime = new DateTime(2021, 1, 1, 1, 0, 0);
        restartAllowedTime = new DateTime(2021, 1, 1, 0, 15, 0);

        billboardText = GetComponent<TextMesh>();
        startBillboardTextScale = this.transform.localScale;
        CameraModeChanger.Instance.ActiveCamera.transform.position = cameraStartpositie;
        CameraModeChanger.Instance.ActiveCamera.transform.rotation = cameraStartRotatie;
    }

    // Update is called once per frame
    void Update()
    {
        //check dateTime
        if (DateTime.Now<sunSettings.dateTimeNow & previewAllowed==false)
        {
            scriptDateTime = DateTime.Now;
        }
        else
        {
            scriptDateTime = sunSettings.dateTimeNow;
        }
        EditBillboardText();
    }

    //Alows us to cheat from javascript
    public void Cheat()
    {
        previewAllowed = true;
    }

    void EditBillboardText()
    {
        verschil = midnightTime.Subtract(scriptDateTime);

        days = verschil.Days;
        hours = verschil.Hours;
        minutes = verschil.Minutes;
        seconds = verschil.Seconds;

        //Make sure we reset our text scale back to default if we scrubbed back
        billboardText.transform.localScale = startBillboardTextScale;

        countDownAboutToStart = scriptDateTime > buttonAppearTime;

        //Within the hour of allowed fireworks, enable fireworks!!
        allowFireworks = (scriptDateTime > midnightTime && scriptDateTime < fireWorksEndTime);
        allowFireworksFlicker = (scriptDateTime > fireWorksFlickerTime && scriptDateTime < fireWorksEndTime);

        fireworksLayer.enabled = allowFireworks;

        if (days>0)
        {
            billboardText.text = "NOG " + days.ToString() + "\nDAG" + ((days>1) ? "EN" : "");
            return;
        }
        if (hours > 0)
        {
            billboardText.text = "NOG " + hours.ToString() + " UUR\nEN " + minutes.ToString() + " MIN";
            return;
        }
        if(minutes >0)
        {
            billboardText.text = "NOG " + minutes.ToString() + "MIN\nEN " + seconds.ToString() + " SEC";
            return;
        }
        if (seconds >10)
        {
            billboardText.text = "NOG\n" + seconds.ToString() + " SECONDEN";
            return;
        }
        if (midnightTime>=scriptDateTime)
        {
            billboardText.text = seconds.ToString();
            billboardText.transform.localScale = startBillboardTextScale * 2.5f;
            return;
        }

        if(scriptDateTime > fireWorksEndTime)
        {
            Cheat(); //Allow cheating after 01:00 to be able to scrub the time and restart the show
        }
        if(scriptDateTime > restartAllowedTime)
        {
            allowReplay = true;
        }

        //After midnight, keep switching the text every second
        if ((seconds % 2 == 0))
        {
            billboardText.text = "GELUKKIG\nNIEUWJAAR!!";
            billboardText.transform.localScale = startBillboardTextScale;
        }
        else
        {
            billboardText.text = "TEAM 3D\nWENST JE";
            billboardText.transform.localScale = startBillboardTextScale;// * 0.7f;
        }
    }

	public void GoToCountdown()
	{
        sunSettings.dateTimeNow = restartCountdownTime;

        restartCountdownTime = midnightTime; //Allows quicker restart after 1 restart/later visit

        allowReplay = false; //Makes button go away, and appear again at the triggertime
    }
}
