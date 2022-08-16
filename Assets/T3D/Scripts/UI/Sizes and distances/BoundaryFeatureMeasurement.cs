using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Netherlands3D.T3D.Uitbouw.BoundaryFeatures
{
    public class BoundaryFeatureMeasurement : DistanceMeasurement
    {
        private BoundaryFeature boundaryFeature;

        protected override void Awake()
        {
            base.Awake();
            boundaryFeature = GetComponent<BoundaryFeature>();
        }

        protected override void DrawLines()
        {
            var leftWallPoint = Vector3.ProjectOnPlane(boundaryFeature.Wall.LeftBoundPosition - transform.position, transform.forward);
            var rightWallPoint = Vector3.ProjectOnPlane(boundaryFeature.Wall.RightBoundPosition - transform.position, transform.forward);
            var topWallPoint = Vector3.ProjectOnPlane(boundaryFeature.Wall.TopBoundPosition - transform.position, transform.forward);
            var bottomWallPoint = Vector3.ProjectOnPlane(boundaryFeature.Wall.BottomBoundPosition - transform.position, transform.forward);

            leftWallPoint = Vector3.Project(leftWallPoint, transform.right) + transform.position;
            rightWallPoint = Vector3.Project(rightWallPoint, transform.right) + transform.position;
            topWallPoint = Vector3.Project(topWallPoint, transform.up) + transform.position;
            bottomWallPoint = Vector3.Project(bottomWallPoint, transform.up) + transform.position;

            var wallPoints = new Vector3[]
            {
                leftWallPoint,
                rightWallPoint,
                topWallPoint,
                bottomWallPoint
            };

            var featurePoints = new Vector3[]
            {
                boundaryFeature.LeftBoundPosition,
                boundaryFeature.RightBoundPosition,
                boundaryFeature.TopBoundPosition,
                boundaryFeature.BottomBoundPosition
            };

            for (int i = 0; i < lines.Count; i++) //make a loop to easily disable lines by reducing numberOfLines
            {
                DrawLine(i, featurePoints[i], wallPoints[i]);//direction matters for resize
            }
        }

        protected override void Measuring_DistanceInputOverride(BuildingMeasuring source, Vector3 direction, float delta)
        {
            //var deltaVector = Quaternion.Inverse(transform.rotation) * direction * delta;
            //boundaryFeature.transform.localPosition += deltaVector;
            boundaryFeature.transform.position += direction * delta;
        }
    }
}
