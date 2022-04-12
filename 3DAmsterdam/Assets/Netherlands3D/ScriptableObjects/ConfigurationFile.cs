using Amsterdam3D.Sewerage;
using ConvertCoordinates;
using Netherlands3D.BAG;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
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

        [Header("Bounding Box coordinates")]
        public Vector2RD RelativeCenterRD;
        public Vector2RD BottomLeftRD;
        public Vector2RD TopRightRD;

        public float zeroGroundLevelY = 43.0f;

        [Header("Minimap Tiled Web Map")]
        [Tooltip("The variables {x} and {y} in the URL will be replaced with their corresponding RD coordinates.")]
        public bool EnableMinimap = true;
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
        public string LocationSuggestionUrl = "https://geodata.nationaalgeoregister.nl/locatieserver/v3/suggest?q={SEARCHTERM}%20and%20Amsterdam%20&rows=5";
        public string LookupUrl = "https://geodata.nationaalgeoregister.nl/locatieserver/v3/lookup?id={ID}";

        private Regex regex_url = new Regex(@"https:\/\/t3d-.", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private string _T3DSandboxEnvironment = null;
        public string _T3DAzureFunctionURL = null;

        [HideInInspector]
        public string T3DSandboxEnvironment
        {
            get
            {
                if( string.IsNullOrEmpty(_T3DSandboxEnvironment))
                {
                    var url = Application.absoluteURL;
                    var defaultURL = "https://t3d-o-cdn.azureedge.net/"; //testurl

                    if (string.IsNullOrEmpty(url)) return defaultURL;
                    if (url.StartsWith("http://localhost")) return defaultURL;
                    if (url.StartsWith("https://t3dstorage.")) return defaultURL;

                    MatchCollection matches = regex_url.Matches(url);
                    if (matches.Count == 0) throw new Exception("Kan omgeving niet detecteren");

                    var urlstart = matches[0].Value;
                    _T3DSandboxEnvironment = $"{urlstart}-cdn.azureedge.net/";
                }

                return _T3DSandboxEnvironment;
            }
        }

        [HideInInspector]
        public string T3DAzureFunctionURL
        {
            get
            {
                if (string.IsNullOrEmpty(_T3DAzureFunctionURL))
                {
                    var url = Application.absoluteURL;
                    var defaultURL = "https://t3d-o-functions.azurewebsites.net/"; //testurl

                    if (string.IsNullOrEmpty(url)) return defaultURL;
                    if (url.StartsWith("http://localhost")) return defaultURL;
                    if (url.StartsWith("https://t3dstorage.")) return defaultURL;

                    MatchCollection matches = regex_url.Matches(url);
                    if (matches.Count == 0) throw new Exception("Kan omgeving niet detecteren");

                    var urlstart = matches[0].Value;
                    _T3DAzureFunctionURL = $"{urlstart}-functions.azurewebsites.net/";
                }

                return _T3DAzureFunctionURL;                
            }
        }


        [Header("Sewerage Api URLs")]
        public SewerageApiType sewerageApiType;
        public string sewerPipesWfsUrl = "https://api.data.amsterdam.nl/v1/wfs/rioolnetwerk/?SERVICE=WFS&VERSION=2.0.0&REQUEST=GetFeature&outputFormat=geojson&srsname=epsg:4258&typeName=rioolleidingen&bbox=";
        public string sewerManholesWfsUrl = "https://api.data.amsterdam.nl/v1/wfs/rioolnetwerk/?SERVICE=WFS&VERSION=2.0.0&REQUEST=GetFeature&outputFormat=geojson&srsname=epsg:4258&typeName=rioolknopen&bbox=";

        [Header("Bag Api URLs")]
        public BagApyType BagApiType;
        public string buildingUrl = "https://api.data.amsterdam.nl/bag/v1.1/pand/";
        public string numberIndicatorURL = "https://api.data.amsterdam.nl/bag/v1.1/nummeraanduiding/?page_size=10000&pand=";
        public string numberIndicatorInstanceURL = "https://api.data.amsterdam.nl/bag/v1.1/nummeraanduiding/";
        public string monumentURL = "https://api.data.amsterdam.nl/monumenten/monumenten/?betreft_pand=";
        public string moreBuildingInfoUrl = "https://data.amsterdam.nl/data/bag/pand/id{bagid}/";
        public string moreAddressInfoUrl = "https://data.amsterdam.nl/data/bag/nummeraanduiding/id{bagid}/";
        public string bagIdRequestServiceBoundingBoxUrl = "https://map.data.amsterdam.nl/maps/bag?REQUEST=GetFeature&SERVICE=wfs&version=2.0.0&typeName=bag:pand&propertyName=bag:id&outputFormat=csv&bbox=";
        public string bagIdRequestServicePolygonUrl = "https://map.data.amsterdam.nl/maps/bag?REQUEST=GetFeature&SERVICE=wfs&version=2.0.0&typeName=bag:pand&propertyName=bag:id&outputFormat=csv&Filter=";
        public string previewBackdropImage = "https://geodata.nationaalgeoregister.nl/luchtfoto/rgb/wms?styles=&layers=Actueel_ortho25&service=WMS&request=GetMap&format=image%2Fpng&version=1.1.0&bbox={xmin},{ymin},{xmax},{ymax}&width={w}&height={h}&srs=EPSG:28992";


        [Header("Graphics")]
        public Sprite logo;

        public Color primaryColor;
        public Color secondaryColor;
    }
}