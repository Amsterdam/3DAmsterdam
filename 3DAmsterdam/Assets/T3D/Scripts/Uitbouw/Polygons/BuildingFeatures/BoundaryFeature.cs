using System;
using System.Collections;
using System.Collections.Generic;
using Netherlands3D.Cameras;
using Netherlands3D.Interface;
using UnityEngine;

namespace Netherlands3D.T3D.Uitbouw.BoundaryFeatures
{
    public class BoundaryFeature : SquareSurface
    {
        public UitbouwMuur Wall { get; private set; }
        public EditMode ActiveMode { get; private set; }

        private DistanceMeasurement[] distanceMeasurements;

        private EditUI editUI;
        [SerializeField]
        float editUIOffset = 0.2f;


        protected override void Awake()
        {
            base.Awake();
            distanceMeasurements = GetComponents<DistanceMeasurement>();
            editUI = CoordinateNumbers.Instance.CreateEditUI(this);

            SetMode(EditMode.None);
        }

        public void SetWall(UitbouwMuur wall)
        {
            this.Wall = wall;
            transform.position = wall.transform.position;
            transform.forward = wall.transform.forward;
        }

        protected override void Update()
        {
            base.Update();
            SnapToWall();
            SetButtonPositions();
            ProcessDrag();
        }

        Vector3 deltaPos;
        private void ProcessDrag()
        {
            Ray ray = CameraModeChanger.Instance.ActiveCamera.ScreenPointToRay(Input.mousePosition);
            var mask = LayerMask.GetMask("Maskable");
            bool casted = Physics.Raycast(ray, out var hit, Mathf.Infinity, mask);

            if (casted && Input.GetMouseButtonDown(0))
            {
                deltaPos = hit.point - transform.position;
            }

            ObjectClickHandler.GetDrag(out var wallCollider, mask);
            if (ObjectClickHandler.GetDragOnObject(GetComponentInChildren<Collider>(), true) && casted && Wall.GetComponent<Collider>() == wallCollider)
            {
                transform.position = hit.point - deltaPos;
            }
        }

        private void SnapToWall()
        {
            transform.position = Wall.WallPlane.ClosestPointOnPlane(transform.position);
        }

        private void SetButtonPositions()
        {
            //var pos = meshTransform.position + meshTransform.rotation * meshTransform.GetComponent<SpriteRenderer>().bounds.extents;
            var trCorner = GetCorner(rightBound, topBound);
            var dir = (trCorner - transform.position).normalized;
            editUI.AlignWithWorldPosition(trCorner + dir * editUIOffset);
        }

        public void SetMode(EditMode newMode)
        {
            ActiveMode = newMode;
            //var distanceMeasurements = this.distanceMeasurements;
            for (int i = 0; i < distanceMeasurements.Length; i++)
            {
                distanceMeasurements[i].DrawDistanceActive = distanceMeasurements[i].Mode == newMode;
            }

            editUI.gameObject.SetActive(newMode != EditMode.None);
        }

        public void DeleteFeature()
        {
            Destroy(editUI.gameObject);
            Destroy(gameObject);
        }

        public void EditFeature()
        {
            if (ActiveMode == EditMode.Reposition)
                SetMode(EditMode.Resize);
            else
                SetMode(EditMode.Reposition);

            editUI.UpdateSprites(ActiveMode);
        }
    }
}