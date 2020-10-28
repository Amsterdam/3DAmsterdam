using UnityEngine;
using System.Collections;
using UnityEngine.Events;
using System.Collections.Generic;

namespace Assets.Amsterdam3D.Scripts.SelectionTools
{
    public abstract class SelectionTool : ScriptableObject
    {


        public GameObject Canvas;
        public UnityEvent onSelectionCompleted;
        public ToolType toolType { get; protected set; }
        public Bounds bounds = new Bounds();
        public List<Vector3> vertexes = new List<Vector3>();

        // Use this for initialization
        public abstract void EnableTool();
        public abstract void DisableTool();


        // Update is called once per frame
        public abstract void Update();
    }
}