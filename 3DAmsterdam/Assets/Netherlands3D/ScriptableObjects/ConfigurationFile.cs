using ConvertCoordinates;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Netherlands3D
{
    [CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/ConfigurationFile", order = 0)]
    [System.Serializable]
    public class ConfigurationFile : ScriptableObject
    {
        public enum TmsTileNumberingType
        {
            GoogleAndOSM, // (0 to 2zoom-1, 0 to 2zoom-1) for the range(-180, +85.0511) - (+180, -85.0511)
            TMS, //'Tile Map Service' (0 to 2zoom-1, 2zoom-1 to 0) for the range(-180, +85.0511) - (+180, -85.0511). (That is, the same as the previous with the Y value flipped.)
            QuadTrees //Used by Microsoft
        }

        [Header("Coordinates")]

        public Vector2RD RelativeCenterRD;

        public float zeroGroundLevelY = 43.0f;

        [Header("Minimap Tiled Web Map")]
        [Tooltip("The variables {x} and {y} in the URL will be replaced with their corresponding RD coordinates.")]
        public string minimapServiceUrl = "https://t1.data.amsterdam.nl/topo_rd/{zoom}/{x}/{y}.png";
        public TmsTileNumberingType MinimapTileNumberingType = TmsTileNumberingType.GoogleAndOSM;
        public float minimapBottomLeftRD_X = -285401.920f; // zoomlevel 0 at RD WMTS
        public float minimapBottomLeftRD_Y = 22598.080f; // zoomlevel 0 at RD WMTS
        [Tooltip("Zoomlevel 0 width/height of tiles")]
        public float MinimapZoom0RDSize = 880803.84f;

        [Header("Tile layers external assets paths")]
        public string webserverRootPath = "https://3d.amsterdam.nl/web/data/";
        public string buildingsMetaDataPath = "https://3d.amsterdam.nl/web/data/buildings/objectdata/";

        public string sharingBaseURL = "https://3d.amsterdam.nl/";
        public string sharingSceneSubdirectory = "customScene.php?id=";
        public string sharingViewUrl = "https://3d.amsterdam.nl/web/app/index.html?view=";

        [Header("External URLs")]
        public string downloadsPage = "https://3d.amsterdam.nl/web/downloads/index.html";
        public string LocationSuggestionUrl = "https://geodata.nationaalgeoregister.nl/locatieserver/v3/suggest?q={SEARCHTERM}%20and%20Amsterdam%20&rows=5";
        public string LookupUrl = "https://geodata.nationaalgeoregister.nl/locatieserver/v3/lookup?id={ID}";

        [Header("Graphics")]
        public Sprite logo;

        public Color primaryColor;
        public Color secondaryColor;
    }
}