/*
*  Copyright (C) X Gemeente
*                X Amsterdam
*                X Economic Services Departments
*
*  Licensed under the EUPL, Version 1.2 or later (the "License");
*  You may not use this work except in compliance with the License.
*  You may obtain a copy of the License at:
*
*    https://github.com/Amsterdam/3DAmsterdam/blob/master/LICENSE.txt
*
*  Unless required by applicable law or agreed to in writing, software
*  distributed under the License is distributed on an "AS IS" basis,
*  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or
*  implied. See the License for the specific language governing
*  permissions and limitations under the License.
*/
using Amsterdam3D.Sewerage;
using Netherlands3D.Core;
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

        [Header("User scene sharing URLs")]
        public string sharingUploadScenePath = "https://3d.amsterdam.nl/customUpload.php";
        public string sharingDownloadScenePath = "https://3d.amsterdam.nl/customScene.php?id={sceneId}_scene.json";

        public string sharingUploadModelPath = "https://3d.amsterdam.nl/customUpload.php?sceneId={sceneId}&meshToken={modelToken}";
        public string sharingDownloadModelPath = "https://3d.amsterdam.nl/customScene.php?id={modelToken}.dat";

        public string sharingViewScenePath = "https://3d.amsterdam.nl/web/app/index.html?view={sceneId}";

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