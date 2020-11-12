using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;
using LayerSystem;
using Amsterdam3D.CameraMotion;

namespace Amsterdam3D.Interface
{
    // currently works as MVP, still has a bunch of TODOs for better usage.

    //TODO: Move Single click tool to this class, or to a selection tool?
    public class SelectionTools : MonoBehaviour
    {
        [SerializeField]
        private GameObject canvas;
        [SerializeField]
        private SelectionTool selectionTool;

        [SerializeField]
        private Bounds bounds;
        private List<Vector3> vertices;

        [SerializeField]
        private TileHandler tileHandler;

        [SerializeField]
        private LayerMask buildingLayer;


        [SerializeField]
        private Layer layer;

        private Vector3 groundLevel = new Vector3(0, Constants.ZERO_GROUND_LEVEL_Y, 0);

        private void Start()
        {
            tileHandler = FindObjectOfType<TileHandler>();

            selectionTool.canvas = canvas;
            selectionTool.onSelectionCompleted.AddListener(OnSelectionFunction);
            selectionTool.EnableTool();
        }
        public Bounds GetBounds() 
        {
            return bounds;
        }
        // NOTE: Only checks for X and Z positions, Y isn't taken into account
        public bool Contains(Vector3 position) 
        {
            return selectionTool.ContainsPoint(position);
        }

        public ToolType GetCurrentToolType() 
        {
            return selectionTool.toolType;
        }

        public List<Vector3> GetVertices() 
        {
            // copy selection and return copy
            List<Vector3> returnValue = new List<Vector3>();
            returnValue.AddRange(selectionTool.vertices);
            return returnValue;
        }

        private void OnSelectionFunction()
		{
			//Hard coded for now, should be calculated later based on what type of selection tool etc?
			var min = selectionTool.vertices[0];
			var max = selectionTool.vertices[2];
			Vector3 center = (min + max) / 2;
			Vector3 extends = max - min;

			layer.AddMeshColliders();
			StartCoroutine(BoxCastToFindTilesInRange(center, extends));
		}

		private IEnumerator BoxCastToFindTilesInRange(Vector3 center, Vector3 extends)
		{
            //We wait one frame to make sure the colliders are there.
            yield return new WaitForEndOfFrame();

			var hits = Physics.BoxCastAll(center + groundLevel, extends, -Vector3.up, Quaternion.Euler(Vector3.zero), (center.y + Constants.ZERO_GROUND_LEVEL_Y), buildingLayer);
			foreach (var hit in hits)
			{
				tileHandler.GetIDData(hit.collider.gameObject, hit.triangleIndex * 3);
			}
		}
    }

    public enum ToolType 
    {
        Invalid,
        Box,
        Polygon,
        Circle
    }
}