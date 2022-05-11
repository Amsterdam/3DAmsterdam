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

        private Material normalMaterial;
        [SerializeField]
        private Material highlightMaterial;

        private static float textureScale = 0.3f;

        protected override void Awake()
        {
            base.Awake();

            meshFilter = GetComponent<MeshFilter>();
            oldPosition = transform.position;

            left = leftBound.GetComponent<UitbouwMuur>();
            right = rightBound.GetComponent<UitbouwMuur>();
            top = topBound.GetComponent<UitbouwMuur>();
            bottom = bottomBound.GetComponent<UitbouwMuur>();

            normalMaterial = GetComponent<MeshRenderer>().material;
        }

        public void RecalculateSides(Vector3 newPosition)
        {
            deltaPosition = newPosition - oldPosition;
            oldPosition = transform.position;
            transform.position = newPosition;

            left.RecalculatePosition(deltaPosition / 2);
            right.RecalculatePosition(deltaPosition / 2);
            top.RecalculatePosition(deltaPosition / 2);
            bottom.RecalculatePosition(deltaPosition / 2);

            left.RecalculateScale();
            right.RecalculateScale();
            top.RecalculateScale();
            bottom.RecalculateScale();

            left.RecalculateMaterialTiling();
            right.RecalculateMaterialTiling();
            top.RecalculateMaterialTiling();
            bottom.RecalculateMaterialTiling();
        }

        public void RecalculateMaterialTiling()
        {
            normalMaterial.mainTextureScale = Size * textureScale;
        }

        public void SetHighlightActive(bool enable)
        {
            GetComponent<MeshRenderer>().material = enable ? highlightMaterial : normalMaterial;
        }

        public void RecalculatePosition(Vector3 delta)
        {
            transform.position += delta;
        }

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