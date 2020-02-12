using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


public class LoadCityGML : MonoBehaviour
{
    public CityModel cityModel;
    public Material standaardmateriaal;
    public Material Gevelmateriaal;
    public Material Platdakmateriaal;
    public Material Schuindakmateriaal;
    // Start is called before the first frame update
    void Start()
    {
        //string filename = "";
        //int xmin = 105000;
        //int xmax = 136000;
        //int ymin = 475000;
        //int ymax = 498000;
        ////filename = "D:/Helsinki/CityGML_BUILDINGS_LOD2_WITHTEXTURES_664502x2/CityGML_BUILDINGS_LOD2_WITHTEXTURES_664502x2/CityGML_BUILDINGS_LOD2_WITHTEXTURES_664502x2.gml";
        //int col = 9;
        //int row = 19;

        //int x = xmin+(row * 1000);
        //int y = ymin+(col * 1000);
        //filename = "D:/CityGmlfromDBV2/tile_9_19/gebouwenLOD2.gml";
        //cityModel = new CityModel(filename, standaardmateriaal);
        //cityModel.CreateGameObjects();
        //ProcessCityModel(cityModel, x,y);

        StartCoroutine(ImportTiles());


    }

    IEnumerator ImportTiles()
    {
        int xmin = 105000;
        int xmax = 136000;
        int ymin = 475000;
        int ymax = 498000;

        string filename;
        int col; //y-richting
        int row; //x-richting
        for (int x = xmin; x < xmax; x+=1000)
        {
            row = (x - xmin) / 1000;
            for (int y = ymin; y < ymax; y+=1000)
            {
                col = (y - ymin) / 1000;
                filename = "D:/CityGmlfromDBV2/tile_"+ col + "_"+ row +"/gebouwenLOD2.gml";
                cityModel = new CityModel(filename, standaardmateriaal);

                cityModel.createMeshes();

                SaveOBJFile(cityModel.CreateOBJFile(), "gebouwen_" + x + "-" + y + ".obj");
                ProcessCityModel(cityModel, x, y);
                yield return null;
            }
        }
        
    }
    void SaveOBJFile(string[] content, string name)
    {
        System.IO.File.WriteAllLines(@"D:\LOD2gebouwen\"+name, content);
    }

    void ProcessCityModel(CityModel cityModel, int x, int y)
    {
        GameObject gebouwobject;
        string Assetbundlenaam = "gebouwen_" + x + "_" + y + "_LOD2";

        string BaseFolderName = "Assets/Buildings/LOD2";
        string tegelfolder = CreateAssetFolder(BaseFolderName, x + "_" + y);
        string meshfolder = CreateAssetFolder(tegelfolder, "meshes");
        string prefabfolder = CreateAssetFolder(tegelfolder, "Prefabs");

        gebouwobject = new GameObject(x + "-" + y + "-gebouwen-LOD2");

        MeshMetIDLijst GevelMesh = new MeshMetIDLijst();
        MeshMetIDLijst PlatDakMesh = new MeshMetIDLijst(); ;
        MeshMetIDLijst SchuinDakMesh = new MeshMetIDLijst();
        if (cityModel.Gevelmeshes.Count>0)
        {
            
            GevelMesh = CombineMeshes(cityModel.Gevelmeshes,x,y);
            AddMesh(x, y, "gevel", Assetbundlenaam, gebouwobject, GevelMesh, meshfolder);
        }
        if (cityModel.Platdakmeshes.Count > 0)
        {
            PlatDakMesh = CombineMeshes(cityModel.Platdakmeshes, x, y);
            AddMesh(x, y, "plat dak", Assetbundlenaam, gebouwobject, PlatDakMesh, meshfolder);
        }
        if (cityModel.Schuindakmeshes.Count > 0)
        {
            SchuinDakMesh = CombineMeshes(cityModel.Schuindakmeshes, x, y);
            AddMesh(x, y, "schuin dak", Assetbundlenaam, gebouwobject, SchuinDakMesh, meshfolder);
        }

        string prefabpad = prefabfolder + "/" + gebouwobject.name + ".prefab";

        PrefabUtility.SaveAsPrefabAssetAndConnect(gebouwobject, prefabpad, InteractionMode.AutomatedAction);
        AssetImporter.GetAtPath(prefabpad).assetBundleName = Assetbundlenaam;
        AssetDatabase.SaveAssets();
        // move gameobject to correct location
        Vector3 UnityOrigin = ConvertCoordinates.CoordConvert.RDtoUnity(new Vector3(x + 500, y + 500, 0));
        gebouwobject.transform.position = UnityOrigin;

    }

    private void AddMesh(int x, int y, string type, string Assetbundlenaam, GameObject parentobject, MeshMetIDLijst mesh, string meshfolder)
    {
        GameObject meshobject = new GameObject(type);
        meshobject.transform.parent = parentobject.transform;
        meshobject.AddComponent<MeshFilter>().sharedMesh = mesh.mesh;
        meshobject.AddComponent<MeshItems>().MeshIDS = mesh.idlijst;
        if (type == "gevel")
        {
            meshobject.AddComponent<MeshRenderer>().material = Gevelmateriaal;
        }
        if (type == "plat dak")
        {
            meshobject.AddComponent<MeshRenderer>().material = Platdakmateriaal;
        }
        if (type == "schuin dak")
        {
            meshobject.AddComponent<MeshRenderer>().material = Schuindakmateriaal;
        }
        
        meshobject.AddComponent<MeshCollider>();
        string meshnaam = x + "-" + y + "-"+ type +"-LOD2.mesh";
        AssetDatabase.CreateAsset(mesh.mesh, meshfolder + "/" + meshnaam);
        AssetDatabase.SaveAssets();
        AssetImporter.GetAtPath(meshfolder + "/" + meshnaam).assetBundleName = Assetbundlenaam;
    }
    private MeshMetIDLijst CombineMeshes(List<Meshdata> meshdata,int x, int y)
    {
        //offset bepalen om tegels te verplaatsen naar unity 0,0
        Vector3 UnityOrigin = ConvertCoordinates.CoordConvert.RDtoUnity(new Vector3(x + 500, y + 500, 0));
        Quaternion rotation = Quaternion.Euler(0, 0, 0);
        Matrix4x4 m = Matrix4x4.identity;
        m.SetTRS(Vector3.zero - UnityOrigin, rotation, Vector3.one);

        Dictionary<string, int> idlijst = new Dictionary<string, int>();
        int idnummer = 0;
        CombineInstance[] combine = new CombineInstance[meshdata.Count];
        int meshnummer = 0;
        foreach (Meshdata md in meshdata)
        {
            if (idlijst.ContainsKey(md.id))
            {
                idnummer = idlijst[md.id];
            }
            else
            {
                idnummer = idlijst.Count;
                idlijst.Add(md.id, idnummer);
                
            }
            
            Vector2[] uv3 = new Vector2[md.mesh.vertexCount];
            for (int i = 0; i < uv3.Length; i++)
            {
                uv3[i] = new Vector2(idnummer, idnummer);
            }
            md.mesh.uv3 = uv3;
            Vector3 eersteVector = md.mesh.vertices[0];
            
            combine[meshnummer].mesh = md.mesh;

            
            combine[meshnummer].transform = m;
            meshnummer++;
        }
        Mesh combinedMesh = new Mesh();
        combinedMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;   //
        combinedMesh.CombineMeshes(combine,true,true);
        MeshMetIDLijst mmid = new MeshMetIDLijst();
        mmid.mesh = combinedMesh;
        mmid.idlijst = idlijst;
        return mmid;

    }
    private string CreateAssetFolder(string parentfolder, string foldername)
    {
        string returnname = parentfolder + "/" + foldername;
#if UNITY_EDITOR
        if (AssetDatabase.IsValidFolder(returnname) == false)
        {
            AssetDatabase.CreateFolder(parentfolder, foldername);
            AssetDatabase.SaveAssets();
        }
#endif
        return returnname;
    }
}

class MeshMetIDLijst
    {
    public Mesh mesh;
    public Dictionary<string, int> idlijst;
    }

