using ConvertCoordinates;
using Netherlands3D.T3D.Uitbouw;
using SimpleJSON;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using T3D.LoadData;
using T3D.Uitbouw;
//using T3D.LoadData;
using UnityEngine;

public struct CityObjectIdentifier
{
    public string Key;
    public JSONNode Node;
    public int Lod;
    public bool FlipYZ;

    public CityObjectIdentifier(string key, JSONNode node, int lod, bool flipYZ)
    {
        Key = key;
        Lod = lod;
        Node = node;
        FlipYZ = flipYZ;
    }
}

public class CityJsonVisualiser : MonoBehaviour, IUniqueService
{
    public Material MeshMaterial;
    private Vector3RD? perceelCenter;
    private string cityJson = string.Empty;
    private UploadedUitbouw uitbouw;

    public bool HasLoaded { get; private set; }

    [SerializeField]
    private TextAsset testJSON;
    [SerializeField]
    private bool useTestJSON;
    private static string[] definedNodes = { "type", "version", "CityObjects", "vertices", "extensions", "metadata", "transform", "appearance", "geometry-templates" };

    private void Awake()
    {
        uitbouw = GetComponentInChildren<UploadedUitbouw>(true);
    }

    void OnEnable()
    {
        ServiceLocator.GetService<MetadataLoader>().BimCityJsonReceived += OnBimCityJsonReceived;
        ServiceLocator.GetService<MetadataLoader>().PerceelDataLoaded += OnPerceelDataLoaded;
    }

    void OnDisable()
    {
        ServiceLocator.GetService<MetadataLoader>().BimCityJsonReceived -= OnBimCityJsonReceived;
        ServiceLocator.GetService<MetadataLoader>().PerceelDataLoaded -= OnPerceelDataLoaded;
    }

    private void OnPerceelDataLoaded(object source, PerceelDataEventArgs args)
    {
        perceelCenter = new Vector3RD(args.Center.x, args.Center.y, 0);
    }

    private void OnBimCityJsonReceived(string cityJson)
    {
        this.cityJson = cityJson;
    }

    public void VisualizeCityJson()
    {
        StartCoroutine(ParseCityJson(useTestJSON));
    }

    private IEnumerator ParseCityJson(bool useTestJson)
    {
        if (useTestJSON)
            cityJson = testJSON.text;

        yield return new WaitUntil(() => cityJson != string.Empty);

        var meshFilter = uitbouw.MeshFilter;
        var cityJsonModel = new CityJsonModel(cityJson, new Vector3RD(), true);
        var meshes = ParseCityJson(cityJsonModel, meshFilter.transform.localToWorldMatrix, false, false);
        var attributes = GetAttributes(cityJsonModel.cityjsonNode["CityObjects"]);
        AddExtensionNodes(cityJsonModel.cityjsonNode);
        //var combinedMesh = CombineMeshes(meshes.Values.ToList(), meshFilter.transform.localToWorldMatrix);

        var cityObject = meshFilter.gameObject.AddComponent<CityJSONToCityObject>();
        cityObject.SetNodes(meshes, attributes, cityJsonModel.vertices);
        uitbouw.AddCityObject(cityObject);
        uitbouw.ReparentToMainBuilding(RestrictionChecker.ActiveBuilding.GetComponent<CityObject>());
        var highestLod = meshes.Keys.Max(k => k.Lod);
        print("Enabling the highest lod: " + highestLod);
        cityObject.SetMeshActive(highestLod);

        //meshFilter.mesh = combinedMesh;
        //uitbouw.GetComponentInChildren<MeshCollider>().sharedMesh = meshFilter.mesh;

        uitbouw.SetMeshFilter(uitbouw.MeshFilter);

        //center mesh
        var offset = uitbouw.MeshFilter.mesh.bounds.center;
        offset.y -= uitbouw.MeshFilter.mesh.bounds.extents.y;
        offset += uitbouw.transform.forward * uitbouw.Depth / 2;
        uitbouw.MeshFilter.transform.localPosition = -offset;

        //re-initialize the usermovementaxes to ensure the new meshes are dragable
        //EnableUploadedModel(true);

        //var depthOffset = -transform.forward * uitbouw.Depth / 2;
        //var heightOffset = transform.up * ((uitbouw.Height / 2) - Vector3.Distance(uitbouw.CenterPoint, transform.position));
        //uitbouw.MeshFilter.transform.localPosition = depthOffset + heightOffset;

        uitbouw.InitializeUserMovementAxes();

        HasLoaded = true;
    }

    public static void AddExtensionNodes(JSONNode cityjsonNode)
    {
        foreach (var node in cityjsonNode)
        {
            if (definedNodes.Contains(node.Key))
                continue;

            CityJSONFormatter.AddExtensionNode(node.Key, node.Value);
        }
    }

    public static JSONObject GetAttributes(JSONNode cityjsonNode)
    {
        var attributesNode = new JSONObject();
        foreach (KeyValuePair<string, JSONNode> co in cityjsonNode)
        {
            var attributes = co.Value["attributes"];
            foreach (var attr in attributes)
            {
                attributesNode[attr.Key] = attr.Value; //todo: might overwrite attributes due to merge
            }
        }
        return attributesNode;
    }

    public static Dictionary<CityObjectIdentifier, Mesh> ParseCityJson(string cityJson, Matrix4x4 localToWorldMatrix, bool flipYZ, bool useKeytoSetExportIdPrefix)
    {
        var cityJsonModel = new CityJsonModel(cityJson, new Vector3RD(), false);
        return ParseCityJson(cityJsonModel, localToWorldMatrix, flipYZ, useKeytoSetExportIdPrefix);
    }

    public static Dictionary<CityObjectIdentifier, Mesh> ParseCityJson(CityJsonModel cityJsonModel, Matrix4x4 localToWorldMatrix, bool flipYZ, bool useKeytoSetExportIdPrefix)
    {
        //var cityJsonModel = new CityJsonModel(cityJson, new Vector3RD(), false);
        var meshmaker = new CityJsonMeshUtility();

        Dictionary<CityObjectIdentifier, Mesh> meshes = new Dictionary<CityObjectIdentifier, Mesh>();

        foreach (KeyValuePair<string, JSONNode> co in cityJsonModel.cityjsonNode["CityObjects"])
        {
            var key = co.Key;
            if (useKeytoSetExportIdPrefix)
            {
                var bagId = ServiceLocator.GetService<T3DInit>().HTMLData.BagId;
                CityObject.IdPrefix = key.Split(bagId.ToCharArray())[0];
            }
            var geometries = meshmaker.CreateMeshes(key, localToWorldMatrix, cityJsonModel, co.Value, flipYZ);

            foreach (var g in geometries)
            {
                meshes.Add(g.Key, g.Value);
            }
        }

        return meshes;
    }

    public static Mesh CombineMeshes(List<Mesh> meshes, Matrix4x4 localToWorldMatrix)
    {
        //if (meshes.Any())
        //{
        CombineInstance[] combine = new CombineInstance[meshes.Count];

        for (int i = 0; i < meshes.Count; i++)
        {
            combine[i].mesh = meshes[i];
            combine[i].transform = localToWorldMatrix;
        }

        var mesh = new Mesh();
        mesh.CombineMeshes(combine);
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
        return mesh;
        //}
        //return null;
    }

    //void AddMesh(Mesh newMesh)
    //{
    //    var meshFilter = uitbouw.MeshFilter;
    //    CombineInstance[] combine = new CombineInstance[2];
    //    combine[0].mesh = meshFilter.sharedMesh;
    //    combine[0].transform = meshFilter.transform.localToWorldMatrix;
    //    combine[1].mesh = newMesh;
    //    combine[1].transform = meshFilter.transform.localToWorldMatrix;

    //    meshFilter.mesh = new Mesh();
    //    meshFilter.mesh.CombineMeshes(combine);
    //    meshFilter.mesh.RecalculateBounds();
    //    meshFilter.mesh.RecalculateNormals();

    //    uitbouw.GetComponentInChildren<MeshCollider>().sharedMesh = meshFilter.mesh;
    //}

    //void AddMeshGameObject(string name, Mesh mesh)
    //{
    //    GameObject gam = new GameObject(name);
    //    var meshfilter = gam.AddComponent<MeshFilter>();
    //    meshfilter.sharedMesh = mesh;
    //    var meshrenderer = gam.AddComponent<MeshRenderer>();
    //    meshrenderer.material = MeshMaterial;
    //    gam.transform.SetParent(uitbouw.transform);
    //    gam.AddComponent<BoxCollider>();

    //    uitbouw.SetMeshFilter(meshfilter);

    //    var depthOffset = -transform.forward * uitbouw.Depth / 2;
    //    var heightOffset = transform.up * ((uitbouw.Height / 2) - Vector3.Distance(uitbouw.CenterPoint, transform.position));
    //    gam.transform.localPosition = depthOffset + heightOffset;
    //}
}
