using UnityEngine;

public static class Constants
{
    /// <summary>
    /// The world Y axis is based on GPS coordinates.
    /// The height of 43 is close to 0 NAP, a nice average value for the Amsterdam ground level.
    /// </summary>
    public const float ZERO_GROUND_LEVEL_Y = 43.0f;

    /// <summary>
    /// The minimap coordinates (starting from the bottom left)
    /// </summary>
    public const float MINIMAP_RD_BOTTOMLEFT_X = -285401.920f; // zoomlevel 0 at RD WMTS
    public const float MINIMAP_RD_BOTTOMLEFT_Y = 22598.080f; // zoomlevel 0 at RD WMTS
    public const float MINIMAP_RD_ZOOM_0_TILESIZE = 880803.84f; // zoomlevel 0 width/height of tiles 

    /// <summary>
    /// Swap data URL based on the branch type. Optionaly we can choose to use a relative path for WebGL.
    /// </summary>
#if !UNITY_EDITOR && RELATIVE_BASE_PATH
        public const string BASE_DATA_URL = "./AssetBundles/WebGL/";
#elif PRODUCTION
        public const string BASE_DATA_URL = "https://3d.amsterdam.nl/web/data/";
        //public const string FEATURE_DATA_URL = "https://3d.amsterdam.nl/web/data/";
#elif DEVELOPMENT_FEATURE
    //version contains branch name, for example feature/new-feature-name
    //public static string FEATURE_DATA_URL = "https://acc.3d.amsterdam.nl/webmap/data/" + Application.version;
    
    //USE DEVELOPMENT PATH BY DEFAULT
    public const string BASE_DATA_URL = "https://acc.3d.amsterdam.nl/webmap/data/develop/";

    
#endif
}
