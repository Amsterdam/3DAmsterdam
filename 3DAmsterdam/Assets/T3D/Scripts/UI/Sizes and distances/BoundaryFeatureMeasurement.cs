using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Netherlands3D.T3D.Uitbouw.BoundaryFeatures
{
    public class BoundaryFeatureMeasurement : DistanceMeasurement
    {
        private BoundaryFeature boundaryFeature;

        private void Awake()
        {
            boundaryFeature = GetComponent<BoundaryFeature>();
        }

        protected override void DrawLines()
        {
            var leftWallPoint = Vector3.ProjectOnPlane(boundaryFeature.Wall.LeftBound - transform.position, transform.forward);
            var rightWallPoint = Vector3.ProjectOnPlane(boundaryFeature.Wall.RightBound - transform.position, transform.forward);
            var topWallPoint = Vector3.ProjectOnPlane(boundaryFeature.Wall.TopBound - transform.position, transform.forward);
            var bottomWallPoint = Vector3.ProjectOnPlane(boundaryFeature.Wall.BottomBound - transform.position, transform.forward);

            leftWallPoint = Vector3.Project(leftWallPoint, transform.right) + transform.position;
            rightWallPoint = Vector3.Project(rightWallPoint, transform.right) + transform.position;
            topWallPoint = Vector3.Project(topWallPoint, transform.up) + transform.position;
            bottomWallPoint = Vector3.Project(bottomWallPoint, transform.up) + transform.position;

            DrawLine(0, boundaryFeature.LeftBound, leftWallPoint); //direction matters for resize
            DrawLine(1, boundaryFeature.RightBound, rightWallPoint); //direction matters for resize
            DrawLine(2, boundaryFeature.TopBound, topWallPoint); //direction matters for resize
            DrawLine(3, boundaryFeature.BottomBound, bottomWallPoint); //direction matters for resize
        }

        protected override void Measuring_DistanceInputOverride(BuildingMeasuring source, Vector3 direction, float delta)
        {
            var deltaVector = Quaternion.Inverse(transform.rotation) * direction * delta;
            boundaryFeature.featureTransform.localPosition += deltaVector;
        }
    }
}
