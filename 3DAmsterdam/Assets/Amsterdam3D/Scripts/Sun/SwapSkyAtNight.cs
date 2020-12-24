using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwapSkyAtNight : MonoBehaviour
{
    [SerializeField]
    private Material daySky;
    [SerializeField]
    private Material nightSky;

    [SerializeField]
    private GameObject[] enableAtNight;

    SunVisuals sunVisuals;
    void Start()
    {
        sunVisuals = GetComponent<SunVisuals>();
    }

    void Update()
    {
        RenderSettings.skybox = (sunVisuals.Day) ? daySky : nightSky;

        foreach(GameObject gameObject in enableAtNight)
        {
            gameObject.SetActive(!sunVisuals.Day);
        }
    }
}
