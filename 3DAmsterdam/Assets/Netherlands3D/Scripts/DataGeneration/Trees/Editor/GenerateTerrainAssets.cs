using System.Collections.Generic;
using UnityEngine;
using cityJSON;
using UnityEditor;
using System.IO;
using ConvertCoordinates;
using SimpleJSON;

public static class GenerateTerrainAssets
{
    //public List<Material> materialList = new List<Material>(7);
    //private Material[] materialsArray;
    
    // Start is called before the first frame update
    static void Start()
    {
        double originX=0;
        double originY=0;
        string[] args = System.Environment.GetCommandLineArgs();
        //materialsArray = materialList.ToArray();
        for (int i = 0; i < args.Length; i++)
        {
            if (args[i].StartsWith("Xmin="))
            {
                originX = double.Parse(args[i].Replace("Xmin=", ""));
            }
            if (args[i].StartsWith("Ymin="))
            {
                originY = double.Parse(args[i].Replace("Ymin=", ""));
            }
        }


        //originX = double.Parse(args[args.Length-2]);
        //originY = double.Parse(args[args.Length-1]);
        //ImportSingle(originX, originY);
        Debug.Log("klaar");
        //importeer();
        GenerateTerrainAssetBundles();
    }

    
    static void ImportSingle(double OriginX, double OriginY)
    {

        double originX = OriginX;
        double originY = OriginY;
        string basefilepath = "E:/TiledData/Terrain1000x1000/";
        
        string jsonfilename = originX.ToString() + "-" +originY.ToString() +".json";
        
        int LOD = 1;
        
       
        float tileSize = 1000;
        string filepath = basefilepath;
        Debug.Log(filepath);

        if (File.Exists(filepath+jsonfilename))
        {

            
        CityModel cm = new CityModel(filepath, jsonfilename);

            //type voetpad

            Mesh RoadsvoetpadMesh = CreateCityObjectMesh(cm, "Road", originX, originY, tileSize,"bgt_functie",new List<string> { "voetpad","voetgangersgebied", "ruiterpad", "voetpad op trap" },true);
            Mesh LandUseVoetpadMesh = CreateCityObjectMesh(cm, "LandUse", originX, originY, tileSize, "bgt_fysiekvoorkomen", new List<string> { "open verharding" }, true);
            LandUseVoetpadMesh = SimplifyMesh(LandUseVoetpadMesh, 0.05f);
            //combine meshes of type "voetpad"
            CombineInstance[] voetpadcombi = new CombineInstance[2];
            voetpadcombi[0].mesh = RoadsvoetpadMesh;
            voetpadcombi[1].mesh = LandUseVoetpadMesh;
            Mesh voetpadmesh = new Mesh();
            voetpadmesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            voetpadmesh.CombineMeshes(voetpadcombi, true,false);
            //type fietspad
            Mesh fietspadMesh = CreateCityObjectMesh(cm, "Road", originX, originY, tileSize, "bgt_functie", new List<string> { "fietspad" }, true);

            //type parkeervak
            Mesh parkeervlakMesh = CreateCityObjectMesh(cm, "Road", originX, originY, tileSize, "bgt_functie", new List<string> { "parkeervlak" }, true);
            //type spoorbaan
            Mesh spoorbaanMesh = CreateCityObjectMesh(cm, "Road", originX, originY, tileSize, "bgt_functie", new List<string> { "spoorbaan" }, true);
            //type woonerf
            Mesh WoonerfMesh = CreateCityObjectMesh(cm, "Road", originX, originY, tileSize, "bgt_functie", new List<string> { "transitie", "woonerf" }, true);
            
            // type weg
            Mesh roadsMesh = CreateCityObjectMesh(cm, "Road", originX, originY, tileSize, "bgt_functie", new List<string> { "fietspad","parkeervlak","ruiterpad","spoorbaan","voetgangersgebied","voetpad","voetpad op trap","woonerf"}, false);
            Mesh LandUseVerhardMesh = CreateCityObjectMesh(cm, "LandUse", originX, originY, tileSize, "bgt_fysiekvoorkomen", new List<string> { "gesloten verharding" }, true);
            LandUseVerhardMesh = SimplifyMesh(LandUseVerhardMesh, 0.05f);
            // combine meshes of type "weg"
            CombineInstance[] wegcombi = new CombineInstance[2];
            wegcombi[0].mesh = roadsMesh;
            wegcombi[1].mesh = LandUseVerhardMesh;
            Mesh wegmesh = new Mesh();
            wegmesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            wegmesh.CombineMeshes(wegcombi, true,false);

            // type groen
            Mesh plantcoverMesh = CreateCityObjectMesh(cm, "PlantCover", originX, originY, tileSize, "bgt_fysiekvoorkomen", new List<string> { "alles" }, false);
            Mesh LanduseGroenMesh = CreateCityObjectMesh(cm, "LandUse", originX, originY, tileSize, "bgt_fysiekvoorkomen", new List<string> { "groenvoorziening" }, true);
            LanduseGroenMesh = SimplifyMesh(LanduseGroenMesh, 0.05f);
            //combine meshes of type "groen"
            CombineInstance[] groencombi = new CombineInstance[2];
            groencombi[0].mesh = plantcoverMesh;
            groencombi[1].mesh = LanduseGroenMesh;
            Mesh groenMesh = new Mesh();
            groenMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            groenMesh.CombineMeshes(groencombi, true,false);

            //type erf
            Mesh erfMesh = CreateCityObjectMesh(cm, "LandUse", originX, originY, tileSize, "bgt_fysiekvoorkomen", new List<string> { "erf" }, true);
            erfMesh = SimplifyMesh(erfMesh, 0.05f);
            
            //type onverhard
            Mesh LandUseMesh = CreateCityObjectMesh(cm, "LandUse", originX, originY, tileSize, "bgt_fysiekvoorkomen", new List<string> {  "erf", "groenvoorziening", "gesloten verharding", "open verharding" }, false);
            LandUseMesh = SimplifyMesh(LandUseMesh, 0.05f);

            Mesh genericCityObjectMesh = CreateCityObjectMesh(cm, "GenericCityObject", originX, originY, tileSize, "bgt_type", null, true);
            Mesh waterBodyMesh = CreateCityObjectMesh(cm, "WaterBody", originX, originY, tileSize, "bgt_type", null, true);
            Mesh bridgeMesh = CreateCityObjectMesh(cm, "Bridge", originX, originY, tileSize, "bgt_type", null, true);

            

            

            

            //create LOD1 Mesh
            CombineInstance[] combi = new CombineInstance[12];
            combi[0].mesh = voetpadmesh; //
            combi[1].mesh = fietspadMesh; //
            combi[2].mesh = parkeervlakMesh; //
            combi[3].mesh = wegmesh; //
            combi[4].mesh = groenMesh; //
            combi[5].mesh = erfMesh; //
            combi[6].mesh = LandUseMesh; //
            combi[7].mesh = spoorbaanMesh; //
            combi[8].mesh = WoonerfMesh; //
            combi[9].mesh = genericCityObjectMesh;
            combi[10].mesh = bridgeMesh;
            combi[11].mesh = waterBodyMesh;
            ;

            Mesh lod1Mesh = new Mesh();
            lod1Mesh.CombineMeshes(combi, false, false);
            lod1Mesh.uv2 = RDuv2(lod1Mesh.vertices, CoordConvert.RDtoUnity(new Vector3RD(originX, originY,0)), tileSize);
            Physics.BakeMesh(lod1Mesh.GetInstanceID(), false);
            AssetDatabase.CreateAsset(lod1Mesh, "Assets/terrainMeshes/LOD0/terrain_" + originX + "-" + originY + "-lod1.mesh");
            //for debug

            //GetComponent<MeshFilter>().sharedMesh = lod1Mesh;



            //create LOD0MEsh
            combi = new CombineInstance[5];
            combi[0].mesh = voetpadmesh;
            combi[1].mesh = fietspadMesh;
            combi[2].mesh = parkeervlakMesh;
            combi[3].mesh = wegmesh;
            combi[4].mesh = spoorbaanMesh;


            Mesh Roads = new Mesh();
            Roads.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            Roads.CombineMeshes(combi, true, false);
            Roads = SimplifyMesh(Roads, 0.05f);

            combi = new CombineInstance[3];
            combi[0].mesh = erfMesh;
            combi[1].mesh = LandUseMesh;
            combi[2].mesh = WoonerfMesh;
            
            Mesh landuse = new Mesh();
            landuse.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            landuse.CombineMeshes(combi, true, false);
            landuse = SimplifyMesh(landuse, 0.05f);

            combi = new CombineInstance[12];


            combi[0].mesh = CreateEmptyMesh(); //
            combi[1].mesh = CreateEmptyMesh(); //
            combi[2].mesh = CreateEmptyMesh(); //
            combi[3].mesh = Roads; //
            combi[4].mesh = SimplifyMesh(groenMesh, 0.05f);//
            combi[5].mesh = CreateEmptyMesh(); //
            combi[6].mesh = landuse; //
            combi[7].mesh = CreateEmptyMesh(); //
            combi[8].mesh = CreateEmptyMesh(); //
            combi[9].mesh = genericCityObjectMesh;
            combi[10].mesh = bridgeMesh;
            combi[11].mesh = waterBodyMesh;


            Mesh lod0Mesh = new Mesh();
            lod0Mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            lod0Mesh.CombineMeshes(combi, false, false);
            lod0Mesh.uv2 = RDuv2(lod0Mesh.vertices, CoordConvert.RDtoUnity(new Vector3RD(originX, originY, 0)), tileSize);
            Physics.BakeMesh(lod0Mesh.GetInstanceID(), false);
            AssetDatabase.CreateAsset(lod0Mesh, "Assets/terrainMeshes/LOD0/terrain_" + originX + "-" + originY + "-lod0.mesh");
            
        }
    }

    static private Mesh SimplifyMesh(Mesh mesh, float quality)
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
        meshSimplifier.EnableSmartLink = true;
        DecimatedMesh = meshSimplifier.ToMesh();
        DecimatedMesh.RecalculateNormals();
        DecimatedMesh.Optimize();
        return DecimatedMesh;
    }


    static private bool PointISInsideArea(Vector3RD point, double OriginX, double OriginY, float tileSize)
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

    static private Mesh CreateCityObjectMesh(CityModel cityModel, string cityObjectType, double originX, double originY, float tileSize, string bgtProperty, List<string> bgtValues, bool include)
    {
        

        List<Vector3RD> RDTriangles = GetTriangleListRD(cityModel, cityObjectType, bgtProperty, bgtValues,include);

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

    static private Mesh CreateEmptyMesh()
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

    static private Vector2[] RDuv2(Vector3[] verts, Vector3 UnityOrigin, float tileSize)
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

    static private Mesh WeldVertices(Mesh mesh)
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


    static void importeer()
    {
        int Xmin = 135000;
        int Ymin = 474000;
        int Xmax = 140000;
        int Ymax = 500000;

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


    public static void GenerateTerrainAssetBundles()
    {
        DirectoryInfo directory = new DirectoryInfo(Application.dataPath + "/terrainMeshes/LOD0/");
        var fileInfo = directory.GetFiles();

        foreach (var file in fileInfo)
        {
            if (!file.Name.Contains(".meta") && !File.ReadAllText(file.FullName).Contains("vertexCount: 0"))
            {
                //Create asset bundle from mesh we just made
                AssetBundleBuild[] buildMap = new AssetBundleBuild[1];
                string[] assetNames = new string[1];
                assetNames[0] = "Assets/terrainMeshes/LOD0/" + file.Name;

                buildMap[0].assetBundleName = file.Name.Replace(".mesh", "");
                buildMap[0].assetNames = assetNames;

                BuildPipeline.BuildAssetBundles("Terrain", buildMap, BuildAssetBundleOptions.None, BuildTarget.WebGL);
            }
        }

        Debug.Log("Done exporting Tree tile AssetBundles");
    }






    static private List<Vector3RD> GetVertsRD(CityModel cityModel)
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
    static public List<Vector3RD> GetTriangleListRD(CityModel cityModel, string cityObjectType, string bgtProperty, List<string> bgtValues, bool include)
    {
        List<Vector3RD> vertsRD = GetVertsRD(cityModel);
        List<Vector3RD> triangleList = new List<Vector3RD>();
        List<int> triangles = new List<int>();
        bool Include;
        foreach (JSONNode cityObject in cityModel.cityjsonNode["CityObjects"])
        {
            Include = !include;
            if (cityObject["type"] == cityObjectType)
            {
                if (bgtValues==null)
                {
                    Include = !Include;
                }
                else if (bgtValues.Contains(cityObject["attributes"][bgtProperty]))
                {
                    Include = !Include;
                }
                if (Include)
                {
                    triangles.AddRange(ReadTriangles(cityObject));
                }
                    

            }
        }
        for (int i = 0; i < triangles.Count; i++)
        {
            triangleList.Add(vertsRD[triangles[i]]);
        }
        return triangleList;
    }

    static private List<Vector3> CreateClippingPolygon(float tilesize)
    {
        List<Vector3> polygon = new List<Vector3>();
        polygon.Add(new Vector3(0, 0, 0));
        polygon.Add(new Vector3(tilesize, 0, 0));
        polygon.Add(new Vector3(tilesize, 0, tilesize));
        polygon.Add(new Vector3(0, 0, tilesize));
        return polygon;
    }
    static private List<int> ReadTriangles(JSONNode cityObject)
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
    static string CreateAssetFolder(string folderpath, string foldername)
    {

        if (!AssetDatabase.IsValidFolder(folderpath + "/" + foldername))
        {
            AssetDatabase.CreateFolder(folderpath, foldername);
        }
        return folderpath + "/" + foldername;
    }
}
