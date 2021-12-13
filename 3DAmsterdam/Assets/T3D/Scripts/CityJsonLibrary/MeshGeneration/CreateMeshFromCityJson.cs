

using CityJsonLibrary.Coordinates;
using CityJsonLibrary.MeshLibrary;
using Netherlands3D.CityJSON;
using System;
using System.Collections.Generic;
using System.Numerics;

public class CreateMeshFromCityJson
{
    private CityObjectFilter[] cityObjectFilters;

    public void SetObjectFilters(CityObjectFilter[] cityObjectFilters)
    {
        this.cityObjectFilters = cityObjectFilters;
    }

    private void CityObjectToMesh(CityObject cityObject, bool calculateNormals)
    {
        List<Vector3Double> vertexlist = new List<Vector3Double>();
        Vector3 defaultNormal = new Vector3(0, 1, 0);
        List<Vector3> defaultnormalList = new List<Vector3> { defaultNormal, defaultNormal, defaultNormal };
        List<Vector3> normallist = new List<Vector3>();
        List<int> indexlist = new List<int>();
        int count = subObject.vertices.Count;
        foreach (var surface in cityObject.surfaces)
        {
            //findout if ity is already a triangle

            if (surface.outerRing.Count == 3 && surface.innerRings.Count == 0)
            {
                List<int> newindices = new List<int> { count, count + 1, count + 2 };
                count += 3;
                indexlist.AddRange(newindices);
                vertexlist.AddRange(surface.outerRing);
                if (calculateNormals)
                {
                    Vector3 normal = calculateNormal(surface.outerRing[0], surface.outerRing[1], surface.outerRing[2]);
                    normallist.Add(normal);
                    normallist.Add(normal);
                    normallist.Add(normal);
                }
                else
                {
                    normallist.AddRange(defaultnormalList);
                }

                continue;
            }

            //Our mesh output data per surface
            Vector3[] surfaceVertices;
            Vector3[] surfaceNormals;
            Vector2[] surfaceUvs;
            int[] surfaceIndices;

            //offset using first outerRing vertex position
            Vector3Double offsetPolygons = surface.outerRing[0];
            List<Vector3> outside = new List<Vector3>();
            for (int i = 0; i < surface.outerRing.Count; i++)
            {
                outside.Add((Vector3)(surface.outerRing[i] - offsetPolygons));
            }

            List<List<Vector3>> holes = new List<List<Vector3>>();
            for (int i = 0; i < surface.innerRings.Count; i++)
            {
                List<Vector3> inner = new List<Vector3>();
                for (int j = 0; j < surface.innerRings[i].Count; j++)
                {
                    inner.Add((Vector3)(surface.innerRings[i][j] - offsetPolygons));
                }
                holes.Add(inner);
            }

            //Turn poly into triangulated geometry data
            Poly2Mesh.Polygon poly = new Poly2Mesh.Polygon();
            poly.outside = outside;
            poly.holes = holes;

            if (poly.outside.Count < 3)
            {
                Console.WriteLine("Polygon seems to be a line");
                continue;
            }
            //Poly2Mesh takes care of calculating normals, using a right-handed coordinate system
            Poly2Mesh.CreateMeshData(poly, out surfaceVertices, out surfaceNormals, out surfaceIndices, out surfaceUvs);

            var offset = subObject.vertices.Count;

            //Append verts, normals and uvs
            for (int j = 0; j < surfaceVertices.Length; j++)
            {
                subObject.vertices.Add(((Vector3Double)surfaceVertices[j]) + offsetPolygons);
                subObject.normals.Add(surfaceNormals[j]);

                if (surfaceUvs != null)
                    subObject.uvs.Add(surfaceUvs[j]);
            }

            //Append indices ( corrected to offset )
            for (int j = 0; j < surfaceIndices.Length; j++)
            {
                subObject.triangleIndices.Add(offset + surfaceIndices[j]);
            }
        }

        if (vertexlist.Count > 0)
        {
            subObject.vertices.AddRange(vertexlist);
            subObject.triangleIndices.AddRange(indexlist);
            subObject.normals.AddRange(normallist);
        }
    }

    private SubObject ToSubObjectMeshData(CityObject cityObject)
    {
        List<SubObject> subObjects = new List<SubObject>();
        var subObject = new SubObject();
        subObject.vertices = new List<Vector3Double>();
        subObject.normals = new List<Vector3>();
        subObject.uvs = new List<Vector2>();
        subObject.triangleIndices = new List<int>();
        subObject.id = cityObject.keyName;

        int submeshindex = -1;

        // figure out the intended submesh and required meshDensity
        for (int i = 0; i < cityObjectFilters.Length; i++)
        {
            if (cityObjectFilters[i].objectType == cityObject.cityObjectType)
            {
                submeshindex = cityObjectFilters[i].defaultSubmeshIndex;
                subObject.maxVerticesPerSquareMeter = cityObjectFilters[i].maxVerticesPerSquareMeter;
                for (int j = 0; j < cityObjectFilters[i].attributeFilters.Length; j++)
                {
                    string attributename = cityObjectFilters[i].attributeFilters[j].attributeName;
                    for (int k = 0; k < cityObjectFilters[i].attributeFilters[j].valueToSubMesh.Length; k++)
                    {
                        string value = cityObjectFilters[i].attributeFilters[j].valueToSubMesh[k].value;
                        for (int l = 0; l < cityObject.semantics.Count; l++)
                        {
                            if (cityObject.semantics[l].name == attributename)
                            {
                                if (cityObject.semantics[l].value == value)
                                {
                                    submeshindex = cityObjectFilters[i].attributeFilters[j].valueToSubMesh[k].submeshIndex;
                                }
                            }
                        }
                    }
                }
            }
        }
        subObject.parentSubmeshIndex = submeshindex;
        if (submeshindex == -1)
        {
            return null;
        }


        //If we supplied a specific identifier field, use it as ID instead of object key index
        if (identifier != "")
        {
            foreach (var semantic in cityObject.semantics)
            {
                //Console.WriteLine(semantic.name);
                if (semantic.name == identifier)
                {
                    subObject.id = semantic.value;
                    break;
                }
            }
        }
        bool calculateNormals = false;
        if (subObject.maxVerticesPerSquareMeter == 0)
        {
            calculateNormals = true;
        }
        AppendCityObjectGeometry(cityObject, subObject, calculateNormals);
        //Append all child geometry too
        for (int i = 0; i < cityObject.children.Count; i++)
        {
            var childObject = cityObject.children[i];
            //Add child geometry to our subobject. (Recursive children are not allowed in CityJson)
            AppendCityObjectGeometry(childObject, subObject, calculateNormals);
        }

        //Winding order of triangles should be reversed
        subObject.triangleIndices.Reverse();

        //Check if the list if triangles is complete (divisible by 3)
        if (subObject.triangleIndices.Count % 3 != 0)
        {
            Console.WriteLine($"{subObject.id} triangle list is not divisible by 3. This is not correct.");
            return null;
        }

        //Calculate centroid using the city object vertices
        Vector3Double centroid = new Vector3Double();
        for (int i = 0; i < subObject.vertices.Count; i++)
        {
            centroid.X += subObject.vertices[i].X;
            centroid.Y += subObject.vertices[i].Y;
        }
        subObject.centroid = new Vector2Double(centroid.X / subObject.vertices.Count, centroid.Y / subObject.vertices.Count);

        return subObject;
    }

    private Vector3 calculateNormal(Vector3Double v1, Vector3Double v2, Vector3Double v3)
    {
        Vector3 normal = new Vector3();
        Vector3Double U = v2 - v1;
        Vector3Double V = v3 - v1;

        double X = ((U.Y * V.Z) - (U.Z * V.Y));
        double Y = ((U.Z * V.X) - (U.X * V.Z));
        double Z = ((U.X * V.Y) - (U.Y * V.X));

        // normalize it
        double scalefactor = Math.Sqrt((X * X) + (Y * Y) + (Z * Z));
        normal.X = (float)(X / scalefactor);
        normal.Y = (float)(Y / scalefactor);
        normal.Z = (float)(Z / scalefactor);
        return normal;

    }
}
