using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ConvertCoordinates;

namespace Netherlands3D.LayerSystem
{
    abstract public class Layer : MonoBehaviour
    {
        [SerializeField]
        private bool isenabled = true;
        public bool isEnabled {
            get
            {
                return isenabled;
            }
            set
            {
                isenabled = value;
                Debug.Log(transform.childCount);
                for (int i = 0; i < transform.childCount; i++)
                {
                    transform.GetChild(i).gameObject.SetActive(isenabled);
                }
                OnDisableTiles(isenabled);
            }
        }
        public int tileSize = 1000;
        public int layerPriority = 0;
        public List<DataSet> Datasets = new List<DataSet>();
        public Dictionary<Vector2Int, Tile> tiles = new Dictionary<Vector2Int, Tile>();
        public bool pauseLoading = false;

        public abstract void OnDisableTiles(bool isenabled);
        public abstract void HandleTile(TileChange tileChange, System.Action<TileChange> callback = null);

        public void Start()
        {
            foreach (DataSet dataset in Datasets)
            {
                dataset.maximumDistanceSquared = dataset.maximumDistance * dataset.maximumDistance;
            }
        }
    }
}
