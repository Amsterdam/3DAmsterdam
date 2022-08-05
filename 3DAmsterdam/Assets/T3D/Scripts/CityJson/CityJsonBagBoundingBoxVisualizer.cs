using ConvertCoordinates;
using Netherlands3D.T3D.Uitbouw;
using SimpleJSON;
using System.Collections.Generic;
using System.Linq;
using T3D.LoadData;
using UnityEngine;


public class CityJsonBagBoundingBoxVisualizer : MonoBehaviour
{
    void OnEnable()
    {
        ServiceLocator.GetService<MetadataLoader>().CityJsonBagBoundingBoxReceived += OnCityJsonBagBoundingBoxReceived;
    }

    void OnDisable()
    {
        ServiceLocator.GetService<MetadataLoader>().CityJsonBagBoundingBoxReceived -= OnCityJsonBagBoundingBoxReceived;
    }

    private void OnCityJsonBagBoundingBoxReceived(string cityJson, string excludeBagId)
    {
        ParseCityJson(cityJson, excludeBagId, false);
    }

    private void ParseCityJson(string cityjson, string excludeBagId, bool checkDistanceFromCenter)
    {
        var buildingMeshes = CityJsonVisualiser.ParseCityJson(cityjson, transform.localToWorldMatrix, true);

        foreach (var pair in buildingMeshes)
        {
            if (pair.Key.Key.Contains(excludeBagId))
            {
                buildingMeshes.Remove(pair.Key);
                break;
            }
        }

        var combinedMesh = CityJsonVisualiser.CombineMeshes(buildingMeshes.Values.ToList(), transform.localToWorldMatrix);
        GetComponent<MeshFilter>().sharedMesh = combinedMesh;
    }
}
