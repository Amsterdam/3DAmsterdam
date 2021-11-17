using System;
using System.Collections;
using System.Collections.Generic;
using Netherlands3D.Cameras;
using Netherlands3D.Interface;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Netherlands3D.T3D.Uitbouw.BoundaryFeatures
{
    public class BoundaryFeatureEditHandler : MonoBehaviour
    {
        public BoundaryFeature ActiveFeature { get; private set; }

        public void SelectFeature(BoundaryFeature feature)
        {
            feature.SetMode(EditMode.Reposition);
            ActiveFeature = feature;
        }

        public void DeselectFeature()
        {
            if (ActiveFeature)
                ActiveFeature.SetMode(EditMode.None);

            ActiveFeature = null;
        }

        private void Update()
        {
            ProcessUserInput();
        }

        private void ProcessUserInput()
        {
            //Ray ray = CameraModeChanger.Instance.ActiveCamera.ScreenPointToRay(Input.mousePosition);
            //LayerMask boundaryFeaturesMask = LayerMask.GetMask("StencilMask");
            LayerMask boundaryFeaturesMask = LayerMask.GetMask("Default");

            //print(ObjectClickHandler.GetClickOnObject(boundaryFeaturesMask));
            var click = ObjectClickHandler.GetClickOnObject(true, out var collider, boundaryFeaturesMask);
            if (click && !EventSystem.current.IsPointerOverGameObject())
            {
                var clickedBoundaryFeature = collider?.GetComponentInParent<BoundaryFeature>();
                if (ActiveFeature)
                {
                    DeselectFeature();
                }
                if (clickedBoundaryFeature)
                {
                    SelectFeature(clickedBoundaryFeature);
                }
            }
        }
    }
}
