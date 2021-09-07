using ConvertCoordinates;
using Netherlands3D;
using Netherlands3D.Interface;
using Netherlands3D.Interface.SidePanel;
using Netherlands3D.LayerSystem;
using System;
using SimpleJSON;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.ProBuilder;
using UnityEngine.ProBuilder.MeshOperations;
using UnityEngine.UI;
using Netherlands3D.Interface.Layers;

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

public class PerceelRenderer : MonoBehaviour
{
    public Material LineMaterial;
    public static PerceelRenderer Instance;
    public GameObject TerrainLayer;
    public GameObject BuildingsLayer;
    public GameObject HuisRichtingCube;
    public GameObject Uitbouw;
    public InterfaceLayer BuildingInterfaceLayer;

    [SerializeField]
    private Text MainTitle;

    [SerializeField]
    private Transform GeneratedFieldsContainer;

    public delegate void BuildingMetaDataLoadedEventHandler(object source, ObjectDataEventArgs args);
    public event BuildingMetaDataLoadedEventHandler BuildingMetaDataLoaded;

    private float terrainFloor;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        PropertiesPanel.Instance.SetDynamicFieldsTargetContainer(GeneratedFieldsContainer);
        MainTitle.text = "Uitbouw plaatsen";

        PropertiesPanel.Instance.AddSpacer();
        PropertiesPanel.Instance.AddActionCheckbox("Toon alle gebouwen", true, (action) =>
        {
            BuildingInterfaceLayer.ToggleLinkedObject(action);           
        });
        PropertiesPanel.Instance.AddSpacer();
    }

    public IEnumerator GetAndRenderPerceel(Vector3RD position)
    {
        yield return null;

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
            List<Vector2[]> list = new List<Vector2[]>();

            var json = JSON.Parse(req.downloadHandler.text);

            yield return null;

            UpdateSidePanelPerceelData(json);

            foreach (JSONNode feature in json["features"])
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


            foreach (Transform gam in TerrainLayer.transform)
            {
                // Debug.Log(gam.name);
                gam.gameObject.AddComponent<MeshCollider>();
            }

            yield return null;

            Netherlands3D.T3D.Test.Uitbouw.perceel = list;
            StartCoroutine(RenderPolygons(list));
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

            foreach(JSONObject adres in addresses)
            {
                var kortenaam = adres["korteNaam"].Value;
                var huisnummer = adres["huisnummer"].Value;
                var postcode = adres["postcode"].Value;
                var plaats = adres["woonplaatsNaam"].Value;
                PropertiesPanel.Instance.AddTextfield($"{kortenaam} {huisnummer}\n{postcode} {plaats}");
            }
        }

    }

    ActionButton btn;
    Transform uitbouwTransform;
    Vector3 startPosition;

    void UpdateSidePanelPerceelData(JSONNode json)
    {
        Debug.Log("UpdateSidePanelPerceelData");

        JSONNode feature = json["features"][0];

        var perceelGrootte = $"Perceeloppervlakte: {feature["properties"]["kadastraleGrootteWaarde"]} m2";
        PropertiesPanel.Instance.AddLabel(perceelGrootte);

        btn = PropertiesPanel.Instance.AddActionButtonBigRef("Plaats uitbouw", (action) =>
        {
            //var rd = new Vector2RD(feature["properties"]["perceelnummerPlaatscoordinaatX"], feature["properties"]["perceelnummerPlaatscoordinaatY"]);
            //var pos = CoordConvert.RDtoUnity(rd);
            
            var uitbouw = Instantiate(Uitbouw);           
            
            uitbouwTransform = uitbouw.transform;

            //snap uitbouw tegen gebouw
            uitbouw.transform.rotation = HuisRichtingCube.transform.rotation;
            var halfHuisRichtingCube = (HuisRichtingCube.transform.localScale.z / 2);
            var halfUitbouw = (uitbouw.transform.localScale.z / 2);
            var snappedPos = HuisRichtingCube.transform.position - (HuisRichtingCube.transform.forward * (halfHuisRichtingCube + halfUitbouw));
            snappedPos.y = terrainFloor + uitbouw.transform.localScale.y / 2;
            uitbouw.transform.position = snappedPos;
            startPosition = snappedPos;

            Invoke("RemoveButton", 0.3f);

            //PropertiesPanel.Instance.AddLabel("Draai de uitbouw");
            //PropertiesPanel.Instance.AddActionSlider("", "", 0, 1, 0.5f, (value) => {
            //    uitbouwTransform.rotation = Quaternion.Euler(0, value * 360, 0);
            //}, false, "Draai de uitbouw");

            //PropertiesPanel.Instance.AddLabel("Verplaats de uitbouw");
            //PropertiesPanel.Instance.AddActionSlider("", "", -10, 10, 0, (value) =>
            //{
            //    uitbouwTransform.position = startPosition + (uitbouwTransform.forward * value);
            //}, false, "Draai de uitbouw");
        });
        
        /*
        var perceelGrootte = $"Perceeloppervlakte: {wfs.features[0].properties.kadastraleGrootteWaarde} m2";
        PropertiesPanel.Instance.AddLabel(perceelGrootte);

        btn = PropertiesPanel.Instance.AddActionButtonBigRef("Plaats uitbouw", (action) =>
        {
            var rd = new Vector2RD(wfs.features[0].properties.perceelnummerPlaatscoordinaatX, wfs.features[0].properties.perceelnummerPlaatscoordinaatY);
            var pos = CoordConvert.RDtoUnity(rd);
            InstantiateUitbouw(pos);

        });*/
    }

    private void InstantiateUitbouw(Vector3 pos)
    {
        pos.y = terrainFloor + Uitbouw.transform.localScale.y / 2;
        var uitbouw = Instantiate(Uitbouw, pos, Quaternion.identity);
        //startPosition = pos;
        //uitbouwTransform = uitbouw.transform;

        Invoke("RemoveButton", 0.3f); //todo: why with delay?
    }

    void RemoveButton()
    {
        Destroy(btn.gameObject);
    }

    public IEnumerator HandlePosition(Vector3RD position, string id)
    {

        StartCoroutine(UpdateSidePanelAddress(id));

        yield return new WaitForSeconds(1);

        yield return StartCoroutine(GetAndRenderPerceel(position));

        yield return StartCoroutine(HighlightBuilding(id));        

    }

    IEnumerator HighlightBuilding(string id)
    {
        Debug.Log($"HighlightBuilding id: {id}");
        
        yield return null;

        var child = BuildingsLayer.transform.GetChild(0);
        var rd = child.name.GetRDCoordinate();

        StartCoroutine(DownloadObjectData(rd, id, child.gameObject));

    }

    IEnumerator HighlightBuildingArea(List<Vector2[]> polygons, Vector3 position)
    {
        yield return null;
        var q = from i in polygons
                from p in i
                select CoordConvert.RDtoUnity(p) into v3
                select new Vector2(v3.x, v3.z);

        var polyPoints = q.ToArray();

        foreach (var point in polyPoints)
        {
          //  Debug.Log($"pointx:{point.x} pointy:{point.y}"); //unity absolute 
        }

        foreach (Transform gam in BuildingsLayer.transform)
        {
            var rd = gam.name.GetRDCoordinate();
            var tile = CoordConvert.RDtoUnity(rd);
            //Debug.Log($"tilex: {tile.x} tilez: {tile.z}");

            var filter = gam.GetComponent<MeshFilter>();
            var verts = filter.mesh.vertices;

            var xmin = tile.x + verts.Min(o => o.x) + 500;
            var xmax = tile.x + verts.Max(o => o.x) + 500;
            var zmin = tile.z + verts.Min(o => o.z) + 500;
            var zmax = tile.z + verts.Max(o => o.z) + 500;

            //Debug.Log($"xmin:{xmin} xmax:{xmax} zmin:{zmin} zmax:{zmax}");  //unity absolute ( tile + relative verts pos)

            Color[] colors = new Color[filter.mesh.vertexCount];

            for (int i = 0; i < filter.mesh.vertexCount; i++)
            {
                var vertx = tile.x + verts[i].x + 500;
                var verty = tile.z + verts[i].z + 500;

                //var dist = Vector2.Distance(polyPoints[0], new Vector2(vertx, verty) );
                //distances.Add(dist);
                //check if vertexpoint is in the polygon, if so color it
                if (ContainsPoint(polyPoints, new Vector2(vertx, verty)))   //unity absolute
                //if (dist < 100)
                {
                    //colors[i] = new Color(1, (dist / 100), (dist / 100));
                    colors[i] = Color.red;
                }
                else
                {
                    colors[i] = Color.white;
                }

            }

            filter.mesh.colors = colors;

            //Debug.Log($"building gameobject: {gam.name}"  );
        }
    }

    void RenderPolygonMesh(List<Vector2[]> polygons)
    {
        var perceelGameObject = new GameObject();

        var go = new GameObject();
        go.name = "Perceel mesh";
        go.transform.parent = perceelGameObject.transform;
        ProBuilderMesh m_Mesh = go.gameObject.AddComponent<ProBuilderMesh>();
        go.GetComponent<MeshRenderer>().material = LineMaterial;

        List<Vector3> verts = new List<Vector3>();

        foreach (var points in polygons)
        {
            foreach (var point in points)
            {
                var pos = CoordConvert.RDtoUnity(point);
                verts.Add(pos);
            }
        }

        m_Mesh.CreateShapeFromPolygon(verts.ToArray(), 5, false);  // TODO place op top of maaiveld            


        var go_rev = new GameObject();
        go_rev.transform.parent = perceelGameObject.transform;
        go_rev.name = "Perceel mesh_rev";
        ProBuilderMesh m_Mesh_rev = go_rev.gameObject.AddComponent<ProBuilderMesh>();
        go_rev.GetComponent<MeshRenderer>().material = LineMaterial;
        verts.Reverse();
        m_Mesh_rev.CreateShapeFromPolygon(verts.ToArray(), 5, false);

    }

    IEnumerator RenderPolygons(List<Vector2[]> polygons)
    {

        List<Vector2> vertices = new List<Vector2>();
        List<int> indices = new List<int>();

        int count = 0;
        foreach (var list in polygons)
        {
            for (int i = 0; i < list.Length - 1; i++)
            {
                indices.Add(count + i);
                indices.Add(count + i + 1);            
            }
            count += list.Length;
            vertices.AddRange(list);
        }

        GameObject gam = new GameObject();
        gam.name = "Perceel";
        gam.transform.parent = transform;

        MeshFilter filter = gam.AddComponent<MeshFilter>();
        gam.AddComponent<MeshRenderer>().material = LineMaterial;

        var verts = vertices.Select(o => CoordConvert.RDtoUnity(o)).ToArray();

        List<float> terrainFloorPoints = new List<float>();

        //use collider to place the polygon points on the terrain
        for(int i=0; i < verts.Length; i++)
        {
            var point = verts[i];
            var from = new Vector3(point.x, point.y + 10, point.z);


            Ray ray = new Ray(from, Vector3.down);
            RaycastHit hit;

            yield return null;

            //raycast from the polygon point to hit the terrain so we can place the outline so that it is visible
            if(Physics.Raycast( ray, out hit, Mathf.Infinity  ))
            {
                //Debug.Log("we have a hit");
                verts[i].y = hit.point.y + 0.5f;
                terrainFloorPoints.Add(hit.point.y);
            }
            else
            {
                Debug.Log("raycast failed..");
            }
        }

        terrainFloor = terrainFloorPoints.Average();

        var mesh = new Mesh();
        mesh.vertices = verts;
        mesh.SetIndices(indices.ToArray(), MeshTopology.Lines, 0);
        filter.sharedMesh = mesh;
    }


    public static bool ContainsPoint(Vector2[] polyPoints, Vector2 p)
    {
        var j = polyPoints.Length - 1;
        var inside = false;
        for (int i = 0; i < polyPoints.Length; j = i++)
        {
            var pi = polyPoints[i];
            var pj = polyPoints[j];
            if (((pi.y <= p.y && p.y < pj.y) || (pj.y <= p.y && p.y < pi.y)) &&
                (p.x < (pj.x - pi.x) * (p.y - pi.y) / (pj.y - pi.y) + pi.x))
                inside = !inside;
        }
        return inside;
    }

    //private IEnumerator DownloadObjectData(GameObject obj, int vertexIndex, System.Action<string> callback)
    private IEnumerator DownloadObjectData( Vector3RD rd, string id, GameObject buildingGameObject )
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
                var tileOffset = ConvertCoordinates.CoordConvert.RDtoUnity(rd) + new Vector3(500, 0, 500);
                BuildingMetaDataLoaded?.Invoke(this, new ObjectDataEventArgs(true, objectMapping, tileOffset));
            }
            
        }        
        yield return null;
        
    }


}
