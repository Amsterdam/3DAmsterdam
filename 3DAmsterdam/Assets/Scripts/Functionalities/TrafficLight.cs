using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class TrafficLight : MonoBehaviour
{
    public GameObject lightBall; //Reference to the light in the trafficlight
    private float timer = 0; //Timer for switching between colours

    public BoxCollider triggerArea; //Reference to the boxcollider

    private Color green = new Color(0, 255, 0); //The colour green saved in a variable
    private Color red = new Color(255, 0, 0); //the colour red saved in a variable

    void Update()
    {
        timer += Time.deltaTime; //Count up

        if(timer <= 5f) //if the timer is still lower than 5 sec
        {
            triggerArea.enabled = false; //The boxcollider is inactive
            lightBall.GetComponent<Renderer>().material.color = green; //And the light is green, cars can pass

        } else if(timer > 5f || timer <= 10f) //when the timer is between 5 and 10 sec
        {
            triggerArea.enabled = true; //the boxcollider becomes active
            lightBall.GetComponent<Renderer>().material.color = red; //and the light becomes red: cars cant pass and should stop

        }

        if (timer > 10f) //When the timer becomes greater than 10 sec
        {
            timer = 0f; //Return back to 0 and the light will become green again to create a cycle.
        }
    }
}
