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
        public bool AllowBoundaryFeatureEditing { get; private set; } = true;

        private Vector3 featureDeltaPos;

        private void OnEnable()
        {
            LibraryComponentSelectedEvent.OnComponentSelectedEvent += LibraryComponentSelected;
        }

        private void OnDisable()
        {
            LibraryComponentSelectedEvent.OnComponentSelectedEvent -= LibraryComponentSelected;
        }

        private void LibraryComponentSelected(object source, LibraryComponentSelectedEvent.LibraryEventArgs args)
        {
            if (args.Type == LibraryComponentSelectedEvent.LibraryEventArgsType.BoundaryFeature && ActiveFeature)
            {
                DeselectFeature();
            }
        }

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
            if (AllowBoundaryFeatureEditing)
            {
                ProcessUserInput();

                if (ActiveFeature && ActiveFeature.ActiveMode == EditMode.Reposition)
                    ProcessDrag(ActiveFeature);
            }
        }

        private void ProcessUserInput()
        {
            //Ray ray = ServiceLocator.GetService<CameraModeChanger>().ActiveCamera.ScreenPointToRay(Input.mousePosition);
            //LayerMask boundaryFeaturesMask = LayerMask.GetMask("StencilMask");
            LayerMask boundaryFeaturesMask = LayerMask.GetMask("BoundaryFeatures");

            //print(ObjectClickHandler.GetClickOnObject(boundaryFeaturesMask));
            var click = ObjectClickHandler.GetClickOnObject(true, out var hit, boundaryFeaturesMask);
            if (click && !EventSystem.current.IsPointerOverGameObject())
            {
                BoundaryFeature clickedBoundaryFeature = null;
                if (hit.collider != null)
                    clickedBoundaryFeature = hit.collider.GetComponentInParent<BoundaryFeature>();
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

        private void ProcessDrag(BoundaryFeature feature)
        {
            Ray ray = ServiceLocator.GetService<CameraModeChanger>().ActiveCamera.ScreenPointToRay(Input.mousePosition);
            var mask = LayerMask.GetMask("Uitbouw");
            bool casted = Physics.Raycast(ray, out var hit, Mathf.Infinity, mask);


            if (casted && Input.GetMouseButtonDown(0))
            {
                featureDeltaPos = hit.point - feature.transform.position;
            }

            ObjectClickHandler.GetDrag(out var wallCollider, mask);
            if (ObjectClickHandler.GetDragOnObject(feature.GetComponentInChildren<Collider>(), true) && casted && feature.Wall.GetComponent<Collider>() == wallCollider)
            {
                feature.transform.position = hit.point - featureDeltaPos;
            }
        }

        public void SetAllowBoundaryFeatureEditing(bool allow)
        {
            AllowBoundaryFeatureEditing = allow;
            if (!allow && ActiveFeature)
            {
                DeselectFeature();
            }
        }
    }
}
