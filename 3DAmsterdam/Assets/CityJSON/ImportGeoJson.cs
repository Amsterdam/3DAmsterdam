#if UNITY_EDITOR

using cityJSON;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using ConvertCoordinates;
using System.IO;
using System.Threading;

public class ImportGeoJson : MonoBehaviour
{
    public int LOD = 1;
    public string objectType = "Buildings";
    public Material DefaultMaterial;

    [SerializeField]
    private string geoJsonSourceFilesFolder = "C:/Users/Sam/Desktop/downloaded_tiles_amsterdam/";

    public void Start()
    {
        StartCoroutine(ImportFilesFromFolder(geoJsonSourceFilesFolder));
    }

    private IEnumerator ImportFilesFromFolder(string folderName)
    {
        var info = new DirectoryInfo(folderName);
        var files = info.GetFiles();
        foreach(FileInfo file in files)
        {
            print("Importing file " + file);
            string[] fileNameParts = file.Name.Replace(".json","").Split('_');

            var id = fileNameParts[0];
            var count = fileNameParts[1];

            var xmin = double.Parse(fileNameParts[3]);
            var ymin = double.Parse(fileNameParts[4]);
            var xmax = double.Parse(fileNameParts[5]);
            var ymax = double.Parse(fileNameParts[6]);

            CreateGameObjects(file.FullName, "");
            yield return new WaitForEndOfFrame();
        }
    }

    private void CreateGameObjects(string filepath, string suffix = "")
    {
        CityModel Citymodel = new CityModel(filepath, suffix, true, false);
        List<Building> buildings = Citymodel.LoadBuildings(2.2);
        
        CreateGameObjects creator = new CreateGameObjects();
        creator.minimizeMeshes = true;
        creator.CreatePrefabs = false;
        creator.singleMeshBuildings = true;

        creator.CreateBuildings(buildings, new Vector3Double(), DefaultMaterial, new GameObject(), false);
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