using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using ConvertCoordinates;
using BruTile;
using System;
using System.Text.RegularExpressions;
using Amsterdam3D.CameraMotion;

/// <summary>
/// enum to indentify the status of a BuildingTile
/// </summary>
public enum TileStatus
{
    WaitingForDownload,
    Downloading,
    PendingBuild,
    Building,
    Built,
    PendingDestroy
}
//class BuildingTileData
//{
//    public Vector3 id;
//    public List<GameObject> Gameobjecten = new List<GameObject>();
//    public AssetBundle AB;
//    public bool TeVerwijderen = false;
//    public BuildingTileStatus Status;

//    public BuildingTileData(Vector3 TileID)
//    {
//        id = TileID;
//        Status = BuildingTileStatus.WaitingForDownload;
//    }
//}

public class BoomTileManager : MonoBehaviour
    {
        public Material kruinmateriaal;
        public Material stammateriaal;
        public float Max_Afstand = 2000f;

        [SerializeField] private string dataFolder = "trees";

        private GodViewCameraExtents CV;
        private Extent vorigeCV = new Extent(0, 0, 0, 0);
        public int MAX_Concurrent_Downloads = 5;

        private Dictionary<string, float> gebouwenlijst = new Dictionary<string, float>();
        private Dictionary<Vector3, TileData> buildingTiles = new Dictionary<Vector3, TileData>();
        private List<Vector3> PendingDownloads = new List<Vector3>();
        private List<Vector3> ActiveDownloads = new List<Vector3>();
        private List<Vector3> PendingBuilds = new List<Vector3>();
        private List<Vector3> ActiveBuilds = new List<Vector3>();
        private List<Vector3> PendingDestroy = new List<Vector3>();
        // Start is called before the first frame update
        void Start()
        {
            CV = CameraModeChanger.Instance.ActiveCamera.GetComponent<GodViewCameraExtents>();
            //Add(new Vector3(105000, 471000, 0));
            //Add(new Vector3(105000, 473000, 0));
        }

        // Update is called once per frame
        void Update()
        {
            if (vorigeCV.CenterX != CV.cameraExtent.CenterX || vorigeCV.CenterY != CV.cameraExtent.CenterY)
            {
                UpdateGebouwen(CV.cameraExtent);
                vorigeCV = CV.cameraExtent;
            }
            //}
            //if (BijwerkenGereed)
            //{
            if (PendingDestroy.Count > 0 || PendingBuilds.Count > 0 || PendingDownloads.Count > 0)
            {
                StartCoroutine(TilesBijwerken());
            }



        }

        private void UpdateGebouwen(Extent WGSExtent)
        {
            // WGS-extent omzetten naar RD-extent
            int tegelgrootte = 500;
            Vector3RD RDmin = CoordConvert.WGS84toRD(WGSExtent.MinX, WGSExtent.MinY);
            Vector3RD RDmax = CoordConvert.WGS84toRD(WGSExtent.MaxX, WGSExtent.MaxY);
            Extent RDExtent = new Extent(RDmin.x, RDmin.y, RDmax.x, RDmax.y);
            int X0 = ((int)Math.Floor(RDExtent.MinX / tegelgrootte) * tegelgrootte);
            int Y0 = ((int)Math.Floor(RDExtent.MinY / tegelgrootte) * tegelgrootte);
            int X1 = ((int)Math.Floor(RDExtent.MaxX / tegelgrootte) * tegelgrootte);
            int Y1 = ((int)Math.Floor(RDExtent.MaxY / tegelgrootte) * tegelgrootte);

            // cameralocatie omzetten naar RD-locatie
            Vector3RD Camlocation3RD = CoordConvert.UnitytoRD(CameraModeChanger.Instance.ActiveCamera.transform.localPosition);
            Vector3 CamlocationRD = new Vector3((float)Camlocation3RD.x, (float)Camlocation3RD.y, (float)Camlocation3RD.z);


            //Benodigde tegels uitzoeken
            Dictionary<Vector3, int> BuildingTilesNeeded = new Dictionary<Vector3, int>();
            for (int deltax = X0; deltax <= X1; deltax += tegelgrootte)
            {
                for (int deltay = Y0; deltay <= Y1; deltay += tegelgrootte)
                {
                    Vector3 Hart = new Vector3(deltax + (tegelgrootte/2), deltay+(tegelgrootte / 2), 0);
                    ;
                    Vector3 Verschil = CamlocationRD - Hart;
                    double afstand = Verschil.magnitude;
                    if (Max_Afstand > afstand)
                    {
                        Add(new Vector3(deltax, deltay, 0));
                        BuildingTilesNeeded.Add(new Vector3(deltax, deltay, 0), 1);
                    }
                    

                }
            }

            // tegels verwijderen die niet meer nodig zijn
            foreach (KeyValuePair<Vector3, TileData> KeyPair in buildingTiles)
            {
                if (!BuildingTilesNeeded.ContainsKey(KeyPair.Key))
                {
                    Remove(KeyPair.Key);
                }
            }

        }

        /// <summary>
        /// Add BuildingTile to Buildingtiles if not already present
        /// </summary>
        /// <param name="TileID"></param>
        public void Add(Vector3 TileID)
        {
            if (!buildingTiles.ContainsKey(TileID))
            {
                buildingTiles.Add(TileID, new TileData(TileID));
                PendingDownloads.Add(TileID);
            }
            else
            {
                if (buildingTiles[TileID].Status == TileStatus.PendingDestroy)
                {
                    buildingTiles[TileID].Status = TileStatus.Built; //status terugzetten naar Built
                    PendingDestroy.RemoveAll(elem => elem == TileID); //verwijderen uit RemoveBuilds
                }
            }
        }

        /// <summary>
        /// Mark a BuildingTIle "To Be Removed"
        /// </summary>
        /// <param name="TileID"></param>
        public void Remove(Vector3 TileID)
        {
            if (!PendingDestroy.Contains(TileID) && !ActiveDownloads.Contains(TileID) && !ActiveBuilds.Contains(TileID) && buildingTiles.ContainsKey(TileID))
            {
                PendingDestroy.Add(TileID);
                buildingTiles[TileID].Status = TileStatus.PendingDestroy;
                PendingDownloads.RemoveAll(elem => elem == TileID);
                PendingBuilds.RemoveAll(elem => elem == TileID);

            }

        }

        private IEnumerator TilesBijwerken()
        {
            // verwijderen wanneer mogelijk
            for (int j = PendingDestroy.Count - 1; j > -1; j--)

            {
                Vector3 TileID = PendingDestroy[j];
                if (buildingTiles[TileID].assetBundle != null)
                {
                    buildingTiles[TileID].assetBundle.Unload(true);
                    for (int i = buildingTiles[TileID].gameObjects.Count - 1; i > -1; i--)
                    {
                        
                        Destroy(buildingTiles[TileID].gameObjects[i]);
                    }
                }
                buildingTiles.Remove(TileID);
                PendingDestroy.RemoveAt(j);
            }
            //downloaden wanneer mogelijk
            if (ActiveDownloads.Count < MAX_Concurrent_Downloads && PendingDownloads.Count > 0)
            {
                Vector3 TileID = PendingDownloads[0];
                PendingDownloads.RemoveAt(0);
                ActiveDownloads.Add(TileID);
                buildingTiles[TileID].Status = TileStatus.Downloading;
                StartCoroutine(DownloadAssetBundleWebGL(TileID));
            }
            //builden wanneer mogelijk
            if (PendingBuilds.Count > 0)
            {
            
                Vector3 TileID = PendingBuilds[0];
                if (buildingTiles.ContainsKey(TileID))
                {
                    PendingBuilds.RemoveAt(0);
                    ActiveBuilds.Add(TileID);
                    StartCoroutine(ProcessBuildingTile(TileID));
                }
                else
                {
                    PendingBuilds.RemoveAt(0);
                }

            }
            yield return new WaitForSeconds(0.1f);
        }

        /// <summary>
        /// Download AssetBundle and store it in the BuildingTileData
        /// </summary>
        /// <param name="TileID"></param>
        /// <returns></returns>
        private IEnumerator DownloadAssetBundleWebGL(Vector3 TileID)
        {
            /////downloaden van de Assetbundle
            ///
            TileData btd = buildingTiles[TileID];
//#if UNITY_EDITOR        // inde editor staand de assetbundles in de map Assetbundles naast de map 3DAmsterdam
//            BuildingURL = "file:///D:/Github/WebGL/Bomen/";
//#endif
            string url = Constants.BASE_DATA_URL + dataFolder + "/trees_" + ((int)btd.tileID.x).ToString() + "-" + ((int)btd.tileID.y).ToString();
        
            using (UnityWebRequest uwr = UnityWebRequestAssetBundle.GetAssetBundle(url))
            {
                yield return uwr.SendWebRequest();

                if (uwr.isNetworkError || uwr.isHttpError)
                {
                    ActiveDownloads.Remove(btd.tileID);
                    btd.Status = TileStatus.Built;
                    
                }
                else
                {
                    // Get downloaded asset bundle
                    AssetBundle bundle = DownloadHandlerAssetBundle.GetContent(uwr);
                    btd.assetBundle = bundle;
                    ActiveDownloads.Remove(btd.tileID);
                    btd.Status = TileStatus.PendingBuild;
                    PendingBuilds.Add(btd.tileID);
                }
            }

            yield return null;
        }

        private IEnumerator ProcessBuildingTile(Vector3 TileID)
        {
            TileData btd = buildingTiles[TileID];

            //meshes uit assetbundle inlezen
            GameObject[] ABAssets = new GameObject[0];
            try
            {
                btd.assetBundle.LoadAllAssets<Mesh>();
            
                ABAssets = btd.assetBundle.LoadAllAssets<GameObject>();
            }
            catch (Exception)
            {
                btd.Status = TileStatus.Built;
            ActiveBuilds.Remove(btd.tileID);
            //////instantiaten en assets loaden
            btd.Status = TileStatus.Built;
            yield break;

            }
            foreach (GameObject ass in ABAssets)
            {
                GameObject boomasset = Instantiate(ass, transform);
                //add to Buildingtiledata
                btd.gameObjects.Add(boomasset);
            yield return null;
            if (ass.name.Contains("bomenkruin"))
            {
                boomasset.GetComponent<MeshRenderer>().sharedMaterial = kruinmateriaal;
            }
            else
            {
                boomasset.GetComponent<MeshRenderer>().sharedMaterial = stammateriaal;
            }
                


                yield return null;
            }
        ActiveBuilds.Remove(btd.tileID);
        //////instantiaten en assets loaden
        btd.Status = TileStatus.Built;


        }
    }

