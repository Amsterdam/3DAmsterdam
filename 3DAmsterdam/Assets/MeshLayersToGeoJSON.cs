using Netherlands3D.Core;
using Netherlands3D.TileSystem;
using Netherlands3D.Utilities;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class MeshLayersToGeoJSON : MonoBehaviour
{
    [SerializeField]
    private Transform meshesRoot;

    [ContextMenu("Export .geojson")]
    private void ExportGeoJSON()
    {
        StartCoroutine(Export());
    }

    private IEnumerator Export()
    {
        var meshRenderers = meshesRoot.GetComponentsInChildren<MeshFilter>();

        var targetFile = EditorUtility.SaveFilePanel("Where to export .geojson", "", "export.json", "*");
        using (StreamWriter writer = new StreamWriter(targetFile))
        {
            writer.WriteLine("{\"type\":\"FeatureCollection\",\"features\":[");

            foreach(var mesh in meshRenderers)
            {
                var sharedMesh = mesh.sharedMesh;
                var triangles = sharedMesh.triangles;
                var vertices = sharedMesh.vertices;

                writer.WriteLine("{\"type\":\"Feature\",\"geometry\":{\"type\":\"Polygon\",\"coordinates\":[[");
                for (int i = 0; i < triangles.Length; i++)
                {
                    var coordinate = CoordConvert.UnitytoRD(vertices[triangles[i]] + mesh.transform.position);
                    writer.Write($"[{coordinate.x},{coordinate.y}]");
                    if(i < triangles.Length) writer.Write($",");
                }
                writer.WriteLine("]]},");
            }


            Vector3 normal = Vector3.zero;
            writer.WriteLine("properties\":{\"normalX\":\"" + normal.x + "\",\"normalY\":\"" + normal.y + "\",\"normalY\":\"" + normal.z + "\"}}");
            writer.WriteLine("]}");
        }
        yield return null;
    }
}
