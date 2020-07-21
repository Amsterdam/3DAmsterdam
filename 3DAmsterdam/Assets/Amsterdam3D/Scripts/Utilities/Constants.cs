public static class Constants
{
    /// <summary>
    /// The world Y axis is based on GPS coordinates.
    /// The height of 43 is close to 0 NAP, a nice average value for the Amsterdam ground level.
    /// </summary>
    public const float ZERO_GROUND_LEVEL_Y = 43.0f;

    /// <summary>
    /// Swap data URL based on the branch type. If we build, use a relative path in WebGL.
    /// </summary>
#if !UNITY_EDITOR && UNITY_WEBGL
    public const string BASE_DATA_URL = "./AssetBundles/WebGL/";
#elif UNITY_EDITOR && BRANCH_MASTER
    public const string BASE_DATA_URL = "https://3d.amsterdam.nl/web/app/AssetBundles/WebGL/";
#elif UNITY_EDITOR && BRANCH_DEVELOP
    public const string BASE_DATA_URL = "https://acc.3d.amsterdam.nl/webmap/AssetBundles/WebGL/";
#elif UNITY_EDITOR && BRANCH_FEATURE
    public const string BASE_DATA_URL = "https://acc.3d.amsterdam.nl/webmap/AssetBundles/WebGL/";
#endif
}
