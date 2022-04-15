using System;
using System.Collections;
using System.Collections.Generic;
using Netherlands3D.Interface;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Netherlands3D.T3D.Uitbouw
{
    public class UitbouwTransformGizmoButtons : WorldPointFollower
    {
        [SerializeField]
        private GizmoDragButton moveToggle, rotateToggle;
        private UitbouwTransformGizmo gizmo;

        //public bool MoveModeSelected => moveToggle.isOn;
        //public bool RotateModeSelected => rotateToggle.isOn;

        public void SetActive(bool active, bool allowMove = true, bool allowRotate = true)
        {
            gameObject.SetActive(active);
            moveToggle.transform.parent.gameObject.SetActive(allowMove);
            rotateToggle.transform.parent.gameObject.SetActive(allowRotate);
        }

        public void SubscribeListeners(UitbouwTransformGizmo rotateGizmo)
        {
            gizmo = rotateGizmo;
            moveToggle.Toggle.onValueChanged.AddListener(OnMoveToggleValueChanged);
            rotateToggle.Toggle.onValueChanged.AddListener(OnRotateToggleValueChanged);
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
