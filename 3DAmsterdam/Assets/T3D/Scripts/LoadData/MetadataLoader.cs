using System;
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

        public Vector2RD PerceelnummerPlaatscoordinaat;

        public PerceelDataEventArgs(bool isLoaded, List<Vector2[]> perceel, float area, Vector2RD perceelnummerPlaatscoordinaat)
        {
            IsLoaded = isLoaded;
            Perceel = perceel;
            Area = area;
            PerceelnummerPlaatscoordinaat = perceelnummerPlaatscoordinaat;
        }
    }

    public class BuildingOutlineEventArgs : EventArgs
    {
        public bool IsLoaded { get; private set; }
        public List<Vector2[]> Outline { get; private set; } //in RD coordinaten
        public float TotalArea { get; private set; }

        public BuildingOutlineEventArgs(bool isLoaded, List<Vector2[]> outline, float totalArea)
        {
            IsLoaded = isLoaded;
            Outline = outline;
            TotalArea = totalArea;
        }
    }

    public class AdressDataEventArgs : EventArgs
    {
        public string Straat { get; private set; }

        public string Huisnummer { get; private set; }

        public string Plaats { get; private set; }

        public string Postcode { get; private set; }

        public AdressDataEventArgs(string kortenaam, string huisnummer, string postcode, string plaats)
        {
            Straat = kortenaam;
            Huisnummer = huisnummer;
            Postcode = postcode;
            Plaats = plaats;
        }
    }

    public class AdresUitgebreidDataEventArgs : EventArgs
    {
        public Vector2RD Coordinate { get; private set; }

        public AdresUitgebreidDataEventArgs(Vector2RD coordinate)
        {
            Coordinate = coordinate;
        }
    }

    public class MetadataLoader : MonoBehaviour
    {
        public static MetadataLoader Instance;

        public Vector3RD PositionRD;

        [SerializeField]
        private GameObject shapableUitbouwPrefab;
        [SerializeField]
        private GameObject uploadedUitbouwPrefab;

        [SerializeField]
        private GameObject buildingsLayer;
        [SerializeField]
        private GameObject terrainLayer;

        public delegate void BuildingMetaDataLoadedEventHandler(object source, ObjectDataEventArgs args);
        public event BuildingMetaDataLoadedEventHandler BuildingMetaDataLoaded;

        public delegate void PerceelDataLoadedEventHandler(object source, PerceelDataEventArgs args);
        public event PerceelDataLoadedEventHandler PerceelDataLoaded;

        public delegate void AddressLoadedEventHandler(object source, AdressDataEventArgs args);
        public event AddressLoadedEventHandler AddressLoaded;

        public delegate void MonumentEventHandler(bool isMonument);
        public event MonumentEventHandler IsMonumentEvent;

        public delegate void AdresUitgebreidLoadedEventHandler(object source, AdresUitgebreidDataEventArgs args);
        public event AdresUitgebreidLoadedEventHandler AdresUitgebreidLoaded;

        public delegate void BeschermdEventHandler(bool isBeschermd);
        public event BeschermdEventHandler IsBeschermdEvent;

        public delegate void BuildingOutlineLoadedEventHandler(object source, BuildingOutlineEventArgs args);
        public event BuildingOutlineLoadedEventHandler BuildingOutlineLoaded;

        public delegate void BimStatusEventHandler(string status, string modelId);
        public event BimStatusEventHandler BimStatus;

        public delegate void BimCityJsonEventHandler(string cityJson);
        public event BimCityJsonEventHandler BimCityJsonReceived;


        //public List<Vector2[]> PerceelData;
        private string postcode;
        private string huisnummer;

        public Vector2RD perceelnummerPlaatscoordinaat;
        private Vector2RD buildingcenter;

        public bool UploadedModel;

        public string BimModelId;
        public string BimModelVersionId;
        public string BlobId;

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

            Instance = this;
            AddressLoaded += OnAddressLoaded;
        }

        private void OnAddressLoaded(object source, AdressDataEventArgs args)
        {
            Debug.Log($"adres geladen: {args.Straat} {args.Huisnummer} {args.Postcode} {args.Plaats}");
            StartCoroutine(GetAdressenUitgebreid(args.Postcode, args.Huisnummer));
        }

        public void RequestBuildingData(Vector3RD position, string id)
        {
            if (UploadedModel)
            {
                //StartCoroutine(GetBimStatus(BimModelId));
                StartCoroutine(GetBimCityJson());
                //StartCoroutine(GetBimCityJsonFile());
            }

            StartCoroutine(UpdateSidePanelAddress(id));

            //yield return new WaitForSeconds(1); //todo: replace this with a wait for the tile to be loaded or something similar

            StartCoroutine(GetPerceelData(position));

            StartCoroutine(HighlightBuilding(position, id));

            StartCoroutine(GetMonumentStatus(position));

            StartCoroutine(GetBeschermdStatus(position));

            StartCoroutine(RequestBuildingOutlineData(id));

        }

        IEnumerator HighlightBuilding(Vector3RD position, string id)
        {
            Transform buildingTile = null;
            Vector3RD tegelRD = new Vector3RD();

            yield return new WaitUntil(
                () =>
                {
                    if (buildingsLayer.transform.childCount > 0)
                    {
                        foreach (Transform tile in buildingsLayer.transform)
                        {
                            tegelRD = tile.name.GetRDCoordinate();

                            if (tegelRD.x == 0) return false;

                            if (position.x >= tegelRD.x && position.x < tegelRD.x + 1000 && position.y >= tegelRD.y && position.y < tegelRD.y + 1000)
                            {
                                buildingTile = tile;
                                return true;
                            }
                        }
                        return false;
                    }
                    return false;
                }
            );
            StartCoroutine(DownloadBuildingData(tegelRD, id, buildingTile.gameObject));
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
                WarningDialogs.Instance.ShowNewDialog("Pand data kon niet opgehaald worden");
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

        IEnumerator UpdateSidePanelAddress(string bagId)
        {

            List<Vector2[]> list = new List<Vector2[]>();
            var builtArea = 0f;

            var url = $"https://api.bag.kadaster.nl/lvbag/individuelebevragingen/v2/adressen?pandIdentificatie={bagId}";

            UnityWebRequest req = UnityWebRequest.Get(url);
            req.SetRequestHeader("X-Api-Key", "l772bb9814e5584919b36a91077cdacea7");
            req.SetRequestHeader("Accept-Crs", "epsg:28992");

            yield return req.SendWebRequest();

            if (req.result == UnityWebRequest.Result.ConnectionError || req.result == UnityWebRequest.Result.ProtocolError)
            {
                WarningDialogs.Instance.ShowNewDialog("Perceel data kon niet opgehaald worden");
            }
            else
            {
                var json = JSON.Parse(req.downloadHandler.text);
                var addresses = json["_embedded"]["adressen"];

                foreach (JSONObject adres in addresses)
                {
                    var kortenaam = adres["korteNaam"].Value;
                    var huisnummer = adres["huisnummer"].Value;
                    var postcode = adres["postcode"].Value;
                    var plaats = adres["woonplaatsNaam"].Value;

                    AddressLoaded?.Invoke(this, new AdressDataEventArgs(kortenaam, huisnummer, postcode, plaats));
                }
            }
        }

        IEnumerator GetAdressenUitgebreid(string postcode, string huisnummer)
        {
            var url = $"https://api.bag.kadaster.nl/lvbag/individuelebevragingen/v2/adressenuitgebreid?postcode={postcode}&huisnummer={huisnummer}&exacteMatch=true";

            UnityWebRequest req = UnityWebRequest.Get(url);
            req.SetRequestHeader("X-Api-Key", "l772bb9814e5584919b36a91077cdacea7");
            req.SetRequestHeader("Accept-Crs", "epsg:28992");

            yield return req.SendWebRequest();

            if (req.result == UnityWebRequest.Result.ConnectionError || req.result == UnityWebRequest.Result.ProtocolError)
            {
                WarningDialogs.Instance.ShowNewDialog("Adressenuitgebreid data kon niet opgehaald worden");
            }
            else
            {
                var json = JSON.Parse(req.downloadHandler.text);
                var addresses = json["_embedded"]["adressen"];

                foreach (JSONObject adres in addresses)
                {
                    var point = adres["adresseerbaarObjectGeometrie"]["punt"]["coordinates"].AsArray;

                    Vector2RD building_center = new Vector2RD(point[0], point[1]);
                    AdresUitgebreidLoaded?.Invoke(this, new AdresUitgebreidDataEventArgs(building_center));
                }

            }
        }

        IEnumerator GetPerceelData(Vector3RD position)
        {
            //yield return null;

            Debug.Log($"GetAndRenderPerceel x:{position.x} y:{position.y}");

            var bbox = $"{ position.x - 0.5},{ position.y - 0.5},{ position.x + 0.5},{ position.y + 0.5}";
            var url = $"https://geodata.nationaalgeoregister.nl/kadastralekaart/wfs/v4_0?SERVICE=WFS&REQUEST=GetFeature&VERSION=2.0.0&TYPENAMES=kadastralekaartv4:perceel&STARTINDEX=0&COUNT=1&SRSNAME=urn:ogc:def:crs:EPSG::28992&BBOX={bbox},urn:ogc:def:crs:EPSG::28992&outputFormat=json";

            UnityWebRequest req = UnityWebRequest.Get(url);
            yield return req.SendWebRequest();
            if (req.result == UnityWebRequest.Result.ConnectionError || req.result == UnityWebRequest.Result.ProtocolError)
            {
                WarningDialogs.Instance.ShowNewDialog("Perceel data kon niet opgehaald worden");
            }
            else
            {
                var json = JSON.Parse(req.downloadHandler.text);
                ProcessPerceelData(json);
            }
        }

        IEnumerator GetMonumentStatus(Vector3RD position)
        {
            yield return null;

            Debug.Log($"GetAndRenderPerceel x:{position.x} y:{position.y}");

            var bbox = $"{ position.x - 0.5},{ position.y - 0.5},{ position.x + 0.5},{ position.y + 0.5}";
            var url = $"https://services.rce.geovoorziening.nl/rce/wfs?SERVICE=WFS&REQUEST=GetFeature&VERSION=2.0.0&TYPENAMES=rce:NationalListedMonuments&STARTINDEX=0&COUNT=1&SRSNAME=EPSG:28992&BBOX={bbox}&outputFormat=json";

            UnityWebRequest req = UnityWebRequest.Get(url);
            yield return req.SendWebRequest();
            if (req.result == UnityWebRequest.Result.ConnectionError || req.result == UnityWebRequest.Result.ProtocolError)
            {
                WarningDialogs.Instance.ShowNewDialog("MonumentStatus data kon niet opgehaald worden");
            }
            else
            {
                var json = JSON.Parse(req.downloadHandler.text);
                var isMonument = json["features"].Linq.Any();

                IsBeschermdEvent?.Invoke(isMonument);
            }
        }

        IEnumerator GetBeschermdStatus(Vector3RD position)
        {
            yield return null;

            Debug.Log($"GetAndRenderPerceel x:{position.x} y:{position.y}");

            var bbox = $"{ position.x - 0.5},{ position.y - 0.5},{ position.x + 0.5},{ position.y + 0.5}";
            var url = $"https://services.rce.geovoorziening.nl/rce/wfs?SERVICE=WFS&REQUEST=GetFeature&VERSION=2.0.0&TYPENAMES=rce:ArcheologicalMonuments&STARTINDEX=0&COUNT=1&SRSNAME=EPSG:28992&BBOX={bbox}&outputFormat=json";

            UnityWebRequest req = UnityWebRequest.Get(url);
            yield return req.SendWebRequest();
            if (req.result == UnityWebRequest.Result.ConnectionError || req.result == UnityWebRequest.Result.ProtocolError)
            {
                WarningDialogs.Instance.ShowNewDialog("Perceel data kon niet opgehaald worden");
            }
            else
            {
                var json = JSON.Parse(req.downloadHandler.text);
                var isBeschermd = json["features"].Linq.Any();

                IsBeschermdEvent?.Invoke(isBeschermd);
            }
        }

        IEnumerator GetBimStatus(string modelId)
        {
            yield return null;
            
            var url = $"https://t3dapi.azurewebsites.net/api/getbimversionstatus/{modelId}";

            UnityWebRequest req = UnityWebRequest.Get(url);            
            req.SetRequestHeader("Content-Type", "application/json");

            yield return req.SendWebRequest();
            if (req.result == UnityWebRequest.Result.ConnectionError || req.result == UnityWebRequest.Result.ProtocolError)
            {
                WarningDialogs.Instance.ShowNewDialog(req.error);
            }
            else
            {
                var json = JSON.Parse(req.downloadHandler.text);
                var status = json["conversions"]["cityjson"];
                Debug.Log($"Status CityJSON:{status}");

                BimStatus?.Invoke(status, modelId);

            }
        }

        IEnumerator GetBimCityJsonFile()
        {
            yield return null;

            //var filepath = @"F:\T3D\CityJson stuff\data\15ad2866-8d14-44e1-8b2e-2a18275134b6.json";
            //var filepath = @"F:\T3D\CityJson stuff\data\ASP9 - Nieuw.json";
            var filepath = @"F:\T3D\Data\Sketchup\cityjson\v3\01_2018_layers.skp.json";

            //var filepath = @"F:\T3D\CityJson stuff\data\61ae0794bca82a123496d257.json";
            //var filepath = @"F:\T3D\CityJson stuff\data\gebouw_met_uitbouw.json";
            string cityjson = File.ReadAllText(filepath);

            BimCityJsonReceived?.Invoke(cityjson);
        }

        IEnumerator GetBimCityJson()
        {
            yield return null;

            var urlIfc = $"https://t3dapi.azurewebsites.net/api/getbimcityjson/{BimModelId}";
            var urlSketchup = $"https://t3dapi.azurewebsites.net/api/downloadcityjson/{BlobId}.json";

            UnityWebRequest req = UnityWebRequest.Get(string.IsNullOrEmpty(BimModelId) == false ? urlIfc : urlSketchup);
            req.SetRequestHeader("Content-Type", "application/json");

            yield return req.SendWebRequest();
            if (req.result == UnityWebRequest.Result.ConnectionError || req.result == UnityWebRequest.Result.ProtocolError)
            {
                WarningDialogs.Instance.ShowNewDialog(req.error);
            }
            else
            {
                BimCityJsonReceived?.Invoke(req.downloadHandler.text);
            }
        }

        void ProcessPerceelData(JSONNode jsonData)
        {
            JSONNode feature1 = jsonData["features"][0];
            //var perceelGrootte = $"Perceeloppervlakte: {feature1["properties"]["kadastraleGrootteWaarde"]}";

            perceelnummerPlaatscoordinaat = new Vector2RD(feature1["properties"]["perceelnummerPlaatscoordinaatX"], feature1["properties"]["perceelnummerPlaatscoordinaatY"]);

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


            foreach (Transform gam in terrainLayer.transform)
            {
                // Debug.Log(gam.name);
                gam.gameObject.AddComponent<MeshCollider>();
            }

            //PerceelData = list;
            var perceelGrootte = float.Parse(jsonData["features"][0]["properties"]["kadastraleGrootteWaarde"]);
            PerceelDataLoaded?.Invoke(this, new PerceelDataEventArgs(true, list, perceelGrootte, perceelnummerPlaatscoordinaat));

            //PlaatsUitbouw();
        }

        public void PlaatsUitbouw()
        {
            var pos = CoordConvert.RDtoUnity(perceelnummerPlaatscoordinaat);
            if (UploadedModel && !Uitbouw)
            {
                var obj = Instantiate(uploadedUitbouwPrefab, pos, Quaternion.identity);
                Uitbouw = obj.GetComponentInChildren<UitbouwBase>();
            }
            else if (!Uitbouw)
            {
                var obj = Instantiate(shapableUitbouwPrefab, pos, Quaternion.identity);
                Uitbouw = obj.GetComponentInChildren<UitbouwBase>();
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

