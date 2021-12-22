

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

    void Start()
    {
        Netherlands3D.T3D.Uitbouw.MetadataLoader.Instance.BimCityJsonReceived += OnBimCityJsonReceived;
    }

    private void OnBimCityJsonReceived(string cityJson)
    {
        UnitySystemConsoleRedirector.Redirect();

        var cityJsonModel = new CityJsonModel(cityJson);
        var meshmaker = new CityJsonMeshUtility();
        
        foreach (KeyValuePair<string, JSONNode> co in cityJsonModel.cityjsonNode["CityObjects"])
        {
            var key = co.Key;
            var mesh = meshmaker.CreateMesh(transform,  cityJsonModel, co.Value);            
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
