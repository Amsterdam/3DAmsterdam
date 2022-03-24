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

        [SerializeField]
        private bool snapToWall;

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

            ProcessSnapping();
            LimitPositionOnWall();

            //SetArrowPositions();
        }

        private void ProcessUserInput()
        {
            foreach (var axis in uitbouw.UserMovementAxes) //drag input
            {
                if (snapToWall)
                    transform.position += axis.LateralDeltaPosition;
                else
                    transform.position += axis.PlanarDeltaPosition;
            }
        }

        private void ProcessSnapping()
        {
            if (uitbouw.ActiveBuilding && uitbouw.ActiveBuilding.SelectedWall.WallIsSelected)
            {
                if (snapToWall)
                    SnapToWall(uitbouw.ActiveBuilding.SelectedWall);

                SnapToGround(uitbouw.ActiveBuilding);
            }
        }

        private void SnapToWall(WallSelector selectedWall)
        {
            var dir = selectedWall.WallPlane.normal;
            transform.forward = -dir; //rotate towards correct direction

            //remove local x component
            var diff = selectedWall.WallPlane.ClosestPointOnPlane(transform.position) - transform.position; //moveVector
            var rotatedPoint = Quaternion.Inverse(transform.rotation) * diff; //moveVector aligned in world space
            rotatedPoint.x = 0; //remove horizontal component
            var projectedPoint = transform.rotation * rotatedPoint; //rotate back
            var newPoint = projectedPoint + transform.position; // apply movevector

            transform.position = newPoint;//hit.point - uitbouwAttachDirection * Depth / 2;
        }

        private void SnapToGround(BuildingMeshGenerator building)
        {
            transform.position = new Vector3(transform.position.x, building.GroundLevel /*+ Height / 2*/, transform.position.z);
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

            SnapToGround(uitbouw.ActiveBuilding);
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