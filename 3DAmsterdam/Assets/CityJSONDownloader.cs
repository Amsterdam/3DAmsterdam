using Netherlands3D.T3DPipeline;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;
using System.IO;
#if UNITY_EDITOR
using UnityEditor;
#endif
public class CityJSONDownloader : MonoBehaviour
{
#if UNITY_WEBGL
    [DllImport("__Internal")]
    private static extern void DownloadFile(byte[] array, int byteLength, string fileName);
#endif

    private GameObject cityJsonDataContainer;

    public void DownloadMeshAsCityJSON(GameObject gameObjectWithMesh)
    {
        if (gameObjectWithMesh.TryGetComponent(out MeshFilter meshFilter) && meshFilter.sharedMesh)
        {
            cityJsonDataContainer = gameObjectWithMesh;
            DownloadMeshAsCityJSON(meshFilter.sharedMesh);
        }
        else
        {
            Debug.LogWarning("Object does not have a MeshFilter with a Mesh", gameObjectWithMesh);
        }
    }

    public void DownloadMeshAsCityJSON(Mesh mesh)
    {
        if (!cityJsonDataContainer) cityJsonDataContainer = this.gameObject;

        var cityObjectFromMesh = cityJsonDataContainer.AddComponent<MeshToCityObject>();
        cityObjectFromMesh.CreateGeometryFromMesh(mesh);

        CityJSONFormatter.Reset();
        CityJSONFormatter.AddCityObject(cityObjectFromMesh);
        var cityJson = CityJSONFormatter.GetCityJSON();

#if UNITY_WEBGL && !UNITY_EDITOR
        byte[] byteArray = Encoding.UTF8.GetBytes(cityJson);
        DownloadFile(byteArray, byteArray.Length, $"{cityJsonDataContainer.name}_CityJSON.json");
#elif UNITY_EDITOR
        string selectedPath = EditorUtility.SaveFilePanel("Select File", "", $"{cityJsonDataContainer.name}_CityJSON", "json");
        if (!string.IsNullOrEmpty(selectedPath))
        {
            File.WriteAllText(selectedPath, cityJson);
            Debug.Log($"{selectedPath} saved.");
        }
#endif
    }
}
