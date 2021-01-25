
#if UNITY_EDITOR

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using cityJSON;
using UnityEditor;
using System.IO;
using ConvertCoordinates;
using SimpleJSON;

public class ImportCityJsonTerrain : MonoBehaviour
{
    public List<Material> materialList = new List<Material>(7);
    private Material[] materialsArray;
    
    // Start is called before the first frame update
    void Start()
    {
        materialsArray = materialList.ToArray();
        double originX = 124000;
        double originY = 443000;
        //ImportSingle(originX, originY);
        importeer();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    void ImportSingle(double OriginX, double OriginY)
    {

        double originX = OriginX;
        double originY = OriginY;
        string basefilepath = "D:/3DUtrecht/terrainTiles/";
        
        string jsonfilename = originX.ToString() + "-" +originY.ToString() +".json";
        
        int LOD = 1;
        
       
        float tileSize = 1000;
        string filepath = basefilepath;
        Debug.Log(filepath);

        if (File.Exists(filepath+jsonfilename))
        {

            
        CityModel cm = new CityModel(filepath, jsonfilename);

            Mesh LanduseMesh = CreateCityObjectMesh(cm, "LandUse", originX, originY, tileSize);
            
            LanduseMesh = SimplifyMesh(LanduseMesh, 0.05f);
            Mesh RoadMesh = CreateCityObjectMesh(cm, "Road", originX, originY, tileSize);
            Mesh plantcoverMesh = CreateCityObjectMesh(cm, "PlantCover", originX, originY, tileSize);
            plantcoverMesh = SimplifyMesh(plantcoverMesh, 0.05f);
            Mesh genericCityObjectMesh = CreateCityObjectMesh(cm, "GenericCityObject", originX, originY, tileSize);
            Mesh waterBodyMesh = CreateCityObjectMesh(cm, "WaterBody", originX, originY, tileSize);
            Mesh bridgeMesh = CreateCityObjectMesh(cm, "Bridge", originX, originY, tileSize);
            CombineInstance[] combi = new CombineInstance[6];
            //combi[0].mesh = LanduseMesh;
            //combi[1].mesh = RoadMesh;
            //combi[2].mesh = genericCityObjectMesh;
            //combi[3].mesh = waterBodyMesh;
            //combi[4].mesh = bridgeMesh;
            //combi[5].mesh = plantcoverMesh;

            //Mesh lod1Mesh = new Mesh();
            //lod1Mesh.CombineMeshes(combi, false, false);
            //lod1Mesh.uv2 = RDuv2(lod1Mesh.vertices, CoordConvert.RDtoUnity(new Vector3RD(originX, originY,0)), tileSize);

            

            combi[0].mesh = SimplifyMesh(LanduseMesh, 0.05f);
            combi[1].mesh = SimplifyMesh(RoadMesh, 0.05f);
            combi[2].mesh = SimplifyMesh(genericCityObjectMesh, 0.05f);
            combi[3].mesh = SimplifyMesh(waterBodyMesh, 0.05f);
            combi[4].mesh = bridgeMesh;
            combi[5].mesh = SimplifyMesh(plantcoverMesh, 0.05f); ;

            Mesh lod0Mesh = new Mesh();
            lod0Mesh.CombineMeshes(combi, false, false);
            lod0Mesh.uv2 = RDuv2(lod0Mesh.vertices, CoordConvert.RDtoUnity(new Vector3RD(originX, originY,0)), tileSize);
            AssetDatabase.CreateAsset(lod0Mesh, "Assets/terrainMeshes/LOD0/terrain_" + originX + "-" + originY + ".mesh");
            //for debug
            GetComponent<MeshFilter>().sharedMesh = lod0Mesh;


        }
    }

    private Mesh SimplifyMesh(Mesh mesh, float quality)
    {

        if (mesh.triangles.Length<100)
        {
            return mesh;
        }

        var DecimatedMesh = mesh;
        
        var meshSimplifier = new UnityMeshSimplifier.MeshSimplifier();
        meshSimplifier.Initialize(DecimatedMesh);
        meshSimplifier.PreserveBorderEdges = true;
        meshSimplifier.MaxIterationCount = 500;
        meshSimplifier.SimplifyMesh(quality);
        DecimatedMesh = meshSimplifier.ToMesh();
        DecimatedMesh.RecalculateNormals();
        DecimatedMesh.Optimize();
        return DecimatedMesh;
    }


    private bool PointISInsideArea(Vector3RD point, double OriginX, double OriginY, float tileSize)
    {
        
        if (point.x < OriginX || point.x > (OriginX+tileSize))
        {
            return false;
        }
        if (point.y < OriginY || point.y > (OriginY + tileSize))
        {
            return false;
        }

        return true;
    }

    private Mesh CreateCityObjectMesh(CityModel cityModel, string cityObjectType, double originX, double originY, float tileSize)
    {
        

        List<Vector3RD> RDTriangles = GetTriangleListRD(cityModel, cityObjectType);

        List<Vector3RD> clippedRDTriangles = new List<Vector3RD>();
        List<Vector3> vectors = new List<Vector3>();

        List<Vector3> clipboundary = CreateClippingPolygon(tileSize);

        if (RDTriangles.Count==0)
        {
            return CreateEmptyMesh();
        }

        //clip all the triangles
        for (int i = 0; i < RDTriangles.Count; i += 3)
        {

            if (PointISInsideArea(RDTriangles[i], originX, originY, tileSize) && PointISInsideArea(RDTriangles[i + 1], originX, originY, tileSize) && PointISInsideArea(RDTriangles[i + 2], originX, originY, tileSize))
            {
                clippedRDTriangles.Add(RDTriangles[i+2]);
                clippedRDTriangles.Add(RDTriangles[i + 1]);
                clippedRDTriangles.Add(RDTriangles[i]);
                continue;
            }


            //offset RDvertices so coordinates can be saved as a float
            // flip y and z-axis so clippingtool works
            //reverse order to make them clockwise so the clipping-algorithm can use them
            vectors.Clear();
            vectors.Add(new Vector3((float)(RDTriangles[i + 2].x - originX), (float)RDTriangles[i + 2].z, (float)(RDTriangles[i + 2].y - originY)));
            vectors.Add(new Vector3((float)(RDTriangles[i + 1].x - originX), (float)RDTriangles[i + 1].z, (float)(RDTriangles[i + 1].y - originY)));
            vectors.Add(new Vector3((float)(RDTriangles[i].x - originX), (float)RDTriangles[i].z, (float)(RDTriangles[i].y - originY)));
           
            
            List<Vector3> defshape = TriangleClipping.SutherlandHodgman.ClipPolygon(vectors, clipboundary);

            if (defshape.Count < 3)
            {
                continue;
            }

            if (defshape[0].x.ToString() == "NaN")
            {
                continue;
            }

            Vector3RD vectorRD = new Vector3RD();
            // add first three vectors
            
            vectorRD.x = defshape[0].x + originX;
            vectorRD.y = defshape[0].z + originY;
            vectorRD.z = defshape[0].y;
            clippedRDTriangles.Add(vectorRD);

            vectorRD.x = defshape[1].x + originX;
            vectorRD.y = defshape[1].z + originY;
            vectorRD.z = defshape[1].y;
            clippedRDTriangles.Add(vectorRD);
            vectorRD.x = defshape[2].x + originX;
            vectorRD.y = defshape[2].z + originY;
            vectorRD.z = defshape[2].y;
            clippedRDTriangles.Add(vectorRD);

            // add extra vectors. vector makes a triangle with the first and the previous vector.
            for (int j = 3; j < defshape.Count; j++)
            {
                vectorRD.x = defshape[0].x + originX;
                vectorRD.y = defshape[0].z + originY;
                vectorRD.z = defshape[0].y;
                clippedRDTriangles.Add(vectorRD);

                vectorRD.x = defshape[j - 1].x + originX;
                vectorRD.y = defshape[j - 1].z + originY;
                vectorRD.z = defshape[j - 1].y;
                clippedRDTriangles.Add(vectorRD);

                vectorRD.x = defshape[j].x + originX;
                vectorRD.y = defshape[j].z + originY;
                vectorRD.z = defshape[j].y;
                clippedRDTriangles.Add(vectorRD);
            }
        }

        //createMesh
        List<Vector3> verts = new List<Vector3>();
        Vector3RD tileCenterRD = new Vector3RD();
        tileCenterRD.x = originX + (tileSize / 2);
        tileCenterRD.y = originY + (tileSize / 2);
        tileCenterRD.z = 0;
        Vector3 tileCenterUnity = CoordConvert.RDtoUnity(tileCenterRD);
        List<int> ints = new List<int>();
        for (int i = 0; i < clippedRDTriangles.Count; i++)
        {
            Vector3 coord = CoordConvert.RDtoUnity(clippedRDTriangles[i]) - tileCenterUnity;
            ints.Add(i);
            verts.Add(coord);
        }
        ints.Reverse(); //reverse the trianglelist to make the triangles counter-clockwise again

        if (ints.Count==0)
        {
            return CreateEmptyMesh();
        }
        Mesh mesh = new Mesh();
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        mesh.vertices = verts.ToArray();
        mesh.triangles = ints.ToArray();
        mesh = WeldVertices(mesh);
        mesh.RecalculateNormals();
        mesh.Optimize();
        return mesh;
    }

    private Mesh CreateEmptyMesh()
    {
        Mesh emptyMesh = new Mesh();
        Vector3[] emptyVertsList = new Vector3[3];
        emptyVertsList[0] = new Vector3(0, 0, 0);
        List<int> emptyIndices = new List<int>();
        emptyIndices.Add(0);
        emptyIndices.Add(0);
        emptyIndices.Add(0);
        emptyMesh.vertices = emptyVertsList;
        emptyMesh.SetIndices(emptyIndices.ToArray(), MeshTopology.Triangles, 0);
        return emptyMesh;
    }

    private Vector2[] RDuv2(Vector3[] verts, Vector3 UnityOrigin, float tileSize)
    {
        Vector3 UnityCoordinate;
        Vector3RD rdCoordinate;
        Vector3RD rdOrigin = CoordConvert.UnitytoRD(UnityOrigin);
        float offset = -tileSize / 2;
        Vector2[] uv2 = new Vector2[verts.Length];
        for (int i = 0; i < verts.Length; i++)
        {
            UnityCoordinate = verts[i] + UnityOrigin;
            rdCoordinate = CoordConvert.UnitytoRD(UnityCoordinate);
            uv2[i].x = ((float)(rdCoordinate.x - rdOrigin.x) + offset)/tileSize;
            uv2[i].y = ((float)(rdCoordinate.y - rdOrigin.y) + offset) / tileSize;
        }
        return uv2;
    }

    private Mesh WeldVertices(Mesh mesh)
    {
        Vector3[] originalVerts = mesh.vertices;
        int[] originlints = mesh.GetIndices(0);
        int[] newIndices = new int[originlints.Length];
        Dictionary<Vector3,int> vertexMapping = new Dictionary<Vector3, int>();
        //fill the dictionary
        foreach (Vector3 vert in originalVerts)
        {
            if (!vertexMapping.ContainsKey(vert))
            {
                vertexMapping.Add(vert, vertexMapping.Count);
            }
        }
        for (int i = 0; i < originlints.Length; i++)
        {
            newIndices[i] = vertexMapping[originalVerts[originlints[i]]];
        }
        mesh.Clear();
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        mesh.vertices = new List<Vector3>(vertexMapping.Keys).ToArray();
        mesh.triangles = newIndices;

        return mesh;
    }


    void importeer()
    {
        int Xmin = 124000;
        int Ymin = 443000;
        int Xmax = 145000;
        int Ymax = 463000;

        int stepSize = 1000;


        

        for (int X = Xmin; X < Xmax; X+=stepSize)
        {
            for (int Y = Ymin; Y < Ymax; Y+=stepSize)
            {
                Debug.Log(X +"=" + Y);
                ImportSingle(X, Y);
            }



        }
    }









private List<Vector3RD> GetVertsRD(CityModel cityModel)
    {
        List<Vector3RD> vertsRD = new List<Vector3RD>();
        Vector3RD vertexCoordinate = new Vector3RD();
        foreach (Vector3Double vertex in cityModel.vertices)
        {
            vertexCoordinate.x = vertex.x;
            vertexCoordinate.y = vertex.y;
            vertexCoordinate.z = vertex.z;
            vertsRD.Add(vertexCoordinate);

        }
        return vertsRD;
    }
    public List<Vector3RD> GetTriangleListRD(CityModel cityModel, string cityObjectType)
    {
        List<Vector3RD> vertsRD = GetVertsRD(cityModel);
        List<Vector3RD> triangleList = new List<Vector3RD>();
        List<int> triangles = new List<int>();
        foreach (JSONNode cityObject in cityModel.cityjsonNode["CityObjects"])
        {
            if (cityObject["type"] == cityObjectType)
            {

                    triangles.AddRange(ReadTriangles(cityObject));

            }
        }
        for (int i = 0; i < triangles.Count; i++)
        {
            triangleList.Add(vertsRD[triangles[i]]);
        }
        return triangleList;
    }

    private List<Vector3> CreateClippingPolygon(float tilesize)
    {
        List<Vector3> polygon = new List<Vector3>();
        polygon.Add(new Vector3(0, 0, 0));
        polygon.Add(new Vector3(tilesize, 0, 0));
        polygon.Add(new Vector3(tilesize, 0, tilesize));
        polygon.Add(new Vector3(0, 0, tilesize));
        return polygon;
    }
    private List<int> ReadTriangles(JSONNode cityObject)
    {
        List<int> triangles = new List<int>();
        JSONNode boundariesNode = cityObject["geometry"][0]["boundaries"];
        // End if no BoundariesNode
        if (boundariesNode is null)
        {
            return triangles;
        }
        foreach (JSONNode boundary in boundariesNode)
        {
            JSONNode outerRing = boundary[0];
            triangles.Add(outerRing[2].AsInt);
            triangles.Add(outerRing[1].AsInt);
            triangles.Add(outerRing[0].AsInt);
        }

        return triangles;
    }
    string CreateAssetFolder(string folderpath, string foldername)
    {

        if (!AssetDatabase.IsValidFolder(folderpath + "/" + foldername))
        {
            AssetDatabase.CreateFolder(folderpath, foldername);
        }
        return folderpath + "/" + foldername;
    }
}

#endif