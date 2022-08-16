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

        public bool AllowRotation { get; private set; } = true; //global lock

        private float lastx;
        private float lastAngle;

        private void Awake()
        {
            uitbouw = GetComponent<UitbouwBase>();
        }

        private void Update()
        {
            if (AllowRotation && uitbouw.Gizmo.Mode == GizmoMode.Rotate && uitbouw.IsDraggingMovementAxis)                
                HandleMouse();
            else
            {                
                lastAngle = transform.rotation.eulerAngles.y;
                lastx = Input.mousePosition.x;
            }
        }

        void HandleMouse()
        {
            var xdiff = Input.mousePosition.x - lastx;
            var angleDiff = lastAngle - transform.rotation.eulerAngles.y;
            transform.RotateAround(uitbouw.CenterPoint, transform.up, angleDiff - (xdiff / 5));
        }

        public virtual void SetAllowRotation(bool allowed)
        {
            AllowRotation = allowed && ServiceLocator.GetService<T3DInit>().IsEditMode;
        }
    }
}
