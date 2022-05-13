﻿using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using ConvertCoordinates;
using Netherlands3D;
using Netherlands3D.Interface;
using Netherlands3D.Interface.SidePanel;
using Netherlands3D.LayerSystem;
using SimpleJSON;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using Netherlands3D.Utilities;
using System.Globalization;
using System.IO;

namespace Netherlands3D.T3D.Uitbouw
{
    public class ObjectDataEventArgs : EventArgs
    {
        public bool IsLoaded { get; private set; }
        public ObjectData ObjectData { get; private set; }
        public Vector3 TileOffset;

        public ObjectDataEventArgs(bool isLoaded, ObjectData objectData, Vector3 tileOffset)
        {
            IsLoaded = isLoaded;
            ObjectData = objectData;
            TileOffset = tileOffset;
        }
    }

    public class PerceelDataEventArgs : EventArgs
    {
        public bool IsLoaded { get; private set; }
        public List<Vector2[]> Perceel { get; private set; } //in RD coordinaten
        public float Area { get; private set; }

        public Vector2RD Center;
        public float Radius;

        public PerceelDataEventArgs(bool isLoaded, List<Vector2[]> perceel, float area)
        {
            IsLoaded = isLoaded;
            Perceel = perceel;
            Area = area;

            var centerAndRadius = Utilities.GeometryCalculator.GetCenterAndRadius(perceel);
            Center = new Vector2RD( centerAndRadius.Center.x, centerAndRadius.Center.y);
            Radius = centerAndRadius.Radius;
        }
    }

    public class BuildingOutlineEventArgs : EventArgs
    {
        public bool IsLoaded { get; private set; }
        public List<Vector2[]> Outline { get; private set; } //in RD coordinaten
        public float TotalArea { get; private set; }

        public Vector2RD Center;
        public float Radius;

        public BuildingOutlineEventArgs(bool isLoaded, List<Vector2[]> outline, float totalArea)
        {
            IsLoaded = isLoaded;
            Outline = outline;
            TotalArea = totalArea;

            var centerAndRadius = Utilities.GeometryCalculator.GetCenterAndRadius(outline);
            Center = new Vector2RD(centerAndRadius.Center.x, centerAndRadius.Center.y);
            Radius = centerAndRadius.Radius;

        }
    }

    public class MetadataLoader : MonoBehaviour, IUniqueService
    {
        [SerializeField]
        private GameObject shapableUitbouwPrefab;
        [SerializeField]
        private GameObject uploadedUitbouwPrefab;

        public delegate void BuildingMetaDataLoadedEventHandler(object source, ObjectDataEventArgs args);
        public event BuildingMetaDataLoadedEventHandler BuildingMetaDataLoaded;

        public void RaiseBuildingMetaDataLoaded(ObjectData objectdata, Vector3 offset)
        {
            BuildingMetaDataLoaded?.Invoke(this, new ObjectDataEventArgs(true, objectdata, offset));
        }

        public delegate void PerceelDataLoadedEventHandler(object source, PerceelDataEventArgs args);
        public event PerceelDataLoadedEventHandler PerceelDataLoaded;

        public delegate void BuildingOutlineLoadedEventHandler(object source, BuildingOutlineEventArgs args);
        public event BuildingOutlineLoadedEventHandler BuildingOutlineLoaded;

        public delegate void BimCityJsonEventHandler(string cityJson);
        public event BimCityJsonEventHandler BimCityJsonReceived;

        [SerializeField]
        private BuildingMeshGenerator building;
        [SerializeField]
        private PerceelRenderer perceel;

        //todo: separate?
        public static UitbouwBase Uitbouw { get; private set; }
        public static BuildingMeshGenerator Building { get; private set; }
        public static PerceelRenderer Perceel { get; private set; }

        void Awake()
        {
            CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;

            Building = building;
            Perceel = perceel;
        }

        public void RequestBuildingData(Vector3RD position, string id)
        {
            if (ServiceLocator.GetService<T3DInit>().HTMLData.HasFile && (!string.IsNullOrEmpty(ServiceLocator.GetService<T3DInit>().HTMLData.ModelId) || !string.IsNullOrEmpty(ServiceLocator.GetService<T3DInit>().HTMLData.BlobId)))
            { 
                StartCoroutine(GetBimCityJson());
            }

            StartCoroutine(GetPerceelData(position));

            StartCoroutine(RequestBuildingOutlineData(id));

        }

        IEnumerator RequestBuildingOutlineData(string bagId)
        {
            var url = $"https://api.bag.kadaster.nl/lvbag/individuelebevragingen/v2/panden/{bagId}";

            UnityWebRequest req = UnityWebRequest.Get(url);
            req.SetRequestHeader("X-Api-Key", "l772bb9814e5584919b36a91077cdacea7");
            req.SetRequestHeader("Accept-Crs", "epsg:28992");

            yield return req.SendWebRequest();

            if (req.result == UnityWebRequest.Result.ConnectionError || req.result == UnityWebRequest.Result.ProtocolError)
            {
                ServiceLocator.GetService<WarningDialogs>().ShowNewDialog("Pand data kon niet opgehaald worden");
            }
            else
            {
                var json = JSON.Parse(req.downloadHandler.text);
                //var geometry = json["pand"]["geometrie"]["coordinates"];

                List<Vector2[]> list = new List<Vector2[]>();
                float totalArea = 0f;

                foreach (JSONNode feature in json["pand"]["geometrie"]["coordinates"]) //for each pand op het perceel
                {
                    List<Vector2> polygonList = new List<Vector2>();

                    foreach (JSONNode point in feature)
                    {
                        polygonList.Add(new Vector2(point[0], point[1]));
                    }
                    var polygonArray = polygonList.ToArray();
                    list.Add(polygonArray);
                    totalArea += GeometryCalculator.Area(polygonArray);
                }
                //print("outline loaded");
                BuildingOutlineLoaded?.Invoke(this, new BuildingOutlineEventArgs(true, list, totalArea));
            }
        }

        IEnumerator GetPerceelData(Vector3RD position)
        {
            //yield return null;

            var bbox = $"{ position.x - 0.5},{ position.y - 0.5},{ position.x + 0.5},{ position.y + 0.5}";
            var url = $"https://geodata.nationaalgeoregister.nl/kadastralekaart/wfs/v4_0?SERVICE=WFS&REQUEST=GetFeature&VERSION=2.0.0&TYPENAMES=kadastralekaartv4:perceel&STARTINDEX=0&COUNT=1&SRSNAME=urn:ogc:def:crs:EPSG::28992&BBOX={bbox},urn:ogc:def:crs:EPSG::28992&outputFormat=json";

            UnityWebRequest req = UnityWebRequest.Get(url);
            yield return req.SendWebRequest();
            if (req.result == UnityWebRequest.Result.ConnectionError || req.result == UnityWebRequest.Result.ProtocolError)
            {
                ServiceLocator.GetService<WarningDialogs>().ShowNewDialog("Perceel data kon niet opgehaald worden");
            }
            else
            {
                var json = JSON.Parse(req.downloadHandler.text);
                ProcessPerceelData(json);
            }
        }

        IEnumerator GetBimCityJson()
        {
            yield return null;

            var urlIfc = Config.activeConfiguration.T3DAzureFunctionURL + $"api/getbimcityjson/{ServiceLocator.GetService<T3DInit>().HTMLData.ModelId}";
            var urlSketchup = Config.activeConfiguration.T3DAzureFunctionURL + $"api/downloadcityjson/{ServiceLocator.GetService<T3DInit>().HTMLData.BlobId}.json";

            var requestUrl = !string.IsNullOrEmpty(ServiceLocator.GetService<T3DInit>().HTMLData.ModelId) ? urlIfc : urlSketchup;

            UnityWebRequest req = UnityWebRequest.Get(requestUrl);

            req.SetRequestHeader("Content-Type", "application/json");

            yield return req.SendWebRequest();
            if (req.result == UnityWebRequest.Result.ConnectionError || req.result == UnityWebRequest.Result.ProtocolError)
            {
                ErrorService.GoToErrorPage(req.error);
            }
            else
            {
                BimCityJsonReceived?.Invoke(req.downloadHandler.text);
            }
        }

        IEnumerator GetBimCityJsonFile()
        {
            yield return null;

            //var filepath = @"F:\T3D\CityJson stuff\data\15ad2866-8d14-44e1-8b2e-2a18275134b6.json";
            //var filepath = @"F:\T3D\CityJson stuff\data\ASP9 - Nieuw.json";
            var filepath = @"/Users/Tom/Documents/TSCD/T3D/sketchup cityjson files/02_2018_uitbouw_components.skp.json";

            //var filepath = @"F:\T3D\CityJson stuff\data\61ae0794bca82a123496d257.json";
            //var filepath = @"F:\T3D\CityJson stuff\data\gebouw_met_uitbouw.json";
            string cityjson = File.ReadAllText(filepath);

            BimCityJsonReceived?.Invoke(cityjson);
        }

        void ProcessPerceelData(JSONNode jsonData)
        {
            JSONNode feature1 = jsonData["features"][0];
            //var perceelGrootte = $"Perceeloppervlakte: {feature1["properties"]["kadastraleGrootteWaarde"]}";

            List<Vector2[]> list = new List<Vector2[]>();

            foreach (JSONNode feature in jsonData["features"])
            {
                List<Vector2> polygonList = new List<Vector2>();

                var coordinates = feature["geometry"]["coordinates"];
                foreach (JSONNode points in coordinates)
                {
                    foreach (JSONNode point in points)
                    {
                        polygonList.Add(new Vector2(point[0], point[1]));
                    }
                }
                list.Add(polygonList.ToArray());
            }

            //PerceelData = list;
            var perceelGrootte = float.Parse(jsonData["features"][0]["properties"]["kadastraleGrootteWaarde"]);
            PerceelDataLoaded?.Invoke(this, new PerceelDataEventArgs(true, list, perceelGrootte));

            //PlaatsUitbouw();
        }

        public void PlaatsUitbouw(Vector3 spawnPosition)
        {
            //var pos = CoordConvert.RDtoUnity(perceelnummerPlaatscoordinaat);
            if (!Uitbouw)
            {
                if (ServiceLocator.GetService<T3DInit>().HTMLData.HasFile)
                {
                    var obj = ServiceLocator.GetService<CityJsonVisualiser>();
                    //obj.VisualizeCityJson();
                    obj.EnableUploadedModel(true);
                    Uitbouw = obj.GetComponentInChildren<UitbouwBase>(true);
                    Uitbouw.transform.position = spawnPosition;
                }
                else
                {
                    var obj = Instantiate(shapableUitbouwPrefab, spawnPosition, Quaternion.identity);
                    Uitbouw = obj.GetComponentInChildren<UitbouwBase>();
                }
            }
            //uitbouwPrefab.SetActive(true);
            //uitbouwPrefab.transform.position = pos;
        }

        private IEnumerator DownloadBuildingData(Vector3RD rd, string id, GameObject buildingGameObject)
        {
            var dataURL = $"{Config.activeConfiguration.buildingsMetaDataPath}/buildings_{rd.x}_{rd.y}.2.2-data";

            ObjectMappingClass data;

            using (UnityWebRequest uwr = UnityWebRequestAssetBundle.GetAssetBundle(dataURL))
            {
                yield return uwr.SendWebRequest();

                if (uwr.result == UnityWebRequest.Result.ConnectionError || uwr.result == UnityWebRequest.Result.ProtocolError)
                {
                    Debug.LogError($"Error getting data file: {uwr.error} {dataURL}");
                }
                else
                {
                    //  Debug.Log($"buildingGameObject:{buildingGameObject.name}");

                    ObjectData objectMapping = buildingGameObject.AddComponent<ObjectData>();
                    AssetBundle newAssetBundle = DownloadHandlerAssetBundle.GetContent(uwr);
                    data = newAssetBundle.LoadAllAssets<ObjectMappingClass>()[0];
                    objectMapping.ids = data.ids;
                    objectMapping.uvs = data.uvs;
                    objectMapping.vectorMap = data.vectorMap;

                    objectMapping.highlightIDs = new List<string>()
                {
                    id
                };

                    //Debug.Log($"hasid:{data.ids.Contains(id)}");

                    newAssetBundle.Unload(true);

                    objectMapping.ApplyDataToIDsTexture();
                    var tileOffset = CoordConvert.RDtoUnity(rd) + new Vector3(500, 0, 500);
                    BuildingMetaDataLoaded?.Invoke(this, new ObjectDataEventArgs(true, objectMapping, tileOffset));
                }

            }
            yield return null;
        }
    }
}

