using ConvertCoordinates;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace LayerSystem
{
    public class Layer : MonoBehaviour
    {
        [SerializeField]
        public Material DefaultMaterial;
        public Material HighlightMaterial;
        public int tileSize = 1000;
        public int layerPriority = 0;
        public List<DataSet> Datasets = new List<DataSet>();
        public Dictionary<Vector2Int, Tile> tiles = new Dictionary<Vector2Int, Tile>();

        private TileHandler tileHandler;

        void Start()
        {
            tileHandler = GetComponentInParent<TileHandler>();

            foreach (DataSet dataset in Datasets)
            {
                dataset.maximumDistanceSquared = dataset.maximumDistance * dataset.maximumDistance;
            }
        }

        /// <summary>
        /// Check object data of our tiles one frame at a time, and highlight matching ID's
        /// </summary>
        /// <param name="ids">List of unique (BAG) id's we want to highlight</param>
        public void Highlight(List<string> ids)
        {
            StopAllCoroutines();
            StartCoroutine(HighlightIDs(ids));
        }

        /// <summary>
        /// Hide mesh parts with the matching object data ID's
        /// </summary>
        /// <param name="ids">List of unique (BAG) id's we want to hide</param>
        public void Hide(List<string> ids) 
        {
            StopAllCoroutines();
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
                        if (Mathf.Abs(onlyTileUnderPosition.x - meshFilter.gameObject.transform.position.x) < Constants.TILE_SIZE * 0.5f && Mathf.Abs(onlyTileUnderPosition.z - meshFilter.gameObject.transform.position.z) < Constants.TILE_SIZE * 0.5f)
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
            tileHandler.pauseLoading = true;
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
                    objectData.mesh = objectData.gameObject.GetComponent<MeshFilter>().mesh;
                    objectData.UpdateUVs();
                }
            }
            tileHandler.pauseLoading = false;
            yield return null;
        }

        private IEnumerator HideIDs(List<string> ids)
        {
            tileHandler.pauseLoading = true;
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
                    else{
                        objectData.hideIDs.Clear();
                    }
                    objectData.mesh = objectData.gameObject.GetComponent<MeshFilter>().mesh;
                    objectData.UpdateUVs();
                }
            }
            tileHandler.pauseLoading = false;
            yield return null;
        }
    }
}
