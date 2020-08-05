using Amsterdam3D.Sun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SunSettings : MonoBehaviour
{
    [SerializeField]
    private SunDragWheel sunDragWheel;

    [SerializeField]
    private InputField timeInput;

    [SerializeField]
    private InputField dayInput;
    [SerializeField]
    private InputField monthInput;
    [SerializeField]
    private InputField yearInput;

    private Text speedMultiplierText;
    private int speedMultiplier = 1;

    private bool useTimedSun = false;
    private bool paused = false;

    [SerializeField]
    private Image coverTimeSettings;

    public void Start()
    {
        sunDragWheel.changedDirection += SunPositionChangedFromOutside;
    }

    private void SunPositionChangedFromOutside(float rotation){
        //Convert flat rotation to expected time/sun position
    }

    private void ToggleTimedSun(bool useTimedSun)
    {
        coverTimeSettings.enabled = !useTimedSun;
    }

    private void TogglePausePlay()
    {
        
    }
    
    public void ResetTimeToNow()
    {
        
    }

    public void FastForward()
    {
        
    }
    public void FastBackward()
    {

    }
}
