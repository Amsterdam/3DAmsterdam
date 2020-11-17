using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BruTile;
using ConvertCoordinates;
using System.Linq;
using UnityEngine.Networking;
using Amsterdam3D.CameraMotion;


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

        public bool pauseLoading = false;
        public int maximumConcurrentDownloads = 5;

        public List<Layer> layers = new List<Layer>();
        private List<int> tileSizes = new List<int>();
        private List<List<Vector3Int>> tileDistances = new List<List<Vector3Int>>();
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
        public ICameraExtents cameraExtents;
        private Vector3Int cameraPosition;
        private Extent previousCameraViewExtent;

        private bool objectDataLoaded = false;

        private int lod = 10;
        private string url;
        private const string ApiUrl = "https://api.data.amsterdam.nl/bag/v1.1/pand/";
        private Vector3RD bottomLeft;
        private Vector3RD topRight;
        private Vector3RD cameraPositionRD;

        private Vector2Int tileKey;

        public void OnCameraChanged() 
        {
            cameraExtents = CameraModeChanger.Instance.CurrentCameraExtends;
        }
        
        void Start()
        {
            cameraExtents = CameraModeChanger.Instance.CurrentCameraExtends;
            CameraModeChanger.Instance.OnFirstPersonModeEvent += OnCameraChanged;
            CameraModeChanger.Instance.OnGodViewModeEvent += OnCameraChanged;
        }

        void Update()
        {
            UpdateViewRange();
            GetTilesizes();
            GetPossibleTiles();
            
            GetTileChanges();

            RemoveOutOfViewTiles();



            if (pendingTileChanges.Count==0){return;}

            if (activeTileChanges.Count<maximumConcurrentDownloads)
            {
                TileChange highestPriorityTileChange = FindHighestPriorityTileChange();
                Vector3Int tilekey = new Vector3Int(highestPriorityTileChange.X, highestPriorityTileChange.Y, highestPriorityTileChange.layerIndex);
                if (activeTileChanges.ContainsKey(tilekey) == false)
                {
                activeTileChanges.Add(tilekey, highestPriorityTileChange);
                    pendingTileChanges.Remove(highestPriorityTileChange);
                    HandleTile(highestPriorityTileChange);
                }
            }
        }

        private void CheckForObjectData()
        {
            foreach (Layer layer in layers)
            {
                foreach (KeyValuePair< Vector2Int, Tile> kvp in layer.tiles)
                {
                    if (kvp.Value.gameObject.GetComponent<ObjectData>() !=null)
                    {
                        Debug.Log(kvp.Value.tileKey);
                    }
                }
            }
        }




        public void GetIDData(GameObject obj, int vertexIndex, System.Action<string> callback = null) 
        {
            StartCoroutine(AddObjectData(obj, vertexIndex, callback));
        }
        private IEnumerator AddObjectData(GameObject obj, int vertexIndex, System.Action<string> callback) 
        {
            string name = obj.GetComponent<MeshFilter>().mesh.name;
            Debug.Log(name);
            string dataName = name.Replace(" Instance", "");
            dataName = dataName.Replace("mesh", "building");
            dataName = dataName.Replace("-", "_") + "-data";
            string dataURL = Constants.TILE_METADATA_URL + dataName;
            Debug.Log(dataURL);
            ObjectMappingClass data;
            string id = "null";
            using (UnityWebRequest uwr = UnityWebRequestAssetBundle.GetAssetBundle(dataURL))
            {
                yield return uwr.SendWebRequest();

                if (uwr.isNetworkError || uwr.isHttpError)
                {
                    //Not showing warnings for now, because this can occur pretty often. I dialog would be annoying.
                    //WarningDialogs.Instance.ShowNewDialog("De metadata voor " + obj.name + " kon niet worden geladen. Ben je nog online?");
                }
                else
                {
                    ObjectData objectMapping = obj.GetComponent<ObjectData>();
                    if (objectMapping is null)
                    {
                        objectMapping = obj.AddComponent<ObjectData>();
                    }

                    AssetBundle newAssetBundle = DownloadHandlerAssetBundle.GetContent(uwr);
                    data = newAssetBundle.LoadAllAssets<ObjectMappingClass>()[0];
                    int idIndex = data.vectorMap[vertexIndex];
                    id = data.ids[idIndex];
                    objectMapping.highlightIDs.Clear();
                    objectMapping.highlightIDs.Add(id);
                    objectMapping.ids = data.ids;
                    objectMapping.uvs = data.uvs;
                    objectMapping.vectorMap = data.vectorMap;
                    objectMapping.mappedUVs = data.mappedUVs;

                    newAssetBundle.Unload(true);
                }
            }
            callback?.Invoke(id);
            yield return null;
            pauseLoading = false;
        }
        
        



        private IEnumerator UpdateHighlight(Tile oldTile, GameObject newTile)
        {
            ObjectData oldObjectMapping = oldTile.gameObject.GetComponent<ObjectData>();
            if (oldObjectMapping == null)
            {
                objectDataLoaded = true;
                yield break;
            }
            if (oldObjectMapping.highlightIDs.Count==0)
            {
                objectDataLoaded = true;
                yield break;
            }
            yield return null;
            string name =  newTile.GetComponent<MeshFilter>().mesh.name;
            Debug.Log(name);
            string dataName = name.Replace(" Instance", "");
            dataName = dataName.Replace("mesh", "building");
            dataName = dataName.Replace("-", "_") + "-data";
            string dataURL = Constants.TILE_METADATA_URL + dataName;
            Debug.Log(dataURL);
            ObjectMappingClass data;
            using (UnityWebRequest uwr = UnityWebRequestAssetBundle.GetAssetBundle(dataURL))
            {
                yield return uwr.SendWebRequest();

                if (uwr.isNetworkError || uwr.isHttpError)
                {

                }
                else
                {
                    ObjectData objectMapping = newTile.AddComponent<ObjectData>();
                    AssetBundle newAssetBundle = DownloadHandlerAssetBundle.GetContent(uwr);
                    data = newAssetBundle.LoadAllAssets<ObjectMappingClass>()[0];
                             
                    objectMapping.highlightIDs = oldObjectMapping.highlightIDs;
                    objectMapping.hideIDs = oldObjectMapping.hideIDs;
                    objectMapping.ids = data.ids;
                    objectMapping.uvs = data.uvs;
                    objectMapping.vectorMap = data.vectorMap;
                    objectMapping.mappedUVs = data.mappedUVs;
                    objectMapping.mesh = newTile.GetComponent<MeshFilter>().mesh;
                    objectMapping.triangleCount = data.triangleCount;
                    objectMapping.UpdateUVs();
                    newAssetBundle.Unload(true);
                }
                objectDataLoaded = true;
            }

            yield return null;
        }
        private GameObject BuildNewTile(AssetBundle assetBundle, TileChange tileChange)
        {
            GameObject container = new GameObject();
            container.name = tileChange.X.ToString() + "-" + tileChange.Y.ToString();
            container.transform.parent = layers[tileChange.layerIndex].gameObject.transform;
            container.layer = container.transform.parent.gameObject.layer;
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
            Mesh mesh = meshesInAssetbundle[0];
            Vector2 uv = new Vector2(0.33f, 0.5f);
            int count = mesh.vertexCount;
            List<Vector2> uvs = new List<Vector2>();
            Vector2 defaultUV = new Vector2(0.33f, 0.6f);
            for (int i = 0; i < count; i++)
            {
                uvs.Add(defaultUV);
            }
            mesh.uv2 = uvs.ToArray();


            float X = float.Parse(mesh.name.Split('_')[0]);
            float Y = float.Parse(mesh.name.Split('_')[1]);

            //positioning container
            Vector3RD cornerPoint = new Vector3RD(X, Y, 0);
            double OriginOffset = 500;
            Vector3RD origin = new Vector3RD(cornerPoint.x + OriginOffset, cornerPoint.y + OriginOffset, 0);
            Vector3 unityOrigin = CoordConvert.RDtoUnity(origin);
            container.transform.position = unityOrigin;

            container.AddComponent<MeshFilter>().mesh = mesh;
            container.AddComponent<MeshRenderer>().sharedMaterial = material;
            
            assetBundle.Unload(false);
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
            bottomLeft = CoordConvert.WGS84toRD(cameraExtents.GetExtent().MinX, cameraExtents.GetExtent().MinY);
            topRight = CoordConvert.WGS84toRD(cameraExtents.GetExtent().MaxX, cameraExtents.GetExtent().MaxY);

            viewRange.x = (float)bottomLeft.x;
            viewRange.y = (float)bottomLeft.y;
            viewRange.z = (float)(topRight.x - bottomLeft.x);
            viewRange.w = (float)(topRight.y- bottomLeft.y);

            cameraPositionRD = CoordConvert.UnitytoRD(cameraExtents.GetPosition());
            cameraPosition.x = (int)cameraPositionRD.x;
            cameraPosition.y = (int)cameraPositionRD.y;
            cameraPosition.z = (int)cameraPositionRD.z;
        }

        private bool HasCameraViewChanged()
        {
            bool cameraviewChanged = false;
            if (previousCameraViewExtent.CenterX != cameraExtents.GetExtent().CenterX || previousCameraViewExtent.CenterY != cameraExtents.GetExtent().CenterY)
            {
                cameraviewChanged = true;
                previousCameraViewExtent = cameraExtents.GetExtent();
            }
            return cameraviewChanged;
        }

        private void GetPossibleTiles()
        {
            tileDistances.Clear();

            int startX;
            int startY;
            int endX;
            int endY;

            List<Vector3Int> tileList;
            foreach (int tileSize in tileSizes)
            {
                tileList = new List<Vector3Int>();
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
                tileDistances.Add(tileList);
            }
        }

        private float GetTileDistanceSquared(Vector3Int tileID)
        {
            float distance =0;
            int centerOffset = (int)tileID.z / 2;
            Vector3Int center = new Vector3Int(tileID.x + centerOffset, tileID.y + centerOffset, 0);
            float delta = center.x - cameraPosition.x;
            distance += (delta * delta);
            delta = center.y - cameraPosition.y;
            distance += (delta * delta);
            delta = cameraPosition.z * cameraPosition.z;
            distance += (delta);

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
                if (dataSet.maximumDistanceSquared > (tiledistance.z))
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
                foreach (Vector3Int tileDistance in tileDistances[tilesizeIndex])
                {
                    tileKey = new Vector2Int(tileDistance.x, tileDistance.y);
                    
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

        private void RemoveOutOfViewTiles()
        {
            List<Vector3Int> neededTileSizesDistance;
            List<Vector2Int> neededTileSizes = new List<Vector2Int>();
            List<Vector2Int> activeTiles;
            for (int layerIndex = 0; layerIndex < layers.Count; layerIndex++)
            {
                Layer layer = layers[layerIndex];
                if (layer.gameObject.activeSelf == false) { continue; }
                int tilesizeIndex = tileSizes.IndexOf(layer.tileSize);
                neededTileSizesDistance = tileDistances[tilesizeIndex];
                neededTileSizes.Clear();
                foreach (var neededTileSize in neededTileSizesDistance)
                {
                    neededTileSizes.Add(new Vector2Int(neededTileSize.x, neededTileSize.y));
                }

                activeTiles = new List<Vector2Int>(layer.tiles.Keys);
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







        private void HandleTile(TileChange tileChange)
        {
            
            TileAction action = tileChange.action;
            switch (action)
            {
                case TileAction.Create:
                    Tile newTile = createNewTile(tileChange);
                    layers[tileChange.layerIndex].tiles.Add(new Vector2Int(tileChange.X, tileChange.Y), newTile);
                    break;
                case TileAction.Upgrade:
                    layers[tileChange.layerIndex].tiles[new Vector2Int(tileChange.X, tileChange.Y)].LOD++;
                    break;
                case TileAction.Downgrade:
                    layers[tileChange.layerIndex].tiles[new Vector2Int(tileChange.X, tileChange.Y)].LOD--;
                    break;
                case TileAction.Remove:
                    activeTileChanges.Remove(new Vector3Int(tileChange.X, tileChange.Y, tileChange.layerIndex));
                    RemoveGameObjectFromTile(tileChange);
                    layers[tileChange.layerIndex].tiles.Remove(new Vector2Int(tileChange.X, tileChange.Y));
                    
                    return;
                    break;
                default:
                    break;
            }
            StartCoroutine(DownloadAssetBundle(tileChange));
        }


        private bool tileHasHighlight(TileChange tileChange)
        {
            Tile tile = layers[tileChange.layerIndex].tiles[new Vector2Int(tileChange.X, tileChange.Y)];
            if (tile.gameObject == null)
            {
                return false;
            }
            if (tile.gameObject.GetComponent<ObjectData>() == null)
            {
                return false;
            }
            if (tile.gameObject.GetComponent<ObjectData>().highlightIDs.Count + tile.gameObject.GetComponent<ObjectData>().hideIDs.Count == 0)
            {
                return false;
            }

            return true;
        }

        private IEnumerator DownloadAssetBundle(TileChange tileChange)
        {
            int lod = layers[tileChange.layerIndex].tiles[new Vector2Int(tileChange.X, tileChange.Y)].LOD;
            string url = Constants.BASE_DATA_URL + layers[tileChange.layerIndex].Datasets[lod].path;
            url = url.Replace("{x}", tileChange.X.ToString());
            url = url.Replace("{y}", tileChange.Y.ToString());
            using (UnityWebRequest uwr = UnityWebRequestAssetBundle.GetAssetBundle(url))
            {
                yield return uwr.SendWebRequest();

                if (uwr.isNetworkError || uwr.isHttpError)
                {
                    RemoveGameObjectFromTile(tileChange);
                    activeTileChanges.Remove(new Vector3Int(tileChange.X, tileChange.Y, tileChange.layerIndex));
                }
                else
                {
                    AssetBundle assetBundle = DownloadHandlerAssetBundle.GetContent(uwr);
                    yield return new WaitUntil(() => pauseLoading == false);
                    GameObject newGameobject = CreateNewGameObject(assetBundle, tileChange);
                    if (newGameobject!=null)
                    {
                        if (tileHasHighlight(tileChange))
                        {
                            StartCoroutine(DownloadIDMappingData(tileChange, newGameobject));
                        }
                        else
                        {
                            RemoveGameObjectFromTile(tileChange);
                            layers[tileChange.layerIndex].tiles[new Vector2Int(tileChange.X, tileChange.Y)].gameObject = newGameobject;
                            activeTileChanges.Remove(new Vector3Int(tileChange.X, tileChange.Y, tileChange.layerIndex));
                        }
                    }
                    else
                    {
                        activeTileChanges.Remove(new Vector3Int(tileChange.X, tileChange.Y, tileChange.layerIndex));
                    }
                    
                }
            }
        }
        private IEnumerator DownloadIDMappingData(TileChange tileChange, GameObject newGameobject)
        {
            Tile tile = layers[tileChange.layerIndex].tiles[new Vector2Int(tileChange.X, tileChange.Y)];
            ObjectData oldObjectMapping = tile.gameObject.GetComponent<ObjectData>();
            GameObject newTile = newGameobject;
            string name = newTile.GetComponent<MeshFilter>().mesh.name;
            Debug.Log(name);
            string dataName = name.Replace(" Instance", "");
            dataName = dataName.Replace("mesh", "building");
            dataName = dataName.Replace("-", "_") + "-data";
            string dataURL = Constants.TILE_METADATA_URL + dataName;
            Debug.Log(dataURL);
            ObjectMappingClass data;
            using (UnityWebRequest uwr = UnityWebRequestAssetBundle.GetAssetBundle(dataURL))
            {
                yield return uwr.SendWebRequest();

                if (uwr.isNetworkError || uwr.isHttpError)
                {

                }
                else
                {
                    ObjectData objectMapping = newTile.AddComponent<ObjectData>();
                    AssetBundle newAssetBundle = DownloadHandlerAssetBundle.GetContent(uwr);
                    data = newAssetBundle.LoadAllAssets<ObjectMappingClass>()[0];

                    objectMapping.highlightIDs = oldObjectMapping.highlightIDs;
                    objectMapping.hideIDs = oldObjectMapping.hideIDs;
                    objectMapping.ids = data.ids;
                    objectMapping.uvs = data.uvs;
                    objectMapping.vectorMap = data.vectorMap;
                    objectMapping.mappedUVs = data.mappedUVs;
                    objectMapping.mesh = newTile.GetComponent<MeshFilter>().mesh;
                    objectMapping.triangleCount = data.triangleCount;
                    objectMapping.UpdateUVs();
                    newAssetBundle.Unload(true);
                }
                objectDataLoaded = true;
            }
            yield return new WaitUntil(() => pauseLoading == false);
            RemoveGameObjectFromTile(tileChange);
            layers[tileChange.layerIndex].tiles[new Vector2Int(tileChange.X, tileChange.Y)].gameObject = newGameobject;
            activeTileChanges.Remove(new Vector3Int(tileChange.X, tileChange.Y, tileChange.layerIndex));
            yield return null;

        }

        private GameObject CreateNewGameObject(AssetBundle assetBundle, TileChange tileChange)
        {
            GameObject container = new GameObject();
            container.name = tileChange.X.ToString() + "-" + tileChange.Y.ToString();
            container.transform.parent = layers[tileChange.layerIndex].gameObject.transform;
            container.layer = container.transform.parent.gameObject.layer;
            container.transform.position = CoordConvert.RDtoUnity(new Vector2(tileChange.X + 500, tileChange.Y + 500));
            Material defaultMaterial = layers[tileChange.layerIndex].DefaultMaterial;

            Mesh[] meshesInAssetbundle = new Mesh[0];
            try
            {
                meshesInAssetbundle = assetBundle.LoadAllAssets<Mesh>();
            }
            catch (Exception)
            {
                Destroy(container);
                assetBundle.Unload(true);
                return null;
            }
            Mesh mesh = meshesInAssetbundle[0];
            Vector2 uv = new Vector2(0.33f, 0.5f);
            int count = mesh.vertexCount;
            List<Vector2> uvs = new List<Vector2>();
            Vector2 defaultUV = new Vector2(0.33f, 0.6f);
            for (int i = 0; i < count; i++)
            {
                uvs.Add(defaultUV);
            }
            mesh.uv2 = uvs.ToArray();

            container.AddComponent<MeshFilter>().mesh = mesh;
            container.AddComponent<MeshRenderer>().sharedMaterial = defaultMaterial;

            assetBundle.Unload(false);
            return container;
        }

        private void RemoveGameObjectFromTile(TileChange tileChange)
        {
            if (layers[tileChange.layerIndex].tiles.ContainsKey(new Vector2Int(tileChange.X, tileChange.Y)))
            {

                Tile tile = layers[tileChange.layerIndex].tiles[new Vector2Int(tileChange.X, tileChange.Y)];
                if (tile == null)
                {
                    return;
                }
                if (tile.gameObject == null)
                {
                    return;
                }
                MeshFilter mf = tile.gameObject.GetComponent<MeshFilter>();
                if (mf != null)
                {
                    DestroyImmediate(tile.gameObject.GetComponent<MeshFilter>().sharedMesh, true);
                }
                Destroy(layers[tileChange.layerIndex].tiles[new Vector2Int(tileChange.X, tileChange.Y)].gameObject);
            }
        }

        private Tile createNewTile(TileChange tileChange)
        {
            Tile tile = new Tile();
            tile.LOD = 0;
            tile.tileKey = tileKey;
            tile.layer = layers[tileChange.layerIndex];
            tile.gameObject = new GameObject();
            tile.gameObject.transform.parent = layers[tileChange.layerIndex].gameObject.transform;
            tile.gameObject.layer = tile.gameObject.transform.parent.gameObject.layer;
            tile.gameObject.transform.position = CoordConvert.RDtoUnity(new Vector2(tileChange.X, tileChange.Y));
            
            return tile;
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
        public Layer layer;
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
