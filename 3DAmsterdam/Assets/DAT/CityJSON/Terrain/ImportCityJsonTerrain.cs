
#if UNITY_EDITOR

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using cityJSON;
using UnityEditor;
using System.IO;


public class ImportCityJsonTerrain : MonoBehaviour
{
    public List<Material> materialList = new List<Material>(7);
    private Material[] materialsArray;
    
    // Start is called before the first frame update
    void Start()
    {
        materialsArray = materialList.ToArray();
        ImportSingle();
        //importeer();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    void ImportSingle()
    {
        int Xmin = 109000;
        int Ymin = 474000;
        int Xmax = 140000;
        int Ymax = 501000;

        
        string basefilepath = "E:/TiledData/TerrainLOD1/tile_121000.0_487000.0/";
        
        string jsonfilename = "500x500.json";
        
        int LOD = 1;
        
        double originX = 121000;
        double originY = 487000;

        string filepath = basefilepath;
        Debug.Log(filepath);

        if (File.Exists(filepath+jsonfilename))
        {

            
        CityModel cm = new CityModel(filepath, jsonfilename);

            CreateTerrainSurface surfaceCreator = transform.gameObject.GetComponent<CreateTerrainSurface>();

            GameObject go = surfaceCreator.CreateMesh(cm, new ConvertCoordinates.Vector3RD(originX + 500, originY + 500, 0));
            //Mesh mesh = go.GetComponent<MeshFilter>().sharedMesh;
            //UnityEngine.ProBuilder.MeshUtility.CollapseSharedVertices(mesh);
            //mesh.Optimize();
            //SavePrefab("Terrain_Original", go, originX.ToString(), originY.ToString(), 1);

            //var DecimatedMesh = mesh;
            //float quality = 0.02f;
            //var meshSimplifier = new UnityMeshSimplifier.MeshSimplifier();
            //meshSimplifier.Initialize(DecimatedMesh);
            //meshSimplifier.PreserveBorderEdges = true;
            //meshSimplifier.MaxIterationCount = 500;
            //meshSimplifier.SimplifyMesh(quality);
            //DecimatedMesh = meshSimplifier.ToMesh();
            //DecimatedMesh.RecalculateNormals();

            //Debug.Log(DecimatedMesh.vertexCount);
            //go.GetComponent<MeshFilter>().sharedMesh = DecimatedMesh;
               
            //go.name = originX.ToString()+ "-" +originY.ToString()+"-LOD" + LOD;
            //go.transform.parent = transform;
            //go.GetComponent<MeshRenderer>().sharedMaterials = materialsArray;
            //SavePrefab("Terrain_2percent", go, originX.ToString(), originY.ToString(), 1);







            //go = new GameObject("lod0");
            //go.AddComponent<MeshFilter>();
            //go.AddComponent<MeshRenderer>().materials = materialList.ToArray();
            //meshSimplifier = new UnityMeshSimplifier.MeshSimplifier();
            //meshSimplifier.Initialize(DecimatedMesh);
            //meshSimplifier.PreserveBorderEdges = true;
            //meshSimplifier.MaxIterationCount = 500;
            //quality = 0.2f;
            //meshSimplifier.SimplifyMesh(quality);
            //DecimatedMesh = meshSimplifier.ToMesh();
            //DecimatedMesh.RecalculateNormals();
            //Debug.Log(DecimatedMesh.vertexCount);
            //go.GetComponent<MeshFilter>().sharedMesh = DecimatedMesh;
            //SavePrefab("Terrain_lod0", go, originX.ToString(), originY.ToString(), 1);

        }
    }


    void importeer()
    {
        int Xmin = 109000;
        int Ymin = 475000;
        int Xmax = 136000;
        int Ymax = 498000;

        int stepSize = 1000;

        string basefilepath = "E:/TiledData/TerrainLOD1/";
        string filepath = "";
        string jsonfilename = "terraintile.json";
        int LOD = 1;
        bool skip = false;
        //testpurpose
        int Xstart = (109000 - Xmin) / stepSize;
        
        for (int X = Xstart; X < ((Xmax - Xmin) / stepSize); X++)
        {
            for (int Y = 0; Y < ((Ymax - Ymin) / stepSize); Y++)
            {
                skip = false;
                filepath = basefilepath + "tile_" + ((X*stepSize)+Xmin )+ ".0_" + ((Y * stepSize) + Ymin) + ".0/";
                
                if (File.Exists(filepath+jsonfilename)==false)
                {
                    Debug.Log(filepath + jsonfilename + " bestaat niet");
                    skip = true;
                }
                    double originX = (X * stepSize) + Xmin;
                    double originY = (Y * stepSize) + Ymin;

                if (skip == false)
                {
                    Debug.Log("loading: " +filepath + jsonfilename);
                    CityModel cm = new CityModel(filepath, jsonfilename);
                    CreateTerrainSurface surfaceCreator = new CreateTerrainSurface();
                    GameObject go = surfaceCreator.CreateMesh(cm, new ConvertCoordinates.Vector3RD(originX + 500, originY + 500, 0));
                    go.name = originX.ToString() + "-" + originY.ToString() + "-LOD" + LOD;
                    go.transform.parent = transform;
                    go.GetComponent<MeshRenderer>().sharedMaterials = materialsArray;
                    if (go.GetComponent<MeshFilter>().mesh.vertexCount>0)
                    {
                        SavePrefab("Terrain", go, originX.ToString(), originY.ToString(), 1);
                    }
                    Debug.Log("loaded: " + filepath + jsonfilename);
                }


                
            }
        }

        

    }

    void SavePrefab(string type, GameObject container, string X, string Y, int LOD)
    {

        MeshFilter[] mfs = container.GetComponentsInChildren<MeshFilter>();
        string LODfolder = CreateAssetFolder("Assets/Terrain", "LOD" + LOD);
        string SquareFolder = CreateAssetFolder(LODfolder, X + "_" + Y);
        string MeshFolder = CreateAssetFolder(SquareFolder, "meshes");
        string PrefabFolder = CreateAssetFolder(SquareFolder, "Prefabs");
        int meshcounter = 0;
        foreach (MeshFilter mf in mfs)
        {
            AssetDatabase.CreateAsset(mf.sharedMesh, MeshFolder + "/"+ type+"-" + X +"-" + Y+ "-mesh_" + meshcounter + ".mesh");
            AssetDatabase.SaveAssets();
            AssetImporter.GetAtPath(MeshFolder + "/" + type + "-" + X + "-" + Y + "-mesh_" + meshcounter + ".mesh").SetAssetBundleNameAndVariant("Terrain_" + X + "_" + Y + "_LOD" + LOD, "");
            meshcounter++;
        }
        AssetDatabase.SaveAssets();
        PrefabUtility.SaveAsPrefabAssetAndConnect(container, PrefabFolder + "/" + type + "-" + container.name + ".prefab", InteractionMode.AutomatedAction);
        AssetDatabase.SaveAssets();
        AssetImporter.GetAtPath(PrefabFolder + "/" + type + "-" + container.name + ".prefab").SetAssetBundleNameAndVariant("Terrain_" + X + "_" + Y + "_LOD" + LOD, "");
        AssetDatabase.SaveAssets();



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