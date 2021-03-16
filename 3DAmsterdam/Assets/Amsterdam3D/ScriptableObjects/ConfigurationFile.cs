using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/ConfigurationFile", order = 0)]
[System.Serializable]
public class ConfigurationFile : ScriptableObject
{
    public const float ZERO_GROUND_LEVEL_Y = 43.0f;

    public float MINIMAP_RD_BOTTOMLEFT_X = -285401.920f; // zoomlevel 0 at RD WMTS
    public float MINIMAP_RD_BOTTOMLEFT_Y = 22598.080f; // zoomlevel 0 at RD WMTS
    public float MINIMAP_RD_ZOOM_0_TILESIZE = 880803.84f; // zoomlevel 0 width/height of tiles 

    /// <summary>
    /// Swap data URL based on the branch type. Optionaly we can choose to use a relative path for WebGL.
    /// </summary>
    public string BASE_DATA_URL = "https://3d.amsterdam.nl/web/data/";
    public string SHARE_URL = "https://3d.amsterdam.nl/";
    public string SHARE_OBJECTSTORE_PATH = "customScene.php?id=";
    public string SHARED_VIEW_URL = "https://3d.amsterdam.nl/web/app/index.html?view=";

    public string TILE_METADATA_URL = "https://3d.amsterdam.nl/web/data/buildings/objectdata/";
}
