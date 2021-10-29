using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Netherlands3D.T3D.Uitbouw
{
    public class Window : SquarePolygon
    {
        private UitbouwMuur wall;
        private Transform windowTransform;

        private void Awake()
        {
            windowTransform = transform.parent;
        }

        public void SetWall(UitbouwMuur wall)
        {
            this.wall = wall;
            windowTransform.position = wall.transform.position;
            windowTransform.forward = wall.transform.forward;
        }
    }
}