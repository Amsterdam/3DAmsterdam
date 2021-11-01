using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Netherlands3D.T3D.Uitbouw
{
    public class SquarePolygon : CityPolygon
    {
        [SerializeField]
        protected Transform leftBound;
        public Vector3 LeftBound => leftBound.position;
        [SerializeField]
        protected Transform rightBound;
        public Vector3 RightBound => rightBound.position;
        [SerializeField]
        protected Transform topBound;
        public Vector3 TopBound => topBound.position;
        [SerializeField]
        protected Transform bottomBound;
        public Vector3 BottomBound => bottomBound.position;

        public override Vector3[] Polygon
        {
            get
            {
                return new Vector3[] {

                GetCorner(leftBound, topBound),
                GetCorner(rightBound, topBound),
                GetCorner(rightBound, bottomBound),
                GetCorner(leftBound, bottomBound),
            };
            }
        }

        public Vector2 Size { get; private set; }

        private void Start()
        {
            RecalculateScale();
        }

        public void SetSize(Vector2 size)
        {
            Size = size;
            leftBound.localPosition = new Vector3(-size.x / 2, 0, 0);
            rightBound.localPosition = new Vector3(size.x / 2, 0, 0);
            topBound.localPosition = new Vector3(0, size.y / 2, 0);
            bottomBound.localPosition = new Vector3(0, -size.y / 2, 0);

            RecalculateScale();
        }

        public void RecalculateScale()
        {
            Size = CalculateXYScale(leftBound, rightBound, topBound, bottomBound);
            transform.localScale = Size;
        }

        protected static Vector3 CalculateXYScale(Transform left, Transform right, Transform top, Transform bottom)
        {
            float hDist = Vector3.Distance(left.position, right.position);
            float vDist = Vector3.Distance(top.position, bottom.position);

            return new Vector3(hDist, vDist, 1);
        }

        protected Vector3 GetCorner(Transform hBound, Transform vBound)
        {
            var plane = new Plane(-transform.forward, transform.position);

            var projectedHPoint = plane.ClosestPointOnPlane(hBound.position);
            var projectedVPoint = plane.ClosestPointOnPlane(vBound.position);

            float hDist = Vector3.Distance(transform.position, projectedHPoint);
            float vDist = Vector3.Distance(transform.position, projectedVPoint);

            var hDir = (projectedHPoint - transform.position).normalized;
            var vDir = (projectedVPoint - transform.position).normalized;

            return transform.position + hDir * hDist + vDir * vDist;
        }

        //private void OnDrawGizmos()
        //{
        //    foreach (var corner in Polygon)
        //    {
        //        Gizmos.DrawSphere(corner, 0.1f);
        //    }
        //}
    }
}
