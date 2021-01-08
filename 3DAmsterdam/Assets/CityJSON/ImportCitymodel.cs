#if UNITY_EDITOR

using cityJSON;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using ConvertCoordinates;

public class ImportCitymodel: MonoBehaviour
{
    public int LOD = 1;
    public string objectType = "Buildings";
    public Material DefaultMaterial;
    public Material windowMaterial;
    private string basefilepath;
    private string filename;
    private string basefilename;
    public void Start()
    {

        if (LOD==2 && objectType == "Buildings")
        {
            basefilepath = "E:/brondata/LOD2Buildings/";
            filename = "gebouwenLOD2.json";
            ImportBuildingsLOD2();
        }
        if (LOD == 1 && objectType=="Buildings")
        {
            basefilepath = "E:/TiledData/BuildingsLOD1/";
            filename = "buildingsLOD1.json";
            ImportBuildingsLOD1();
        }
        if (LOD == 1 && objectType == "Bridges")
        {
            basefilepath = "E:/TiledData/BridgesLOD1/";
            filename = "BridgesLOD1.json";
            ImportBuildingsLOD1();
        }
        if (objectType == "Single")
        {
            filename = "D:/3DAmsterdam/lod2rondDam/buildingsLOD3Dam.json";
            ImportSingleFile();
        }
        if (objectType == "Utrecht")
        {
            CoordConvert.ReferenceRD = new Vector3RD(136778, 455859, -42.962);
            CoordConvert.ReferenceWGS84 = new Vector3WGS(5.121314, 52.090695, 0);
            basefilepath = "D:/3DUtrecht/tiledBuildings/";
            basefilename = "gebouwen_{x}.0_{y}.0.json";
            ImportBuildingsNew();
        }
        //int X = 12;
        //int Y = 1;

        //            filepath = basefilepath + "tile_" + Y + "_" + X + "/";
        //        Debug.Log(filepath);
        //double originX = (X * stepSize) + Xmin;
        //double originY = (Y * stepSize) + Ymin;
        //CreateTile(filepath, filename, LOD, originX, originY);


        //GameObject go = Selection.activeGameObject;
        //CityJSONSettings settings = go.GetComponent<CityJSONSettings>();

        //CreateTile(settings.filepath, settings.filename, settings.LOD, settings.Origin.x, settings.Origin.y);

    }
    private void ImportSingleFile()
    {
        int originX = 120000;
        int originY = 490000;

        CityModel Citymodel = new CityModel("", filename);
        List<Building> buildings = Citymodel.LoadBuildings(LOD);
        CreateGameObjects creator = new CreateGameObjects();
        creator.minimizeMeshes=true;
        creator.CreatePrefabs = true; 
        Dictionary<string, Material> materialmapping = new Dictionary<string, Material>();
        materialmapping.Add("Window", windowMaterial);
        //creator.CreateMeshesByIdentifier(buildings, "name", new ConvertCoordinates.Vector3RD(originX, originY, 0));
        creator.CreateBuildings(buildings, new Vector3Double(originX, originY, 0), DefaultMaterial, this.gameObject,false,materialmapping);
        //CreateTile("", filename, LOD, originX, originY, objectType);

    }

    private void ImportBuildingsLOD1()
    {
        int Xmin = 125000;
        int Ymin = 443000;
        int Xmax = 145000;
        int Ymax = 462000;

        int stepSize = 1000;
        string filepath = "";
        int LOD = 1;


        for (int X = Xmin; X <= Xmax; X+=stepSize)
        {
            for (int Y = Ymin; Y <= Ymax; Y+=stepSize)
            {
                filepath = basefilepath + "tile_" + X + ".0_" + Y + ".0/";
                Debug.Log(filepath);
                double originX = X;
                double originY = Y;
                CreateTile(filepath, filename, LOD, originX, originY, objectType);
            }
        }
    }


    private void ImportBuildingsNew()
    {
        int Xmin = 125000;
        int Ymin = 443000;
        int Xmax = 145000;
        int Ymax = 462000;

        int stepSize = 1000;
        string filepath = "";
        int LOD = 1;


        for (int X = Xmin; X < Xmax; X += stepSize)
        {
            for (int Y = Ymin; Y < Ymax; Y += stepSize)
            {
                filepath = basefilepath;
                string filename = basefilename.Replace("{x}", X.ToString());
                filename = filename.Replace("{y}",Y.ToString());
                Debug.Log(filepath);
                double originX = X;
                double originY = Y;
                CreateTile(filepath, filename, LOD, originX, originY, objectType);
            }
        }
    }
    private void ImportBuildingsLOD2()
    {
        int Xmin = 105000;
        int Ymin = 475000;
        int Xmax = 136000;
        int Ymax = 498000;

        int stepSize = 1000;

        
        string filepath = "";
        
        int LOD = 2;

        //testpurpose
        //Xmin = 129000;
        //Xmax = 130000;
        //Ymin = 185000;
        //Ymax = 186000;
        //Ymax = 476000;

        for (int X = 0; X < (Xmax - Xmin) / stepSize; X++)
        {
            for (int Y = 0; Y < (Ymax - Ymin) / stepSize; Y++)
            {
                filepath = basefilepath + "tile_" + Y + "_" + X + "/";
                Debug.Log(filepath);
                double originX = (X * stepSize) + Xmin;
                double originY = (Y * stepSize) + Ymin;
                CreateTile(filepath, filename, LOD, originX, originY, objectType);
            }
        }
    }


    static void CreateTile(string filepath, string filename,int LOD, double X, double Y, string objectType)
    {
       // Debug.Log("started Building");
        CityModel Citymodel = new CityModel(filepath, filename);
        List<Building> buildings = Citymodel.LoadBuildings(LOD);
        

        CreateBuildingSurface createBuildingSurface = new CreateBuildingSurface();
        GameObject container;
        container = createBuildingSurface.CreateMesh(Citymodel, new ConvertCoordinates.Vector3RD(X, Y, 0));
        
        SavePrefab(container, X.ToString(), Y.ToString(), LOD, objectType);
        buildings = null;
        Citymodel = null;

        

    }

    static void SavePrefab(GameObject container, string X, string Y, int LOD, string objectType)
    {

        MeshFilter[] mfs = container.GetComponentsInChildren<MeshFilter>();
        string objectFolder = CreateAssetFolder("Assets", objectType);
        string LODfolder = CreateAssetFolder(objectFolder,"LOD"+LOD);
        string SquareFolder = CreateAssetFolder(LODfolder, X + "_" + Y);
        string MeshFolder = CreateAssetFolder(SquareFolder, "meshes");
        //string PrefabFolder = CreateAssetFolder(SquareFolder, "Prefabs");
        string dataFolder = CreateAssetFolder(SquareFolder, "data");
        ObjectMappingClass objectMappingClass = ScriptableObject.CreateInstance<ObjectMappingClass>();
        objectMappingClass.ids = container.GetComponent<ObjectMapping>().BagID;
        objectMappingClass.triangleCount = container.GetComponent<ObjectMapping>().TriangleCount;
        Vector2[] meshUV = container.GetComponent<MeshFilter>().mesh.uv;
        objectMappingClass.vectorMap = container.GetComponent<ObjectMapping>().vectorIDs;
        List <Vector2> mappedUVs = new List<Vector2>();
        Vector2Int TextureSize = ObjectIDMapping.GetTextureSize(objectMappingClass.ids.Count);
        for (int i = 0; i < objectMappingClass.ids.Count; i++)
        {
            mappedUVs.Add(ObjectIDMapping.GetUV(i, TextureSize));
        }

        objectMappingClass.mappedUVs = mappedUVs;
        objectMappingClass.TextureSize = TextureSize;
        objectMappingClass.uvs = meshUV;

        string typeName = objectType.ToLower();

        container.GetComponent<MeshFilter>().mesh.uv = null;
        AssetDatabase.CreateAsset(objectMappingClass, dataFolder +"/"+ X + "_" + Y + "_"+typeName+"_lod" + LOD + "-data.asset");
        AssetDatabase.SaveAssets();
        AssetImporter.GetAtPath(dataFolder+"/"  + X + "_" + Y + "_"+typeName+"_lod" + LOD + "-data.asset").SetAssetBundleNameAndVariant(X + "_" + Y + "_"+typeName+"_lod" + LOD + "-data", "");
        int meshcounter = 0;
        foreach (MeshFilter mf in mfs)
        {
            AssetDatabase.CreateAsset(mf.sharedMesh, MeshFolder + "/" + X + "_"+Y+"_"+typeName+"_lod"+LOD+".mesh");
            AssetDatabase.SaveAssets();
            AssetImporter.GetAtPath(MeshFolder + "/" + X + "_" + Y + "_"+typeName+"_lod" + LOD + ".mesh").SetAssetBundleNameAndVariant(typeName+"_" + X + "_" + Y + "_lod" + LOD,"");
            meshcounter++;
        }
        AssetDatabase.SaveAssets();
        //PrefabUtility.SaveAsPrefabAssetAndConnect(container, PrefabFolder + "/" + container.name + ".prefab",InteractionMode.AutomatedAction);
        //AssetImporter.GetAtPath(PrefabFolder + "/" + container.name + ".prefab").SetAssetBundleNameAndVariant("Building_" + X + "_" +Y + "_LOD" + LOD, "");
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
#endif