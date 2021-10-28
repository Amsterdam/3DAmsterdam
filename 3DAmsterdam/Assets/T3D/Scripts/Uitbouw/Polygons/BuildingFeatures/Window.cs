using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Netherlands3D.T3D.Uitbouw
{
    public class Window : SquarePolygon
    {
        private UitbouwMuur wall;

        void SetWall(UitbouwMuur wall)
        {
            this.wall = wall;
            transform.parent.forward = wall.transform.forward;
        }

        public void SetSize(Vector2 size)
        {
            leftBound.localPosition = new Vector3(-size.x / 2, 0, 0);
            rightBound.localPosition = new Vector3(size.x / 2, 0, 0);
            topBound.localPosition = new Vector3(0, size.y / 2, 0);
            bottomBound.localPosition = new Vector3(0, -size.y / 2, 0);

            RecalculateScale();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.K))
            {
                SetSize(new Vector2(3, 3));
            }
        }
    }
}