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

        public void Highlight(string id)
        {
            StartCoroutine(PrivateHighlight(id));
        }
        public void Highlight(List<string> ids)
        {
            StartCoroutine(HighlightIDsOneTilePerFrame(ids));
        }

        public void Hide(string id) 
        {
            StartCoroutine(PrivateHide(id));
        }

        public void Hide(List<string> ids) 
        {
            StartCoroutine(PrivateHide(ids));
        }

        public void UnhideAll() 
        {
            StartCoroutine(PrivateHide("null"));
        }

        public void AddMeshColliders() 
        {
            MeshCollider meshCollider;
            MeshFilter[] meshFilters = gameObject.GetComponentsInChildren<MeshFilter>();
            if (meshFilters == null)
            {
                return;
            }
            foreach (MeshFilter meshFilter in meshFilters)
            {
                meshCollider = meshFilter.gameObject.GetComponent<MeshCollider>();
                if (meshCollider == null)
                {
                    meshFilter.gameObject.AddComponent<MeshCollider>().sharedMesh = meshFilter.sharedMesh;
                }
            }
        }

        private IEnumerator PrivateHide(List<string> id)
        {
            tileHandler.pauseLoading = true;
            ObjectData objectdata;
            Vector2[] UVs;
            foreach (KeyValuePair<Vector2Int, Tile> kvp in tiles)
            {
                objectdata = kvp.Value.gameObject.GetComponent<ObjectData>();
                if (objectdata != null)
                {
                    objectdata.hideIDs.AddRange(id);
                    objectdata.mesh = objectdata.gameObject.GetComponent<MeshFilter>().mesh;
                    objectdata.SetHideUVs();
                //objectdata.gameObject.GetComponent<MeshFilter>().mesh.uv2 = UVs;
                yield return null;
                }
            }
            tileHandler.pauseLoading = false;
        }


        private IEnumerator PrivateHide(string id) 
        {
            tileHandler.pauseLoading = true;
            ObjectData objectdata;
            Vector2[] UVs;
            foreach (KeyValuePair<Vector2Int, Tile> kvp in tiles)
            {
                objectdata = kvp.Value.gameObject.GetComponent<ObjectData>();
                if (objectdata != null)
                {
                    if (objectdata.ids.Contains(id) == false)
                    {
                        if (objectdata.hideIDs.Count == 0)
                        {
                            continue;
                        }
                    }

                    if (id == "null")
                    {
                        objectdata.hideIDs.Clear();
                        objectdata.SetUVs();
                    }
                    else
                    {
                        objectdata.hideIDs.Add(id);
                        objectdata.mesh = objectdata.gameObject.GetComponent<MeshFilter>().mesh;
                        objectdata.SetHideUVs();
                    }
                    //objectdata.gameObject.GetComponent<MeshFilter>().mesh.uv2 = UVs;
                    yield return null;
                }
            }
            tileHandler.pauseLoading = false;
        }

        private IEnumerator PrivateHighlight(string id)
        {
            tileHandler.pauseLoading = true;
            ObjectData objectData;
            Vector2[] UVs;
            foreach (KeyValuePair<Vector2Int, Tile> kvp in tiles)
            {
                objectData = kvp.Value.gameObject.GetComponent<ObjectData>();
                if (objectData != null)
                {
                    if (objectData.ids.Contains(id)==false)
                    {
                        if (objectData.highlightIDs.Count == 0)
                        {
                            continue;
                        }
                    }
                    if (id == "null")
                    {
                        objectData.SetUVs();
                    }
                    else
                    {
                        objectData.highlightIDs.Add(id);
                        objectData.mesh = objectData.gameObject.GetComponent<MeshFilter>().mesh;
                        objectData.SetUVs();
                    }
                    yield return null;
                }
                yield return new WaitForEndOfFrame();
            }
            tileHandler.pauseLoading = false;   
        }

        private IEnumerator HighlightIDsOneTilePerFrame(List<string> ids)
        {
            tileHandler.pauseLoading = true;
            ObjectData objectData;
            Vector2[] UVs;
            foreach (KeyValuePair<Vector2Int, Tile> kvp in tiles)
            {
                objectData = kvp.Value.gameObject.GetComponent<ObjectData>();
                if (objectData != null)
                {
                    objectData.highlightIDs = ids.Where(targetID => objectData.ids.Any(objectId => objectId == targetID)).ToList<string>();
                    objectData.mesh = objectData.gameObject.GetComponent<MeshFilter>().mesh;
                    objectData.SetUVs();
                }
                yield return new WaitForEndOfFrame();
            }
            tileHandler.pauseLoading = false;
        }
    }
}
