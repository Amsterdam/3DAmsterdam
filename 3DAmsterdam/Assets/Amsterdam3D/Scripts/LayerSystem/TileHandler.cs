using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BruTile;
using ConvertCoordinates;
using System.Linq;
using UnityEngine.Networking;

namespace LayerSystem
{
    public enum TileAction
    {
        Create,
        Upgrade,
        Downgrade,
        Remove
    }
    public class TileHandler : MonoBehaviour
    {


        public int maximumConcurrentDownloads = 5;
        public List<Layer> layers = new List<Layer>();
        private List<int> tileSizes = new List<int>();

        private List<List<Vector3Int>> TileDistances = new List<List<Vector3Int>>();
        
        private List<TileChange> pendingTileChanges = new List<TileChange>();
        private Dictionary<Vector3Int, TileChange> activeTileChanges = new Dictionary<Vector3Int, TileChange>();
        // key : Vector3Int where
        //                  x = bottomleft x-coordinate in RD
        //                  y = bottomleft y-coordinate in RD
        //                  z = layerIndex.

        private Vector4 viewRange = new Vector4();
        // x= minimum X-coordinate in RD
        // y= minimum Y-coordinate in RD
        // z= size in X-direction in M.
        // w= size in Y-direction in M.
        private CameraView CV;
        private Vector3Int cameraPosition;
        private Extent previousCameraViewExtent;


        // Start is called before the first frame update
        void Start()
        {
            CV = Camera.main.GetComponent<CameraView>();
        }

        // Update is called once per frame
        void Update()
        {
            //if (HasCameraViewChanged())
            //{
                UpdateViewRange();
                GetTilesizes();
                getPossibleTiles();
                GetTileChanges();
                RemoveOUtOfViewTiles();
            //}

            if (pendingTileChanges.Count==0){return;}

            if (activeTileChanges.Count<maximumConcurrentDownloads)
            {
                TileChange highestPriorityTIleChange = FindHighestPriorityTileChange();
                Vector3Int tilekey = new Vector3Int(highestPriorityTIleChange.X, highestPriorityTIleChange.Y, highestPriorityTIleChange.layerIndex);
                if (activeTileChanges.ContainsKey(tilekey) == false)
                {
                    activeTileChanges.Add(tilekey, highestPriorityTIleChange);
                    pendingTileChanges.Remove(highestPriorityTIleChange);
                    HandleTile(highestPriorityTIleChange);
                }
                
                
                
            }
            // only if viewRange has changed:
            // check which tiles are required in view for each tilesize and add to TileDistances (key: X-bottom-left,Y-bottom-left,tilesize)
            // calculate distance for each tile in TileDistances
                
            // for each active Layer:
                // check which tiles can be destroyed
                // check which tiles can be removed from DownloadQueue

                // only for tiles that have status.Ready:
                // check required LOD for each tile
                // up- or downgrade LOD and set status.pendingdownload

            // only for tiles that have status.pendingdownload:
            //start the download, prioritize bij LOD and layer-priority
        }

        private void HandleTile(TileChange tileChange)
        {
            int lod=10;
            string url;
            switch (tileChange.action)
            {
                case TileAction.Create:
                    lod = 0;

                    break;
                case TileAction.Upgrade:
                    lod = layers[tileChange.layerIndex].tiles[new Vector2Int(tileChange.X, tileChange.Y)].LOD + 1;
                    
                    break;
                case TileAction.Downgrade:
                    lod = layers[tileChange.layerIndex].tiles[new Vector2Int(tileChange.X, tileChange.Y)].LOD - 1;
                    if (lod<0)
                    {
                        Destroy(layers[tileChange.layerIndex].tiles[new Vector2Int(tileChange.X, tileChange.Y)].gameObject);
                        layers[tileChange.layerIndex].tiles.Remove(new Vector2Int(tileChange.X, tileChange.Y));
                        activeTileChanges.Remove(new Vector3Int(tileChange.X, tileChange.Y, tileChange.layerIndex));
                        return;
                    }
                    break;
                case TileAction.Remove:
                    Destroy(layers[tileChange.layerIndex].tiles[new Vector2Int(tileChange.X, tileChange.Y)].gameObject);
                    layers[tileChange.layerIndex].tiles.Remove(new Vector2Int(tileChange.X, tileChange.Y));
                    activeTileChanges.Remove(new Vector3Int(tileChange.X, tileChange.Y, tileChange.layerIndex));
                    return;
                    break;
                default:
                    break;
            }

            if (lod >=0 && lod< layers[tileChange.layerIndex].Datasets.Count)
            {
                url = Constants.BASE_DATA_URL + layers[tileChange.layerIndex].Datasets[lod].path;
                // temp
                //url = "file://E:/UnityData/Assetbundles/WebGL/BuildingData/"+ layers[tileChange.layerIndex].Datasets[lod].path;


                url = url.Replace("{x}", tileChange.X.ToString());
                url = url.Replace("{y}", tileChange.Y.ToString());
                url = url.Replace("{lod}", lod.ToString());
                StartCoroutine(DownloadTile(url, tileChange));

            }
            
        }

        private IEnumerator DownloadTile(string url, TileChange tileChange)
        {
            using (UnityWebRequest uwr = UnityWebRequestAssetBundle.GetAssetBundle(url))
            {
                
                Vector2Int tileKey = new Vector2Int(tileChange.X, tileChange.Y);
                Tile tile;
                if (tileChange.action == TileAction.Create)
                {
                    tile = new Tile();
                    tile.LOD = 0;
                    tile.tileKey = tileKey;
                    tile.gameObject = new GameObject();
                    tile.gameObject.transform.parent = layers[tileChange.layerIndex].gameObject.transform;
                    tile.gameObject.transform.position = CoordConvert.RDtoUnity(new Vector2(tileChange.X, tileChange.Y));
                    layers[tileChange.layerIndex].tiles.Add(tileKey, tile);
                }
                else
                {
                    tile = layers[tileChange.layerIndex].tiles[tileKey];
                }

                yield return uwr.SendWebRequest();

                if (uwr.isNetworkError || uwr.isHttpError)
                {
                    if (tile.assetBundle is null)
                    {

                    }
                    else
                    {
                        tile.assetBundle.Unload(true);
                    }
                }
                else
                {
                    AssetBundle newAssetBundle = DownloadHandlerAssetBundle.GetContent(uwr);
                    GameObject newTile = buildNewTile(newAssetBundle, tileChange);
                    if (tileChange.action == TileAction.Downgrade)
                    {
                        layers[tileChange.layerIndex].tiles[tileKey].LOD--;
                    }
                    if (tileChange.action == TileAction.Upgrade)
                    {
                        layers[tileChange.layerIndex].tiles[tileKey].LOD++;
                    }
                    Destroy(layers[tileChange.layerIndex].tiles[tileKey].gameObject);
                    layers[tileChange.layerIndex].tiles[tileKey].gameObject = newTile;
                    
                }
                activeTileChanges.Remove(new Vector3Int(tileChange.X, tileChange.Y, tileChange.layerIndex));
            }
            yield return null;

        }

        private GameObject buildNewTile(AssetBundle assetBundle, TileChange tileChange)
        {
            GameObject container = new GameObject();
            container.name = tileChange.X.ToString() + "-" + tileChange.Y.ToString();
            container.transform.parent = layers[tileChange.layerIndex].gameObject.transform;
            container.transform.position = CoordConvert.RDtoUnity(new Vector2(tileChange.X+500, tileChange.Y+500));
            Material material = layers[tileChange.layerIndex].DefaultMaterial;
            Mesh[] meshesInAssetbundle = new Mesh[0];
            try
            {
                meshesInAssetbundle = assetBundle.LoadAllAssets<Mesh>();
            }
            catch (Exception)
            {
                Destroy(container);
                assetBundle.Unload(true);
            }

            foreach (Mesh mesh in meshesInAssetbundle)
            {
                Mesh newMesh = new Mesh();
                newMesh.vertices = mesh.vertices;
                newMesh.triangles = mesh.triangles;
                newMesh.normals = mesh.normals;
                newMesh.name = mesh.name;
                GameObject subObject = new GameObject();
                subObject.transform.parent = container.transform;

                
                float X = float.Parse(mesh.name.Split('_')[0]);
                float Y = float.Parse(mesh.name.Split('_')[1]);

                //positioning container
                Vector3RD hoekpunt = new Vector3RD(X, Y, 0);
                double OriginOffset = 500;
                Vector3RD origin = new Vector3RD(hoekpunt.x+OriginOffset, hoekpunt.y+OriginOffset, 0);
                Vector3 unityOrigin = CoordConvert.RDtoUnity(origin);
                subObject.transform.position = unityOrigin;
                double Rotatie = CoordConvert.RDRotation(origin);
                subObject.transform.Rotate(Vector3.up, (float)Rotatie);

                //subObject.transform.localPosition = Vector3.zero;
                subObject.AddComponent<MeshFilter>().mesh = newMesh;
                subObject.AddComponent<MeshRenderer>().sharedMaterial = material;
            }
            assetBundle.Unload(true);
            return container;

        }
        private TileChange FindHighestPriorityTileChange()
        {
            TileChange highestPriorityTileChange = pendingTileChanges[0];
            float highestPriority = highestPriorityTileChange.priorityScore;
            
            for (int i = 1; i < pendingTileChanges.Count; i++)
            {
                if (pendingTileChanges[i].priorityScore < highestPriority)
                {
                    highestPriorityTileChange = pendingTileChanges[i];
                    highestPriority = highestPriorityTileChange.priorityScore;
                }
            }
            return highestPriorityTileChange;
        }

        private void UpdateViewRange()
        {
            Vector3RD bottomleft = CoordConvert.WGS84toRD(CV.cameraExtent.MinX, CV.cameraExtent.MinY);
            Vector3RD topright = CoordConvert.WGS84toRD(CV.cameraExtent.MaxX, CV.cameraExtent.MaxY);

            viewRange.x = (float)bottomleft.x;
            viewRange.y = (float)bottomleft.y;
            viewRange.z = (float)(topright.x -bottomleft.x);
            viewRange.w = (float)(topright.y-bottomleft.y);

            Vector3RD cameraPositionRD = CoordConvert.UnitytoRD(CV.gameObject.transform.position);
            cameraPosition.x = (int)cameraPositionRD.x;
            cameraPosition.y = (int)cameraPositionRD.y;
            cameraPosition.z = (int)cameraPositionRD.z;
        }

        private bool HasCameraViewChanged()
        {
            bool cameraviewChanged = false;
            if (previousCameraViewExtent.CenterX != CV.cameraExtent.CenterX || previousCameraViewExtent.CenterY != CV.cameraExtent.CenterY)
            {
                cameraviewChanged = true;
                previousCameraViewExtent = CV.cameraExtent;
            }
            return cameraviewChanged;
        }

        private void getPossibleTiles()
        {
            TileDistances.Clear();

            int startX;
            int startY;
            int endX;
            int endY;
            foreach (int tileSize in tileSizes)
            {
                List<Vector3Int> tileList = new List<Vector3Int>();
                startX = (int)Math.Floor(viewRange.x / tileSize) * tileSize;
                startY = (int)Math.Floor(viewRange.y / tileSize) * tileSize;
                endX = (int)Math.Ceiling((viewRange.x+viewRange.z) / tileSize) * tileSize;
                endY = (int)Math.Ceiling((viewRange.y + viewRange.z) / tileSize) * tileSize;
                for (int x = startX; x < endX; x+=tileSize)
                {
                    for (int y = startY; y < endY; y+=tileSize)
                    {
                        Vector3Int tileID = new Vector3Int(x, y, tileSize);
                        tileList.Add(new Vector3Int(x,y, (int)GetTileDistanceSquared(tileID)));
                    }
                }
                TileDistances.Add(tileList);
            }
        }

        private float GetTileDistanceSquared(Vector3Int tileID)
        {
            float distance =0;
            int centerOffset = (int)tileID.z / 2;
            Vector3Int center = new Vector3Int(tileID.x + centerOffset, tileID.y + centerOffset, 0);
            float delta = center.x - cameraPosition.x;
            distance += delta * delta;
            delta = center.y - cameraPosition.y;
            distance += delta * delta;
            delta = cameraPosition.z * cameraPosition.z;
            distance += delta;

            //Vector3Int difference = new Vector3Int(, tileID.y+centerOffset, 0) - cameraPosition;
            //distance = difference.magnitude;
            return distance;
        }

        private void GetTilesizes()
        {
            int tilesize;
            tileSizes.Clear();
            foreach (Layer layer in layers)
            {
                if (layer.gameObject.activeSelf==true)
                {
                    tilesize = layer.tileSize;
                    if (tileSizes.Contains(tilesize) == false)
                    {
                        tileSizes.Add(tilesize);
                    }
                }
            }
        }

        private int CalculatePriorityScore(int layerPriority, int lod)
        {
            return (10 * lod) - layerPriority;
        }

        private int CalculateLOD(Vector3Int tiledistance, Layer layer)
        {
            int lod = -1;

            foreach (DataSet dataSet in layer.Datasets)
            {
                if (dataSet.maximumDistanceSquared > tiledistance.z)
                {
                    lod = dataSet.lod;

                }
            }
            return lod;
            
        }

        private void GetTileChanges()
        {
            pendingTileChanges.Clear();

            for (int layerIndex = 0; layerIndex < layers.Count; layerIndex++)
            {
                Layer layer = layers[layerIndex];
            if (layer.gameObject.activeSelf==false){continue;}
                int tilesizeIndex = tileSizes.IndexOf(layer.tileSize);
                foreach (Vector3Int tileDistance in TileDistances[tilesizeIndex])
                {
                    Vector2Int tileKey = new Vector2Int(tileDistance.x, tileDistance.y);
                    
                    if (activeTileChanges.ContainsKey(new Vector3Int(tileKey.x, tileKey.y, layerIndex)))
                    {
                        continue;
                    }
                    int LOD = CalculateLOD(tileDistance, layer);
                    if (LOD == -1)
                    {
                        if (layer.tiles.ContainsKey(tileKey))
                        {
                            TileChange tileChange = new TileChange();
                            tileChange.action = TileAction.Remove;
                            tileChange.X = tileKey.x;
                            tileChange.Y = tileKey.y;
                            pendingTileChanges.Add(tileChange);
                            
                        }
                        continue;
                    }

                    if (layer.tiles.ContainsKey(tileKey))
                    {
                        int activeLOD = layer.tiles[tileKey].LOD;
                        if (activeLOD > LOD)
                        {
                            TileChange tileChange = new TileChange();
                            tileChange.action = TileAction.Downgrade;
                            tileChange.X = tileKey.x;
                            tileChange.Y = tileKey.y;
                            tileChange.layerIndex = layerIndex;
                            tileChange.priorityScore = CalculatePriorityScore(layer.layerPriority, activeLOD - 1);
                            pendingTileChanges.Add(tileChange);
                        }
                        else if (activeLOD<LOD)
                        {
                            TileChange tileChange = new TileChange();
                            tileChange.action = TileAction.Upgrade;
                            tileChange.X = tileKey.x;
                            tileChange.Y = tileKey.y;
                            tileChange.layerIndex = layerIndex;
                            tileChange.priorityScore = CalculatePriorityScore(layer.layerPriority, activeLOD + 1);
                            pendingTileChanges.Add( tileChange);
                        }
                    }
                    else
                    {
                        TileChange tileChange = new TileChange();
                        tileChange.action = TileAction.Create;
                        tileChange.X = tileKey.x;
                        tileChange.Y = tileKey.y;
                        tileChange.priorityScore = CalculatePriorityScore(layer.layerPriority, 0);
                        tileChange.layerIndex = layerIndex;
                        pendingTileChanges.Add( tileChange);
                    }
                }
            }
           
        }

        private void RemoveOUtOfViewTiles()
        {
            for (int layerIndex = 0; layerIndex < layers.Count; layerIndex++)
            {
                Layer layer = layers[layerIndex];
                if (layer.gameObject.activeSelf == false) { continue; }
                int tilesizeIndex = tileSizes.IndexOf(layer.tileSize);
                List<Vector3Int> neededTileSizesDistance = TileDistances[tilesizeIndex];
                List<Vector2Int> neededTileSizes = new List<Vector2Int>();
                foreach (var neededTileSize in neededTileSizesDistance)
                {
                    neededTileSizes.Add(new Vector2Int(neededTileSize.x, neededTileSize.y));
                }

                List<Vector2Int> activeTiles = new List<Vector2Int>(layer.tiles.Keys);
                foreach (Vector2Int activeTile in activeTiles)
                {
                    if (neededTileSizes.Contains(activeTile) == false)
                    {
                        TileChange tileChange = new TileChange();
                        tileChange.action = TileAction.Remove;
                        tileChange.X = activeTile.x;
                        tileChange.Y = activeTile.y;
                        tileChange.layerIndex = layerIndex;
                        tileChange.priorityScore = 0;
                        pendingTileChanges.Add(tileChange);
                    }
                }

            }
        }
    }
    [Serializable]
    public class DataSet
    {
        public string Description;
        public int lod;
        public string path;
        public float maximumDistance;
        [HideInInspector]
        public float maximumDistanceSquared;
        public bool enabled = true;
    }

    public class Tile
    {
        public int LOD;
        public GameObject gameObject;
        public AssetBundle assetBundle;
        public Vector2Int tileKey;
    }
    public class TileChange
    {
        public TileAction action;
        public int priorityScore;
        public int layerIndex;
        public int X;
        public int Y;
    }

}
