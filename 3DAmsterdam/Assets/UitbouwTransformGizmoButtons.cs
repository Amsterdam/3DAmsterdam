using System;
using System.Collections;
using System.Collections.Generic;
using Netherlands3D.Interface;
using UnityEngine;
using UnityEngine.UI;

namespace Netherlands3D.T3D.Uitbouw
{
    public class UitbouwTransformGizmoButtons : WorldPointFollower
    {
        [SerializeField]
        private Toggle moveToggle, rotateToggle;

        public bool MoveModeSelected => moveToggle.isOn;
        public bool RotateModeSelected => rotateToggle.isOn;

        private UitbouwTransformGizmo gizmo;

        public void SetActive(bool active)
        {
            gameObject.SetActive(active);
        }

        public void SubscribeListeners(UitbouwTransformGizmo rotateGizmo)
        {
            gizmo = rotateGizmo;
            moveToggle.onValueChanged.AddListener(OnMoveToggleValueChanged);
            rotateToggle.onValueChanged.AddListener(OnRotateToggleValueChanged);
        }

        private void OnMoveToggleValueChanged(bool isOn)
        {
            if (isOn)
                gizmo.SetMode(GizmoMode.Move);
        }

        private void OnRotateToggleValueChanged(bool isOn)
        {
            if (isOn)
                gizmo.SetMode(GizmoMode.Rotate);
        }
    }
}
