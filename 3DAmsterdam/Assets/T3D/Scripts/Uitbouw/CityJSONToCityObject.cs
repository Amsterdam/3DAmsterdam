using System;
using System.Collections;
using System.Collections.Generic;
//using Netherlands3D.AssetGeneration.CityJSON;
using Netherlands3D.T3D.Uitbouw;
using SimpleJSON;
using T3D.LoadData;
using T3D.Uitbouw;
using UnityEngine;

public class CityJSONToCityObject : CityObject
{
    private JSONNode objectNode;
    private List<Vector3Double> combinedVertices;
    private int geometryDepth = -1;

    //private JSONArray sourceSolids;
    //private JSONArray sourceShells;
    private JSONArray sourceSurfaces = new JSONArray();
    //private JSONArray sourcePolygons;
    private JSONArray sourceBoundaries = new JSONArray();

    public override CitySurface[] GetSurfaces()
    {
        print("getting surfaces: " + sourceSurfaces.ToString());

        List<CitySurface> citySurfaces = new List<CitySurface>();

        foreach (var surfaceNode in sourceSurfaces) //multiple geometry objects represent different LODs
        {
            //var tri = new int[] { tris[i], tris[i + 2], tris[i + 1] }; //reverse the order for the CityJson to work
            //var triVerts = new Vector3[] { transform.position + verts[tri[0]], transform.position + verts[tri[1]], transform.position + verts[tri[2]] };
            var surfaceArray = surfaceNode.Value.AsArray;
            List<int[]> indices = new List<int[]>();
            List<Vector3[]> vertices = new List<Vector3[]>();
            for (int i = 0; i < surfaceArray.Count; i++)
            {
                //indices.Add(GetInidces(surfaceArray[i].AsArray));
                var localIndices = new int[surfaceArray[i].AsArray.Count];
                Vector3[] polygonVerts = new Vector3[surfaceArray[i].Count];
                for (int j = 0; j < surfaceArray[i].Count; j++)
                {
                    //var oldIndex = indices[i][j];
                    var oldIndex = surfaceArray[i][j];
                    var v = combinedVertices[oldIndex.AsInt];
                    localIndices[i] = j;
                    polygonVerts[j] = new Vector3((float)v.x, (float)v.y, (float)v.z);
                }
                indices.Add(localIndices);
                vertices.Add(polygonVerts);
            }

            var polygon = new CityPolygon(vertices[0], indices[0]);
            var surface = new CitySurface(polygon);
            for (int i = 1; i < indices.Count; i++)
            {
                var p = new CityPolygon(vertices[i], indices[i]);
                surface.TryAddHole(p);
            }
            citySurfaces.Add(surface);
        }
        return citySurfaces.ToArray();
    }

    private int[] GetInidces(JSONArray indicesNode)
    {
        var indices = new int[indicesNode.Count];
        for (int i = 0; i < indicesNode.Count; i++)
        {
            indices[i] = indicesNode[i];
        }
        return indices;
    }

    public override JSONObject GetGeometryNode()
    {
        var newNode = new JSONObject();
        var geometries = objectNode["geometry"];

        foreach (var geometry in geometries) //multiple geometry objects represent different LODs
        {
            GeometryType geometryType = (GeometryType)Enum.Parse(typeof(GeometryType), geometry.Value["type"].Value);
            newNode["type"] = geometryType.ToString();
            newNode["lod"] = geometry.Value["lod"].AsInt;

            geometryDepth = GeometryDepth[geometryType];
            newNode["boundaries"] = GetBoundaryArray(geometryDepth);

            if (objectNode["semantics"] != null)
            {
                var semantics = GetSemantics();
                newNode["semantics"] = semantics;
            }
        }
        return newNode;
    }

    private JSONArray GetBoundaryArray(int depth)
    {
        var boundaries = new JSONArray();
        //var solids = new JSONArray();
        //var shells = new JSONArray();
        sourceBoundaries = objectNode["geometry"]["boundaries"].AsArray;
        print("gnode: " + objectNode.ToString());
        print("geometry node: " + objectNode["boundaries"].ToString());
        switch (depth)
        {
            case 0:
                throw new NotImplementedException();
                break;
            case 1:
                throw new NotImplementedException();
                break;
            case 2:
                sourceSurfaces = sourceBoundaries;
                UpdateSurfaces();
                for (int i = 0; i < Surfaces.Length; i++)
                {
                    var surfaceArray = Surfaces[i].GetJSONPolygons();
                    boundaries.Add(surfaceArray);
                }
                return boundaries;
            case 3: //todo: this code currently only supports 1 outer shell, and no inner shells, because the CityJsonVisualizer and CityObject classes are not built for inner shells at this time
                sourceSurfaces = sourceBoundaries[0].AsArray;
                print("outer shell: " + sourceSurfaces.ToString());
                UpdateSurfaces();
                var shells = new JSONArray();
                for (int i = 0; i < Surfaces.Length; i++)
                {
                    var surfaceArray = Surfaces[i].GetJSONPolygons();
                    shells.Add(surfaceArray);
                }
                boundaries.Add(shells);
                return boundaries;
            case 4:
                throw new NotImplementedException();
                break;
            default:
                throw new IndexOutOfRangeException("Boundary depth: " + depth + " is out of range");
        }

    }

    protected override JSONNode GetSemantics()
    {
        return objectNode["semantics"];
    }

    public void SetNode(JSONNode objectNode, List<Vector3Double> combinedVertices)
    {
        this.objectNode = objectNode;
        this.combinedVertices = combinedVertices;
    }

    //public void ParseCityJSON(string json)
    //{
    //    geometryNode = JSONNode.Parse(json);
    //}
}
