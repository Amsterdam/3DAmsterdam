using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Amsterdam3D.CameraMotion;
using LayerSystem;

public class billboardtext : MonoBehaviour
{
    public bool previewAllowed = true;
    public GameObject fireworksLayer;
    public Vector3 cameraStartpositie;
    public Quaternion cameraStartRotatie;
    public SunSettings sunSettings;
    private TextMesh billboardText;
    DateTime scriptDateTime;
    private bool AfterMidnight = false;
    // Start is called before the first frame update
    void Start()
    {
        billboardText = GetComponent<TextMesh>();
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
    void EditBillboardText()
    {

        DateTime middernacht = new DateTime(2021, 1, 1, 0, 0, 0);

        TimeSpan verschil = middernacht.Subtract(scriptDateTime);

        int days = verschil.Days;
        int hours = verschil.Hours;
        int minutes = verschil.Minutes;
        int seconds = verschil.Seconds;

        if (days>0)
        {
            billboardText.text = "nog " + days.ToString() + " dagen";
            return;
        }
        if (hours > 0)
        {
            billboardText.text = "nog " + hours.ToString() + " uur en " + minutes.ToString() + " minuten";
            return;
        }
        if(minutes >0)
        {
            billboardText.text = "nog " + minutes.ToString() + " minuten en " + seconds.ToString() + " seconden";
            return;
        }
        if (seconds >10)
        {
            billboardText.text = "nog " +  seconds.ToString() + " seconden";
            return;
        }
        if (middernacht>=scriptDateTime)
        {
            billboardText.text = seconds.ToString();
            return;
        }
        if (AfterMidnight == false)
        {
            AfterMidnight = true;
            fireworksLayer.GetComponent<Layer>().enabled=true;
        }

        billboardText.text = "gelukkig Nieuwjaar!!";
    }
}
