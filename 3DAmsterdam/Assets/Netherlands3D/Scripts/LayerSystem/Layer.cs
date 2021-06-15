using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ConvertCoordinates;
using UnityEngine.Events;

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
				LayerToggled();
            }
		}

		public int tileSize = 1000;
        public int layerPriority = 0;
        public List<DataSet> Datasets = new List<DataSet>();
        public Dictionary<Vector2Int, Tile> tiles = new Dictionary<Vector2Int, Tile>();
        public bool pauseLoading = false;

        public abstract void HandleTile(TileChange tileChange, System.Action<TileChange> callback = null);

        [HideInInspector]
        public UnityEvent onLayerEnabled;
        [HideInInspector]
        public UnityEvent onLayerDisabled;

        public void Start()
        {
            foreach (DataSet dataset in Datasets)
            {
                dataset.maximumDistanceSquared = dataset.maximumDistance * dataset.maximumDistance;
            }
        }

        private void LayerToggled()
        {
            //Invoke enabled/disabled event
            if (isEnabled)
            {
                onLayerEnabled.Invoke();
            }
            else
            {
                onLayerDisabled.Invoke();
            }

            //Activate children accordingly
            for (int i = 0; i < transform.childCount; i++)
            {
                transform.GetChild(i).gameObject.SetActive(isenabled);
            }
        }

        public virtual void InteruptRunningProcesses(Vector2Int tileKey)
		{
			if (!tiles.ContainsKey(tileKey)) return;

            if (tiles[tileKey].assetBundle) tiles[tileKey].assetBundle.Unload(true);

			if (tiles[tileKey].runningWebRequest != null) tiles[tileKey].runningWebRequest.Abort();
			if (tiles[tileKey].runningCoroutine != null) StopCoroutine(tiles[tileKey].runningCoroutine);
		}
    }
}
