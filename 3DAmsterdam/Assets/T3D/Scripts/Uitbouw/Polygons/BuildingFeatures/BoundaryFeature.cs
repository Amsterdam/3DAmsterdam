using System;
using System.Collections;
using System.Collections.Generic;
using Netherlands3D.Interface;
using UnityEngine;

namespace Netherlands3D.T3D.Uitbouw.BoundaryFeatures
{
    public class BoundaryFeature : SquarePolygon
    {
        public UitbouwMuur Wall { get; private set; }
        //public Transform featureTransform { get; private set; }

        public EditMode ActiveMode { get; private set; }

        private DistanceMeasurement[] distanceMeasurements;

        private EditUI editUI;

        private MeshFilter meshFilter;

        protected override void Awake()
        {
            base.Awake();
            //featureTransform = transform.parent;
            distanceMeasurements = GetComponents<DistanceMeasurement>();

            meshFilter = meshTransform.GetComponent<MeshFilter>();

            editUI = CoordinateNumbers.Instance.CreateEditUI(this);

            SetMode(EditMode.None);
        }

        public void SetWall(UitbouwMuur wall)
        {
            //if a new wall is being set
            if (Wall != wall)
            {
                //remove the hole from the current wall, if the current wall exists
                if (Wall != null)
                    Wall.GetComponent<CitySurface>().TryRemoveHole(this); 

                //set the new wall
                Wall = wall;

                //add the hole to the new wall, if the new wall exists
                if (Wall != null)
                    Wall.GetComponent<CitySurface>().TryAddHole(this); //add the hole to the new wall

            }
            transform.position = wall.transform.position;
            transform.forward = wall.transform.forward;
        }

        protected virtual void Update()
        {
            SnapToWall();
            SetButtonPositions();
        }

        private void SnapToWall()
        {
            transform.position = Wall.WallPlane.ClosestPointOnPlane(transform.position);
        }

        private void SetButtonPositions()
        {
            var pos = transform.position + transform.rotation * meshFilter.mesh.bounds.extents;
            editUI.AlignWithWorldPosition(pos);
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