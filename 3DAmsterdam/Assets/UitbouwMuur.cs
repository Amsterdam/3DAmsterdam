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

    public class UitbouwMuur : MonoBehaviour
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

        private void Awake()
        {
            oldPosition = transform.position;
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

        internal void MoveWall(float delta)
        {
            SetActive(true);
            var newPosition = transform.position + transform.forward * -delta;
            //transform.position += transform.forward * -delta;
            RecalculateSides(newPosition);
            SetActive(false);
        }
    }
}