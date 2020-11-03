using UnityEngine;
using System.Collections;
using UnityEngine.Events;
using System.Collections.Generic;

namespace Amsterdam3D.SelectionTools
{
    public abstract class SelectionTool : MonoBehaviour
    {


        public GameObject Canvas;
        public UnityEvent onSelectionCompleted;
        public ToolType toolType { get; protected set; }
        public List<Vector3> vertexes = new List<Vector3>();

        // Use this for initialization
        public abstract void EnableTool();
        public abstract void DisableTool();
        // source: https://wiki.unity3d.com/index.php/PolyContainsPoint
        public bool ContainsPoint(Vector3 p)
        {
            var j = vertexes.Count - 1;
            var inside = false;
            for (int i = 0; i < vertexes.Count; j = i++)
            {
                var pi = vertexes[i];
                var pj = vertexes[j];
                if (((pi.z <= p.z && p.z < pj.z) || (pj.z <= p.z && p.z < pi.z)) &&
                    (p.x < (pj.x - pi.x) * (p.z - pi.z) / (pj.z - pi.z) + pi.x))
                    inside = !inside;
            }
            return inside;
        }
    }
}