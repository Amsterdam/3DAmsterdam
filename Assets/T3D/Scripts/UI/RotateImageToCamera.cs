using System;
using System.Collections;
using System.Collections.Generic;
using Netherlands3D.Cameras;
using UnityEngine;

public enum Axis
{
    //None = -1,
    X = 0,
    Y = 1,
    Z = 2,
}

namespace Netherlands3D.T3D.Uitbouw
{
    public class RotateImageToCamera : MonoBehaviour
    {
        [SerializeField]
        private bool RotateToX;
        [SerializeField]
        private bool RotateToY;
        [SerializeField]
        private bool RotateToZ;

        private void Update()
        {
            ProcessAngle();
        }

        private void ProcessAngle()
        {
            var closestAxis = GetClosestAxis();
            print(closestAxis);
            switch (closestAxis)
            {
                case Axis.X:
                    if (RotateToX)
                        transform.rotation = Quaternion.LookRotation(transform.parent.right, transform.parent.up);
                    //transform.forward = transform.parent.right;//transform.rotation * Vector3.right;
                    break;
                case Axis.Y:
                    if (RotateToY)
                        transform.rotation = Quaternion.LookRotation(transform.parent.up, transform.parent.right);
                    //transform.forward = transform.parent.up; //transform.rotation * Vector3.up;
                    break;
                case Axis.Z:
                    if (RotateToZ)
                        transform.rotation = Quaternion.LookRotation(transform.parent.forward, transform.parent.right);
                    //transform.forward = transform.parent.forward; //transform.rotation * Vector3.forward;
                    break;
            }
        }

        private Axis GetClosestAxis()
        {
            var cameraComponent = ServiceLocator.GetService<CameraModeChanger>().ActiveCamera;
            var dir = (cameraComponent.transform.position - transform.position).normalized;

            //Vector3 pos = cameraComponent.transform.position;
            //Vector3 dir = (this.transform.position - cameraComponent.transform.position).normalized;
            Debug.DrawLine(transform.position, transform.position + dir * 10, Color.red);

            var rotatedDir = Quaternion.Inverse(transform.parent.rotation) * dir;

            if (Mathf.Abs(rotatedDir.x) > Mathf.Abs(rotatedDir.y) && Mathf.Abs(rotatedDir.x) > Mathf.Abs(rotatedDir.z))
                return Axis.X;
            else if (Mathf.Abs(rotatedDir.y) > Mathf.Abs(rotatedDir.z))
                return Axis.Y;
            else
                return Axis.Z;
        }
    }
}