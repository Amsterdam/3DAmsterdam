using Amsterdam3D.Sewerage;
using ConvertCoordinates;
using Netherlands3D.BAG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Netherlands3D
{
    [CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/ConfigurationFile", order = 0)]
    [System.Serializable]
    public class ConfigurationFile : ScriptableObject
    {
        public enum MinimapOriginAlignment
        {
            TopLeft,
            BottomLeft
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
		public int minimapTileSize = 256;
        public MinimapOriginAlignment minimapOriginAlignment = MinimapOriginAlignment.TopLeft;
        public Vector2RD minimapOrigin;
        public double minimapPixelInMeters = 0.00028;
        public double minimapScaleDenominator = 12288000;

        [Header("Tile layers external assets paths")]
        public string webserverRootPath = "https://3d.amsterdam.nl/web/data/";
        public string buildingsMetaDataPath = "https://3d.amsterdam.nl/web/data/buildings/objectdata/";

        public string sharingBaseURL = "https://3d.amsterdam.nl/";
        public string sharingSceneSubdirectory = "customScene.php?id=";
        public string sharingViewUrl = "https://3d.amsterdam.nl/web/app/index.html?view=";

        [Header("External URLs")]
        public string LocationSuggestionUrl = "https://geodata.nationaalgeoregister.nl/locatieserver/v3/suggest?q={SEARCHTERM}%20and%20Amsterdam%20&rows=5";
        public string LookupUrl = "https://geodata.nationaalgeoregister.nl/locatieserver/v3/lookup?id={ID}";

        [Header("Sewerage Api URLs")]
        public SewerageApiType sewerageApiType = SewerageApiType.Pdok;
        public string sewerPipesWfsUrl = "https://geodata.nationaalgeoregister.nl/rioned/gwsw/wfs/v1_0?SERVICE=WFS&language=eng&SERVICE=WFS&REQUEST=GetFeature&VERSION=2.0.0&TYPENAMES=gwsw:beheer_leiding&SRSNAME=urn:ogc:def:crs:EPSG::28992&outputFormat=application/json&BBOX=";
        public string sewerManholesWfsUrl = "https://geodata.nationaalgeoregister.nl/rioned/gwsw/wfs/v1_0?SERVICE=WFS&language=eng&SERVICE=WFS&REQUEST=GetFeature&VERSION=2.0.0&TYPENAMES=gwsw:beheer_put&SRSNAME=urn:ogc:def:crs:EPSG::28992&outputFormat=application/json&BBOX=";

        [Header("Bag Api URLs")]
        public BagApyType BagApiType = BagApyType.Kadaster;
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