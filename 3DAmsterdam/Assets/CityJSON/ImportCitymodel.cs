using cityJSON;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class ImportCitymodel: MonoBehaviour
{

    [MenuItem("GameObject/CityJSON/Import Citymodel",false,10)]
    static void importeer()
    {
        int Xmin = 105000;
        int Ymin = 475000;
        int Xmax = 136000;
        int Ymax = 498000;

        int stepSize = 1000;

        string basefilepath = "D:/CityGmlFromDBV2/";
        string filepath = "";
        string filename = "gebouwenLOD2.json";
        int LOD = 2;

        //testpurpose
        Xmin = 129000;
        //Ymax = 476000;

        for (int X = 0; X < (Xmax-Xmin)/stepSize; X++)
        {
            for (int Y = 0; Y < (Ymax - Ymin) / stepSize; Y++)
            {
                filepath = basefilepath + "tile_" + Y + "_" + X + "/";
                Debug.Log(filepath);
                double originX = (X * stepSize) + Xmin;
                double originY = (Y * stepSize) + Ymin;
                CreateTile(filepath, filename, LOD, originX, originY);
            }
        }
    
    //GameObject go = Selection.activeGameObject;
        //CityJSONSettings settings = go.GetComponent<CityJSONSettings>();

        //CreateTile(settings.filepath, settings.filename, settings.LOD, settings.Origin.x, settings.Origin.y);

    }

   static void CreateTile(string filepath, string filename,int LOD, double X, double Y)
    {
        CityModel Citymodel = new CityModel(filepath, filename);
        List<Building> buildings = Citymodel.LoadBuildings(LOD);
        Citymodel = null;

        CreateGameObjectsV2 objCreatorV2 = new CreateGameObjectsV2();
        GameObject container;
        container = objCreatorV2.CreateMeshesByIdentifier(buildings, "name",new ConvertCoordinates.Vector3RD(X+500,Y+500,0));
        objCreatorV2 = null;
        SavePrefab(container, X.ToString(), Y.ToString(), LOD);
        buildings = null;
    }

    static void SavePrefab(GameObject container, string X, string Y, int LOD)
    {

        MeshFilter[] mfs = container.GetComponentsInChildren<MeshFilter>();
        string LODfolder = CreateAssetFolder("Assets/Buildings","LOD"+LOD);
        string SquareFolder = CreateAssetFolder(LODfolder, X + "_" + Y);
        string MeshFolder = CreateAssetFolder(SquareFolder, "meshes");
        string PrefabFolder = CreateAssetFolder(SquareFolder, "Prefabs");
        int meshcounter = 0;
        foreach (MeshFilter mf in mfs)
        {
            AssetDatabase.CreateAsset(mf.sharedMesh, MeshFolder + "/mesh_" + meshcounter + ".mesh");
            AssetDatabase.SaveAssets();
            AssetImporter.GetAtPath(MeshFolder + "/mesh_" + meshcounter + ".mesh").SetAssetBundleNameAndVariant("Building_" + X + "_" + Y + "_LOD" + LOD,"");
            meshcounter++;
        }
        AssetDatabase.SaveAssets();
        PrefabUtility.SaveAsPrefabAssetAndConnect(container, PrefabFolder + "/" + container.name + ".prefab",InteractionMode.AutomatedAction);
        AssetImporter.GetAtPath(PrefabFolder + "/" + container.name + ".prefab").SetAssetBundleNameAndVariant("Building_" + X + "_" +Y + "_LOD" + LOD, "");
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
