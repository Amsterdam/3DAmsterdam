using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;
using Assets.Amsterdam3D.Scripts.Camera;

namespace Assets.Amsterdam3D.Scripts.SelectionTools
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


        [SerializeField]
        RayCastBehaviour rayCast;

        GameObject selectionRange;
        
        
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
            rayCast = FindObjectOfType<RayCastBehaviour>();
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
        inSelection = true; 
        }

        // Update is called once per frame
        void Update()
        {

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