using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;
using Assets.Amsterdam3D.Scripts.Camera;
using LayerSystem;
using Amsterdam3D.CameraMotion;
namespace Amsterdam3D.SelectionTools
{
    // currently works as MVP, still has a bunch of TODOs for better usage.

    //TODO: Move Single click tool to this class, or to a selection tool?
    public class SelectionToolBehaviour : MonoBehaviour
    {

        // Use this for initialization

        [SerializeField]
        private GameObject canvas;
        [SerializeField]
        private SelectionTool tool;

        [SerializeField]
        private Bounds bounds;
        private List<Vector3> vertices;

        [SerializeField]
        private TileHandler tileHandler;

        public bool inSelection;

        [SerializeField]
        private LayerMask buildingLayer;


        [SerializeField]
        private Layer layer;

        private bool collidersLoaded;

        private Vector3 GroundLevel = new Vector3(0, Constants.ZERO_GROUND_LEVEL_Y, 0);

        private void Start()
        {
            tileHandler = FindObjectOfType<TileHandler>();
        }
        public Bounds GetBounds() 
        {
            return bounds;
        }
        // NOTE: Only checks for X and Z positions, Y isn't taken into account
        public bool Contains(Vector3 position) 
        {
            return tool.ContainsPoint(position);
        }

        public ToolType GetCurrentToolType() 
        {
            return tool.toolType;
        }

        public List<Vector3> GetVertices() 
        {
            // copy selection and return copy
            List<Vector3> returnValue = new List<Vector3>();
            returnValue.AddRange(tool.vertices);
            return returnValue;
        }


        private void OnEnable()
        {
            tool.Canvas = canvas;
            tool.onSelectionCompleted.AddListener(OnSelectionFunction);
            tool.OnDeselect.AddListener(OnDeselect);
            tool.EnableTool();
        }

        private void OnDisable()
        {
            tool.DisableTool();
        }

        private void OnSelectionFunction() 
        {
            //Hard coded for now, should be calculated later based on what type of selection tool etc?
            var min = tool.vertices[0];
            var max = tool.vertices[2];
            Vector3 center = (min + max) / 2;
            Vector3 extends = max - min;

            layer.AddMeshColliders();
            var hits = Physics.BoxCastAll(center + GroundLevel, extends,  -Vector3.up, Quaternion.Euler(Vector3.zero), (center.y + Constants.ZERO_GROUND_LEVEL_Y), buildingLayer);
            foreach (var hit in hits)
            {
                tileHandler.GetIDData(hit.collider.gameObject, hit.triangleIndex * 3);
            }
            inSelection = true; 
        }

        private void OnDeselect() 
        {
            inSelection = false;
        }
    }

    public enum ToolType 
    {
        Invaild,
        Box,
        Polygon,
        Circle
    }
}