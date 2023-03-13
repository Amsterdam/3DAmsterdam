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
using Netherlands3D.Core;
using Netherlands3D.Events;
using Netherlands3D.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

namespace Amsterdam3D.Maps
{
    public class MapsDataLoader : MonoBehaviour
    {
        [SerializeField]
        private StringEvent tableNameReceiveEvent;

        [SerializeField]
        private List<MapsDataTable> mapsDataTable;

        [System.Serializable]
        public struct MapsDataTable
        {
            public string name;
            public GeoJsonURLS[] geoJsonURLs;
        }
        [System.Serializable]
        public struct GeoJsonURLS
        {
            public string geoJsonURL;
            public Vector3Event drawPointEvent;
            public Vector3ListEvent drawLineEvent;
            public Vector3ListsEvent drawGeometryEvent;
            public StringEvent setObjectNameEvent;
            public string matchingPropertyName;
            public string matchingPropertyValue;
            public string objectNameProperty;
        }

        void Awake()
        {
            tableNameReceiveEvent.AddListenerStarted(LoadAllDataURLs);
        }

        void LoadAllDataURLs(string tableNames)
        {
            string[] tableNameValues = tableNames.Split(',');
            foreach (var tableName in tableNameValues)
            {
                var targetDataTable = mapsDataTable.Where((item) => tableName == item.name);
                if (targetDataTable.Any())
                {
                    var firstResult = targetDataTable.First();
                    foreach (var geoJsonURLData in firstResult.geoJsonURLs)
                    {
                        StartCoroutine(LoadGeoJSON(geoJsonURLData));
                    }
                }
            }
        }

        private IEnumerator LoadGeoJSON(GeoJsonURLS geoJsonURLData)
        {
            var geoJsonDataRequest = UnityWebRequest.Get(geoJsonURLData.geoJsonURL);
            yield return geoJsonDataRequest.SendWebRequest();

            if (geoJsonDataRequest.result == UnityWebRequest.Result.Success)
            {
                GeoJSON geoJSON = new GeoJSON(geoJsonDataRequest.downloadHandler.text);
                yield return null;

                //We already filtered the request, so we can draw all features
                while (geoJSON.GotoNextFeature())
                {
                    var type = geoJSON.GetGeometryType();
                    
                    if(geoJsonURLData.matchingPropertyName != "")
                    {
                        var propertyName = geoJsonURLData.matchingPropertyName;
                        var propertyValue = geoJsonURLData.matchingPropertyValue;

                        if(propertyValue != geoJSON.GetPropertyStringValue(propertyName)){
                            continue;
						}
                    }

                    switch (type)
                    {
                        case GeoJSON.GeoJSONGeometryType.Point:
                            if (!geoJsonURLData.drawPointEvent) continue;
                            double[] location = geoJSON.GetGeometryPoint2DDouble();
                            var unityCoordinates = CoordConvert.WGS84toUnity(location[0], location[1]);

                            geoJsonURLData.drawPointEvent.InvokeStarted(unityCoordinates);
                            break;
                        case GeoJSON.GeoJSONGeometryType.LineString:
                            if (!geoJsonURLData.drawLineEvent) continue;

                            List<GeoJSONPoint> line = geoJSON.GetGeometryLineString();
                            List<Vector3> unityLine = new List<Vector3>();
                            for (int i = 0; i < line.Count; i++)
                            {
                                unityLine.Add(CoordConvert.WGS84toUnity(line[i].x, line[i].y));
                            }
                            geoJsonURLData.drawLineEvent.InvokeStarted(unityLine);
                            break;
						case GeoJSON.GeoJSONGeometryType.Polygon:
							if (!geoJsonURLData.drawGeometryEvent) continue;
                            
							List<List<GeoJSONPoint>> polygon = geoJSON.GetPolygon();

							yield return DrawPolygonRequest(geoJsonURLData, polygon);

							break;
						case GeoJSON.GeoJSONGeometryType.MultiPolygon:
                            List<List<List<GeoJSONPoint>>> multiPolygons = geoJSON.GetMultiPolygon();
                            for (int i = 0; i < multiPolygons.Count; i++)
                            {
                                var multiPolygon = multiPolygons[i];
                                DrawPolygonRequest(geoJsonURLData, multiPolygon);
                            }
                            break;
                    }
                }
            }
        }

		private IEnumerator DrawPolygonRequest(GeoJsonURLS geoJsonURLData, List<List<GeoJSONPoint>> polygon)
		{            
			List<List<Vector3>> unityPolygon = new List<List<Vector3>>();

			//Grouped polys
			for (int i = 0; i < polygon.Count; i++)
			{
				var contour = polygon[i];

                List<Vector3> polyList = new List<Vector3>();
				for (int j = 0; j < contour.Count; j++)
				{
                    polyList.Add(CoordConvert.WGS84toUnity(contour[j].x, contour[j].y));
				}
				unityPolygon.Add(polyList);
			}

            if (unityPolygon.Count > 0)
            {
                if(geoJsonURLData.setObjectNameEvent) geoJsonURLData.setObjectNameEvent.InvokeStarted(geoJsonURLData.objectNameProperty);
                geoJsonURLData.drawGeometryEvent.InvokeStarted(unityPolygon);
            }

            yield return null;
		}
	}
}