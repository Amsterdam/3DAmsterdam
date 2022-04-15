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

    //[RequireComponent(typeof(CitySurface))]
    public class UitbouwMuur : SquareSurface
    {
        [SerializeField]
        private WallSide side;
        public WallSide Side => side;

        private UitbouwMuur left;
        private UitbouwMuur right;
        private UitbouwMuur top;
        private UitbouwMuur bottom;

        public override Vector3 LeftBoundPosition => WallPlane.ClosestPointOnPlane(leftBound.position);
        public override Vector3 RightBoundPosition => WallPlane.ClosestPointOnPlane(rightBound.position);
        public override Vector3 TopBoundPosition => WallPlane.ClosestPointOnPlane(topBound.position);
        public override Vector3 BottomBoundPosition => WallPlane.ClosestPointOnPlane(bottomBound.position);

        private Vector3 oldPosition;
        public Vector3 deltaPosition { get; private set; }

        private MeshFilter meshFilter;
        public MeshFilter MeshFilter => meshFilter;

        public Plane WallPlane => new Plane(-transform.forward, transform.position);

        [SerializeField]
        private Material normalMaterial, highlightMaterial;

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

        protected override void Awake()
        {
            base.Awake();

            meshFilter = GetComponent<MeshFilter>();
            oldPosition = transform.position;

            left = leftBound.GetComponent<UitbouwMuur>();
            right = rightBound.GetComponent<UitbouwMuur>();
            top = topBound.GetComponent<UitbouwMuur>();
            bottom = bottomBound.GetComponent<UitbouwMuur>();
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

        public void SetHighlightActive(bool enable)
        {
            GetComponent<MeshRenderer>().material = enable ? highlightMaterial : normalMaterial;
        }

        public void RecalculatePosition(Vector3 delta)
        {
            transform.position += delta;
        }

        //public void RecalculateScale()
        //{
        //    transform.localScale = CalculateXYScale(leftBound, rightBound, topBound, bottomBound);
        //}

        public void SetActive(bool active)
        {
            oldPosition = transform.position;
            deltaPosition = Vector3.zero;
            //isActive = active;
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