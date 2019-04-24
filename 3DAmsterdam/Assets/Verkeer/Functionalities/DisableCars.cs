using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DisableCars : MonoBehaviour
{
    public GameObject[] cars;
    int switcher;

    void Start()
    {
        cars = GameObject.FindGameObjectsWithTag("Car");
        switcher = 1;
    }

    public void CarDisable()
    {
        if(switcher == 1)
        {
            foreach (GameObject car in cars)
            {
                car.active = false;          
            }

            switcher *= -1;

        }
        else if (switcher == -1)
        {
            foreach (GameObject car in cars)
            {
                car.active = true;

            }
            switcher *= -1;
        }
    }
}
