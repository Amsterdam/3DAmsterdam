using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Netherlands3D.T3D.Uitbouw
{
    [RequireComponent(typeof(UitbouwBase))]
    public class UitbouwMovement : MonoBehaviour
    {
        private UitbouwBase uitbouw;

        private bool allowDragOnMouseUp = false; //needed to avoid inadvertently dragging when placing the uitbouw in the first state.
        public bool AllowDrag { get; private set; } = false;

        //public bool AllowHeightDrag { get; private set; } = false;
        public float HeightOffset { get; set; }

        private void Awake()
        {
            uitbouw = GetComponent<UitbouwBase>();
        }

        private void Start()
        {
            SetAllowMovement(AllowDrag);
        }

        private void Update()
        {
            if (AllowDrag && uitbouw.Gizmo.Mode == GizmoMode.Move)
            {
                ProcessPlanarMoveUserInput();
            }
            else if (AllowDrag && uitbouw.Gizmo.Mode == GizmoMode.MoveHeight)
            {
                ProcessHeightMoveUserInput();
            }

            ProcessMovementLimits();

            if (allowDragOnMouseUp && Input.GetMouseButtonUp(0)) //set allowdrag if the button is released and the flag is set in SetAllowMovement()
            {
                AllowDrag = true;
                allowDragOnMouseUp = false;
                print("setting allow drag to true");
            }
        }

        private void ProcessMovementLimits()
        {
            if (ServiceLocator.GetService<T3DInit>().HTMLData.SnapToWall && uitbouw.ActiveBuilding && uitbouw.ActiveBuilding.SelectedWall.WallIsSelected)
            {
                SnapToWall(uitbouw.ActiveBuilding.SelectedWall);
                LimitPositionOnWall();
            }
            else
            {
                LimitPositionWithinPerceelRadius(RestrictionChecker.ActivePerceel);
            }

            SnapToGround(uitbouw.ActiveBuilding);
        }

        private void ProcessPlanarMoveUserInput()
        {
            foreach (var axis in uitbouw.UserMovementAxes) //drag input
            {
                if (ServiceLocator.GetService<T3DInit>().HTMLData.SnapToWall)
                    transform.position += axis.LateralDeltaPosition;
                else
                    transform.position += axis.PlanarDeltaPosition;
            }
        }

        private void ProcessHeightMoveUserInput()
        {
            foreach (var axis in uitbouw.UserMovementAxes) //drag input
            {
                HeightOffset += axis.HeightDeltaPosition;
            }
        }

        public void SetPosition(Vector3 pos)
        {
            transform.position = pos;
            ProcessMovementLimits();
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
            transform.position = new Vector3(transform.position.x, building.GroundLevel + HeightOffset /*+ Height / 2*/, transform.position.z);
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

        private void LimitPositionWithinPerceelRadius(PerceelRenderer perceel)
        {
            var perceelRadius = perceel.Radius;
            var perceelCenter = new Vector2(perceel.Center.x, perceel.Center.z);
            var position2D = new Vector2(transform.position.x, transform.position.z);
            var dist = Vector2.Distance(position2D, perceelCenter);

            if (dist > perceelRadius)
            {
                var fromOriginToObject = position2D - perceelCenter;
                fromOriginToObject *= perceelRadius / dist;
                var newPos = perceelCenter + fromOriginToObject;
                transform.position = new Vector3(newPos.x, transform.position.y, newPos.y);
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

        public void SetAllowMovement(bool allowed)
        {
            //if a mouse button is down when setting AllowDrag, it should not automatically enter drag mode, because this can cause the uitbouw to jump position.
            //instead, wait until the mouse button is released to set the AllowDrag (in Update())
            if (!Input.GetMouseButton(0))
            {
                AllowDrag = allowed && ServiceLocator.GetService<T3DInit>().IsEditMode;
            }
            else
            {
                AllowDrag = false;
                allowDragOnMouseUp = allowed && ServiceLocator.GetService<T3DInit>().IsEditMode;
                print("allowing drag on mouse up");
            }

            var measuring = GetComponent<UitbouwMeasurement>();
            measuring.DrawDistanceActive = allowed && ServiceLocator.GetService<T3DInit>().HTMLData.SnapToWall;

            var freeMeasuring = GetComponent<UitbouwFreeMeasurement>();
            freeMeasuring.DrawDistanceActive = allowed && !ServiceLocator.GetService<T3DInit>().HTMLData.SnapToWall;

            //if (allowed && AllowHeightDrag)
            //    SetAllowHeightMovement(false);

            //userMovementAxes[userMovementAxes.Length - 2].gameObject.SetActive(allowed);
            //userMovementAxes[userMovementAxes.Length - 1].gameObject.SetActive(allowed);
        }

        public void SetAllowHeightMovement(bool allowed)
        {
            var heightMeasuring = GetComponent<UitbouwHeightMeasurement>();
            heightMeasuring.DrawDistanceActive = allowed;
        }
    }
}