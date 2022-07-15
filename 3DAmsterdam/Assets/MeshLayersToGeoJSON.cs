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
        if (File.Exists(targetFile)) File.Delete(targetFile);
        using (StreamWriter writer = new StreamWriter(targetFile))
        {
            writer.WriteLine("{\"type\":\"FeatureCollection\",\"features\":[");

            foreach(var mesh in meshRenderers)
            {
                var sharedMesh = mesh.sharedMesh;
                var triangles = sharedMesh.triangles;
                var vertices = sharedMesh.vertices;
                var normals = sharedMesh.normals;
                int trianglesDrawn = 0;
                for (int i = 0; i < triangles.Length; i+=3)
                {
                    if ((i % 10000) == 0)
                    {
                        Debug.Log($"{mesh.name}: {i}");
                        yield return new WaitForEndOfFrame();
                    }

                    var coordinateA = CoordConvert.UnitytoRD(vertices[triangles[i]] + mesh.transform.position);
                    var coordinateB = CoordConvert.UnitytoRD(vertices[triangles[i + 1]] + mesh.transform.position);
                    var coordinateC = CoordConvert.UnitytoRD(vertices[triangles[i + 2]] + mesh.transform.position);

                    var normalA = normals[triangles[i]];
                    var normalB = normals[triangles[i + 1]];
                    var normalC = normals[triangles[i + 2]];

                    Vector3 normal = ((normalA + normalB + normalC) / 3).normalized;
                    var hexColorNormal = ColorUtility.ToHtmlStringRGB(new Color(normal.x, normal.y, normal.z));

                    //Skip triangles that do not point up
                    if (normal.y < 0) continue;
                    trianglesDrawn++;

                    var hexColorNormalRounded = (Vector3.Dot(Vector3.back, normal) > 0.5f) ? "00FF00" : "FF0000";

                    if (trianglesDrawn != 1) writer.Write(",");
                    writer.WriteLine("{\"type\":\"Feature\",\"geometry\":{\"type\":\"Polygon\",\"coordinates\":[[");
                    writer.Write($"[{coordinateA.x:F3},{coordinateA.y:F3}],[{coordinateB.x:F3},{coordinateB.y:F3}],[{coordinateC.x:F3},{coordinateC.y:F3}]]");
                    writer.Write("]},");
                    writer.WriteLine("\"properties\":{\"normal\":\"#" + hexColorNormal + "\",\"roundedNormal\":\"#" + hexColorNormalRounded + "\"}}");
                }
            }

            writer.WriteLine("]}");
        }
        yield return null;
    }
}
