using UnityEngine;

public static class Constants
{
    /// <summary>
    /// The world Y axis is based on GPS coordinates.
    /// The height of 43 is close to 0 NAP, a nice average value for the Amsterdam ground level.
    /// </summary>
    public const float ZERO_GROUND_LEVEL_Y = 43.0f;

    /// <summary>
    /// Swap data URL based on the branch type. Optionaly we can choose to use a relative path for WebGL.
    /// </summary>
#if !UNITY_EDITOR && RELATIVE_BASE_PATH
        public const string BASE_DATA_URL = "./AssetBundles/WebGL/";
#elif PRODUCTION
        public const string BASE_DATA_URL = "https://3d.amsterdam.nl/web/app/";
        public static string SHARE_URL = "";
#elif DEVELOPMENT_FEATURE
        //version contains branch name, for example feature/new-feature-name
        public static string BASE_DATA_URL = "https://acc.3d.amsterdam.nl/webmap/data/" + Application.version; 
        public static string SHARE_URL = "";
#else
        //USE DEVELOPMENT PATH BY DEFAULT
        public const string BASE_DATA_URL = "https://acc.3d.amsterdam.nl/webmap/data/develop/";
	    public static string SHARE_URL = "http://blob.sambaas.nl/";
#endif
}
