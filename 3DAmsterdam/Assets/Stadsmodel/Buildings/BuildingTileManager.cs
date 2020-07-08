using BruTile;
using ConvertCoordinates;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Networking;


class TileData
{
    public Vector3 tileID;
    public List<GameObject> gameObjects = new List<GameObject>();
    public AssetBundle assetBundle;

    public TileStatus Status;
    
    public TileData(Vector3 TileID)
    {
        tileID = TileID;
    }
}

public class BuildingTileManager : MonoBehaviour
{
    

    public string assetURL = "https://3d.amsterdam.nl/web/AssetBundles/Gebouwen/";
    public Material defaultMaterial;
    public Material highLightMaterial;
    public float maximumDistanceLOD2 = 500;
    public float maximumDistanceLOD1 = 1000;
    public float maximumDistanceLOD0 = 3000;
    private CameraView cameraViewExtent;
    private Extent previousCameraViewExtent = new Extent(0, 0, 0, 0);
    public int maximumConcurrentDownloads = 5;

    private bool tileUpdateCompleted = true;

    private Dictionary<Vector3, TileData> activeTileList = new Dictionary<Vector3, TileData>();
    private List<Vector3> PendingDownloads = new List<Vector3>();
    private List<Vector3> ActiveDownloads = new List<Vector3>();
    private List<Vector3> PendingBuilds = new List<Vector3>();
    private List<Vector3> ActiveBuilds = new List<Vector3>();
    private List<Vector3> PendingDestroy = new List<Vector3>();

    // Start is called before the first frame update
    void Start()
    {
        cameraViewExtent = Camera.main.GetComponent<CameraView>();

    }

    // Update is called once per frame
    void Update()
    {
        if (tileUpdateCompleted==false)
        {
            return;
        }
        if (previousCameraViewExtent.CenterX != cameraViewExtent.CameraExtent.CenterX || previousCameraViewExtent.CenterY != cameraViewExtent.CameraExtent.CenterY)
        {
            UpdateActiveTileList(cameraViewExtent.CameraExtent);
            previousCameraViewExtent = cameraViewExtent.CameraExtent;
        }


        if (PendingDestroy.Count>0 || PendingBuilds.Count>0 || PendingDownloads.Count>0 )
            {
                tileUpdateCompleted = false;
                StartCoroutine(ProcessActiveTiles());
            }

    }

    

    private void UpdateActiveTileList(Extent wGSExtent)
    {
 
        Extent rDExtent = ConvertWGSExtentToRDExtent(wGSExtent);

        Vector3RD Camlocation3RD = CoordConvert.UnitytoRD(Camera.main.transform.localPosition);
        Vector3 CamlocationRD = new Vector3((float)Camlocation3RD.x, (float)Camlocation3RD.y, (float)Camlocation3RD.z);

        
        Dictionary<Vector3, int> BuildingTilesNeeded = new Dictionary<Vector3, int>();
        for (double deltax = rDExtent.MinX; deltax <= rDExtent.MaxX; deltax += 1000)
        {
            for (double deltay = rDExtent.MinY; deltay <= rDExtent.MaxY; deltay += 1000)
            {
                Vector3 tileCenterLocationRD = new Vector3((float)deltax + 500, (float)deltay + 500, 0);
                double distanceFromCamera = (CamlocationRD - tileCenterLocationRD).magnitude;

                float requiredLOD = GetRequiredLOD(distanceFromCamera);

                if (requiredLOD!=-1)
                {
                    AddTile(new Vector3((float)deltax, (float)deltay, requiredLOD));
                    BuildingTilesNeeded.Add(new Vector3((float)deltax, (float)deltay, requiredLOD), 1);
                }

            }
        }

        // tegels verwijderen die niet meer nodig zijn
        foreach (KeyValuePair<Vector3, TileData> KeyPair in activeTileList)
        {
            if (!BuildingTilesNeeded.ContainsKey(KeyPair.Key))
            {
                RemoveTile(KeyPair.Key);
            }
        }
        
    }


    public void AddTile(Vector3 TileID)
    {
        // Add the tile to the activeTileList if it is not already on the list
        if (!activeTileList.ContainsKey(TileID)) 
        {
            activeTileList.Add(TileID, new TileData(TileID));
            PendingDownloads.Add(TileID);
        }
        else
        {
            // set tilestatus to Built and remove for PendingDestroy-list if tile was setup for destroy
            if(PendingDestroy.Contains(TileID))
            {
                PendingDestroy.RemoveAll(elem => elem == TileID); 
            }
        }
    }

    public void RemoveTile(Vector3 TileID)
    {
        // Add tile tile to pendingDestroy-List if the tile is not actively worked on.
        if (!PendingDestroy.Contains(TileID)&& !ActiveDownloads.Contains(TileID) && !ActiveBuilds.Contains(TileID))
        {
            PendingDestroy.Add(TileID);
            PendingDownloads.RemoveAll(elem => elem == TileID);
            PendingBuilds.RemoveAll(elem => elem == TileID);

        }
        
    }

    private IEnumerator ProcessActiveTiles()
    {
        // Remove BuildingTiles
        for (int j = PendingDestroy.Count-1; j >-1; j--)
        {
            Vector3 TileID = PendingDestroy[j];
            if (activeTileList[TileID].assetBundle != null)
            {
                
                for (int i = activeTileList[TileID].gameObjects.Count-1; i >-1; i--)
                {
                    Destroy(activeTileList[TileID].gameObjects[i].GetComponent<MeshFilter>().mesh);
                    Destroy(activeTileList[TileID].gameObjects[i]);
                }
                activeTileList[TileID].assetBundle.Unload(true);
            }
            activeTileList.Remove(TileID);
            PendingDestroy.RemoveAt(j);
        }
        // Download BuildingTiles
        if (ActiveDownloads.Count < maximumConcurrentDownloads && PendingDownloads.Count>0)
        {
            Vector3 TileID = PendingDownloads[0];
            PendingDownloads.RemoveAt(0);
            ActiveDownloads.Add(TileID);
            StartCoroutine(DownloadAssetBundleWebGL(TileID));
        }
        //build buildingtiles
        if (PendingBuilds.Count>0)
        {
            Vector3 TileID = PendingBuilds[0];
            if (activeTileList.ContainsKey(TileID))
            {
                PendingBuilds.RemoveAt(0);
                ActiveBuilds.Add(TileID);
                StartCoroutine(BuildTile(TileID));
            }
            else{
                PendingBuilds.RemoveAt(0);
            }
            
        }
        yield return new WaitForSeconds(0.1f);
        tileUpdateCompleted = true;
    }

    private IEnumerator DownloadAssetBundleWebGL(Vector3 TileID)
    {
        /////downloaden van de Assetbundle
        ///
        TileData tileData = activeTileList[TileID];
        //BuildingURL = "file:///D://Github/WebGL/";
        string url = assetURL + "gebouwen_" + ((int)tileData.tileID.x).ToString() + "_" + ((int)tileData.tileID.y).ToString() + "." + ((int)tileData.tileID.z).ToString();
        
        using (UnityWebRequest uwr = UnityWebRequestAssetBundle.GetAssetBundle(url))
        {
            yield return uwr.SendWebRequest();
            
            if (uwr.isNetworkError || uwr.isHttpError)
            {
                ActiveDownloads.Remove(tileData.tileID);  
            }
            else
            {
                // Get downloaded asset bundle
                AssetBundle bundle = DownloadHandlerAssetBundle.GetContent(uwr);
                tileData.assetBundle = bundle;
                ActiveDownloads.Remove(tileData.tileID);
                PendingBuilds.Add(tileData.tileID);
            }
        }
        
        yield return null;
    }

    private IEnumerator BuildTile(Vector3 TileID)
    {
        TileData tileData = activeTileList[TileID];
        //meshes uit assetbundle inlezen
        Mesh[] meshesInAssetbundle = new Mesh[0];
        try
        {
            meshesInAssetbundle = tileData.assetBundle.LoadAllAssets<Mesh>();
        }
        catch (Exception)
        {
            ActiveBuilds.Remove(tileData.tileID);
            
            yield break;

        }

        string[] objectIDs = ReadObjectIDs(tileData.assetBundle);
        

        foreach (Mesh mesh in meshesInAssetbundle)
        {

            int volgnummer = 0;
            if (mesh.name.Split('_').Length > 2)
            {
                volgnummer = int.Parse(mesh.name.Split('_')[2]);
            }

            GameObject container = CreateGameObjectWithMesh(mesh);

            AddObjectIDs(mesh, volgnummer, objectIDs, container);
            
            tileData.gameObjects.Add(container);

            yield return null;
        }
        ActiveBuilds.Remove(tileData.tileID);
        tileData.Status = TileStatus.Built;


    }

    private GameObject CreateGameObjectWithMesh(Mesh mesh)
    {
        string gameObjectName = mesh.name;
        float X = float.Parse(gameObjectName.Split('_')[0]);
        float Y = float.Parse(gameObjectName.Split('_')[1]);
        
        GameObject container = new GameObject(gameObjectName);
        container.transform.parent = transform;
        container.layer = LayerMask.NameToLayer("Panden");

        //positioning container
        Vector3RD hoekpunt = new Vector3RD(X, Y, 0);
        double OriginOffset = 500;
        Vector3RD origin = new Vector3RD(hoekpunt.x + (OriginOffset), hoekpunt.y + (OriginOffset), 0);
        Vector3 UnityOrigin = CoordConvert.RDtoUnity(origin);
        container.transform.localPosition = UnityOrigin;
        double Rotatie = CoordConvert.RDRotation(origin);
        container.transform.Rotate(Vector3.up, (float)Rotatie);

        //add mesh
        MeshFilter mf = container.AddComponent<MeshFilter>();
        mf.sharedMesh = mesh;
        //add material
        MeshRenderer mr = container.AddComponent<MeshRenderer>();
        mr.material = defaultMaterial;

        return container;
    }

    private void AddObjectIDs (Mesh mesh, int meshIndex, string[] objectIDs, GameObject gameObject)
    {
        if (objectIDs.Length==0)
        {
            return;
        }
        ObjectMapping objectMap = gameObject.AddComponent<ObjectMapping>();
        Dictionary<float, string> objectIDList = new Dictionary<float, string>();

        string[] objects = Regex.Split(objectIDs[meshIndex * 2], ",");
        for (int i = 0; i < objects.Length - 1; i++)
        {
            objectIDList.Add((float)i, objects[i]);
        }
        objectMap.Objectenlijst = objectIDList;
        objectMap.DefaultMaterial = defaultMaterial;
        objectMap.HighlightMaterial = highLightMaterial;
        objectMap.SetMesh(mesh);

    }

    private string[] ReadObjectIDs(AssetBundle assetBundle)
    {
        string[] objectIDs = new string[0];
        if (assetBundle.LoadAllAssets<TextAsset>().Length > 0)
        {
            
            TextAsset textAsset = assetBundle.LoadAllAssets<TextAsset>()[0];
            string objectIDList = textAsset.text;
            objectIDs = Regex.Split(objectIDList, "\n|\r|\r\n");
        }
        return objectIDs;
    }

    private Extent ConvertWGSExtentToRDExtent(Extent wGSExtent)
    {
        Vector3RD RDmin = CoordConvert.WGS84toRD(wGSExtent.MinX, wGSExtent.MinY);
        Vector3RD RDmax = CoordConvert.WGS84toRD(wGSExtent.MaxX, wGSExtent.MaxY);

        int X0 = ((int)Math.Floor(RDmin.x / 1000) * 1000);
        int Y0 = ((int)Math.Floor(RDmin.y / 1000) * 1000);
        int X1 = ((int)Math.Floor(RDmax.x / 1000) * 1000);
        int Y1 = ((int)Math.Floor(RDmax.y / 1000) * 1000);

        Extent rDExtent = new Extent(X0, Y0, X1, Y1);
        return rDExtent;
    }

    private float GetRequiredLOD(double distanceFromCamera)
    {
        float requiredLOD = -1;

        if (distanceFromCamera < maximumDistanceLOD0)
        {
            requiredLOD = 0;
        }
        if (distanceFromCamera < maximumDistanceLOD1)
        {
            requiredLOD = 1;
        }
        if (distanceFromCamera < maximumDistanceLOD2)
        {
            requiredLOD = 2;
        }

        return requiredLOD;
    }
}
