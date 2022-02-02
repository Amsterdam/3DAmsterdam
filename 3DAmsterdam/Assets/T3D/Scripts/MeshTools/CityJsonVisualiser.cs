using ConvertCoordinates;
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


public class CityJsonVisualiser : MonoBehaviour
{
    public Material MeshMaterial;
    private Vector3RD? perceelCenter;
    private string cityJson;
    private UploadedUitbouw uitbouw;

    public static CityJsonVisualiser Instance;

    private void Awake()
    {
        Instance = this;
        uitbouw = GetComponentInChildren<UploadedUitbouw>(true);
        uitbouw.gameObject.SetActive(false);
    }

    void OnEnable()
    {
        MetadataLoader.Instance.BimCityJsonReceived += OnBimCityJsonReceived;
        MetadataLoader.Instance.PerceelDataLoaded += OnPerceelDataLoaded;
    }

    void OnDisable()
    {
        MetadataLoader.Instance.BimCityJsonReceived -= OnBimCityJsonReceived;
        MetadataLoader.Instance.PerceelDataLoaded -= OnPerceelDataLoaded;
    }

    private void OnPerceelDataLoaded(object source, PerceelDataEventArgs args)
    {
        perceelCenter = new Vector3RD(args.PerceelnummerPlaatscoordinaat.x, args.PerceelnummerPlaatscoordinaat.y, 0);

        //if (!string.IsNullOrEmpty(this.cityJson))
        //{
        //    VisualizeCityJson();
        //    EnableUploadedModel(false);
        //}
    }

    private void OnBimCityJsonReceived(string cityJson)
    {
        this.cityJson = cityJson;

        //if (centerPerceel != null)
        //{
        //    VisualizeCityJson();
        //    EnableUploadedModel(false);
        //}
    }

    public void VisualizeCityJson()
    {
        EnableUploadedModel(true);

        //var test = File.OpenText("/Users/Tom/Documents/TSCD/Repos/3DAmsterdam/3DAmsterdam/Assets/testcube.json");
        //var testJson = test.ReadToEnd();
        //var cityJsonModel = new CityJsonModel(testJson, new Vector3RD());
        //print("c1: " + cityJsonModel.vertices.Count);

        //for (int i = 0; i < 8; i++)
        //{
        //    var a = cityJsonModel.vertices[i];
        //    print("vert: " + a.x + "," + a.y + "," + a.z);
        //}

        var cityJsonModel = new CityJsonModel(this.cityJson, new Vector3RD());

        File.WriteAllText("/Users/Tom/Documents/TSCD/TEST2.json", this.cityJson);

        var meshmaker = new CityJsonMeshUtility();

        foreach (KeyValuePair<string, JSONNode> co in cityJsonModel.cityjsonNode["CityObjects"])
        {
            var key = co.Key;
            var mesh = meshmaker.CreateMesh(transform, cityJsonModel, co.Value);
            AddMeshGameObject(key, mesh);
            print("extents: " + mesh.bounds.extents);
            print("center: " + mesh.bounds.center);
        }
    }

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

        if (perceelCenter != null)
            uitbouw.transform.position = CoordConvert.RDtoUnity(perceelCenter.Value); //set position to ensure snapping to wall is somewhat accurate

        uitbouw.GetComponent<UitbouwMovement>().enabled = enable;
        uitbouw.GetComponent<UitbouwMeasurement>().enabled = enable;
    }
}
