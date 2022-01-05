

using ConvertCoordinates;
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

    void Start()
    {
        Netherlands3D.T3D.Uitbouw.MetadataLoader.Instance.BimCityJsonReceived += OnBimCityJsonReceived;
        Netherlands3D.T3D.Uitbouw.MetadataLoader.Instance.PerceelDataLoaded += OnPerceelDataLoaded;



    }

    private void OnPerceelDataLoaded(object source, Netherlands3D.T3D.Uitbouw.PerceelDataEventArgs args)
    {
        centerPerceel = new Vector3RD(args.PerceelnummerPlaatscoordinaat.x, args.PerceelnummerPlaatscoordinaat.y, 0) ;

        if( !string.IsNullOrEmpty(this.cityJson))
        {
            VisualizeCityJson();
        }

    }

    private void OnBimCityJsonReceived(string cityJson)
    {
        this.cityJson = cityJson;

        if(centerPerceel != null)
        {
            VisualizeCityJson();
        }


    }

    void VisualizeCityJson()
    {
        var cityJsonModel = new CityJsonModel(this.cityJson, this.centerPerceel.Value);
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
    
}
