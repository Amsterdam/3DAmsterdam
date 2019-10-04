using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using ConvertCoordinates;
using BruTile;
using System;
using System.Text.RegularExpressions;

/// <summary>
/// enum to indentify the status of a BuildingTile
/// </summary>
public enum BuildingTileStatus
{
    WaitingForDownload,     //ready to be downloaded
    Downloading,            //download is active
    PendingBuild,             //waiting for display
    Building,             //getting displayed
    Built,              //Assetbundlecontent is displayed
    PendingDestroy
}
class BuildingTileData
{
    public Vector3 id;
    public List<GameObject> Gameobjecten = new List<GameObject>();
    public AssetBundle AB;
    public bool TeVerwijderen = false;
    public BuildingTileStatus Status;
    
    public BuildingTileData(Vector3 TileID)
    {
        id = TileID;
        Status = BuildingTileStatus.WaitingForDownload;
    }
}



public class BuildingTileManager : MonoBehaviour
{
    

    public string BuildingURL = "file:///D://Git/A3d/AssetBundles/WebGL/";
    public Material GebouwMateriaal;
    public Material HighLightMateriaal;
    public float Max_Afstand_Daken = 500;
    public float Max_Afstand_BAG = 1000;
    public float Max_Afstand_Top10 = 3000;
    private CameraView CV;
    private Extent vorigeCV = new Extent(0, 0, 0, 0);
    public int MAX_Concurrent_Downloads = 5;
    private Boolean BijwerkenGereed = true;

    private Dictionary<string, float> gebouwenlijst = new Dictionary<string, float>();
    private Dictionary<Vector3, BuildingTileData> buildingTiles = new Dictionary<Vector3, BuildingTileData>();
    private List<Vector3> PendingDownloads = new List<Vector3>();
    private List<Vector3> ActiveDownloads = new List<Vector3>();
    private List<Vector3> PendingBuilds = new List<Vector3>();
    private List<Vector3> ActiveBuilds = new List<Vector3>();
    private List<Vector3> PendingDestroy = new List<Vector3>();
    // Start is called before the first frame update
    void Start()
    {
        CV = Camera.main.GetComponent<CameraView>();
        Add(new Vector3(105000, 471000, 0));
        Add(new Vector3(105000, 473000, 0));
    }

    // Update is called once per frame
    void Update()
    {
        if (vorigeCV.CenterX != CV.CameraExtent.CenterX || vorigeCV.CenterY != CV.CameraExtent.CenterY)
        {
            UpdateGebouwen(CV.CameraExtent);
        }
        if (BijwerkenGereed)
        {
            if (PendingDestroy.Count>0 || PendingBuilds.Count>0 || PendingDownloads.Count>0 )
            {
                BijwerkenGereed = false;
                StartCoroutine(TilesBijwerken());
            }
            
        }
        
    }

    private void UpdateGebouwen(Extent WGSExtent)
    {
        // WGS-extent omzetten naar RD-extent
        Vector3RD RDmin = CoordConvert.WGS84toRD(WGSExtent.MinX, WGSExtent.MinY);
        Vector3RD RDmax = CoordConvert.WGS84toRD(WGSExtent.MaxX, WGSExtent.MaxY);
        Extent RDExtent = new Extent(RDmin.x, RDmin.y, RDmax.x, RDmax.y);
        int X0 = ((int)Math.Floor(RDExtent.MinX / 1000) * 1000);
        int Y0 = ((int)Math.Floor(RDExtent.MinY / 1000) * 1000);
        int X1 = ((int)Math.Floor(RDExtent.MaxX / 1000) * 1000);
        int Y1 = ((int)Math.Floor(RDExtent.MaxY / 1000) * 1000);
        
        // cameralocatie omzetten naar RD-locatie
        Vector3RD Camlocation3RD = CoordConvert.UnitytoRD(Camera.main.transform.localPosition);
        Vector3 CamlocationRD = new Vector3((float)Camlocation3RD.x, (float)Camlocation3RD.y, (float)Camlocation3RD.z);

      
        //Benodigde tegels uitzoeken
        Dictionary<Vector3, int> BuildingTilesNeeded = new Dictionary<Vector3, int>();
        for (int deltax = X0; deltax <= X1; deltax += 1000)
        {
            for (int deltay = Y0; deltay <= Y1; deltay += 1000)
            {
                Vector3 Hart = new Vector3(deltax + 500, deltay + 500, 0);
                ;
                Vector3 Verschil = CamlocationRD - Hart;
                double afstand = Verschil.magnitude;
                if (Max_Afstand_BAG < afstand && afstand < Max_Afstand_Top10)
                {
                    Add(new Vector3(deltax, deltay, 0));
                    BuildingTilesNeeded.Add(new Vector3(deltax, deltay, 0),1);
                }
                if (Max_Afstand_Daken<afstand && afstand < Max_Afstand_BAG)
                {
                    //controleren of bagpandtegel binnen view valt
                    Add(new Vector3(deltax, deltay, 1));
                    BuildingTilesNeeded.Add(new Vector3(deltax, deltay, 1), 1);
                    
                }
                if (afstand < Max_Afstand_Daken)
                {
                    //controleren of bagpandtegel binnen view valt
                    Add(new Vector3(deltax, deltay, 2));
                    BuildingTilesNeeded.Add(new Vector3(deltax, deltay, 2), 1);

                }

            }
        }

        // tegels verwijderen die niet meer nodig zijn
        foreach (KeyValuePair<Vector3, BuildingTileData> KeyPair in buildingTiles)
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
            buildingTiles.Add(TileID, new BuildingTileData(TileID));
            PendingDownloads.Add(TileID);
        }
        else
        {
            if(buildingTiles[TileID].Status==BuildingTileStatus.PendingDestroy)
            {
                buildingTiles[TileID].Status = BuildingTileStatus.Built; //status terugzetten naar Built
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
        if (!PendingDestroy.Contains(TileID)&& !ActiveDownloads.Contains(TileID) && !ActiveBuilds.Contains(TileID) && buildingTiles.ContainsKey(TileID))
        {
            PendingDestroy.Add(TileID);
            buildingTiles[TileID].Status = BuildingTileStatus.PendingDestroy;
            PendingDownloads.RemoveAll(elem => elem == TileID);
            PendingBuilds.RemoveAll(elem => elem == TileID);

        }
        
    }

    private IEnumerator TilesBijwerken()
    {
        // verwijderen wanneer mogelijk
        for (int j = PendingDestroy.Count-1; j >-1; j--)
        
        {
            Vector3 TileID = PendingDestroy[j];
            if (buildingTiles[TileID].AB != null)
            {
                buildingTiles[TileID].AB.Unload(true);
                for (int i = buildingTiles[TileID].Gameobjecten.Count-1; i >-1; i--)
                {
                    Destroy(buildingTiles[TileID].Gameobjecten[i].GetComponent<MeshFilter>().mesh);
                    Destroy(buildingTiles[TileID].Gameobjecten[i]);
                }
            }
            buildingTiles.Remove(TileID);
            PendingDestroy.RemoveAt(j);
        }
        //downloaden wanneer mogelijk
        if (ActiveDownloads.Count < MAX_Concurrent_Downloads && PendingDownloads.Count>0)
        {
            Vector3 TileID = PendingDownloads[0];
            PendingDownloads.RemoveAt(0);
            ActiveDownloads.Add(TileID);
            buildingTiles[TileID].Status = BuildingTileStatus.Downloading;
            StartCoroutine(DownloadAssetBundleWebGL(TileID));
        }
        //builden wanneer mogelijk
        if (PendingBuilds.Count>0)
        {
            Vector3 TileID = PendingBuilds[0];
            if (buildingTiles.ContainsKey(TileID))
            {
                PendingBuilds.RemoveAt(0);
                ActiveBuilds.Add(TileID);
                StartCoroutine(ProcessBuildingTile(TileID));
            }
            else{
                PendingBuilds.RemoveAt(0);
            }
            
        }
        yield return new WaitForSeconds(0.1f);
        BijwerkenGereed = true;
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
        BuildingTileData btd = buildingTiles[TileID];
#if UNITY_EDITOR        // inde editor staand de assetbundles in de map Assetbundles naast de map 3DAmsterdam
        BuildingURL = "file:///" + Application.dataPath.Replace("/3DAmsterdam/Assets","") + "/AssetBundles/WebGL/";
#endif
        string url = BuildingURL + "gebouwen_" + ((int)btd.id.x).ToString() + "_" + ((int)btd.id.y).ToString() + "." + ((int)btd.id.z).ToString();
        using (UnityWebRequest uwr = UnityWebRequestAssetBundle.GetAssetBundle(url))
        {
            yield return uwr.SendWebRequest();

            if (uwr.isNetworkError || uwr.isHttpError)
            {
                ActiveDownloads.Remove(btd.id);
                btd.Status = BuildingTileStatus.PendingBuild;
                PendingBuilds.Add(btd.id);
            }
            else
            {
                // Get downloaded asset bundle
                AssetBundle bundle = DownloadHandlerAssetBundle.GetContent(uwr);
                btd.AB = bundle;
                ActiveDownloads.Remove(btd.id);
                btd.Status = BuildingTileStatus.PendingBuild;
                PendingBuilds.Add(btd.id);
            }
        }
        
        yield return null;
    }

    private IEnumerator ProcessBuildingTile(Vector3 TileID)
    {
        BuildingTileData btd = buildingTiles[TileID];

        //meshes uit assetbundle inlezen
        Mesh[] ABAssets = new Mesh[0];
        try
        {
            ABAssets = btd.AB.LoadAllAssets<Mesh>();
        }
        catch (Exception)
        {
            btd.Status = BuildingTileStatus.Built;
            yield break;

        }

        //bagidlijst uit assetbundle inlezen als detailniveau = 2
        string[] bagidRegels = new string[0];
        bool ObjectenlijstAanwezig = false;
        if (btd.AB.LoadAllAssets<TextAsset>().Length > 0)
        {
            ObjectenlijstAanwezig = true;
            TextAsset bagidlijst = btd.AB.LoadAllAssets<TextAsset>()[0];
            string tekst = bagidlijst.text;
            bagidRegels = Regex.Split(tekst, "\n|\r|\r\n");
            
        }

        foreach (Mesh ass in ABAssets)
        {
            string Meshnaam = ass.name;
            float X = float.Parse(Meshnaam.Split('_')[0]);
            float Y = float.Parse(Meshnaam.Split('_')[1]);
            int volgnummer = 0;
            if (Meshnaam.Split('_').Length>2)
            {
                volgnummer = int.Parse(Meshnaam.Split('_')[2]);
            }
            GameObject container = new GameObject(Meshnaam);
            container.transform.parent = transform;
            container.layer = LayerMask.NameToLayer("Panden");

            //positioning container
            Vector3RD hoekpunt = new Vector3RD(X, Y, 0);
            double OriginOffset = 500;
            if (btd.id.z>0)
            {
                OriginOffset = 500;
            }
            Vector3RD origin = new Vector3RD(hoekpunt.x + (OriginOffset), hoekpunt.y + (OriginOffset), 0);
            Vector3 UnityOrigin = CoordConvert.RDtoUnity(origin);
            container.transform.localPosition = UnityOrigin;
            double Rotatie = CoordConvert.RDRotation(origin);
            container.transform.Rotate(Vector3.up, (float)Rotatie);

            //add mesh
            MeshFilter mf = container.AddComponent<MeshFilter>();
            mf.sharedMesh = ass;
            MeshCollider mc = container.AddComponent<MeshCollider>();
            mc.sharedMesh = ass;
            //add material
            MeshRenderer mr = container.AddComponent<MeshRenderer>();
            mr.material = GebouwMateriaal;

            //gebouwenlijst toevoegen
            if (ObjectenlijstAanwezig)
            {
                gebouwenlijst = new Dictionary<string, float>();
                
                string[] panden = Regex.Split(bagidRegels[volgnummer * 2], ",");
                for (int i = 0; i < panden.Length - 1; i++)
                {
                    if (gebouwenlijst.ContainsKey(panden[i]))
                    {
                        Debug.Log("bagid '" + panden[i] + "' bestaat al");
                    }
                    else
                    {
                        gebouwenlijst.Add(panden[i], (float)i);
                    }
                }
                ObjectMapping Bid = container.AddComponent<ObjectMapping>();
                Bid.Objectenlijst = gebouwenlijst;
                Bid.DefaultMaterial = GebouwMateriaal;
                Bid.HighlightMaterial = HighLightMateriaal;
                Bid.SetMesh(ass);

            }
            //add to Buildingtiledata
            btd.Gameobjecten.Add(container);
            ActiveBuilds.Remove(btd.id);
            

            yield return null;
        }
        //////instantiaten en assets loaden
        btd.Status = BuildingTileStatus.Built;
        
        
    }

}
