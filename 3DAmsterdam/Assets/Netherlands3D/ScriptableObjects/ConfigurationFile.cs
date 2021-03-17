using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/ConfigurationFile", order = 0)]
[System.Serializable]
public class ConfigurationFile : ScriptableObject
{
    [Header("Coordinates")]
    public float zeroGroundLevelY = 43.0f;

    [Header("Minimap")]
    [Tooltip("The variables {x} and {y} in the URL will be replaced with their corresponding RD coordinates.")]
    public string minimapTmsServiceUrl = "https://t1.data.amsterdam.nl/topo_rd/{zoom}/{x}/{y}.png";
    public float minimapBottomLeftRD_X = -285401.920f; // zoomlevel 0 at RD WMTS
    public float minimapBottomLeftRD_Y = 22598.080f; // zoomlevel 0 at RD WMTS

    [Tooltip("Zoomlevel 0 width/height of tiles")]
    public float MinimapZoom0RDSize = 880803.84f;

    [Header("External assets paths")]
    public string webserverRootPath = "https://3d.amsterdam.nl/web/data/";
    public string buildingsMetaDataPath = "https://3d.amsterdam.nl/web/data/buildings/objectdata/";

    public string sharingBaseURL = "https://3d.amsterdam.nl/";
    public string sharingSceneSubdirectory = "customScene.php?id=";
    public string sharingViewUrl = "https://3d.amsterdam.nl/web/app/index.html?view=";

    [Header("Graphics")]
    public Sprite logo;

    public Color primaryColor;
    public Color secondaryColor;
}
