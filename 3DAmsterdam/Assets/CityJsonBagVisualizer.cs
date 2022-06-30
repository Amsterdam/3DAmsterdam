using ConvertCoordinates;
using Netherlands3D.T3D.Uitbouw;
using SimpleJSON;
using System.Collections;
using System.Collections.Generic;
using T3D.LoadData;
using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class CityJsonBagVisualizer : MonoBehaviour
{
    public Material MeshMaterial;


    void OnEnable()
    {
        ServiceLocator.GetService<MetadataLoader>().CityJsonBagReceived += OnCityJsonBagReceived;       
    }

    void OnDisable()
    {
        ServiceLocator.GetService<MetadataLoader>().CityJsonBagReceived -= OnCityJsonBagReceived;
    }

    private void OnCityJsonBagReceived(string cityJson)
    {
        ParseCityJson(cityJson);
    }

    private void ParseCityJson(string cityjson)
    {
        var cityJsonModel = new CityJsonModel(cityjson, new Vector3RD());
        var meshmaker = new CityJsonMeshUtility();


        foreach (KeyValuePair<string, JSONNode> co in cityJsonModel.cityjsonNode["CityObjects"])
        {
            var key = co.Key;
            //var mesh = meshmaker.CreateMesh(transform, cityJsonModel, co.Value);

            var meshes = meshmaker.CreateMeshes(cityJsonModel, co.Value);

            for(int i=0;i<meshes.Length; i++)
            {
                AddMeshGameObject(key + i.ToString(), meshes[0]);
            }

            //AddMesh(mesh);
            //AddMeshGameObject(key, mesh);
        }

        
    }

    void AddMesh(Mesh newMesh)
    {
        var meshFilter = GetComponent<MeshFilter>();
        CombineInstance[] combine = new CombineInstance[2];
        combine[0].mesh = meshFilter.sharedMesh;
        combine[0].transform = meshFilter.transform.localToWorldMatrix;
        combine[1].mesh = newMesh;
        combine[1].transform = meshFilter.transform.localToWorldMatrix;

        meshFilter.mesh = new Mesh();
        meshFilter.mesh.CombineMeshes(combine);
        
    }

    void AddMeshGameObject(string name, Mesh mesh)
    {
        GameObject gam = new GameObject(name);
        var meshfilter = gam.AddComponent<MeshFilter>();
        meshfilter.sharedMesh = mesh;
        var meshrenderer = gam.AddComponent<MeshRenderer>();
        meshrenderer.material = MeshMaterial;
        gam.transform.SetParent(transform);
        gam.AddComponent<BoxCollider>();        
        
    }

}
