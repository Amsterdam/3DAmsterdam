using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Netherlands3D.T3D.Uitbouw
{
    [RequireComponent(typeof(UitbouwBase))]
    public class UitbouwMovement : MonoBehaviour
    {
        private UitbouwBase uitbouw;
        public bool AllowDrag { get; private set; } = true;

        //[SerializeField]
        //private GameObject dragableAxisPrefab;

        private void Awake()
        {
            uitbouw = GetComponent<UitbouwBase>();
        }

        private void Start()
        {
            AllowDrag = AllowDrag && T3DInit.Instance.IsEditMode;

            SetAllowMovement(AllowDrag);
        }

        //private void SetArrowPositions()
        //{
        //    var arrowOffsetY = transform.up * (uitbouw.Extents.y - 0.01f);
        //    userMovementAxes[userMovementAxes.Length - 2].transform.position = uitbouw.LeftCenter - arrowOffsetY;
        //    userMovementAxes[userMovementAxes.Length - 1].transform.position = uitbouw.RightCenter - arrowOffsetY;
        //}

        private void Update()
        {
            if (AllowDrag && uitbouw.TransformGizmo.MoveModeSelected)
                ProcessUserInput();

            LimitPositionOnWall();

            //SetArrowPositions();
        }

        private void ProcessUserInput()
        {
            foreach (var axis in uitbouw.UserMovementAxes) //drag input
            {
                transform.position += axis.DeltaPosition;
            }
        }

        private void LimitPositionOnWall()
        {
            var extents = uitbouw.ActiveBuilding.SelectedWall.WallMesh.bounds.extents;
            var projectedExtents = Vector3.ProjectOnPlane(extents, Vector3.up);

            var projectedPosition = Vector3.ProjectOnPlane(transform.position, Vector3.up);
            var projectedCenter = Vector3.ProjectOnPlane(uitbouw.ActiveBuilding.SelectedWall.CenterPoint, Vector3.up);

            var maxOffset = projectedExtents.magnitude + uitbouw.Width / 2 - 0.1f; //subtract 0.1f to deal with rounding

            if (Vector3.Distance(projectedPosition, projectedCenter) > maxOffset)
            {
                ClipDistance(maxOffset);
            }
        }

        private void ClipDistance(float maxOffset)
        {
            var maxPos = uitbouw.ActiveBuilding.SelectedWall.CenterPoint + transform.right * maxOffset;
            var minPos = uitbouw.ActiveBuilding.SelectedWall.CenterPoint - transform.right * maxOffset;

            if (Vector3.Distance(transform.position, minPos) > Vector3.Distance(transform.position, maxPos))
                transform.position = maxPos;
            else
                transform.position = minPos;

            uitbouw.SnapToGround(uitbouw.ActiveBuilding);
        }

        public virtual void SetAllowMovement(bool allowed)
        {
            AllowDrag = allowed && T3DInit.Instance.IsEditMode;
            var measuring = GetComponent<UitbouwMeasurement>();
            measuring.DrawDistanceActive = allowed;

            //userMovementAxes[userMovementAxes.Length - 2].gameObject.SetActive(allowed);
            //userMovementAxes[userMovementAxes.Length - 1].gameObject.SetActive(allowed);
        }
    }
}