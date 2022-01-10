using ConvertCoordinates;
using Netherlands3D.T3D.Uitbouw;
using SimpleJSON;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using T3D.LoadData;
//using T3D.LoadData;
using UnityEngine;


public class CityJsonVisualiser : MonoBehaviour
{
    public Material MeshMaterial;
    private Vector3RD? centerPerceel;
    private string cityJson;

    public static CityJsonVisualiser Instance;

    private void Awake()
    {
        Instance = this;
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
        centerPerceel = new Vector3RD(args.PerceelnummerPlaatscoordinaat.x, args.PerceelnummerPlaatscoordinaat.y, 0);

        if (!string.IsNullOrEmpty(this.cityJson))
        {
            VisualizeCityJson();
            EnableUploadedModel(false);
        }
    }

    private void OnBimCityJsonReceived(string cityJson)
    {
        this.cityJson = cityJson;

        if (centerPerceel != null)
        {
            VisualizeCityJson();
            EnableUploadedModel(false);
        }
    }

    void VisualizeCityJson()
    {
        print(centerPerceel.Value);
        var cityJsonModel = new CityJsonModel(this.cityJson, new Vector3RD());
        var meshmaker = new CityJsonMeshUtility();

        foreach (KeyValuePair<string, JSONNode> co in cityJsonModel.cityjsonNode["CityObjects"])
        {
            var key = co.Key;
            var mesh = meshmaker.CreateMesh(transform, cityJsonModel, co.Value);
            AddMeshGameObject(key, mesh);
        }
    }

    void AddMeshGameObject(string name, Mesh mesh)
    {
        GameObject gam = new GameObject(name);
        var meshfilter = gam.AddComponent<MeshFilter>();
        meshfilter.sharedMesh = mesh;
        var meshrenderer = gam.AddComponent<MeshRenderer>();
        meshrenderer.material = MeshMaterial;
        gam.transform.parent = transform;
    }

    public void EnableUploadedModel(bool enable)
    {
        foreach (Transform t in transform)
        {
            t.gameObject.SetActive(enable);
        }

        if (enable)
        {
            gameObject.AddComponent<UploadedUitbouw>();
        }
    }
}
