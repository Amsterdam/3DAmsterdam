using System;
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

        public PerceelDataEventArgs(bool isLoaded, List<Vector2[]> perceel, float area)
        {
            IsLoaded = isLoaded;
            Perceel = perceel;
            Area = area;
        }
    }

    public class BuildingOutlineEventArgs : EventArgs
    {
        public bool IsLoaded { get; private set; }
        public List<Vector2[]> Outline { get; private set; } //in RD coordinaten

        public BuildingOutlineEventArgs(bool isLoaded, List<Vector2[]> outline)
        {
            IsLoaded = isLoaded;
            Outline = outline;
        }
    }

    public class MetadataLoader : MonoBehaviour
    {
        public static MetadataLoader Instance;

        [SerializeField]
        private Text MainTitle;
        [SerializeField]
        private Transform GeneratedFieldsContainer;
        [SerializeField]
        private GameObject uitbouwPrefab;

        [SerializeField]
        private GameObject buildingsLayer;
        [SerializeField]
        private GameObject terrainLayer;

        private ActionButton btn;

        public delegate void BuildingMetaDataLoadedEventHandler(object source, ObjectDataEventArgs args);
        public event BuildingMetaDataLoadedEventHandler BuildingMetaDataLoaded;

        public delegate void PerceelDataLoadedEventHandler(object source, PerceelDataEventArgs args);
        public event PerceelDataLoadedEventHandler PerceelDataLoaded;

        public delegate void BuildingOutlineLoadedEventHandler(object source, BuildingOutlineEventArgs args);
        public event BuildingOutlineLoadedEventHandler BuildingOutlineLoaded;

        //public List<Vector2[]> PerceelData;

        void Awake()
        {
            Instance = this;
        }

        // Update is called once per frame
        void Start()
        {
            PropertiesPanel.Instance.SetDynamicFieldsTargetContainer(GeneratedFieldsContainer);
            MainTitle.text = "Uitbouw plaatsen";
        }

        public void LoadBuildingData(Vector3RD position, string id)
        {
            //StartCoroutine(HandlePosition(position, id));
            HandlePosition(position, id);
        }

        private void HandlePosition(Vector3RD position, string id)
        {
            StartCoroutine(UpdateSidePanelAddress(id));

            //yield return new WaitForSeconds(1); //todo: replace this with a wait for the tile to be loaded or something similar

            StartCoroutine(GetPerceelData(position));

            StartCoroutine(HighlightBuilding(id));

            StartCoroutine(RequestBuildingOutlineData(id));

        }

        IEnumerator HighlightBuilding(string id)
        {
            Debug.Log($"HighlightBuilding id: {id}");

            yield return null;

            bool hasRD = false;

            Vector3RD rd = new Vector3RD();
            Transform child = null;

            while (!hasRD)
            {
                child = buildingsLayer.transform.GetChild(0);
                rd = child.name.GetRDCoordinate();
                if (rd.x != 0) hasRD = true;
                yield return null;
            }

            StartCoroutine(DownloadBuildingData(rd, id, child.gameObject));
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

                foreach (JSONNode feature in json["pand"]["geometrie"]["coordinates"])
                {
                    List<Vector2> polygonList = new List<Vector2>();

                    foreach (JSONNode point in feature)
                    {
                        polygonList.Add(new Vector2(point[0], point[1]));
                    }
                    list.Add(polygonList.ToArray());
                }
                //print("outline loaded");
                BuildingOutlineLoaded?.Invoke(this, new BuildingOutlineEventArgs(true, list));
            }
        }

        IEnumerator UpdateSidePanelAddress(string bagId)
        {
            Debug.Log($"UpdateSidePanelAddress");

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
                    PropertiesPanel.Instance.AddTextfield($"{kortenaam} {huisnummer}\n{postcode} {plaats}");
                }

                print(req.downloadHandler.text);
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
                UpdateSidePanelPerceelData(json);
                ProcessPerceelData(json);
            }
        }

        void ProcessPerceelData(JSONNode jsonData)
        {
            //var json = JSON.Parse(jsonData);
            List<Vector2[]> list = new List<Vector2[]>();

            //yield return null;
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
            PerceelDataLoaded?.Invoke(this, new PerceelDataEventArgs(true, list, perceelGrootte));
        }

        void UpdateSidePanelPerceelData(JSONNode json)
        {
            Debug.Log("UpdateSidePanelPerceelData");

            JSONNode feature = json["features"][0];

            var perceelGrootte = $"Perceeloppervlakte: {feature["properties"]["kadastraleGrootteWaarde"]} m2";
            PropertiesPanel.Instance.AddLabel(perceelGrootte);

            btn = PropertiesPanel.Instance.AddActionButtonBigRef("Plaats uitbouw", (action) => //todo: move this to uitbouw, but we need the position
            {
                var rd = new Vector2RD(feature["properties"]["perceelnummerPlaatscoordinaatX"], feature["properties"]["perceelnummerPlaatscoordinaatY"]);
                PlaatsUitbouw(rd);
                Destroy(btn);
            });
        }

        public void PlaatsUitbouw(Vector2RD rd)
        {
            var pos = CoordConvert.RDtoUnity(rd);
            uitbouwPrefab.SetActive(true);
            uitbouwPrefab.transform.position = pos;
            //uitbouwPrefab.GetComponent<Uitbouw>().SetPerceel(PerceelData);
            //var uitbouw = Instantiate(uitbouwPrefab, pos, Quaternion.identity);
            //uitbouw.GetComponent<Uitbouw>().SetPerceel(PerceelData);
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
