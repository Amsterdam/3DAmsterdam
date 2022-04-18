using System.Collections;
using System.Collections.Generic;
using Netherlands3D.Cameras;
using UnityEngine;

namespace Netherlands3D.T3D.Uitbouw
{
    [RequireComponent(typeof(UitbouwBase))]
    public class UitbouwRotation : MonoBehaviour
    {
        private UitbouwBase uitbouw;
        private Vector3 previousIntersectionPoint;
        private Vector3 previousOrigin;

        public bool AllowRotation { get; private set; } = true; //global lock
        private bool isRotating;

        private void Awake()
        {
            uitbouw = GetComponent<UitbouwBase>();
        }

        private void Update()
        {
            if (AllowRotation && uitbouw.Gizmo.Mode == GizmoMode.Rotate && uitbouw.IsDraggingMovementAxis)
                ProcessUserInput();
            else
                isRotating = false;
        }

        private void ProcessUserInput()
        {
            if (!isRotating)
            {
                CalculateDeltaAngle(); // used to set the previousOrigin and Intersection so that a jump does not occur due to old garbage values 
                isRotating = true;
            }
            Rotate();
        }

        private void Rotate()
        {
            var deltaAngle = CalculateDeltaAngle();
            transform.RotateAround(uitbouw.CenterPoint, transform.up, deltaAngle); //rotate children because snapping occurs on this Transform
            //foreach (Transform t in transform)
            //{
            //    t.RotateAround(uitbouw.CenterPoint, transform.up, deltaAngle); //rotate children because snapping occurs on this Transform
            //}
        }

        private float CalculateDeltaAngle()
        {
            var newOrigin = transform.position;
            var groundPlane = new Plane(transform.up, newOrigin);
            var ray = ServiceLocator.GetService<CameraModeChanger>().ActiveCamera.ScreenPointToRay(Input.mousePosition);

            Vector3 newIntersectionPoint;
            if (groundPlane.Raycast(ray, out float enter))
            {
                newIntersectionPoint = ray.origin + (ray.direction * enter);
            }
            else
            {
                var horizonPlane = new Plane(-ServiceLocator.GetService<CameraModeChanger>().ActiveCamera.transform.forward, previousIntersectionPoint);
                horizonPlane.Raycast(ray, out enter);
                var planeIntersection = ray.origin + (ray.direction * enter);
                newIntersectionPoint = groundPlane.ClosestPointOnPlane(planeIntersection);
            }

            var previousDir = (previousIntersectionPoint - previousOrigin).normalized;
            var newDir = (newIntersectionPoint - newOrigin).normalized;

            var angle = Vector3.SignedAngle(previousDir, newDir, transform.up);

            previousOrigin = newOrigin;
            previousIntersectionPoint = newIntersectionPoint;

            return angle;
        }

        public virtual void SetAllowRotation(bool allowed)
        {
            AllowRotation = allowed && ServiceLocator.GetService<T3DInit>().IsEditMode;
        }
    }
}
