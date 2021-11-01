using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Netherlands3D.T3D.Uitbouw
{
    public class SquarePolygonMeasuring : DistanceMeasurement
    {
        private SquarePolygon square;

        private void Awake()
        {
            square = GetComponent<SquarePolygon>();
        }

        protected override void Measuring_DistanceInputOverride(BuildingMeasuring source, Vector3 direction, float delta)
        {
            var deltaVector = Quaternion.Inverse(transform.rotation) * direction * delta;
            var newSize = square.Size - (Vector2)deltaVector;

            square.SetSize(newSize);
        }

        protected override void DrawLines()
        {
            var corners = square.Polygon;

            DrawLine(0, corners[0], corners[1]); //direction matters for resize
            DrawLine(1, corners[2], corners[1]); //direction matters for resize
        }
    }
}
