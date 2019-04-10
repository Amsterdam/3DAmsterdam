using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Timechange : MonoBehaviour
{
    GameObject[] headLights; //Array that'll contain all lights in the scene except for the directional light
    public Slider slider;
    private float sliderValue;
    float lightDegree; //Float for the angle of the directional light
    float dayTime; //Amout of minutes in a day
    float timeOffSet = 120;
    public TextMeshProUGUI text;
    float minutes;

    void Start()
    {
        sliderValue = slider.value; //Save the value of the slider in a variable
        headLights = GameObject.FindGameObjectsWithTag("HeadLight"); //Fill the array
        this.transform.rotation = Quaternion.Euler(new Vector3(273, 0, 0));
    }

    public void Update()
    {
        LightActivator();
        TimeHandler();

        slider.value += 0.0001f;

        if(slider.value == 1)
        {
            slider.value = 0;
        }
    }

    public void LightActivator()
    {
        if (slider.value <= 0.4f || slider.value >= 0.8f) // If its dark outside
        {
            foreach (GameObject headlight in headLights)
            {
                headlight.GetComponent<Light>().enabled = true; //Turn all lights on
            }
        }
        else if (slider.value > 0.4f || slider.value < 0.8f) //if its not dark outside
        {
            foreach(GameObject headlight in headLights)
            {
                headlight.GetComponent<Light>().enabled = false; //Turn all lights off
            }
        }
    }

    public void TimeHandler()
    {
        dayTime = (1440f * slider.value) / 60;

        minutes = dayTime - (int)dayTime;
        minutes *= 60;

        dayTime = (int)dayTime;
        minutes = (int)minutes;

        if (minutes < 10)
        {
            text.GetComponent<TextMeshProUGUI>().text = dayTime.ToString() + ":0" + minutes.ToString();
        }
        else if (minutes > 10)
        {
            text.GetComponent<TextMeshProUGUI>().text = dayTime.ToString() + ":" + minutes.ToString();
        }

        lightDegree = (360 * slider.value) - timeOffSet; //The angle of the light will be 180 degrees times the value of the slider
        this.transform.rotation = Quaternion.Euler(new Vector3(lightDegree, 0, 0)); //Change the rotation on the X according to the change in slidervalue
    }
}
