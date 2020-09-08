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
        public const string BASE_DATA_URL = "./web/data/";
#elif PRODUCTION
        public const string BASE_DATA_URL = "https://3d.amsterdam.nl/web/data/";
        public static string SHARE_URL = "https://3d.amsterdam.nl/";
        public static string SHARE_OBJECTSTORE_PATH = "web/userUploads/";
        public static string SHARED_VIEW_URL = "https://3d.amsterdam.nl/web/app/index.html?view=";
#elif DEVELOPMENT_FEATURE
        //version contains branch name, for example feature/new-feature-name
        public static string BASE_DATA_URL = "https://acc.3d.amsterdam.nl/web/data/" + Application.version; 
        public static string SHARE_URL = "https://acc.3d.amsterdam.nl/";
        public static string SHARE_OBJECTSTORE_PATH = "webmap/userUploads/";
        public static string SHARED_VIEW_URL = "https://acc.3d.amsterdam.nl/web/app/index.html?view=";
#else
    //USE DEVELOPMENT PATH BY DEFAULT
    // public const string BASE_DATA_URL = "https://acc.3d.amsterdam.nl/web/data/develop/";
    public const string BASE_DATA_URL = "https://3d.amsterdam.nl/web/data/";
    public static string SHARE_URL = "https://acc.3d.amsterdam.nl/";
        public static string SHARE_OBJECTSTORE_PATH = "webmap/userUploads/";
        public static string SHARED_VIEW_URL = "https://acc.3d.amsterdam.nl/web/app/index.html?view=";
#endif
}
