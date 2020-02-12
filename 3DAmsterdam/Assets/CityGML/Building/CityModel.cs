using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml;
using System;
using ConvertCoordinates;
using System.Globalization;

public class CityModel
{
    private Material Standaardmateriaal;
    public XmlNamespaceManager nsmr; 
    public List<cityObjectMember> cityObjectMembers = new List<cityObjectMember>();
    private bool isHelsinki = false;
    private Vector3 HelsinkiOffset = new Vector3(25502960f,6665241f,0);

    public List<Meshdata> Schuindakmeshes = new List<Meshdata>();
    public List<Meshdata> Platdakmeshes = new List<Meshdata>();
    public List<Meshdata> Gevelmeshes = new List<Meshdata>();

    public CityModel(string filepath, Material standaardmateriaal, bool IsHelsinki = false)
    {
        isHelsinki = IsHelsinki;
        Standaardmateriaal = standaardmateriaal;
        XmlDocument doc = new XmlDocument();
        doc.Load(filepath);
        nsmr = new XmlNamespaceManager(doc.NameTable);
        nsmr.AddNamespace("core", "http://www.opengis.net/citygml/2.0");
        nsmr.AddNamespace("bldg", "http://www.opengis.net/citygml/building/2.0");
        nsmr.AddNamespace("gml", "http://www.opengis.net/gml");
        nsmr.AddNamespace("xlink", "http://www.w3.org/1999/xlink");
        Debug.Log(nsmr.DefaultNamespace);
        foreach (XmlNode xmlNode in doc.DocumentElement)
        {
            if (xmlNode.Name == "cityObjectMember")
            {
                cityObjectMember cityobjectmember = new cityObjectMember(xmlNode,nsmr);
                cityObjectMembers.Add(cityobjectmember);
            }
        }
    }

    public void CreateGameObjects()
    {
        GameObject go = new GameObject("CityModel");
        foreach (cityObjectMember com in cityObjectMembers)
        {
            createCOMGameObjects(com,go,Standaardmateriaal);
        }
    }

    public void createCOMGameObjects(cityObjectMember com, GameObject ParentObject, Material StandaardMateriaal)
    {
        GameObject gebouwobject = new GameObject(com.name + "_LOD2");
        gebouwobject.transform.parent = ParentObject.transform;
        foreach (string requiredSurfaceID in com.RequiredSurfaceIDs)
        {
            if (com.BuildingSurfaces.ContainsKey(requiredSurfaceID)==false)
            {
                Debug.Log(requiredSurfaceID + " is missing");
                continue;
            }
            BuildingSurface surface = com.BuildingSurfaces[requiredSurfaceID];
            GameObject SurfaceGameObject = new GameObject(com.name + "_" + surface.name + "_LOD2");
            SurfaceGameObject.transform.parent = gebouwobject.transform;
            List<Vector3> vertices = new List<Vector3>();
            List<int> indices = new List<int>();
            foreach (Triangle tri in surface.triangles)
            {
                if (isHelsinki)
                {
                    Vector3 vert = new Vector3((float)(tri.vertices[0].x - HelsinkiOffset.x), (float)(tri.vertices[0].z - HelsinkiOffset.z), (float)(tri.vertices[0].y - HelsinkiOffset.y));
                    vertices.Add(vert);
                    vert = new Vector3((float)(tri.vertices[1].x - HelsinkiOffset.x), (float)(tri.vertices[1].z - HelsinkiOffset.z), (float)(tri.vertices[1].y - HelsinkiOffset.y));
                    vertices.Add(vert);
                    vert = new Vector3((float)(tri.vertices[2].x - HelsinkiOffset.x), (float)(tri.vertices[2].z - HelsinkiOffset.z), (float)(tri.vertices[2].y - HelsinkiOffset.y));
                    vertices.Add(vert);
                }
                else
                {
                    vertices.Add(CoordConvert.RDtoUnity(tri.vertices[0]));
                    vertices.Add(CoordConvert.RDtoUnity(tri.vertices[1]));
                    vertices.Add(CoordConvert.RDtoUnity(tri.vertices[2]));
                }
                int vertcounter = vertices.Count - 1;
                indices.Add(vertcounter);
                indices.Add(vertcounter - 1);
                indices.Add(vertcounter - 2);
            }
            Mesh ms = new Mesh();
            ms.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            ms.name = com.name + "_" + surface.name + "_LOD2";
            ms.vertices = vertices.ToArray();
            ms.SetIndices(indices.ToArray(), MeshTopology.Triangles, 0);
            ms.RecalculateNormals();
            ms.RecalculateBounds();
            SurfaceGameObject.AddComponent<MeshFilter>().mesh = ms;
            SurfaceGameObject.AddComponent<MeshRenderer>().sharedMaterial = StandaardMateriaal;
            Meshdata md = new Meshdata();
            md.id = com.name;
            md.mesh = ms;
            if (surface.name == "plat dak"){Platdakmeshes.Add(md);}
            if (surface.name == "gevel") { Gevelmeshes.Add(md); }
            if (surface.name == "schuin dak") { Schuindakmeshes.Add(md); }
        }


    }


    public void createMeshes()
    {
        
        foreach (cityObjectMember com in cityObjectMembers)
        {
            createCOMMeshes(com,  Standaardmateriaal);

        }
    }

    public string[] CreateOBJFile()
    {
        List<string> objContent = new List<string>();
        objContent.Add("mtllib gebouwen.mtl");
        foreach (cityObjectMember com in cityObjectMembers)
        {
            List<string> test;
            test = CreateOBJpart(com);
            objContent.AddRange(test);
        }
        return objContent.ToArray();
    }

    public List<string> CreateOBJpart(cityObjectMember com)
    {

        List<string> output = new List<string>();
        output.Add("o " + com.name);
        foreach (string requiredSurfaceID in com.RequiredSurfaceIDs)
        {
            if (com.BuildingSurfaces.ContainsKey(requiredSurfaceID) == false)
            {
                Debug.Log(requiredSurfaceID + " is missing");
                continue;
            }
            BuildingSurface surface = com.BuildingSurfaces[requiredSurfaceID];
            output.Add("usemtl " + surface.Surfacetype);
            output.Add("g " + surface.name);
            foreach (Triangle tri in surface.triangles)
            {
                output.Add("v " + tri.vertices[0].x.ToString(CultureInfo.InvariantCulture) + " " + tri.vertices[0].y.ToString(CultureInfo.InvariantCulture) +" " + tri.vertices[0].z.ToString(CultureInfo.InvariantCulture));
                output.Add("v " + tri.vertices[1].x.ToString(CultureInfo.InvariantCulture) + " " + tri.vertices[1].y.ToString(CultureInfo.InvariantCulture) +" "+ tri.vertices[1].z.ToString(CultureInfo.InvariantCulture));
                output.Add("v " + tri.vertices[2].x.ToString(CultureInfo.InvariantCulture) + " " + tri.vertices[2].y.ToString(CultureInfo.InvariantCulture) +" "+ tri.vertices[2].z.ToString(CultureInfo.InvariantCulture));
                output.Add("f -9 -8 -7");
                output.Add("f -6 -5 -4");
                output.Add("f -3 -2 -1");
            }
        }

        return output;
    }

    public void createCOMMeshes(cityObjectMember com, Material StandaardMateriaal)
    {
        
        foreach (string requiredSurfaceID in com.RequiredSurfaceIDs)
        {
            if (com.BuildingSurfaces.ContainsKey(requiredSurfaceID) == false)
            {
                Debug.Log(requiredSurfaceID + " is missing");
                continue;
            }
            BuildingSurface surface = com.BuildingSurfaces[requiredSurfaceID];
            
            List<Vector3> vertices = new List<Vector3>();
            List<int> indices = new List<int>();
            foreach (Triangle tri in surface.triangles)
            {
                if (isHelsinki)
                {
                    Vector3 vert = new Vector3((float)(tri.vertices[0].x - HelsinkiOffset.x), (float)(tri.vertices[0].z - HelsinkiOffset.z), (float)(tri.vertices[0].y - HelsinkiOffset.y));
                    vertices.Add(vert);
                    vert = new Vector3((float)(tri.vertices[1].x - HelsinkiOffset.x), (float)(tri.vertices[1].z - HelsinkiOffset.z), (float)(tri.vertices[1].y - HelsinkiOffset.y));
                    vertices.Add(vert);
                    vert = new Vector3((float)(tri.vertices[2].x - HelsinkiOffset.x), (float)(tri.vertices[2].z - HelsinkiOffset.z), (float)(tri.vertices[2].y - HelsinkiOffset.y));
                    vertices.Add(vert);
                }
                else
                {
                    vertices.Add(CoordConvert.RDtoUnity(tri.vertices[0]));
                    vertices.Add(CoordConvert.RDtoUnity(tri.vertices[1]));
                    vertices.Add(CoordConvert.RDtoUnity(tri.vertices[2]));
                }
                int vertcounter = vertices.Count - 1;
                indices.Add(vertcounter);
                indices.Add(vertcounter - 1);
                indices.Add(vertcounter - 2);
            }
            Mesh ms = new Mesh();
            ms.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            ms.name = com.name + "_" + surface.name + "_LOD2";
            ms.vertices = vertices.ToArray();
            ms.SetIndices(indices.ToArray(), MeshTopology.Triangles, 0);
            ms.RecalculateNormals();
            ms.RecalculateBounds();
            Meshdata md = new Meshdata();
            md.id = com.name;
            md.mesh = ms;
            if (surface.name == "plat dak") { Platdakmeshes.Add(md); }
            if (surface.name == "gevel") { Gevelmeshes.Add(md); }
            if (surface.name == "schuin dak") { Schuindakmeshes.Add(md); }
        }


    }
}

public class Meshdata
{
    public Mesh mesh;
    public string id;
}
