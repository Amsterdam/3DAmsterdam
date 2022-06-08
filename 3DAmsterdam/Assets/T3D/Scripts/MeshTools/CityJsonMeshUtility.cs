using ConvertCoordinates;
using Netherlands3D.LayerSystem;
using Netherlands3D.Utilities;
using Poly2Tri;
using SimpleJSON;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using T3D.LoadData;
using UnityEngine;

public class CityJsonMeshUtility
{
    public class PolygonPoint
    {
        public int Index;
        public Vector2 Point;
        public PolygonPoint(int index, Vector2 point)
        {
            Index = index;
            Point = point;
        }
    }

    public static List<Vector2> RemoveInner(List<Vector2> points)
    {
        for (int i = 1; i < points.Count - 1; i++)
        {
            for (int j = i + 1; j < points.Count - 1; j++)
            {
                if (points[i].Equals(points[j]))
                {
                    var volume = GetVolume(points, i, j + 1);
                    var allvol = GetVolume(points, 0, points.Count);

                    if (volume == 0)
                    {
                        points.RemoveRange(i, j - i + 1);
                        break;
                    }
                    else if (volume == allvol)
                    {
                        return points.GetRange(i, j - i + 1);
                    }


                    break;
                }
            }

        }

        return points;
    }

    public static float GetVolume(List<Vector2> points, int startIndex, int endIndex)
    {
        var subselect = points.GetRange(startIndex, endIndex - startIndex);

        var minx = subselect.Min(o => o.x);
        var maxx = subselect.Max(o => o.x);
        var miny = subselect.Min(o => o.y);
        var maxy = subselect.Max(o => o.y);

        return (maxx - minx) * (maxy - miny);
    }




    public static List<List<Vector2>> GetOuterAndInnerPolygons(List<Vector2> outer)
    {
        List<List<PolygonPoint>> result = new List<List<PolygonPoint>>();

        var outerPoints = outer.Select((o, i) => new PolygonPoint(i,o)).ToList();
        GetInner(result, outerPoints);
        foreach (var point in result.SelectMany(points => points))
        {
            outerPoints[point.Index].Index = -1;
        }
        var outerRing = outerPoints.Where(o => o.Index != -1).ToList();

        result.Insert(0, outerRing);

        //var windingorder = PolygonUtil.CalculateWindingOrder(list.Select(o => new Point2D(o.x, o.y)).ToArray());
        //if (windingorder == Point2DList.WindingOrderType.CCW)
        //{
        //    result.Add(list);
        //    break;
        //}

        List<List<Vector2>> outerAndInner = new List<List<Vector2>>();

        foreach(var points in result)
        {
            outerAndInner.Add(points.Select(o => o.Point).ToList());
        }

        return outerAndInner;
    }

    /// <summary>
    /// Alleen toevoegen aan lijst als binnenkomende outer list geen zelfde begin en eindpunt hebben
    /// </summary>
    /// <param name="result"></param>
    private static void GetInner(List<List<PolygonPoint>> result, List<PolygonPoint> outer)
    {
        bool found = false;

        for (int i = 1; i < outer.Count; i++)
        {
            var startPoint = outer[i];
            
            List<PolygonPoint> list = new List<PolygonPoint>();
            list.Add(startPoint);
            
            for (int j = i + 1; j < outer.Count; j++)
            {
                var testpoint = outer[j];
                list.Add(testpoint);

                if (startPoint.Point.Equals(testpoint.Point))
                {
                    found = true;
                    i = j;
                    GetInner(result, list);
                    break;
                }
            }
            
        }

        if (found == false)
        {
            result.Add(outer);
        }
    }

    public Mesh[] CreateMeshes(CityJsonModel cityModel, JSONNode cityObject)
    {
        List<Vector3> verts = GetVerts(cityModel);
        return GetMeshes(verts, cityObject).ToArray();

    }

    public Mesh CreateMesh(Transform transform, CityJsonModel cityModel, JSONNode cityObject)
    {
        var meshes = CreateMeshes(cityModel, cityObject);

        //combine the meshes
        CombineInstance[] combineInstanceArray = new CombineInstance[meshes.Length];
        for (int i = 0; i < meshes.Length; i++)
        {
            combineInstanceArray[i].mesh = meshes[i];
            combineInstanceArray[i].transform = transform.localToWorldMatrix;
        }
        Mesh mesh = new Mesh();
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        mesh.CombineMeshes(combineInstanceArray);
        //mesh.RecalculateBounds();
        //mesh.vertices = TransformVertices(mesh, -mesh.bounds.center, Quaternion.identity, Vector3.one); 
        //offset the vertices so that the center of the mesh bounding box of the selected mesh LOD is at (0,0,0)
        
        mesh.RecalculateNormals();
        return mesh;
    }

    public static Vector3[] TransformVertices(Mesh mesh, Vector3 translaton, Quaternion rotation, Vector3 scale)
    {
        Vector3[] vertices = mesh.vertices;
        Matrix4x4 matrix = Matrix4x4.TRS(translaton, rotation, scale);
        for (int i = 0; i < vertices.Length; i++)
        {
            vertices[i] = matrix.MultiplyPoint(vertices[i]);
        }
        return vertices;
    }

    private List<Vector3> GetVerts(CityJsonModel cityModel)
    {
        return cityModel.vertices.Select(o => new Vector3((float)o.x, (float)o.y, (float)o.z)).ToList();
    }


    public List<Vector3> GetBoundaryVertices(CityJsonModel cityModel, JSONNode cityObject)
    {
        var vertices =cityModel.vertices.Select(o => new Vector3((float)o.x, (float)o.y, (float)o.z)).ToList();

        var geometries = cityObject["geometry"].AsArray;
        int highestLodIndex = 0;
        for (int i = 0; i < geometries.Count; i++)
        {
            var lod = geometries[i]["lod"].AsInt;
            if (lod > highestLodIndex) highestLodIndex = i;
        }
        string geometrytype = cityObject["geometry"][highestLodIndex]["type"].Value;
        JSONNode boundariesNode = cityObject["geometry"][highestLodIndex]["boundaries"];
        if (geometrytype == "Solid")
        {
            boundariesNode = cityObject["geometry"][highestLodIndex]["boundaries"][0];
        }

        var counter = 0;
        foreach (JSONNode boundary in boundariesNode)
        {
            counter++;
            if (counter != 31) continue;

            JSONNode outerRing = boundary[0];
            return GetBounderyVertices(vertices, outerRing);
        }

        return null;

        }
    private List<Mesh> GetMeshes(List<Vector3> verts, JSONNode cityObject)
    {
        var geometries = cityObject["geometry"].AsArray;
        int highestLodIndex = 0;
        for (int i = 0; i < geometries.Count; i++)
        {
            var lod = geometries[i]["lod"].AsInt;
            if (lod > highestLodIndex) highestLodIndex = i;
        }

        List<Mesh> meshes = new List<Mesh>();
        string geometrytype = cityObject["geometry"][highestLodIndex]["type"].Value;

        JSONNode boundariesNode = cityObject["geometry"][highestLodIndex]["boundaries"];
        if (geometrytype == "Solid")
        {
            boundariesNode = cityObject["geometry"][highestLodIndex]["boundaries"][0];
        }

        // End if no BoundariesNode
        if (boundariesNode is null)
        {
            return null;
        }      

        var counter = 0;
        foreach (JSONNode boundary in boundariesNode)
        {
            counter++;
            if (counter  != 31) continue;

            JSONNode outerRing = boundary[0];

            List<List<Vector3>> holes = new List<List<Vector3>>();

            if (boundary.Count > 1)
            {
                for (int i = 1; i < boundary.Count; i++)
                {
                    var innerRing = boundary[i];
                    holes.Add(GetBounderyVertices(verts, innerRing));
                }
            }

            Poly2Mesh.Polygon poly = new Poly2Mesh.Polygon();

            poly.outside = GetBounderyVertices(verts, outerRing);
            poly.holes = holes;

            if (outerRing.Count == 3)
            {
                meshes.Add(Poly2Mesh.CreateTriangle(poly));
            }
            else
            {
                meshes.Add(Poly2Mesh.CreateMesh(poly));
            }
        }

        return meshes;
    }

    List<Vector3> GetBounderyVertices(List<Vector3> verts, JSONNode boundery)
    {
        List<Vector3> vertices = new List<Vector3>();
        foreach (var vertIndex in boundery)
        {
            var index = vertIndex.Value.AsInt;
            vertices.Add(verts[index]);
        }
        vertices.Reverse();
        return vertices;
    }
}
