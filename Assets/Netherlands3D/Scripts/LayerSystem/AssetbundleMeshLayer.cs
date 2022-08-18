using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ConvertCoordinates;
using UnityEngine.Networking;
using System;
using System.Linq;
using UnityEngine.Rendering;

namespace Netherlands3D.LayerSystem
{
    public class AssetbundleMeshLayer : Layer
    {
        //public Material DefaultMaterial;
        public List<Material> DefaultMaterialList = new List<Material>();
        public bool createMeshcollider = false;
        public bool addHighlightuvs = false;
        public ShadowCastingMode tileShadowCastingMode = ShadowCastingMode.On;

        public override void HandleTile(TileChange tileChange, System.Action<TileChange> callback = null)
        {
            TileAction action = tileChange.action;
            var tileKey = new Vector2Int(tileChange.X, tileChange.Y);


            switch (action)
            {
                case TileAction.Create:
                    Tile newTile = CreateNewTile(tileKey);
                    tiles.Add(tileKey, newTile);
                    break;
                case TileAction.Upgrade:
                    tiles[tileKey].LOD++;
                    break;
                case TileAction.Downgrade:
                    tiles[tileKey].LOD--;
                    break;
                case TileAction.Remove:
                    InteruptRunningProcesses(tileKey);
                    RemoveGameObjectFromTile(tileKey);
                    tiles.Remove(tileKey);
                    callback(tileChange);
                    return;
                default:
                    break;
            }
            tiles[tileKey].runningCoroutine = StartCoroutine(DownloadAssetBundle(tileChange, callback));
        }

        private Tile CreateNewTile(Vector2Int tileKey)
        {
            Tile tile = new Tile();
            tile.LOD = 0;
            tile.tileKey = tileKey;
            tile.layer = transform.gameObject.GetComponent<Layer>();
            tile.gameObject = new GameObject();
            tile.gameObject.transform.parent = transform.gameObject.transform;
            tile.gameObject.layer = tile.gameObject.transform.parent.gameObject.layer;
            tile.gameObject.transform.position = CoordConvert.RDtoUnity(tileKey);

            return tile;
        }
        private void RemoveGameObjectFromTile(Vector2Int tileKey)
        {
            if (tiles.ContainsKey(tileKey))
            {

                Tile tile = tiles[tileKey];
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
                Destroy(tiles[tileKey].gameObject);

            }
        }
        private IEnumerator DownloadAssetBundle(TileChange tileChange, System.Action<TileChange> callback = null)
        {
            var tileKey = new Vector2Int(tileChange.X, tileChange.Y);
            int lod = tiles[tileKey].LOD;
            string url = Config.activeConfiguration.webserverRootPath + Datasets[lod].path;
            if (Datasets[lod].path.StartsWith("https://") || Datasets[lod].path.StartsWith("file://"))
            {
                url = Datasets[lod].path;
            }

            url = url.ReplaceXY(tileChange.X, tileChange.Y);
            var webRequest = UnityWebRequestAssetBundle.GetAssetBundle(url);
            tiles[tileKey].runningWebRequest = webRequest;
            yield return webRequest.SendWebRequest();
            tiles[tileKey].runningWebRequest = null;

            if (webRequest.result == UnityWebRequest.Result.ConnectionError || webRequest.result == UnityWebRequest.Result.ProtocolError)
            {
                RemoveGameObjectFromTile(tileKey);
                callback(tileChange);
            }
            else
            {
                AssetBundle assetBundle = DownloadHandlerAssetBundle.GetContent(webRequest);
                tiles[tileKey].assetBundle = assetBundle;
                yield return new WaitUntil(() => pauseLoading == false);
                GameObject newGameobject = CreateNewGameObject(assetBundle, tileChange);
                if (newGameobject != null)
                {
                    if (TileHasHighlight(tileChange))
                    {
                        yield return UpdateObjectIDMapping(tileChange, newGameobject, callback);
                    }
                    else
                    {
                        RemoveGameObjectFromTile(tileKey);
                        tiles[tileKey].gameObject = newGameobject;
                        callback(tileChange);
                    }
                }
                else
                {

                    callback(tileChange);
                }
            }
        }
        public void EnableShadows(bool enabled)
        {
            tileShadowCastingMode = (enabled) ? ShadowCastingMode.On : ShadowCastingMode.Off;

            MeshRenderer[] existingTiles = GetComponentsInChildren<MeshRenderer>();
            foreach (var renderer in existingTiles)
            {
                renderer.shadowCastingMode = tileShadowCastingMode;
            }
        }

        private bool TileHasHighlight(TileChange tileChange)
        {
            Tile tile = tiles[new Vector2Int(tileChange.X, tileChange.Y)];
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

        private IEnumerator UpdateObjectIDMapping(TileChange tileChange, GameObject newGameobject, System.Action<TileChange> callback = null)
        {
            Tile tile = tiles[new Vector2Int(tileChange.X, tileChange.Y)];
            ObjectData oldObjectMapping = tile.gameObject.GetComponent<ObjectData>();
            GameObject newTile = newGameobject;
            string name = newTile.GetComponent<MeshFilter>().mesh.name;
            Debug.Log(name);
            string dataName = name.Replace(" Instance", "");
            dataName = dataName.Replace("mesh", "building");
            dataName = dataName.Replace("-", "_") + "-data";
            string dataURL = Config.activeConfiguration.buildingsMetaDataPath + dataName;
            Debug.Log(dataURL);
            ObjectMappingClass data;
            using (UnityWebRequest uwr = UnityWebRequestAssetBundle.GetAssetBundle(dataURL))
            {
                yield return uwr.SendWebRequest();

                if (uwr.result == UnityWebRequest.Result.ConnectionError || uwr.result == UnityWebRequest.Result.ProtocolError)
                {
                    callback(tileChange);
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
                    objectMapping.mesh = newTile.GetComponent<MeshFilter>().sharedMesh;
                    objectMapping.ApplyDataToIDsTexture();
                    newAssetBundle.Unload(true);
                }
            }
            yield return new WaitUntil(() => pauseLoading == false);
            RemoveGameObjectFromTile(tile.tileKey);
            tiles[tile.tileKey].gameObject = newGameobject;

            yield return null;
            callback(tileChange);

        }

        Mesh[] meshesInAssetbundle = new Mesh[0];
        GameObject container;
        Mesh mesh;
        MeshRenderer meshRenderer;
        Vector2[] uvs;
        Vector2 defaultUV = new Vector2(0.33f, 0.6f);
        private GameObject CreateNewGameObject(AssetBundle assetBundle, TileChange tileChange)
        {
            container = new GameObject();

            container.name = tileChange.X.ToString() + "-" + tileChange.Y.ToString();
            container.transform.parent = transform.gameObject.transform;
            container.layer = container.transform.parent.gameObject.layer;
            container.transform.position = CoordConvert.RDtoUnity(new Vector2(tileChange.X + (tileSize / 2), tileChange.Y + (tileSize / 2)));

            container.SetActive(isEnabled);
            //Mesh[] meshesInAssetbundle = new Mesh[0];
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
            mesh = meshesInAssetbundle[0];
            mesh.RecalculateNormals();
            int count = mesh.vertexCount;

            // creating the UV-s runtime takes a lot of time and causes the garbage-collector to kick in.
            // uv's should be built in in to the meshes in the assetbundles.
            if (addHighlightuvs)
            {
                uvs = new Vector2[count];
                for (int i = 0; i < count; i++)
                {
                    uvs[i] = (defaultUV);
                }
                mesh.uv2 = uvs;
            }

            container.AddComponent<MeshFilter>().sharedMesh = mesh;

            meshRenderer = container.AddComponent<MeshRenderer>();
            meshRenderer.sharedMaterials = DefaultMaterialList.ToArray();
            meshRenderer.shadowCastingMode = tileShadowCastingMode;

            if (createMeshcollider)
            {
                container.AddComponent<MeshCollider>().sharedMesh = mesh;
            }

            assetBundle.Unload(false);
            container.AddComponent<T3D.Uitbouw.TerrainFlattener>(); //todo: remove this when merging back to core branch
            return container;
        }

        public void GetIDData(GameObject obj, int vertexIndex, System.Action<string> callback = null)
        {
            if (!obj) return;

            ObjectData objectMapping = obj.GetComponent<ObjectData>();
            if (!objectMapping || objectMapping.ids.Count == 0)
            {
                //No/empty object data? Download it and return the ID
                StartCoroutine(DownloadObjectData(obj, vertexIndex, callback));
            }
            else
            {
                //Return the ID directly
                int idIndex = objectMapping.vectorMap[vertexIndex];
                var id = objectMapping.ids[idIndex];
                callback?.Invoke(id);
            }
        }

        public void GetAllVerts(List<string> selectedIDs)
        {

        }

        private IEnumerator DownloadObjectData(GameObject obj, int vertexIndex, System.Action<string> callback)
        {
            yield return new WaitUntil(() => pauseLoading == false); //wait for opportunity to start
            pauseLoading = true;
            var meshFilter = obj.GetComponent<MeshFilter>();
            if (!meshFilter) yield break;

            string name = meshFilter.mesh.name;
            string dataName = name.Replace(" Instance", "");
            dataName = dataName.Replace("mesh", "building");
            dataName = dataName.Replace("-", "_") + "-data";
            string dataURL = Config.activeConfiguration.buildingsMetaDataPath + dataName;

            ObjectMappingClass data;
            string id = "null";

            using (UnityWebRequest uwr = UnityWebRequestAssetBundle.GetAssetBundle(dataURL))
            {
                yield return uwr.SendWebRequest();

                if (uwr.result == UnityWebRequest.Result.ConnectionError || uwr.result == UnityWebRequest.Result.ProtocolError)
                {
                    //Not showing warnings for now, because this can occur pretty often. I dialog would be annoying.
                    //ServiceLocator.GetService<WarningDialogs>().ShowNewDialog("De metadata voor " + obj.name + " kon niet worden geladen. Ben je nog online?");
                }
                else if (obj != null)
                {

                    ObjectData objectMapping = obj.AddComponent<ObjectData>();
                    AssetBundle newAssetBundle = DownloadHandlerAssetBundle.GetContent(uwr);
                    data = newAssetBundle.LoadAllAssets<ObjectMappingClass>()[0];
                    int idIndex = data.vectorMap[vertexIndex];
                    id = data.ids[idIndex];
                    objectMapping.ids = data.ids;
                    objectMapping.uvs = data.uvs;
                    objectMapping.vectorMap = data.vectorMap;

                    newAssetBundle.Unload(true);
                }
            }
            callback?.Invoke(id);
            yield return null;
            pauseLoading = false;
        }

        public void Highlight(List<string> ids)
        {
            StartCoroutine(HighlightIDs(ids));
        }

        /// <summary>
        /// Hide mesh parts with the matching object data ID's
        /// </summary>
        /// <param name="ids">List of unique (BAG) id's we want to hide</param>
        public void Hide(List<string> ids)
        {

            StartCoroutine(HideIDs(ids));
        }

        /// <summary>
        /// Adds mesh colliders to the meshes found within this layer
        /// </summary>
        /// <param name="onlyTileUnderPosition">Optional world position where this tile should be close to</param>
        public void AddMeshColliders(Vector3 onlyTileUnderPosition = default)
        {
            MeshCollider meshCollider;
            MeshFilter[] meshFilters = gameObject.GetComponentsInChildren<MeshFilter>();

            if (meshFilters != null)
            {
                if (onlyTileUnderPosition != default)
                {
                    foreach (MeshFilter meshFilter in meshFilters)
                    {
                        if (Mathf.Abs(onlyTileUnderPosition.x - meshFilter.gameObject.transform.position.x) < tileSize && Mathf.Abs(onlyTileUnderPosition.z - meshFilter.gameObject.transform.position.z) < tileSize)
                        {
                            meshCollider = meshFilter.gameObject.GetComponent<MeshCollider>();
                            if (meshCollider == null)
                            {
                                meshFilter.gameObject.AddComponent<MeshCollider>().sharedMesh = meshFilter.sharedMesh;
                            }
                        }
                    }
                    return;
                }

                //Just add all MeshColliders if no specific area was supplied
                foreach (MeshFilter meshFilter in meshFilters)
                {
                    meshCollider = meshFilter.gameObject.GetComponent<MeshCollider>();
                    if (meshCollider == null)
                    {
                        meshFilter.gameObject.AddComponent<MeshCollider>().sharedMesh = meshFilter.sharedMesh;
                    }
                }
            }
        }
        private IEnumerator HighlightIDs(List<string> ids)
        {
            pauseLoading = true;
            ObjectData objectData;

            foreach (KeyValuePair<Vector2Int, Tile> kvp in tiles)
            {
                if (kvp.Value.gameObject == null)
                {
                    continue;
                }
                objectData = kvp.Value.gameObject.GetComponent<ObjectData>();
                if (objectData != null)
                {
                    if (ids.Count > 0)
                    {
                        objectData.highlightIDs = ids.Where(targetID => objectData.ids.Any(objectId => objectId == targetID)).ToList<string>();
                    }
                    else
                    {
                        objectData.highlightIDs.Clear();
                    }
                    objectData.ApplyDataToIDsTexture();
                }
            }
            pauseLoading = false;
            yield return null;
        }

        private IEnumerator HideIDs(List<string> ids)
        {
            pauseLoading = true;
            ObjectData objectData;
            foreach (KeyValuePair<Vector2Int, Tile> kvp in tiles)
            {
                if (kvp.Value.gameObject == null)
                {
                    continue;
                }
                objectData = kvp.Value.gameObject.GetComponent<ObjectData>();
                if (objectData != null)
                {
                    if (ids.Count > 0)
                    {
                        objectData.hideIDs.AddRange(ids.Where(targetID => objectData.ids.Any(objectId => objectId == targetID)).ToList<string>());
                    }
                    else
                    {
                        objectData.hideIDs.Clear();
                    }
                    objectData.ApplyDataToIDsTexture();
                }
            }
            pauseLoading = false;
            yield return null;
        }

    }
}
