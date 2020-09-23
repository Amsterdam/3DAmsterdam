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
                        //Vector2 uv = new Vector2(0.66f, 0.5f);
                        //int count = objectdata.gameObject.GetComponent<MeshFilter>().mesh.vertexCount;
                        //UVs = new Vector2[count];

                        //for (int i = 0; i < count; i++)
                        //{
                        //    UVs[i] = uv;
                        //}
                        objectdata.gameObject.GetComponent<MeshFilter>().mesh.uv2 = null;
                    }
                    else
                    {
                        objectdata.highlightIDs.Add(id);
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
