using BruTile;
using ConvertCoordinates;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Networking;




public class TerrainTileManager : MonoBehaviour
{
    [SerializeField] private string dataFolder = "terrain";
    [SerializeField] private List<Material> defaultMaterialList;
    [SerializeField] private Material highLightMaterial;
   
    [SerializeField] private float maximumDistanceLOD = 5000;
    private CameraView cameraViewExtent;
    private Extent previousCameraViewExtent = new Extent(0, 0, 0, 0);
    public int maximumConcurrentDownloads = 5;

    private bool tileUpdateCompleted = true;

    private Dictionary<Vector3, TileData> activeTileList = new Dictionary<Vector3, TileData>();
    private List<Vector3> pendingDownloads = new List<Vector3>();
    private List<Vector3> activeDownloads = new List<Vector3>();
    private List<Vector3> pendingBuilds = new List<Vector3>();
    private List<Vector3> activeBuilds = new List<Vector3>();
    private List<Vector3> pendingDestroy = new List<Vector3>();

    [SerializeField] private string layerName = "Terrain";

    // Start is called before the first frame update
    void Start()
    {
        cameraViewExtent = Camera.main.GetComponent<CameraView>();

    }

    // Update is called once per frame
    void Update()
    {
        if (tileUpdateCompleted == false)
        {
            return;
        }
        if (previousCameraViewExtent.CenterX != cameraViewExtent.cameraExtent.CenterX || previousCameraViewExtent.CenterY != cameraViewExtent.cameraExtent.CenterY)
        {
            UpdateActiveTileList(cameraViewExtent.cameraExtent);
            previousCameraViewExtent = cameraViewExtent.cameraExtent;
        }


        if (pendingDestroy.Count > 0 || pendingBuilds.Count > 0 || pendingDownloads.Count > 0)
        {
            tileUpdateCompleted = false;
            StartCoroutine(ProcessActiveTiles());
        }

    }


    private void UpdateActiveTileList(Extent wGSExtent)
    {
        int tileSize = 500;
        Extent rDExtent = ConvertWGSExtentToRDExtent(wGSExtent);
        Vector3RD camlocation3RD = CoordConvert.UnitytoRD(Camera.main.transform.localPosition);
        Vector3 camlocationRD = new Vector3((float)camlocation3RD.x, (float)camlocation3RD.y, (float)camlocation3RD.z);

        Dictionary<Vector3, int> BuildingTilesNeeded = new Dictionary<Vector3, int>();
        for (double deltaX = rDExtent.MinX; deltaX <= rDExtent.MaxX; deltaX += tileSize)
        {
            for (double deltaY = rDExtent.MinY; deltaY <= rDExtent.MaxY; deltaY += tileSize)
            {
                Vector3 tileCenterLocationRD = new Vector3((float)deltaX + (tileSize/2), (float)deltaY +(tileSize/2), 0);
                double distanceFromCamera = (camlocationRD - tileCenterLocationRD).magnitude;

                float requiredLOD = GetRequiredLOD(distanceFromCamera);

                if (requiredLOD != -1)
                {
                    AddTile(new Vector3((float)deltaX, (float)deltaY, requiredLOD));
                    BuildingTilesNeeded.Add(new Vector3((float)deltaX, (float)deltaY, requiredLOD), 1);
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
            pendingDownloads.Add(TileID);
        }
        else
        {
            // set tilestatus to Built and remove for PendingDestroy-list if tile was setup for destroy
            if (pendingDestroy.Contains(TileID))
            {
                pendingDestroy.RemoveAll(elem => elem == TileID);
            }
        }
    }

    public void RemoveTile(Vector3 TileID)
    {
        // Add tile tile to pendingDestroy-List if the tile is not actively worked on.
        if (!pendingDestroy.Contains(TileID) && !activeDownloads.Contains(TileID) && !activeBuilds.Contains(TileID))
        {
            pendingDestroy.Add(TileID);
            pendingDownloads.RemoveAll(elem => elem == TileID);
            pendingBuilds.RemoveAll(elem => elem == TileID);

        }

    }

    private IEnumerator ProcessActiveTiles()
    {
        // Remove BuildingTiles
        for (int j = pendingDestroy.Count - 1; j > -1; j--)
        {
            Vector3 TileID = pendingDestroy[j];
            if (activeTileList[TileID].assetBundle != null)
            {

                for (int i = activeTileList[TileID].gameObjects.Count - 1; i > -1; i--)
                {
                    Destroy(activeTileList[TileID].gameObjects[i].GetComponent<MeshFilter>().mesh);
                    Destroy(activeTileList[TileID].gameObjects[i]);
                }
                activeTileList[TileID].assetBundle.Unload(true);
            }
            activeTileList.Remove(TileID);
            pendingDestroy.RemoveAt(j);
        }
        // Download BuildingTiles
        if (activeDownloads.Count < maximumConcurrentDownloads && pendingDownloads.Count > 0)
        {
            Vector3 TileID = pendingDownloads[0];
            pendingDownloads.RemoveAt(0);
            activeDownloads.Add(TileID);
            StartCoroutine(DownloadAssetBundleWebGL(TileID));
        }
        //build buildingtiles
        if (pendingBuilds.Count > 0)
        {
            Vector3 TileID = pendingBuilds[0];
            if (activeTileList.ContainsKey(TileID))
            {
                pendingBuilds.RemoveAt(0);
                activeBuilds.Add(TileID);
                StartCoroutine(BuildTile(TileID));
            }
            else
            {
                pendingBuilds.RemoveAt(0);
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
        //string BuildingURL = "file:///E://UnityData/Assetbundles/WebGL/terrain";
        //string BuildingURL = "https://acc.3d.amsterdam.nl/webmap/data/feature/3DBasisvoorziening/WebGL/Terrain";
        string BuildingURL = "file:///E://UnityData/Assetbundles/WebGL/terrain500x500";
        string url = BuildingURL + "/terrain_" + ((int)tileData.tileID.x).ToString() + "_" + ((int)tileData.tileID.y).ToString() + "_lod1";
        Debug.Log(url);
        using (UnityWebRequest uwr = UnityWebRequestAssetBundle.GetAssetBundle(url))
        {
            yield return uwr.SendWebRequest();

            if (uwr.isNetworkError || uwr.isHttpError)
            {
                Debug.Log(uwr.error);
                activeDownloads.Remove(tileData.tileID);
            }
            else
            {
                Debug.Log(url + " gedownload");
                // Get downloaded asset bundle
                AssetBundle bundle = DownloadHandlerAssetBundle.GetContent(uwr);
                tileData.assetBundle = bundle;
                activeDownloads.Remove(tileData.tileID);
                pendingBuilds.Add(tileData.tileID);
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
            activeBuilds.Remove(tileData.tileID);

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

            GameObject container = CreateGameObjectWithMesh(mesh, TileID.x,TileID.y);

            //AddObjectIDs(mesh, volgnummer, objectIDs, container);

            tileData.gameObjects.Add(container);

            yield return null;
        }
        activeBuilds.Remove(tileData.tileID);
        tileData.Status = TileStatus.Built;


    }

    private GameObject CreateGameObjectWithMesh(Mesh mesh, float X, float Y)
    {
        string gameObjectName = mesh.name;
        

        GameObject container = new GameObject(gameObjectName);
        container.transform.parent = transform;
        container.layer = LayerMask.NameToLayer(layerName);

        //positioning container
        Vector3RD hoekpunt = new Vector3RD(X, Y, 0);
        double OriginOffset = 500;
        Vector3RD origin = new Vector3RD(hoekpunt.x + (OriginOffset), hoekpunt.y + (OriginOffset), 0);
        Vector3 unityOrigin = CoordConvert.RDtoUnity(origin);
        container.transform.localPosition = unityOrigin;
        double Rotatie = CoordConvert.RDRotation(origin);
        container.transform.Rotate(Vector3.up, (float)Rotatie);

        //add mesh
        container.AddComponent<MeshFilter>().sharedMesh = mesh;

        //add material
        container.AddComponent<MeshRenderer>().sharedMaterials = defaultMaterialList.ToArray();


        return container;
    }

    //private void AddObjectIDs(Mesh mesh, int meshIndex, string[] objectIDs, GameObject gameObject)
    //{
    //    if (objectIDs.Length == 0)
    //    {
    //        return;
    //    }
    //    ObjectMapping objectMap = gameObject.AddComponent<ObjectMapping>();
    //    Dictionary<float, string> objectIDList = new Dictionary<float, string>();

    //    string[] objects = Regex.Split(objectIDs[meshIndex * 2], ",");
    //    for (int i = 0; i < objects.Length - 1; i++)
    //    {
    //        objectIDList.Add((float)i, objects[i]);
    //    }
    //    objectMap.Objectenlijst = objectIDList;
    //    objectMap.DefaultMaterial = defaultMaterial;
    //    objectMap.HighlightMaterial = highLightMaterial;
    //    objectMap.SetMesh(mesh);

    //}

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

        if (distanceFromCamera < maximumDistanceLOD)
        {
            requiredLOD = 1;
        }
       return requiredLOD;
    }
}
