using System.Collections;
using System.Collections.Generic;
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

        void Start()
        {
            foreach (DataSet dataset in Datasets)
            {
                dataset.maximumDistanceSquared = dataset.maximumDistance * dataset.maximumDistance;
            }
        }

        public void UnHighlightAll()
        {
            StartCoroutine(PrivateHighlight("null"));
        }

        public void Highlight(string id)
        {
            StartCoroutine(PrivateHighlight(id));
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


        public void LoadMeshColliders(System.Action<bool> callback) 
        {
            StartCoroutine(LoadMeshCollidersRoutine(callback));
        }
        
        private IEnumerator LoadMeshCollidersRoutine(System.Action<bool> callback)
        {
            MeshCollider meshCollider;
            MeshFilter[] meshFilters = gameObject.GetComponentsInChildren<MeshFilter>();
            if (meshFilters == null)
            {
                callback(true);
                yield break;
            }
            foreach (MeshFilter meshFilter in meshFilters)
            {
                meshCollider = meshFilter.gameObject.GetComponent<MeshCollider>();
                if (meshCollider == null)
                {
                    meshFilter.gameObject.AddComponent<MeshCollider>().sharedMesh = meshFilter.sharedMesh;
                }
            }
            callback(true);
            Debug.Log("MeshColliders attached");
        }


        private IEnumerator PrivateHide(List<string> id)
        {
            transform.GetComponentInParent<TileHandler>().pauseLoading = true;
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
            transform.GetComponentInParent<TileHandler>().pauseLoading = false;
        }


        private IEnumerator PrivateHide(string id) 
        {
            transform.GetComponentInParent<TileHandler>().pauseLoading = true;
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
            transform.GetComponentInParent<TileHandler>().pauseLoading = false;
        }

        private IEnumerator PrivateHighlight(string id)
        {
            transform.GetComponentInParent<TileHandler>().pauseLoading = true;
            ObjectData objectdata;
            Vector2[] UVs;
            foreach (KeyValuePair<Vector2Int, Tile> kvp in tiles)
            {
                objectdata = kvp.Value.gameObject.GetComponent<ObjectData>();
                if (objectdata != null)
                {
                    if (objectdata.ids.Contains(id)==false)
                    {
                        if (objectdata.highlightIDs.Count == 0)
                        {
                            continue;
                        }
                    }
                    
                    objectdata.highlightIDs.Clear();
                    if (id == "null")
                    {
                        objectdata.SetUVs();
                    }
                    else
                    {
                        objectdata.highlightIDs.Add(id);
                        objectdata.hideIDs.Remove(id);
                        objectdata.mesh = objectdata.gameObject.GetComponent<MeshFilter>().mesh;
                        objectdata.SetUVs();
                    }
                    //objectdata.gameObject.GetComponent<MeshFilter>().mesh.uv2 = UVs;
                    yield return null;
                }
            }
            transform.GetComponentInParent<TileHandler>().pauseLoading = false;
            
        }
    }
}
