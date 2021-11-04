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
            Ray ray = CameraModeChanger.Instance.ActiveCamera.ScreenPointToRay(Input.mousePosition);
            LayerMask boundaryFeaturesMask = LayerMask.GetMask("StencilMask");
            LayerMask uiMask = LayerMask.GetMask("UI");
            if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
            {
                if (ActiveFeature)
                {
                    DeselectFeature();
                }
                else if (Physics.Raycast(ray, out var hit, Mathf.Infinity, boundaryFeaturesMask))
                {
                    var bf = hit.collider.GetComponentInParent<BoundaryFeature>();
                    print(bf);
                    if (bf)
                    {
                        SelectFeature(bf);
                    }
                }
            }
        }
    }
}
