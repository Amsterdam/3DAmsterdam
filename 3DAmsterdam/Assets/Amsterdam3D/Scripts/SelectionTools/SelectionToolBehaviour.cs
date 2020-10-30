using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;
using Assets.Amsterdam3D.Scripts.Camera;

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

        public bool inSelection;
        
        
        // Gets 
        public Bounds GetBounds() 
        {
            return bounds;
        }

        public ToolType GetCurrentToolType() 
        {
            return tool.toolType;
        }

        public List<Vector3> GetVertexes() 
        {
            // copy selection and return copy
            List<Vector3> returnValue = new List<Vector3>();
            returnValue.AddRange(vertices);
            return returnValue;
        }
        

        private void OnEnable()
        {
            tool.Canvas = canvas;
            tool.onSelectionCompleted.AddListener(onSelectionFunction);
            tool.EnableTool();
        }

        private void OnDisable()
        {
         tool.DisableTool();
        }
        private void onSelectionFunction() 
        {
         bounds = tool.bounds;
        inSelection = true; 
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