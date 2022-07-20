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

    [SerializeField]
    private int maxVerticesPerFrame = 50000;

    [ContextMenu("Export .geojson")]
    private void ExportGeoJSON()
    {
        StartCoroutine(Export());
    }

    private IEnumerator Export()
    {
        var meshRenderers = meshesRoot.GetComponentsInChildren<MeshFilter>();
        var targetFolder = EditorUtility.SaveFolderPanel("Where to export .geojson files", "", "");
        foreach (var mesh in meshRenderers)
        {
            Debug.Log($"Starting: {mesh.name}");
            yield return new WaitForEndOfFrame();

            var fileOutput = targetFolder + "/" + mesh.name + ".geojson";
            if(File.Exists(fileOutput)) File.Delete(fileOutput);

            using (StreamWriter writer = new StreamWriter(fileOutput))
            {
                writer.WriteLine("{\"type\":\"FeatureCollection\",\"features\":[");
                var sharedMesh = mesh.sharedMesh;
                var triangles = sharedMesh.triangles;
                var vertices = sharedMesh.vertices;
                var normals = sharedMesh.normals;
                int trianglesDrawn = 0;
                for (int i = 0; i < triangles.Length; i += 3)
                {
                    if ((i % maxVerticesPerFrame) == 0)
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

                    var hexColorNormalRounded = "";
                    if (Vector3.Dot(Vector3.back, normal) > 0.5f) {
                        hexColorNormalRounded = "00FF00"; //Green faces south
                    }
                    else if (Vector3.Dot(Vector3.forward, normal) > 0.5f)
                    {
                        hexColorNormalRounded = "FF0000"; //Red faces north
                    }
                    else if (Vector3.Dot(Vector3.right, normal) > 0.5f)
                    {
                        hexColorNormalRounded = "0000FF"; //Blue faces east
                    }
                    else if (Vector3.Dot(Vector3.left, normal) > 0.5f)
                    {
                        hexColorNormalRounded = "000000"; //Black faces west
                    }

                    if (trianglesDrawn != 1) writer.Write(",");
                    writer.WriteLine("{\"type\":\"Feature\",\"geometry\":{\"type\":\"Polygon\",\"coordinates\":[[");
                    writer.Write($"[{coordinateA.x:F3},{coordinateA.y:F3}],[{coordinateB.x:F3},{coordinateB.y:F3}],[{coordinateC.x:F3},{coordinateC.y:F3}]]");
                    writer.Write("]},");
                    writer.WriteLine("\"properties\":{\"normal\":\"#" + hexColorNormal + "\",\"roundedNormal\":\"#" + hexColorNormalRounded + "\"}}");
                }

                writer.WriteLine("]}");
            }
            Debug.Log($"Complete: {mesh.name}");
            yield return new WaitForEndOfFrame();
        }
    }
}