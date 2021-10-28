using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Netherlands3D.T3D.Uitbouw
{
    public enum WallSide
    {
        Left,
        Right,
        Top,
        Bottom,
        Front,
        Back,
    }

    public class UitbouwMuur : SquarePolygon
    {
        [SerializeField]
        private WallSide side;
        public WallSide Side => side;

        bool isActive = false;

        [SerializeField]
        private UitbouwMuur left;
        [SerializeField]
        private UitbouwMuur right;
        [SerializeField]
        private UitbouwMuur top;
        [SerializeField]
        private UitbouwMuur bottom;

        private Vector3 oldPosition;
        public Vector3 deltaPosition { get; private set; }

        private MeshFilter meshFilter;
        public MeshFilter MeshFilter => meshFilter;

        //private void OnValidate()
        //{
        //    leftBound = left.transform;
        //    rightBound = right.transform;
        //    topBound = top.transform;
        //    bottomBound = bottom.transform;
        //}

        //private Vector3 GetCorner(UitbouwMuur h, UitbouwMuur v)
        //{
        //    var plane = new Plane(-transform.forward, transform.position);

        //    var projectedHPoint = plane.ClosestPointOnPlane(h.transform.position);
        //    var projectedVPoint = plane.ClosestPointOnPlane(v.transform.position);

        //    float hDist = Vector3.Distance(transform.position, projectedHPoint);
        //    float vDist = Vector3.Distance(transform.position, projectedVPoint);

        //    return transform.position - h.transform.forward * hDist - v.transform.forward * vDist;
        //}

        //private void Start()
        //{
        //    var poly = Polygon;
        //    for (int i = 0; i < poly.Length; i++)
        //    {
        //        var a = new GameObject();
        //        a.transform.position = poly[i];
        //    }
        //}

        private void Awake()
        {
            meshFilter = GetComponent<MeshFilter>();
            oldPosition = transform.position;

            left = leftBound.GetComponent<UitbouwMuur>();
            right = rightBound.GetComponent<UitbouwMuur>();
            top = topBound.GetComponent<UitbouwMuur>();
            bottom = bottomBound.GetComponent<UitbouwMuur>();
        }

        private static Vector3 CalculateXYScale(Transform left, Transform right, Transform top, Transform bottom)
        {
            float hDist = Vector3.Distance(left.position, right.position);
            float vDist = Vector3.Distance(top.position, bottom.position);

            return new Vector3(hDist, vDist, 1);
        }

        public void RecalculateSides(Vector3 newPosition)
        {
            deltaPosition = newPosition - oldPosition;
            oldPosition = transform.position;
            transform.position = newPosition;

            //back.RecalculatePosition(deltaPosition/2);
            //RecalculatePosition(deltaPosition/2);

            left.RecalculatePosition(deltaPosition / 2);
            right.RecalculatePosition(deltaPosition / 2);
            top.RecalculatePosition(deltaPosition / 2);
            bottom.RecalculatePosition(deltaPosition / 2);

            left.RecalculateScale();
            right.RecalculateScale();
            top.RecalculateScale();
            bottom.RecalculateScale();
        }

        public void RecalculatePosition(Vector3 delta)
        {
            transform.position += delta;
        }

        public void RecalculateScale()
        {
            transform.localScale = CalculateXYScale(left.transform, right.transform, top.transform, bottom.transform);
        }

        public void SetActive(bool active)
        {
            oldPosition = transform.position;
            deltaPosition = Vector3.zero;
            isActive = active;
        }

        public void MoveWall(float delta)
        {
            SetActive(true);
            var newPosition = transform.position + transform.forward * -delta;
            //transform.position += transform.forward * -delta;
            RecalculateSides(newPosition);
            SetActive(false);
        }
    }
}