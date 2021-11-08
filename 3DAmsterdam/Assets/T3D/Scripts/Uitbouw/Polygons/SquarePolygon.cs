using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Netherlands3D.T3D.Uitbouw
{
    public class SquarePolygon : CityPolygon
    {
        [SerializeField]
        protected Transform meshTransform;
        [SerializeField]
        protected Transform leftBound;
        public virtual Vector3 LeftBoundPosition => leftBound.position;
        [SerializeField]
        protected Transform rightBound;
        public virtual Vector3 RightBoundPosition => rightBound.position;
        [SerializeField]
        protected Transform topBound;
        public virtual Vector3 TopBoundPosition => topBound.position;
        [SerializeField]
        protected Transform bottomBound;
        public virtual Vector3 BottomBoundPosition => bottomBound.position;

        public override Vector3[] Boundaries
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

        protected virtual void Awake()
        {
            if (!meshTransform)
                meshTransform = transform;
        }

        protected virtual void Start()
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
            var newScale = CalculateXYScale(leftBound, rightBound, topBound, bottomBound);
            meshTransform.localScale = newScale;
            Size = newScale;
        }

        protected static Vector3 CalculateXYScale(Transform left, Transform right, Transform top, Transform bottom)
        {
            float hDist = Vector3.Distance(left.position, right.position);
            float vDist = Vector3.Distance(top.position, bottom.position);

            return new Vector3(hDist, vDist, 1);
        }

        protected Vector3 GetCorner(Transform hBound, Transform vBound)
        {
            var plane = new Plane(-meshTransform.forward, meshTransform.position);

            var projectedHPoint = plane.ClosestPointOnPlane(hBound.position);
            var projectedVPoint = plane.ClosestPointOnPlane(vBound.position);

            float hDist = Vector3.Distance(meshTransform.position, projectedHPoint);
            float vDist = Vector3.Distance(meshTransform.position, projectedVPoint);

            var hDir = (projectedHPoint - meshTransform.position).normalized;
            var vDir = (projectedVPoint - meshTransform.position).normalized;

            return meshTransform.position + hDir * hDist + vDir * vDist;
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
