using System.Collections;
using System.Collections.Generic;
using Netherlands3D.Cameras;
using UnityEngine;
namespace Netherlands3D.T3D.Uitbouw
{
    [RequireComponent(typeof(UitbouwBase))]
    public class UitbouwRotation : MonoBehaviour
    {
        private Vector3 previousIntersectionPoint;
        private Vector3 previousOrigin;

        public bool AllowRotation { get; private set; } = true;

        void Update()
        {
            if (AllowRotation && GetComponent<UitbouwBase>().TransformGizmo.WantsToRotate)
            {
                Rotate();
            }
        }

        private void Rotate()
        {
            var deltaAngle = CalculateDeltaAngle();
            foreach (Transform t in transform)
            {
                t.RotateAround(transform.position, transform.up, deltaAngle); //rotate children because snapping occurs on this Transform
            }
        }

        private float CalculateDeltaAngle()
        {
            var newOrigin = transform.position;
            var groundPlane = new Plane(transform.up, newOrigin);
            var ray = CameraModeChanger.Instance.ActiveCamera.ScreenPointToRay(Input.mousePosition);

            Vector3 newIntersectionPoint;
            if (groundPlane.Raycast(ray, out float enter))
            {
                newIntersectionPoint = ray.origin + (ray.direction * enter);
            }
            else
            {
                var horizonPlane = new Plane(-CameraModeChanger.Instance.ActiveCamera.transform.forward, previousIntersectionPoint);
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
            AllowRotation = allowed && T3DInit.Instance.IsEditMode;
        }
    }
}
