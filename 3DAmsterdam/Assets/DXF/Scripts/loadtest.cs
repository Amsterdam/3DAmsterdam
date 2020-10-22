using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using netDxf;
using netDxf.Entities;
using netDxf.Blocks;
using netDxf.Tables;
using UnityEngine.Networking;
using ConvertCoordinates;

public class loadtest : MonoBehaviour
{

    private List<UnityEngine.Mesh> buildingMeshes;
    private UnityEngine.Mesh terrainMesh;
    private List<ObjectMappingClass> objectlists;
    private UnityEngine.Vector3 UnityOffset = new UnityEngine.Vector3();
    private Vector3RD RDOrigin = new Vector3RD();
    
    private DxfDocument doc = new DxfDocument();

    private bool buildingsDone = false;
    private bool terrainDone = false;

    public bool ConvertTerrain = true;
    public bool ComvertBuildings = true;
    public int buildingsLOD = 2;

    private bool TileReady = true;
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(ProcessTiles());
        
       // ConvertTile(121000,487000);
    }
    IEnumerator ProcessTiles()
    {
        int Xmin = 105000;
        int Xmax = 135000;
        int tileSize = 1000;
        int Ymin = 475000;
        int Ymax = 497000;

        int totalCount = ((Xmax - Xmin+1)/1000) * ((Ymax - Ymin+1)/1000);
        int counter = 0;
        for (int X = Xmin; X < Xmax; X+=tileSize)
        {
            for (int Y = Ymin; Y <Ymax; Y += tileSize)
            {
                yield return new WaitUntil(() => TileReady == true);
                TileReady = false;
                counter++;
                Debug.Log("converting " + X.ToString() + "-" + Y.ToString());
                Debug.Log(counter + " van " + totalCount);
                ConvertTile(X, Y);

            }

            Debug.Log("finishes");
        }
    }
    

    void ConvertTile(int X, int Y)
    {
        objectlists = new List<ObjectMappingClass>();
        buildingMeshes = new List<UnityEngine.Mesh>();
        RDOrigin.x = X + 500;
        RDOrigin.y = Y + 500;
        RDOrigin.z = 0;
        UnityOffset = CoordConvert.RDtoUnity(RDOrigin);
        SetupDXF(X, Y);
        buildingsDone = false;
        terrainDone = false;
        StartCoroutine(SaveDXF(X, Y));
        StartCoroutine(LoadBuildings(X, Y, (myReturnValue) => {
            if (myReturnValue) { ImportObjectData(X, Y); };
        }));

        StartCoroutine(LoadTerrain(X, Y, (myReturnValue) => {
            if (myReturnValue) { AddTerrain(X, Y); };
        }));

    }

    void ImportObjectData(int X, int Y)
    {
        StartCoroutine(LoadBuildingData(X, Y, (myReturnValue) => {
            if (myReturnValue) { AddBuildings(X, Y); };
        }));
    }
    void SetupDXF(int X, int Y)
    {
        doc = new DxfDocument();
        doc.DrawingVariables.InsUnits = netDxf.Units.DrawingUnits.Meters;
    }
    IEnumerator SaveDXF(int X, int Y)
    {
        yield return new WaitUntil(() => buildingsDone == true);
        terrainDone = false;
        StartCoroutine(LoadTerrain(X, Y, (myReturnValue) => {
            if (myReturnValue) { AddTerrain(X, Y); };
        }));
        yield return new WaitUntil(() => terrainDone == true);
        string file = "E:/exporttest/" + X.ToString() + "-" + Y.ToString() + "-LOD" + buildingsLOD + ".dxf";
        doc.Save(file, true);
        TileReady = true;
        //Debug.Log("klaar");
    }

    void AddBuildings(int X, int Y)
    { 
        List<EntityObject>[] blockparts = new List<EntityObject>[objectlists[0].ids.Count];

        for (int i = 0; i < blockparts.Length; i++)
        {
            blockparts[i] = new List<EntityObject>();
        }
        
        
        Layer gebouwenlaag = new Layer("gebouwen");
        gebouwenlaag.Color = netDxf.AciColor.Blue;
        //doc.AddEntity(gebouwenlaag);
        Face3d face = new Face3d();

        UnityEngine.Mesh mesh = buildingMeshes[0];
        int[] Indexes = mesh.GetIndices(0);
        UnityEngine.Vector3[] vertices = mesh.vertices;
        int idnumber = 0;
        for (int i = 0; i < Indexes.Length; i+=3)
        {
            idnumber = objectlists[0].vectorMap[Indexes[i]];
            UnityEngine.Vector3 vertex = vertices[Indexes[i]] + UnityOffset;
            Vector3RD coordinate = CoordConvert.UnitytoRD(vertex);
            netDxf.Vector3 vertex1 = new netDxf.Vector3(coordinate.x-RDOrigin.x, coordinate.y-RDOrigin.y, coordinate.z-RDOrigin.z);
            vertex = vertices[Indexes[i+1]] + UnityOffset;
            coordinate = CoordConvert.UnitytoRD(vertex);
            netDxf.Vector3 vertex2 = new netDxf.Vector3(coordinate.x - RDOrigin.x, coordinate.y - RDOrigin.y, coordinate.z - RDOrigin.z);
            vertex = vertices[Indexes[i + 2]] + UnityOffset;
            coordinate = CoordConvert.UnitytoRD(vertex);
            netDxf.Vector3 vertex3 = new netDxf.Vector3(coordinate.x - RDOrigin.x, coordinate.y - RDOrigin.y, coordinate.z - RDOrigin.z);
            face = new Face3d(vertex1, vertex2, vertex3);
            blockparts[idnumber].Add(face);
        }


        for (int i = 0; i < blockparts.Length; i++)
        {
            string blockname = objectlists[0].ids[i];
            if (blockname =="")
            {
                blockname = "noID";
            }
            Block blok = new Block(blockname, blockparts[i]);
            blok.Layer = gebouwenlaag;
            doc.Blocks.Add(blok);
            Insert blokInsert = new Insert(blok, new netDxf.Vector3(RDOrigin.x, RDOrigin.y, RDOrigin.z), 1);
            blokInsert.Layer = gebouwenlaag;
            doc.AddEntity(blokInsert);
        }

        buildingsDone = true;

        

       
        

    }


    void AddTerrain(int X,int Y)
    {
        Layer terreinLaag = new Layer("terrein");
        terreinLaag.Color = netDxf.AciColor.LightGray;

        Face3d face = new Face3d();
        List<EntityObject> blockparts = new List<EntityObject>();
        UnityEngine.Mesh mesh = terrainMesh;
        int[] Indexes = mesh.GetIndices(0);
        UnityEngine.Vector3[] vertices = mesh.vertices;

        for (int i = 0; i < Indexes.Length; i += 3)
        {
            
            UnityEngine.Vector3 vertex = vertices[Indexes[i]] + UnityOffset;
            Vector3RD coordinate = CoordConvert.UnitytoRD(vertex);
            netDxf.Vector3 vertex1 = new netDxf.Vector3(coordinate.x - RDOrigin.x, coordinate.y - RDOrigin.y, coordinate.z -UnityOffset.y);
            vertex = vertices[Indexes[i + 1]] + UnityOffset;
            coordinate = CoordConvert.UnitytoRD(vertex);
            netDxf.Vector3 vertex2 = new netDxf.Vector3(coordinate.x - RDOrigin.x, coordinate.y - RDOrigin.y, coordinate.z - UnityOffset.y);
            vertex = vertices[Indexes[i + 2]] + UnityOffset;
            coordinate = CoordConvert.UnitytoRD(vertex);
            netDxf.Vector3 vertex3 = new netDxf.Vector3(coordinate.x - RDOrigin.x, coordinate.y - RDOrigin.y, coordinate.z - UnityOffset.y);
            face = new Face3d(vertex1, vertex2, vertex3);
            blockparts.Add(face);
        }
        Block blok = new Block("terrain", blockparts);
        blok.Layer = terreinLaag;
        doc.Blocks.Add(blok);
        Insert blokInsert = new Insert(blok, new netDxf.Vector3(RDOrigin.x-500, RDOrigin.y-500, 0), 1);
        blokInsert.Layer = terreinLaag;
        doc.AddEntity(blokInsert);
        terrainDone = true;
    }
    // Update is called once per frame
    void Update()
    {
        
    }

    IEnumerator LoadBuildings(int X, int Y, System.Action<bool> callback)
    {

        string dataURL = "E:/UnityData/Assetbundles/WebGL/BuildingData/LOD1/building_{x}_{y}_lod1";
        if (buildingsLOD == 2)
        {
            dataURL = "E:/UnityData/Assetbundles/WebGL/BuildingData/LOD2/building_{x}_{y}_lod2";
        }
        dataURL = dataURL.Replace("{x}", X.ToString());
        dataURL = dataURL.Replace("{y}", Y.ToString());
        using (UnityWebRequest uwr = UnityWebRequestAssetBundle.GetAssetBundle(dataURL))
        {
            yield return uwr.SendWebRequest();

            if (uwr.isNetworkError || uwr.isHttpError)
            {
                buildingsDone = true;
                callback(false);
            }
            else
            {
                AssetBundle newAssetBundle = DownloadHandlerAssetBundle.GetContent(uwr);
                UnityEngine.Mesh[] meshes = newAssetBundle.LoadAllAssets<UnityEngine.Mesh>();
                if (meshes == null)
                {
                    buildingsDone = true;
                    callback(false);
                }
                for (int i = 0; i < meshes.Length; i++)
                {
                    buildingMeshes.Add(meshes[i]);
                    yield return null;
                }
                newAssetBundle.Unload(false);
                callback(true);
            }
        }
    }

    IEnumerator LoadBuildingData(int X, int Y, System.Action<bool> callback)
    {

        string dataURL = "E:/UnityData/AssetBundles/WebGL/BuildingData/objectdata/{x}_{y}_buildings_lod1-data";
        if (buildingsLOD == 2)
        {
            dataURL = "E:/UnityData/AssetBundles/WebGL/BuildingData/objectdata/{x}_{y}_buildings_lod2-data";
        }
        dataURL = dataURL.Replace("{x}", X.ToString());
        dataURL = dataURL.Replace("{y}", Y.ToString());
        using (UnityWebRequest uwr = UnityWebRequestAssetBundle.GetAssetBundle(dataURL))
        {
            yield return uwr.SendWebRequest();

            if (uwr.isNetworkError || uwr.isHttpError)
            {
                buildingsDone = true;
                callback(false);
            }
            else
            {
                AssetBundle newAssetBundle = DownloadHandlerAssetBundle.GetContent(uwr);
                ObjectMappingClass[] meshes = newAssetBundle.LoadAllAssets<ObjectMappingClass>();
                if (meshes == null)
                {
                    buildingsDone = true;
                    callback(true);
                }
                for (int i = 0; i < meshes.Length; i++)
                {
                    objectlists.Add(meshes[i]);
                    yield return null;
                }

                callback(true);
            }
        }
    }

    IEnumerator LoadTerrain(int X, int Y, System.Action<bool> callback)
    {

        string dataURL = "E:/UnityData/Assetbundles/WebGL/Terrain/LOD0/{x}_{y}_terrain_lod0";
        dataURL = dataURL.Replace("{x}", X.ToString());
        dataURL = dataURL.Replace("{y}", Y.ToString());
        using (UnityWebRequest uwr = UnityWebRequestAssetBundle.GetAssetBundle(dataURL))
        {
            yield return uwr.SendWebRequest();

            if (uwr.isNetworkError || uwr.isHttpError)
            {
                terrainDone = true;
                callback(false);
            }
            else
            {
                AssetBundle newAssetBundle = DownloadHandlerAssetBundle.GetContent(uwr);
                UnityEngine.Mesh[] meshes = newAssetBundle.LoadAllAssets<UnityEngine.Mesh>();
                if (meshes == null)
                {
                    terrainDone = true;
                    callback(false);
                }
                terrainMesh = meshes[0];
                newAssetBundle.Unload(false);
                callback(true);
            }
        }
    }

}
