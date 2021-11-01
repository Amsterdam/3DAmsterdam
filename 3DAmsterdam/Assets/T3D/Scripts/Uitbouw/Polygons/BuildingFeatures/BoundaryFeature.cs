using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Netherlands3D.T3D.Uitbouw.BoundaryFeatures
{
    public class BoundaryFeature : SquarePolygon
    {
        public UitbouwMuur Wall { get; private set; }
        public Transform featureTransform { get; private set; }

        public DistanceMeasurement[] DistanceMeasurements => GetComponents<DistanceMeasurement>();
        public EditMode ActiveMode { get; private set; }

        private void Awake()
        {
            featureTransform = transform.parent;
        }

        public void SetWall(UitbouwMuur wall)
        {
            this.Wall = wall;
            featureTransform.position = wall.transform.position;
            featureTransform.forward = wall.transform.forward;
        }

        public void SetDistanceMeasurementsActive(EditMode mode)
        {
            ActiveMode = mode;
            var distanceMeasurements = DistanceMeasurements;
            for (int i = 0; i < distanceMeasurements.Length; i++)
            {
                distanceMeasurements[i].DrawDistanceActive = (int)mode == i;
            }
        }
    }
}