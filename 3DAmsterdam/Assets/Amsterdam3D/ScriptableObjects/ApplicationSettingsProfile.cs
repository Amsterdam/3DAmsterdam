using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/ApplicationSettingsProfile", order = 1)]
[System.Serializable]
public class ApplicationSettingsProfile : ScriptableObject
{
    public string profileName = "High";

    //Interface
    public bool drawMap = true;
    public bool drawFPS = false;
    public float canvasDPI = 1.0f;

    //Quality
    public float renderResolution = 1.0f;
    public float shadowQuality = 1.0f;
    public float textureQuality = 1.0f;
    public bool antiAliasing = true;
    public bool postProcessingEffects = true;
}
