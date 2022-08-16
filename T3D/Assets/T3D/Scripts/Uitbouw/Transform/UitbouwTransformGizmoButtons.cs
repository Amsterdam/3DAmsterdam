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
        private GizmoDragButton moveToggle, rotateToggle, measureToggle, moveHeightToggle;
        private UitbouwTransformGizmo gizmo;

        //public bool MoveModeSelected => moveToggle.isOn;
        //public bool RotateModeSelected => rotateToggle.isOn;
        private void Start()
        {
            // needed to update the graphics of the toggles so they don't all apear as isOn = true for some reason
            gameObject.SetActive(false);
            gameObject.SetActive(true);
        }

        public void SetActive(bool active, bool allowMove = true, bool allowRotate = true, bool allowMeasure = true, bool allowHeightMovement = true)
        {
            gameObject.SetActive(active);
            moveToggle.transform.parent.gameObject.SetActive(allowMove);
            rotateToggle.transform.parent.gameObject.SetActive(allowRotate);
            measureToggle.transform.parent.gameObject.SetActive(allowMeasure);
            moveHeightToggle.transform.parent.gameObject.SetActive(allowHeightMovement);
        }

        public void SubscribeListeners(UitbouwTransformGizmo rotateGizmo)
        {
            gizmo = rotateGizmo;
            moveToggle.Toggle.onValueChanged.AddListener(OnMoveToggleValueChanged);
            rotateToggle.Toggle.onValueChanged.AddListener(OnRotateToggleValueChanged);
            measureToggle.Toggle.onValueChanged.AddListener(OnMeasureToggleValueChanged);
            moveHeightToggle.Toggle.onValueChanged.AddListener(OnMoveHeightToggleValueChanged);
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

        private void OnMeasureToggleValueChanged(bool isOn)
        {
            if (isOn)
                gizmo.SetMode(GizmoMode.Measure);
        }

        private void OnMoveHeightToggleValueChanged(bool isOn)
        {
            if (isOn)
                gizmo.SetMode(GizmoMode.MoveHeight);
        }
    }
}
