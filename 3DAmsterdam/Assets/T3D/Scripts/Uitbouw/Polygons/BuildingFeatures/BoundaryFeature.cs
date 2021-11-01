using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Netherlands3D.T3D.Uitbouw
{
    public class BoundaryFeature : SquarePolygon
    {
        public UitbouwMuur Wall { get; private set; }
        public Transform featureTransform { get; private set; }

        private void Awake()
        {
            featureTransform = transform.parent;
        }

        public void SetWall(UitbouwMuur wall)
        {
            this.Wall = wall;
            featureTransform.position = wall.transform.position;
            featureTransform.forward = wall.transform.forward;
        }
    }
}