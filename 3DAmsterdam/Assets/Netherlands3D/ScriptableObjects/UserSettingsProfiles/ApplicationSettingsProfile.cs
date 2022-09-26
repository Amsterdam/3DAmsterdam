using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/ApplicationSettingsProfile", order = 1)]
[System.Serializable]
public class ApplicationSettingsProfile : ScriptableObject
{
    public string applicationVersion = "";

    public bool mobileProfile = false;

    public string profileName = "High";

    //Interface
    public bool drawMap = true;
    public bool drawFPS = false;
    public float canvasScale = 100.0f;
    public bool showExperimentalFeatures = false;

    //Quality
    public float cameraFov = 40.0f;
    public float renderResolution = 1.0f;
    public float reflectionsRenderResolution = 0.2f;
    public float lodDistanceMultiplier = 1;
    public int maxLOD = 2;
    public bool autoLOD = true;
    public int shadowQuality = 3;
    public float textureQuality = 1.0f;
    public bool antiAliasing = true;

    //Extra
    public bool postProcessingEffects = true;
    public bool ambientOcclusion = false;
    public bool realtimeReflections = false;

    public int qualitySettingsSlot = 0;

    //Input
    public float rotateSensitivity = 0.6f;
}
