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
        public Transform featureTransform { get; private set; }

        public EditMode ActiveMode { get; private set; }

        private DistanceMeasurement[] distanceMeasurements;
        private BoundaryFeatureButton editButton;
        private BoundaryFeatureButton deleteButton;

        private MeshFilter meshFilter;

        //[SerializeField]
        private float buttonDistance = 0.15f;

        private void Awake()
        {
            featureTransform = transform.parent;
            distanceMeasurements = GetComponents<DistanceMeasurement>();

            meshFilter = GetComponent<MeshFilter>();

            deleteButton = CoordinateNumbers.Instance.CreateButton(DeleteFeature);
            editButton = CoordinateNumbers.Instance.CreateButton(EditFeature);

            SetMode(EditMode.None);
        }

        public void SetWall(UitbouwMuur wall)
        {
            this.Wall = wall;
            featureTransform.position = wall.transform.position;
            featureTransform.forward = wall.transform.forward;
        }

        protected virtual void Update()
        {
            SetButtonPositions();
        }

        private void SetButtonPositions()
        {
            if (deleteButton)
            {
                var pos = featureTransform.position + transform.rotation * meshFilter.mesh.bounds.extents + featureTransform.right * buttonDistance; 
                deleteButton.AlignWithWorldPosition(pos);
            }
            if (editButton)
            {
                var pos = featureTransform.position + transform.rotation * meshFilter.mesh.bounds.extents - featureTransform.right * buttonDistance;
                editButton.AlignWithWorldPosition(pos);
            }
        }

        public void SetMode(EditMode newMode)
        {
            ActiveMode = newMode;
            var distanceMeasurements = this.distanceMeasurements;
            for (int i = 0; i < distanceMeasurements.Length; i++)
            {
                distanceMeasurements[i].DrawDistanceActive = (int)newMode == i;
            }

            deleteButton.gameObject.SetActive((int)newMode >= 0);
            editButton.gameObject.SetActive((int)newMode >= 0);
        }

        private void DeleteFeature()
        {
            if(deleteButton)
                Destroy(deleteButton.gameObject);
            if(editButton)
                Destroy(editButton.gameObject);

            Destroy(gameObject);
        }

        private void EditFeature()
        {
            if (ActiveMode == EditMode.Reposition)
            {
                SetMode(EditMode.Resize);
            }
            else
            {
                SetMode(EditMode.Reposition);
            }
        }
    }
}