﻿using ConvertCoordinates;
using Netherlands3D.T3D.Uitbouw;
using SimpleJSON;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using T3D.LoadData;
//using T3D.LoadData;
using UnityEngine;

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

    private void Awake()
    {
        uitbouw = GetComponentInChildren<UploadedUitbouw>(true);
        uitbouw.gameObject.SetActive(false);
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
        //EnableUploadedModel(true);
    }

    private IEnumerator ParseCityJson(bool useTestJson)
    {
        yield return new WaitUntil(() => cityJson != string.Empty);

        if (useTestJSON)
            cityJson = testJSON.text;

        //var meshmaker = new CityJsonMeshUtility();
        //foreach (KeyValuePair<string, JSONNode> co in cityJsonModel.cityjsonNode["CityObjects"])
        //{
        //var key = co.Key;
        //var mesh = meshmaker.CreateMesh(transform.localToWorldMatrix, cityJsonModel, co.Value, false);

        var meshFilter = uitbouw.MeshFilter;
        var cityJsonModel = new CityJsonModel(cityJson, new Vector3RD(), true);
        var meshes = ParseCityJson(cityJsonModel, transform.localToWorldMatrix);

        foreach (var pair in meshes)
        {
            var mesh = pair.Value;
            if (mesh != null)
            {
                //AddMesh(mesh);
                var newCityObject = meshFilter.gameObject.AddComponent<CityJSONToCityObject>();
                newCityObject.SetNode(pair.Key, cityJsonModel.vertices);
                uitbouw.AddCityObject(newCityObject);
                //AddMeshGameObject(key, mesh);
            }
        }
        var combinedMesh = CombineMeshes(meshes.Values.ToList(), meshFilter.transform.localToWorldMatrix);


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

    public static Dictionary<JSONNode, Mesh> ParseCityJson(string cityJson, Matrix4x4 localToWorldMatrix)
    {
        var cityJsonModel = new CityJsonModel(cityJson, new Vector3RD(), false);
        return ParseCityJson(cityJsonModel, localToWorldMatrix);
    }

    public static Dictionary<JSONNode, Mesh> ParseCityJson(CityJsonModel cityJsonModel, Matrix4x4 localToWorldMatrix)
    {
        //var cityJsonModel = new CityJsonModel(cityJson, new Vector3RD(), false);
        var meshmaker = new CityJsonMeshUtility();

        Dictionary<JSONNode, Mesh> meshes = new Dictionary<JSONNode, Mesh>();

        foreach (KeyValuePair<string, JSONNode> co in cityJsonModel.cityjsonNode["CityObjects"])
        {
            //var key = co.Key;
            var mesh = meshmaker.CreateMesh(localToWorldMatrix, cityJsonModel, co.Value, true);
            if (mesh != null)
            {
                meshes.Add(co.Value, mesh);
            }
        }
        return meshes;
    }

    public static Mesh CombineMeshes(List<Mesh> meshes, Matrix4x4 localToWorldMatrix)
    {
        if (meshes.Any())
        {
            CombineInstance[] combine = new CombineInstance[meshes.Count];

            for (int i = 0; i < meshes.Count; i++)
            {
                combine[i].mesh = meshes[i];
                combine[i].transform = localToWorldMatrix;
            }

            var mesh = new Mesh();
            mesh.CombineMeshes(combine);
            return mesh;
        }
        return null;
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

    void AddMeshGameObject(string name, Mesh mesh)
    {
        GameObject gam = new GameObject(name);
        var meshfilter = gam.AddComponent<MeshFilter>();
        meshfilter.sharedMesh = mesh;
        var meshrenderer = gam.AddComponent<MeshRenderer>();
        meshrenderer.material = MeshMaterial;
        gam.transform.SetParent(uitbouw.transform);
        gam.AddComponent<BoxCollider>();

        uitbouw.SetMeshFilter(meshfilter);

        var depthOffset = -transform.forward * uitbouw.Depth / 2;
        var heightOffset = transform.up * ((uitbouw.Height / 2) - Vector3.Distance(uitbouw.CenterPoint, transform.position));
        gam.transform.localPosition = depthOffset + heightOffset;
    }

    public void EnableUploadedModel(bool enable)
    {
        uitbouw.gameObject.SetActive(enable);
        uitbouw.GetComponent<UitbouwMovement>().enabled = enable;
        uitbouw.GetComponent<UitbouwMeasurement>().enabled = enable;
    }
}
