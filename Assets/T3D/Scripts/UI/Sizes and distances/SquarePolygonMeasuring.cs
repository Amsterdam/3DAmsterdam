using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Netherlands3D.T3D.Uitbouw
{
    public class SquarePolygonMeasuring : DistanceMeasurement
    {
        private SquareSurface square;

        protected override void Awake()
        {
            base.Awake();
            square = GetComponent<SquareSurface>();
        }

        protected override void Measuring_DistanceInputOverride(BuildingMeasuring source, Vector3 direction, float delta)
        {
            var deltaVector = Quaternion.Inverse(transform.rotation) * direction * delta;
            var newSize = square.Size - (Vector2)deltaVector;

            square.SetSize(newSize - new Vector2(0.0001f, 0.0001f));
        }

        protected override void DrawLines()
        {
            var corners = square.Surface.SolidSurfacePolygon.Vertices;

            DrawLine(0, corners[0], corners[3]); //direction matters for resize
            DrawLine(1, corners[2], corners[3]); //direction matters for resize
        }
    }
}
