using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
//using Netherlands3D.AssetGeneration.CityJSON;
using Netherlands3D.T3D.Uitbouw;
using SimpleJSON;
using T3D.LoadData;
using T3D.Uitbouw;
using UnityEngine;

public class CityJSONToCityObject : CityObject
{
    private Dictionary<CityObjectIdentifier, Mesh> geometryNodes;
    private JSONObject attributes;
    private List<Vector3Double> combinedVertices;
    private int geometryDepth = -1;

    //private JSONArray sourceSolids;
    //private JSONArray sourceShells;
    private JSONArray sourceSurfaces = new JSONArray();
    //private JSONArray sourcePolygons;
    private JSONArray sourceBoundaries = new JSONArray();

    protected override void Start()
    {
        //base.Start(); //dont call base.Start here, call it when the node is set in SetNode()
    }

    public override CitySurface[] GetSurfaces()
    {
        List<CitySurface> citySurfaces = new List<CitySurface>();
        foreach (var surfaceNode in sourceSurfaces) //multiple geometry objects represent different LODs
        {
            var surfaceArray = surfaceNode.Value.AsArray;
            List<int[]> indices = new List<int[]>();
            List<Vector3[]> vertices = new List<Vector3[]>();

            //todo: currently vertices are duplicated, change this to not be the case (perhaps by adding a check in the CityJSON Formatter)
            for (int i = 0; i < surfaceArray.Count; i++)
            {
                //indices.Add(GetInidces(surfaceArray[i].AsArray));
                var localIndices = new int[surfaceArray[i].AsArray.Count];
                Vector3[] polygonVerts = new Vector3[surfaceArray[i].Count];
                for (int j = 0; j < surfaceArray[i].Count; j++)
                {
                    var oldIndex = surfaceArray[i][j];
                    var oldVert = combinedVertices[oldIndex.AsInt]; //combinedVertices are in unity space

                    var flipYZ = geometryNodes.FirstOrDefault(g => g.Key.Lod == activeLod).Key.FlipYZ;
                    Vector3 newVertUnity;
                    if (flipYZ)
                        newVertUnity = meshFilter.transform.rotation * new Vector3((float)oldVert.x, (float)oldVert.z, (float)oldVert.y) + meshFilter.transform.position;
                    else
                        newVertUnity = meshFilter.transform.rotation * new Vector3((float)oldVert.x, (float)oldVert.y, (float)oldVert.z) + meshFilter.transform.position;

                    localIndices[i] = j;
                    polygonVerts[j] = newVertUnity;
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

    //private int[] GetInidces(JSONArray indicesNode)
    //{
    //    var indices = new int[indicesNode.Count];
    //    for (int i = 0; i < indicesNode.Count; i++)
    //    {
    //        indices[i] = indicesNode[i];
    //    }
    //    return indices;
    //}

    public override void UpdateSurfaces()
    {
        Solids = new Dictionary<int, List<CitySurface[]>>();
        //Surfaces = new Dictionary<int, CitySurface[]>();
        //if (!objectNode)
        //    print(gameObject.name + " does not have object node");
        foreach (var node in geometryNodes.Keys) //multiple geometry objects represent different LODs
        {
            var geometry = node.Node;
            GeometryType geometryType = (GeometryType)Enum.Parse(typeof(GeometryType), geometry["type"].Value);
            geometryDepth = GeometryDepth[geometryType];
            sourceBoundaries = geometry["boundaries"].AsArray;//GetBoundaryArrayFromSourceGeometry(geometry, geometryDepth);
            var shell = new List<CitySurface[]>();
            switch (geometryDepth)
            {
                case 0:
                    throw new NotImplementedException();
                    break;
                case 1:
                    throw new NotImplementedException();
                    break;
                case 2:
                    sourceSurfaces = sourceBoundaries.AsArray;
                    shell.Add(GetSurfaces());
                    Solids.Add(node.Lod, shell);
                    break;
                case 3:
                    for (int i = 0; i < sourceBoundaries.Count; i++)
                    {
                        sourceSurfaces = sourceBoundaries[i].AsArray;
                        shell.Add(GetSurfaces());
                        Solids.Add(node.Lod, shell);
                    }
                    break;
                case 4:
                    throw new NotImplementedException();
                    break;
                default:
                    throw new IndexOutOfRangeException("Boundary depth: " + geometryDepth + " is out of range");
            }
        }
    }

    //private JSONArray GetBoundaryArrayFromSourceGeometry(JSONNode geometry, int depth)
    //{
    //    var boundaries = new JSONArray();
    //    //var solids = new JSONArray();
    //    //var shells = new JSONArray();
    //    sourceBoundaries = geometry["boundaries"].AsArray;
    //    //print("geometry node: " + objectNode["boundaries"].ToString());
    //    switch (depth)
    //    {
    //        case 0:
    //            throw new NotImplementedException();
    //            break;
    //        case 1:
    //            throw new NotImplementedException();
    //            break;
    //        case 2:
    //            //sourceSurfaces = sourceBoundaries;
    //            //UpdateSurfaces();
    //            var sourceSurfaces = sourceBoundaries;
    //            for (int i = 0; i < sourceSurfaces.Count; i++)
    //            {
    //                var surfaceArray = Surfaces[i].GetJSONPolygons();
    //                boundaries.Add(surfaceArray);
    //            }
    //            return boundaries;
    //        case 3: //todo: this code currently only supports 1 outer shell, and no inner shells, because the CityJsonVisualizer and CityObject classes are not built for inner shells at this time
    //            //sourceSurfaces = sourceBoundaries[0].AsArray;
    //            //UpdateSurfaces();
    //            for (int i = 0; i < Solids.Count; i++)
    //            {
    //                var shells = new JSONArray();
    //                for (int j = 0; j < Surfaces.Length; j++)
    //                {
    //                    var surfaceArray = Surfaces[i].GetJSONPolygons();
    //                    shells.Add(surfaceArray);
    //                }
    //                boundaries.Add(shells);
    //            }
    //            return boundaries;
    //        case 4:
    //            throw new NotImplementedException();
    //            break;
    //        default:
    //            print(depth + "\t is out of range");
    //            throw new IndexOutOfRangeException("Boundary depth: " + depth + " is out of range");
    //    }
    //}

    public override JSONObject GetGeometryNode(int lod)
    {
        //var newGeometryArray = new JSONArray();

        //foreach (var sourceGeometry in geometryNodes.Keys) //multiple geometry objects represent different LODs
        //{
        var sourceGeometry = geometryNodes.Keys.FirstOrDefault(k => k.Lod == lod);
        var geometryObject = new JSONObject();
        GeometryType geometryType = (GeometryType)Enum.Parse(typeof(GeometryType), sourceGeometry.Node["type"].Value);
        geometryObject["type"] = geometryType.ToString();
        geometryObject["lod"] = lod;

        geometryDepth = GeometryDepth[geometryType];
        geometryObject["boundaries"] = GetBoundariesNode(lod, geometryDepth);

        if (sourceGeometry.Node["semantics"] != null)
        {
            var semantics = GetSemantics(lod);
            geometryObject["semantics"] = semantics;
        }
        //newGeometryArray.Add(geometryObject);
        //}

        //print("ga " + newGeometryArray.ToString());
        //print("go " + newGeometryArray.AsObject.ToString());
        return geometryObject;
    }

    private JSONArray GetBoundariesNode(int lod, int depth)
    {
        var boundaries = new JSONArray();
        switch (depth)
        {
            case 0:
                throw new NotImplementedException();
                break;
            case 1:
                throw new NotImplementedException();
                break;
            case 2:
                for (int i = 0; i < Surfaces[lod].Length; i++)
                {
                    var surfaceArray = Surfaces[lod][i].GetJSONPolygons();
                    boundaries.Add(surfaceArray);
                }
                return boundaries;
            case 3:
                var solidArray = new JSONArray();
                for (int i = 0; i < Solids[lod].Count; i++)
                {
                    var surfaces = Solids[lod][i];
                    for (int j = 0; j < surfaces.Length; j++)
                    {
                        var surfaceArray = surfaces[j].GetJSONPolygons();
                        solidArray.Add(surfaceArray);
                    }
                    boundaries.Add(solidArray);
                }
                return boundaries;
            case 4:
                throw new NotImplementedException();
                break;
            default:
                throw new IndexOutOfRangeException("Boundary depth: " + depth + " is out of range");
        }
    }

    protected override JSONNode GetSemantics(int lod)
    {
        var geometry = geometryNodes.FirstOrDefault(node => lod == node.Key.Lod);
        return geometry.Key.Node["semantics"];
    }

    protected override JSONObject GetAttributes()
    {
        attributes.Add("annotations", GetAnnotationNode());
        return attributes;
    }

    public void SetNodes(Dictionary<CityObjectIdentifier, Mesh> meshes, JSONObject attributes, List<Vector3Double> combinedVertices)
    {
        this.geometryNodes = meshes;
        this.attributes = attributes;
        this.combinedVertices = combinedVertices;
        base.Start();
    }

    public Mesh SetMeshActive(int lod)
    {
        var pair = geometryNodes.FirstOrDefault(i => i.Key.Lod == lod);
        activeLod = lod;
        meshFilter.mesh = pair.Value;
        meshFilter.mesh.RecalculateBounds();

        var col = GetComponent<MeshCollider>();
        if (col)
            col.sharedMesh = pair.Value;

        var uploadedUitbouw = GetComponentInParent<UploadedUitbouw>();
        if (uploadedUitbouw)
            uploadedUitbouw.SetMeshFilter(meshFilter);

        return pair.Value;
    }
}
