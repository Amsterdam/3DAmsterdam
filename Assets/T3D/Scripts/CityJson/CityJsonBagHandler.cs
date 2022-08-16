using ConvertCoordinates;
using Netherlands3D.T3D.Uitbouw;
using SimpleJSON;
using System.Collections.Generic;
using System.Linq;
using T3D.LoadData;
using UnityEngine;


public class CityJsonBagHandler : MonoBehaviour
{
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
        var cityJsonModel = new CityJsonModel(cityjson, new Vector3RD(), false);
        var meshmaker = new CityJsonMeshUtility();

        List<Mesh> meshes = new List<Mesh>();

        foreach (KeyValuePair<string, JSONNode> co in cityJsonModel.cityjsonNode["CityObjects"])
        {
            //var key = co.Key;
            var mesh = meshmaker.CreateMesh(transform, cityJsonModel, co.Value, true);
            if (mesh != null)
            {
                meshes.Add(mesh);
            }
        }

        if (meshes.Any())
        {
            var combinedMesh = CombineMeshes(meshes);
            ServiceLocator.GetService<MetadataLoader>().RaiseCityJsonBagLoaded(combinedMesh);
        }

    }

    Mesh CombineMeshes(List<Mesh> meshes)
    {        
        CombineInstance[] combine = new CombineInstance[meshes.Count];

        for (int i=0; i<meshes.Count; i++)
        {
            combine[i].mesh = meshes[i];        
            combine[i].transform = transform.localToWorldMatrix;
        }

        var mesh = new Mesh();
        mesh.CombineMeshes(combine);
        return mesh;

    }

   
}
