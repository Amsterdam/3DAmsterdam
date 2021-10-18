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
using Netherlands3D.Events;
using Netherlands3D.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

namespace Netherlands3D.Visualisers
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
        }

        void Awake()
        {
            tableNameReceiveEvent.unityEvent.AddListener(LoadAllDataURLs);
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
                    var type = geoJSON.getGeometryType();
                    Debug.Log($"Found type: {type}");

                    switch (type)
                    {
                        case GeoJSON.GeoJSONGeometryType.Point:
                        //case GeoJSON.GeoJSONGeometryType.MultiPoint:
                            double[] location = geoJSON.GetGeometryPoint2DDouble();
                            var unityCoordinates = ConvertCoordinates.CoordConvert.WGS84toUnity(location[0], location[1]);

                            geoJsonURLData.drawPointEvent.unityEvent?.Invoke(unityCoordinates);
                            break;
                        case GeoJSON.GeoJSONGeometryType.LineString:
                            List<GeoJSONPoint> line = geoJSON.GetGeometryLineString();
                            List<Vector3> unityLine = new List<Vector3>();
                            for (int i = 0; i < line.Count; i++)
                            {
                                unityLine.Add(ConvertCoordinates.CoordConvert.WGS84toUnity(line[i].x, line[i].y));
                            }
                            geoJsonURLData.drawLineEvent.unityEvent?.Invoke(unityLine);
                            break;
                        case GeoJSON.GeoJSONGeometryType.Polygon:
                        case GeoJSON.GeoJSONGeometryType.MultiPolygon:
                            List<List<List<GeoJSONPoint>>> multiPolygons = geoJSON.GetMultiPolygon();
                            List<List<IList<Vector3>>> unityMultiPolygons = new List<List<IList<Vector3>>>();
                           
                            for (int i = 0; i < multiPolygons.Count; i++)
                            {
                                
                            }

                            //geoJsonURLData.drawGeometryEvent.unityEvent?.Invoke(unityPoints);
                            break;
                    }
                }
            }
        }
    }
}