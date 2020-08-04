
#if UNITY_EDITOR

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using cityJSON;
using UnityEditor;
using System.IO;
using UnityEditor.PackageManager.Requests;

public class ImportCityJsonTerrain : MonoBehaviour
{
    public List<Material> materialList = new List<Material>(7);
    private Material[] materialsArray;
    
    // Start is called before the first frame update
    void Start()
    {
        materialsArray = materialList.ToArray();
        ImportSingle();
        //StartCoroutine(importeer());
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    void ImportSingle()
    {
        int Xmin = 105000;
        int Ymin = 475000;
        int Xmax = 135000;
        int Ymax = 497000;

        
        string basefilepath = "E:/UnityData/3DBasisgegevens/";
        
        string jsonfilename = "121000-483000-1x1.json";
        
        int LOD = 1;
        
        double originX = 121000;
        double originY = 483000;

        string filepath = basefilepath;
        Debug.Log(filepath);

        if (File.Exists(filepath+jsonfilename))
        {

            
        CityModel cm = new CityModel(filepath, jsonfilename);

            CreateTerrainSurface surfaceCreator = new CreateTerrainSurface();

            GameObject go = surfaceCreator.CreateMesh(cm, new ConvertCoordinates.Vector3RD(originX + 500, originY + 500, 0));
            go.name = originX.ToString()+ "-" +originY.ToString()+"-LOD" + LOD;
            go.transform.parent = transform;
            go.GetComponent<MeshRenderer>().sharedMaterials = materialsArray;
            SavePrefab("Terrain", go, originX.ToString(), originY.ToString(), 1);
        
        
        }
    }


    IEnumerator importeer()
    {
        int Xmin = 105000;
        int Ymin = 475000;
        int Xmax = 136000;
        int Ymax = 498000;

        int stepSize = 1000;

        string basefilepath = "D:/CityGmlFromDBV2/";
        string filepath = "";
        string jsonfilename = "gebouwenLOD2Poly.json";
        int LOD = 1;
        bool skip = false;
        //testpurpose
        int Xstart = (105000 - Xmin) / stepSize;
        
        for (int X = Xstart; X < (Xmax - Xmin) / stepSize; X++)
        {
            for (int Y = 0; Y < (Ymax - Ymin) / stepSize; Y++)
            {
                skip = false;
                filepath = basefilepath + "tile_" + Y + "_" + X + "/";
                Debug.Log(filepath);
                if (File.Exists(filepath+jsonfilename)==false)
                {
                    skip = true;
                }
                    double originX = (X * stepSize) + Xmin;
                    double originY = (Y * stepSize) + Ymin;

                if (skip == false)
                {
                    CityModel cm = new CityModel(filepath, jsonfilename);
                    CreateTerrainSurface surfaceCreator = new CreateTerrainSurface();
                    GameObject go = surfaceCreator.CreateMesh(cm, new ConvertCoordinates.Vector3RD(originX + 500, originY + 500, 0));
                    go.name = originX.ToString() + "-" + originY.ToString() + "-LOD" + LOD;
                    go.transform.parent = transform;
                    go.GetComponent<MeshRenderer>().sharedMaterials = materialsArray;
                    SavePrefab("Terrain", go, originX.ToString(), originY.ToString(), 1);
                }


                yield return null;
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