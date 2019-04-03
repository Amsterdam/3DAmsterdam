using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeatherOptions : MonoBehaviour
{
    public GameObject[] weatherOptions;
    public RectTransform positioning;

    private float moveFactor = 55f;
    private int minBarrier = 0, maxBarrier = 2;

    void Update()
    {
        for (int i=0; i<weatherOptions.Length; i++)
        {
            if (i >= minBarrier && i <= maxBarrier)
            {
                weatherOptions[i].SetActive(true);
            }
            else
            {
                weatherOptions[i].SetActive(false);
            }
        }
    }

    public void RightButton()
    {
        if (positioning.localPosition.x > -((weatherOptions.Length - 3) * moveFactor))
        {
            positioning.position -= new Vector3(moveFactor, 0, 0);

            minBarrier++;
            maxBarrier++;
        }
    }

    public void LeftButton()
    {
        if (positioning.localPosition.x < 0)
        {
            positioning.position += new Vector3(moveFactor, 0, 0);

            minBarrier--;
            maxBarrier--;
        }
    }
}
